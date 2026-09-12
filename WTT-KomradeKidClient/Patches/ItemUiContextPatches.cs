#if !UNITY_EDITOR
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using System.Threading.Tasks;
using EFT.UI;
using EFT.InventoryLogic;
using Comfort.Common;
using System.Collections.Generic;
using System.Linq;
using GameBoyEmulator.CustomEFTData;
using GameBoyEmulator.Managers;

namespace GameBoyEmulator.Patches
{
    internal class InstallModPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(ItemUiContext).GetMethod("InstallMod", BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool Prefix(ItemContext itemContext, CompoundItem[] collections,
            ItemUiContext __instance, ref Task __result)
        {
            if (itemContext.Item is GameBoyCartridge)
            {
                __result = RunCartridgeInstallation(__instance, itemContext, collections);

                return false;
            }
            else if (itemContext.Item is GameBoyAccessory)
            {
                __result = RunAccessoryInstallation(__instance, itemContext, collections);
                return false;
            }
            return true;
        }

        private static async Task RunCartridgeInstallation(ItemUiContext itemUiContext, ItemContext itemContext, CompoundItem[] collections)
        {
            try
            {
                await CustomContextButtonManager.InstallCartridge(itemUiContext, itemContext, collections);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing cartridge: {ex}");
            }
        }
        private static async Task RunAccessoryInstallation(ItemUiContext itemUiContext, ItemContext itemContext, CompoundItem[] collections)
        {
            try
            {
                await CustomContextButtonManager.InstallAccessory(itemUiContext, itemContext, collections);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing cartridge: {ex}");
            }
        }
    }

#if DEBUG
/*
    internal class LoadWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GClass3405).GetMethod("method_47", BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool Prefix(GClass3405 __instance, ref Task __result)
        {
            var itemField = typeof(ItemUiContext).GetField("item_0", BindingFlags.NonPublic | BindingFlags.Instance);

            if (itemField != null)
            {
                var item = itemField.GetValue(__instance);

                if (item is CustomUsableItem)
                {
                    CustomUsableItem customUsableItem = (CustomUsableItem)item;
                    ItemUiContext itemUiContext = ItemUiContext.Instance;
                    CompoundItem[] compoundItem = itemUiContext?.CompoundItem_0;

                    __result = RunCustomLoadMethod(__instance, itemUiContext, customUsableItem, compoundItem);

                    return false;
                }
                return true;
            }
            return true;
        }

        private static async Task RunCustomLoadMethod(GClass3405 __instance, ItemUiContext itemUiContext, CustomUsableItem customUsableItem, CompoundItem[] gclass2644)
        {
            try
            {
                await CustomContextButtonManager.LoadCartridge(itemUiContext, customUsableItem, gclass2644);

                __instance.RequestRedrawForItem();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading custom usable item: {ex}");
            }
        }
    }
    internal class UnLoadWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GClass3402).GetMethod("method_37", BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool Prefix(GClass3402 __instance, ref Task __result)
        {
            var itemField = typeof(ItemUiContext).GetField("item_0", BindingFlags.NonPublic | BindingFlags.Instance);

            if (itemField != null)
            {
                var item = itemField.GetValue(__instance);

                if (item is CustomUsableItem)
                {
                    CustomUsableItem customUsableItem = (CustomUsableItem)item;
                    ItemUiContext itemUiContext = ItemUiContext.Instance;
                    CompoundItem[] compoundItem = itemUiContext?.CompoundItem_0;

                    __result = RunCustomUnloadMethod(itemUiContext, customUsableItem, compoundItem);

                    return false;
                }
                return true;
            }
            return true;
        }

        private static async Task RunCustomUnloadMethod(ItemUiContext itemUiContext, CustomUsableItem customUsableItem, CompoundItem[] gclass2644)
        {
            try
            {
                await CustomContextButtonManager.UnloadCartridge(itemUiContext, customUsableItem, gclass2644);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unloading custom usable item: {ex}");
            }
        }
    }

    internal class IsActivePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GClass3437).GetMethod("IsActive", BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool Prefix(GClass3437 __instance, EItemInfoButton button, ref bool __result)
        {
            var itemField = typeof(GClass3437).GetField("item_0", BindingFlags.NonPublic | BindingFlags.Instance);

            if (itemField != null)
            {
                var item = itemField.GetValue(__instance);

                if (item is CustomUsableItem customUsableItem)
                {
                    ConsoleScreen.Log($"[IsActivePatch] called: {button}");

                    if (!customUsableItem.ItemInteractionButtons.Contains(button))
                    {
                        __result = false;
                        ConsoleScreen.Log($"[IsActivePatch] Button {button} not allowed for this item.");
                        return false;
                    }

                    List<EItemInfoButton> list = new List<EItemInfoButton>
                {
                    EItemInfoButton.Inspect,
                    EItemInfoButton.Examine,
                    EItemInfoButton.Discard
                };

                    if (!__instance.Boolean_2 && !list.Contains(button))
                    {
                        __result = false;
                        ConsoleScreen.Log($"[IsActivePatch] Button {button} blocked due to Boolean_2.");
                        return false;
                    }

                    if (button == EItemInfoButton.Load)
                    {
                        if (customUsableItem.GetCurrentCartridge() == null)
                        {
                            __result = true;
                            ConsoleScreen.Log("[IsActivePatch] Load allowed, no cartridge present.");
                            return false; 
                        }

                        __result = false;
                        ConsoleScreen.Log("[IsActivePatch] Load blocked, cartridge present.");
                        return false;
                    }
                    if (button == EItemInfoButton.Unload)
                    {
                        if (customUsableItem.GetCurrentCartridge() != null)
                        {
                            __result = true;
                            ConsoleScreen.Log("[IsActivePatch] Unload allowed, cartridge present.");
                            return false; 
                        }

                        __result = false;
                        ConsoleScreen.Log("[IsActivePatch] Unload blocked, no cartridge present.");
                        return false;
                    }
                }
                return true;
            }
            ConsoleScreen.Log("[IsActivePatch] item_0 field not found.");
            return true;
        }
    }

    internal class IsInteractivePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GClass3437).GetMethod("IsInteractive", BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool Prefix(GClass3437 __instance, EItemInfoButton button, ref IResult __result)
        {
            var itemField = typeof(GClass3437).GetField("item_0", BindingFlags.NonPublic | BindingFlags.Instance);

            if (itemField == null)
            {
                ConsoleScreen.Log("[IsInteractivePatch] item_0 field not found.");
                return true;
            }

            var item = itemField.GetValue(__instance);

            if (item is CustomUsableItem customUsableItem)
            {
                ConsoleScreen.Log($"[IsInteractivePatch] called: {button}");

                if (button == EItemInfoButton.Unload)
                {
                    if (customUsableItem.GetCurrentCartridge() == null)
                    {
                        __result = new FailedResult("Item doesn't contain cartridge");
                        ConsoleScreen.Log("[IsInteractivePatch] Unload blocked, no cartridge present.");
                        return false; 
                    }

                    __result = new SuccessfulResult();
                    ConsoleScreen.Log("[IsInteractivePatch] Unload allowed, cartridge present.");
                    return false; 
                }
                else if (button == EItemInfoButton.Load)
                {
                    if (customUsableItem.GetCurrentCartridge() != null)
                    {
                        __result = new FailedResult("Item already contains cartridge");
                        ConsoleScreen.Log("[IsInteractivePatch] Load blocked, cartridge already present.");
                        return false; 
                    }

                    __result = new SuccessfulResult();
                    ConsoleScreen.Log("[IsInteractivePatch] Load allowed, no cartridge present.");
                    return false;
                }
            }

            return true;
        }
    }
*/
#endif
}

#endif
