#if !UNITY_EDITOR
using Comfort.Common;
using Diz.LanguageExtensions;
using Diz.Utils;
using EFT;
using EFT.AssetsManager;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using GameBoyEmulator.CustomEFTData;
using GameBoyEmulator.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GameBoyEmulator.Managers
{
    public class GameBoyModItemManager : MonoBehaviour
    {
        public async Task UnloadCartridge(
    CustomUsableItem gameBoy,
    InventoryController inventoryController)
        {
            if (gameBoy == null || inventoryController == null)
            {
                return;
            }

            GameBoyCartridge currentCartridge = gameBoy.GetCurrentCartridge();
            if (currentCartridge == null)
            {
                return;
            }

            GridItemAddress destination = inventoryController.Inventory.Equipment
                .GetPrioritizedGridsForUnloadedObject(false)
                .Select(grid => grid.FindLocationForItem(currentCartridge))
                .Where(address => address != null)
                .OrderBy(address => address.Grid.GridWidth * address.Grid.GridHeight)
                .FirstOrDefault();

            if (destination == null)
            {
                return;
            }

            OperationResult<MoveResult> operationResult = ItemManipulator.Move(
                currentCartridge,
                destination,
                inventoryController,
                true);

            if (operationResult.Failed)
            {
                return;
            }

            if (!operationResult.Value.CanExecute(inventoryController))
            {
                return;
            }

            await inventoryController.TryRunNetworkTransaction(operationResult, null);
        }
        public async Task UnloadCartridgeAsync(CustomUsableItem gameBoy, CompoundItem[] compoundItem, InventoryController inventoryControllerClass, ItemUiContext itemUiContext, AnimationManager animationManager)
        {
            GameBoyCartridge currentCartridge = gameBoy.GetCurrentCartridge();
            if (currentCartridge != null)
            {
                InventoryEquipment equipment = inventoryControllerClass.Inventory.Equipment;
                bool inStorage = equipment.Contains(currentCartridge);

                if (!inStorage && compoundItem == null)
                {
                    NotificationManager.DisplaySingletonWarningNotification("Error: equipment panel is null while cartridge is not in storage.".Localized());
                }
                else
                {
                    IEnumerable<CompoundItem> targets = compoundItem != null
                        ? equipment.ToEnumerable().Concat(compoundItem)
                        : equipment.ToEnumerable();

                    OperationResult<IItemOperationResult> value = ItemManipulator.QuickFindAppropriatePlace(
                        currentCartridge, inventoryControllerClass, targets,
                        ItemManipulator.EMoveItemOrder.UnloadWeapon, true);

                    if (value.Succeeded)
                    {
                        bool success = (await ItemUiContext.RunWithSound(inventoryControllerClass, currentCartridge, value)).Succeed;
                        if (!success && inventoryControllerClass.CanThrow(currentCartridge))
                        {
                            inventoryControllerClass.ThrowItem(currentCartridge, true);
                        }
                    }
                    else
                    {
                        NotificationManager.DisplayWarningNotification("Can't find a place for item".Localized());
                    }
                }
            }
        }

        public async Task LoadCartridgeAsync(CustomUsableItem gameBoy, CompoundItem[] compoundItem, InventoryController inventoryControllerClass, ItemUiContext itemUiContext, AnimationManager animationManager)
        {

            if (gameBoy.GetCurrentCartridge() == null)
            {
                Slot cartridgeSlot = gameBoy.GetCartridgeSlot();
                GameBoyCartridge gameboyCartridge = FindCartridge(compoundItem, inventoryControllerClass);

                if (gameboyCartridge == null)
                {
                    NotificationManager.DisplayWarningNotification("Can't find any appropriate cartridge".Localized());
                }
                else if (gameboyCartridge.PinLockState == EItemPinLockState.Locked)
                {
                    NotificationManager.DisplaySingletonWarningNotification(new ItemManipulator.ItemManuallyLockedError(gameboyCartridge).GetLocalizedDescription());
                }
                else
                {
                    var result = await ItemUiContext.RunWithSound(inventoryControllerClass, gameboyCartridge,
                        ItemManipulator.Move(gameboyCartridge, cartridgeSlot?.CreateItemAddress(),
                            inventoryControllerClass, true));

                    if (!result.Succeed && inventoryControllerClass.CanThrow(gameboyCartridge))
                    {
                        inventoryControllerClass.ThrowItem(gameboyCartridge, true);
                    }
                }
            }
            else
            {
                NotificationManager.DisplaySingletonWarningNotification("A cartridge is already loaded in the GameBoy.".Localized());
            }
        }

        private GameBoyCartridge FindCartridge(IEnumerable<CompoundItem> compoundItem, InventoryController inventoryControllerClass)
        {
            return (InGameStatus.InRaid
                ? inventoryControllerClass.GetReachableItemsOfType<GameBoyCartridge>()
                : compoundItem.GetTopLevelItems().OfType<GameBoyCartridge>())
                .FirstOrDefault();
        }

        public void OnCartridgeAppeared(Slot cartridgeSlot, GameBoyCartridge cartridge, bool animated)
        {
            if (cartridgeSlot == null || cartridge == null)
            {
                return;
            }

            Player player = KomradeClient.Player;

            InsertCartridge(Singleton<ObjectsFactory>.Instance.CreateItem(cartridge, Player.GetVisibleToCamera(player), player, true), animated);
        }

        public void InsertCartridge(GameObject cartridgeObject, bool animated)
        {
            cartridgeObject.SetActive(false);

            if (animated)
            {
                TriggerCartridgeLoadAnimation(cartridgeObject);
            }
            else
            {
                cartridgeObject.SetActive(true);

            }

            InsertCartridgeIntoBone(cartridgeObject);
        }

        private void TriggerCartridgeLoadAnimation(GameObject cartridgeObject)
        {
            GameObject emulatorObject = CommonUtils.GetGameBoyEmulatorObject();

            if (emulatorObject != gameObject)
            {
                return;
            }

            var emulatorManager = emulatorObject.GetComponent<DefaultEmulatorManager>();
            if (emulatorManager == null)
            {
                return;
            }

            var animationManager = emulatorObject.GetComponent<AnimationManager>();
            if (animationManager == null)
            {
                return;
            }

            AudioSource animationAudioSource = animationManager.animationManagerAudioSource;
            if (animationAudioSource == null)
            {
                return;
            }

            GameObjectEnableTrigger enableCartridgeGameObject = new(0.45f, cartridgeObject, true);
            animationManager.AddAnimationTrigger("GameboyLoad", enableCartridgeGameObject);

            SoundTrigger blowAirSfx = new(0.75f, animationAudioSource, emulatorManager.blowAirSfx);
            animationManager.AddAnimationTrigger("GameboyLoad", blowAirSfx);

            animationManager.PlayAnimation("GameboyLoad", 3);
        }

        private void InsertCartridgeIntoBone(GameObject cartridgeObject)
        {
            Player player = KomradeClient.Player;
            GameObject controllerObject = player?.HandsController?.ControllerGameObject;

            Transform cartridgeBone = CommonUtils.FindChildRecursive(controllerObject?.transform, "Cartridge");

            if (cartridgeBone == null)
            {
                return;
            }

            if (cartridgeBone.childCount > 0)
            {
                if (!cartridgeBone.GetChild(0).gameObject.activeSelf)
                {
                    cartridgeBone.GetChild(0).gameObject.SetActive(true);
                }
                return;
            }

            cartridgeObject.transform.SetParent(cartridgeBone, false);

            cartridgeObject.transform.localPosition = Vector3.zero;

            cartridgeObject.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            TransformTools.SetLayersRecursively(cartridgeObject, LayerMask.NameToLayer("Player"));
        }

        public void OnAccessoryAppeared(Slot accessorySlot, GameBoyAccessory accessory)
        {
            if (accessorySlot == null || accessory == null)
            {
                return;
            }

            Player player = KomradeClient.Player;

            string accessoryType = accessory.AccessoryType;

            InsertAccessoryIntoBone(Singleton<ObjectsFactory>.Instance.CreateItem(accessory, Player.GetVisibleToCamera(player), player, true), accessoryType);
        }

        public void InsertAccessoryIntoBone(GameObject accessoryObject, string accessoryType)
        {
            accessoryObject.SetActive(false);

            var player = KomradeClient.Player;
            var controllerObject = player?.HandsController?.ControllerGameObject;
            var emulatorObject = CommonUtils.GetGameBoyEmulatorObject();

            if (emulatorObject != gameObject)
            {
                return;
            }
            var emulatorManager = emulatorObject.GetComponent<DefaultEmulatorManager>();
            if (emulatorManager == null)
            {
                return;
            }
            Transform accessoryBone = CommonUtils.FindChildRecursive(controllerObject?.transform, "Cable");

            if (accessoryBone == null)
            {
                return;
            }

            if (accessoryBone.childCount > 0)
            {
                var existingAccessory = accessoryBone.GetChild(0).gameObject;
                if (!existingAccessory.activeSelf)
                {
                    existingAccessory.SetActive(true);
                }
                return;
            }

            accessoryObject.transform.SetParent(accessoryBone, false);
            accessoryObject.transform.localPosition = Vector3.zero;
            accessoryObject.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            TransformTools.SetLayersRecursively(accessoryObject, LayerMask.NameToLayer("Player"));

            accessoryObject.SetActive(true);

            if (emulatorManager.emulatorOn && accessoryType == "Light")
            {
                emulatorManager.ToggleLightAndCover(true);
            }
        }

        public void RemoveCartridge(bool animated)
        {
            Player player = KomradeClient.Player;
            GameObject controllerObject = player?.HandsController?.ControllerGameObject;

            Transform cartridgeBone = CommonUtils.FindChildRecursive(controllerObject?.transform, "Cartridge");
            if (cartridgeBone == null)
            {
                return;
            }

            if (cartridgeBone.childCount <= 0)
            {
                return;
            }

            GameObject cartridgeObject = cartridgeBone.GetChild(0).gameObject;

            GameObject emulatorGameObject = CommonUtils.GetGameBoyEmulatorObject();

            if (animated)
            {
                var animationManager = emulatorGameObject.GetComponent<AnimationManager>();
                animationManager.AddAnimationTrigger("GameboyUnload", new GameObjectDestroyTrigger(1.6f, cartridgeObject));
                animationManager.PlayAnimation("GameboyUnload", 3);
            }
            else
            {
                AssetPoolObject assetPoolObject = cartridgeObject.GetComponent<AssetPoolObject>();
                if (assetPoolObject != null)
                {
                    ConsoleScreen.Log("Found asset pool component! returning to pool");
                    AssetPoolObject.ReturnToPool(cartridgeObject);
                }
            }
        }
        public void RemoveAccessory()
        {
            Player player = KomradeClient.Player;
            GameObject controllerObject = player?.HandsController?.ControllerGameObject;

            Transform accessoryBone = CommonUtils.FindChildRecursive(controllerObject?.transform, "Cable");
            if (accessoryBone == null)
            {
                return;
            }

            if (accessoryBone.childCount <= 0)
            {
                return;
            }

            GameObject accessoryObject = accessoryBone.GetChild(0).gameObject;

            AssetPoolObject assetPoolObject = accessoryObject.GetComponent<AssetPoolObject>();
            if (assetPoolObject != null)
            {
                ConsoleScreen.Log("Found asset pool component! returning to pool");
                AssetPoolObject.ReturnToPool(accessoryObject);
            }

        }
        public Light FindLightComponent()
        {
            Player player = KomradeClient.Player;
            GameObject controllerObject = player?.HandsController?.ControllerGameObject;

            Transform accessoryBone = CommonUtils.FindChildRecursive(controllerObject?.transform, "Cable");
            if (accessoryBone == null)
            {
                return null;
            }

            if (accessoryBone.childCount <= 0)
            {
                return null;
            }

            GameObject accessoryObject = accessoryBone.GetChild(0).gameObject;
            Light light = accessoryObject.GetComponentsInChildren<Light>().FirstOrDefault();
            if (light != null)
            {
                return light;
            }
            return null;
        }
        public GameObject FindLightCoverOffGameObject()
        {
            Player player = KomradeClient.Player;
            GameObject controllerObject = player?.HandsController?.ControllerGameObject;
            if (controllerObject == null)
            {
                return null;
            }

            Transform accessoryBone = CommonUtils.FindChildRecursive(controllerObject.transform, "Cable");
            if (accessoryBone == null || accessoryBone.childCount == 0)
            {
                return null;
            }

            GameObject accessoryObject = accessoryBone.GetChild(0).gameObject;
            if (accessoryObject == null)
            {
                return null;
            }

            Transform lightCover = accessoryObject.transform.Find("Light_Cover_Off");
            return lightCover?.gameObject;
        }
        public GameObject FindLightCoverOnGameObject()
        {
            Player player = KomradeClient.Player;
            GameObject controllerObject = player?.HandsController?.ControllerGameObject;
            if (controllerObject == null)
            {
                return null;
            }

            Transform accessoryBone = CommonUtils.FindChildRecursive(controllerObject.transform, "Cable");
            if (accessoryBone == null || accessoryBone.childCount == 0)
            {
                return null;
            }

            GameObject accessoryObject = accessoryBone.GetChild(0).gameObject;
            if (accessoryObject == null)
            {
                return null;
            }

            Transform lightCover = accessoryObject.transform.Find("Light_Cover_On");
            return lightCover?.gameObject;
        }
    }
}
#endif