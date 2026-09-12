#if !UNITY_EDITOR
using EFT;
using EFT.NextObservedPlayer;
using GameBoyEmulator.CustomEFTData;
using SPT.Reflection.Patching;
using System.Reflection;

namespace GameBoyEmulator.Patches
{
    internal class HandsControllerGetWeaponAnimationTypePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ObservedPlayerHandsController).GetMethod("GetWeaponAnimationType", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix(ref PlayerAnimator.EWeaponAnimationType __result, ObservedPlayerHandsController __instance)
        {
            if (__instance.ItemInHands is CustomUsableItem)
            {
                __result = PlayerAnimator.EWeaponAnimationType.Pistol;
                return false;
            }

            return true;
        }
    }
}
#endif