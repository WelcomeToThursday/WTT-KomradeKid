#if !UNITY_EDITOR
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using GameBoyEmulator.CustomEFTData;
using SPT.Reflection.Patching;
using System.Reflection;

namespace GameBoyEmulator.Patches
{
    internal class UsableAnimationsHandsControllerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ObservedPlayerUsableItemController).GetMethod("GetObservedUsableItem", BindingFlags.Static | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix(ref IObservedUsableItem __result, Item item)
        {
            if (item is CustomUsableItem)
            {
                __result = new CustomUsableItemInterfaceClass();
                return false;
            }
            return true;
        }
    }
}
#endif