#if !UNITY_EDITOR
using Diz.LanguageExtensions;
using Diz.Utils;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using GameBoyEmulator.CustomEFTData;
using GameBoyEmulator.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameBoyEmulator.Managers
{

    internal abstract class CustomContextButtonManager
    {
        private static CustomUsableItem FindCustomUsableItem(InventoryController inventoryControllerClass, IEnumerable<CompoundItem> collections)
        {
            return (InGameStatus.InRaid
                ? inventoryControllerClass.GetReachableItemsOfType<CustomUsableItem>()
                : collections.GetTopLevelItems().OfType<CustomUsableItem>())
                .FirstOrDefault();
        }
        private static GameBoyCartridge FindGameBoyCartridge(InventoryController inventoryControllerClass, IEnumerable<CompoundItem> collections)
        {
            return (InGameStatus.InRaid
                ? inventoryControllerClass.GetReachableItemsOfType<GameBoyCartridge>()
                : collections.GetTopLevelItems().OfType<GameBoyCartridge>())
                .FirstOrDefault();
        }

        public static async Task InstallCartridge(ItemUiContext __itemUiContext, ItemContext itemContext, CompoundItem[] collections)
        {
            if (__itemUiContext == null)
            {
                return;
            }

            if (itemContext == null)
            {
                return;
            }

            if (collections == null || collections.Length == 0)
            {
                return;
            }

            InventoryController inventoryControllerClass = InventoryControllerAccessor.GetInventoryControllerClass(__itemUiContext);
            if (inventoryControllerClass == null)
            {
                return;
            }

            CustomUsableItem gameBoy = FindCustomUsableItem(inventoryControllerClass, collections);

            if (gameBoy.GetCurrentCartridge() == null)
            {
                Slot cartridgeSlot = gameBoy.GetCartridgeSlot();

                GameBoyCartridge gameboyCartridge = itemContext.Item as GameBoyCartridge;

                if (gameboyCartridge == null)
                {
                    NotificationManager.DisplaySingletonWarningNotification("Can't find any appropriate cartridge".Localized());
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
        public static async Task InstallAccessory(ItemUiContext __itemUiContext, ItemContext itemContext, CompoundItem[] collections)
        {
            if (__itemUiContext == null)
            {
                return;
            }

            if (itemContext == null)
            {
                return;
            }

            if (collections == null || collections.Length == 0)
            {
                return;
            }

            InventoryController inventoryControllerClass = InventoryControllerAccessor.GetInventoryControllerClass(__itemUiContext);
            if (inventoryControllerClass == null)
            {
                return;
            }

            CustomUsableItem gameBoy = FindCustomUsableItem(inventoryControllerClass, collections);

            if (gameBoy.GetCurrentAccessory() == null)
            {
                Slot accessorySlot = gameBoy.GetAccessorySlot();

                GameBoyAccessory gameboyAccessory = itemContext.Item as GameBoyAccessory;

                if (gameboyAccessory == null)
                {
                    NotificationManager.DisplaySingletonWarningNotification("Can't find any appropriate accessory".Localized());
                }
                else
                {

                    var result = await ItemUiContext.RunWithSound(inventoryControllerClass, gameboyAccessory,
                        ItemManipulator.Move(gameboyAccessory, accessorySlot?.CreateItemAddress(),
                            inventoryControllerClass, true));

                    if (!result.Succeed && inventoryControllerClass.CanThrow(gameboyAccessory))
                    {
                        inventoryControllerClass.ThrowItem(gameboyAccessory, true);
                    }
                }
            }
            else
            {
                NotificationManager.DisplaySingletonWarningNotification("Accessory is already loaded in the GameBoy.".Localized());
            }

        }

        public static async Task LoadCartridge(ItemUiContext __itemUiContext, CustomUsableItem gameboy, CompoundItem[] collections)
        {
            InventoryController inventoryControllerClass = InventoryControllerAccessor.GetInventoryControllerClass(__itemUiContext);
            if (inventoryControllerClass == null)
            {
                return;
            }

            if (gameboy.GetCurrentMagazine() == null)
            {
                Slot cartridgeSlot = gameboy.GetCartridgeSlot();
                GameBoyCartridge gameboyCartridge = FindGameBoyCartridge(inventoryControllerClass, collections);
                if (gameboyCartridge == null)
                {
                    NotificationManager.DisplaySingletonWarningNotification("Can't find any appropriate cartridge".Localized());
                }
                else
                {

                    var result = await ItemUiContext.RunWithSound(inventoryControllerClass, gameboyCartridge,
                        ItemManipulator.Move(gameboyCartridge, cartridgeSlot?.CreateItemAddress(),
                            inventoryControllerClass, true));
                    if (!result.Succeed && inventoryControllerClass.CanThrow(gameboyCartridge))
                    {
                        inventoryControllerClass.ThrowItem(gameboyCartridge,  true);
                    }
                }
            }
        }

        public static async Task UnloadCartridge(ItemUiContext __itemUiContext, CustomUsableItem gameboy, CompoundItem[] compoundItem)
        {
                GameBoyCartridge currentCartridge = gameboy.GetCurrentCartridge();
                if (currentCartridge != null)
                {
                    InventoryController inventoryControllerClass = InventoryControllerAccessor.GetInventoryControllerClass(__itemUiContext);

                    InventoryEquipment equipment = inventoryControllerClass.Inventory.Equipment;
                    bool flag;
                    if (!(flag = equipment.Contains(currentCartridge)) && compoundItem == null)
                    {
                        ConsoleScreen.LogError("Something went wrong. Right panel is null while mag is not from equipment.");
                    }
                    else
                    {
                        IEnumerable<CompoundItem> enumerable;
                        if (compoundItem != null)
                        {
                            enumerable = (flag ? equipment.ToEnumerable().Concat(compoundItem) : compoundItem.Concat(equipment.ToEnumerable()));
                        }
                        else
                        {
                            IEnumerable<CompoundItem> enumerable2 = equipment.ToEnumerable();
                            enumerable = enumerable2;
                        }
                        IEnumerable<CompoundItem> targets = enumerable;
                    OperationResult<IItemOperationResult> value = ItemManipulator.QuickFindAppropriatePlace(currentCartridge, inventoryControllerClass, targets, ItemManipulator.EMoveItemOrder.UnloadWeapon, true);
                        bool flag2;
                        if (flag2 = value.Succeeded)
                        {
                            flag2 = (await ItemUiContext.RunWithSound(inventoryControllerClass, currentCartridge, value)).Succeed;
                        }
                        if (!flag2)
                        {
                            if (!InGameStatus.InRaid)
                            {
                            NotificationManager.DisplaySingletonWarningNotification("Can't find a place for item".Localized());
                            }
                            else if (inventoryControllerClass.CanThrow(currentCartridge))
                            {
                                inventoryControllerClass.ThrowItem(currentCartridge, true);
                            }
                        }
                    }
                }
        }
    
    }
}
#endif