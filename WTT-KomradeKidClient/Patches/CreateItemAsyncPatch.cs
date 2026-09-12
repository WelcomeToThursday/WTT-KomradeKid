#if !UNITY_EDITOR
using Diz.Jobs;
using EFT;
using EFT.AssetsManager;
using EFT.CameraControl;
using EFT.InventoryLogic;
using EFT.Visual;
using GameBoyEmulator.CustomEFTData;
using JetBrains.Annotations;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GameBoyEmulator.Patches
{
    internal class CreateItemAsyncPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ObjectsFactory).GetMethod(
                "CreateItemAsync",
                BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(
            ObjectsFactory __instance,
            Item item,
            ECameraType cameraType,
            [CanBeNull] IPlayer player,
            bool isAnimated,
            YieldDelegate yield,
            CancellationToken ct,
            ref Task<GameObject> __result)
        {
            if (item is not CustomUsableItem &&
                item is not GameBoyCartridge &&
                item is not GameBoyAccessory)
            {
                return true;
            }

            __result = CreateCustomItemAsync(
                __instance,
                item,
                cameraType,
                player,
                isAnimated,
                yield,
                ct);

            return false;
        }

        private static async Task<GameObject> CreateCustomItemAsync(
            ObjectsFactory objectsFactory,
            Item item,
            ECameraType cameraType,
            [CanBeNull] IPlayer player,
            bool isAnimated,
            YieldDelegate yield,
            CancellationToken ct)
        {
            if (item == null)
            {
                return null;
            }

            if (objectsFactory._cancellationTokens.TryGetValue(
                    ObjectsFactory.PoolsCategory.Raid,
                    out CancellationToken poolToken))
            {
                using CancellationTokenSource linkedSource =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        poolToken,
                        ct);

                return await CreateCustomItemInternalAsync(
                    objectsFactory,
                    item,
                    cameraType,
                    player,
                    isAnimated,
                    yield,
                    linkedSource.Token);
            }

            return await CreateCustomItemInternalAsync(
                objectsFactory,
                item,
                cameraType,
                player,
                isAnimated,
                yield,
                ct);
        }

        private static async Task<GameObject> CreateCustomItemInternalAsync(
            ObjectsFactory objectsFactory,
            Item item,
            ECameraType cameraType,
            [CanBeNull] IPlayer player,
            bool isAnimated,
            YieldDelegate yield,
            CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            GameObject itemGameObject = objectsFactory.PopOrCreate(
                item.Prefab,
                ObjectsFactory.PoolsCategory.Raid);

            if (itemGameObject == null)
            {
                ObjectsFactory.Logger.LogError(
                    $"Failed to create GameObject for item: {item}",
                    Array.Empty<object>());

                return null;
            }

            ObjectsFactory.CG_Class1455 cleanup =
                new ObjectsFactory.CG_Class1455
                {
                    itemGameObject = itemGameObject
                };

            using CancellationTokenRegistration cancellationRegistration =
                ct.Register(cleanup.method_0);

            if (itemGameObject.scene.buildIndex != -1 &&
                itemGameObject.transform.parent == null &&
                Application.isPlaying)
            {
                UnityEngine.Object.DontDestroyOnLoad(itemGameObject);
            }

            itemGameObject.transform.localScale = Vector3.one;

            await yield(null);

            if (ct.IsCancellationRequested)
            {
                return null;
            }

            AssetPoolObject poolObject =
                itemGameObject.GetComponent<AssetPoolObject>();

            if (poolObject == null)
            {
                ObjectsFactory.Logger.LogError(
                    $"No AssetPoolObject found for item: {item}",
                    Array.Empty<object>());

                return null;
            }

            bool isGameBoyHandsItem = item is CustomUsableItem;

            WeaponPrefab weaponPrefab = null;
            Transform weaponHierarchy = null;

            if (isGameBoyHandsItem)
            {
                weaponPrefab = poolObject as WeaponPrefab;

                if (weaponPrefab == null)
                {
                    ObjectsFactory.Logger.LogError(
                        $"Custom usable item '{item}' does not have a WeaponPrefab.",
                        Array.Empty<object>());

                    return null;
                }

                weaponHierarchy = weaponPrefab.Hierarchy.transform;
            }

            await yield(null);

            if (ct.IsCancellationRequested)
            {
                return null;
            }

            bool hasAnimatedMods = false;

            if (item is ContainerCollection collection &&
                item is not CylinderMagazine)
            {
                hasAnimatedMods = await objectsFactory.AssembleMods(
                    collection,
                    isGameBoyHandsItem,
                    cameraType,
                    player,
                    isAnimated,
                    itemGameObject,
                    poolObject,
                    weaponHierarchy,
                    ct,
                    yield);
            }

            if (ct.IsCancellationRequested)
            {
                return null;
            }

            if (isGameBoyHandsItem)
            {
                weaponPrefab.Init(player, player != null);

                if (hasAnimatedMods && player != null)
                {
                    weaponPrefab.RebindAnimator(player);
                }
            }

            if (item is GameBoyCartridge cartridge)
            {
                cartridge.ApplyStickerTexture(itemGameObject);

                itemGameObject.name = itemGameObject.name.Replace(
                    "(Clone)",
                    string.Empty);
            }
            else if (item is GameBoyAccessory)
            {
                itemGameObject.name = itemGameObject.name.Replace(
                    "(Clone)",
                    string.Empty);
            }

            foreach (IDress dress in itemGameObject.GetComponents<IDress>())
            {
                dress.Init(item, isAnimated);
            }

            if (item is Mod mod && mod.IsAnimated)
            {
                itemGameObject.name = itemGameObject.name.Replace(
                    "(Clone)",
                    string.Empty);
            }

            return itemGameObject;
        }
    }
}
#endif