#if !UNITY_EDITOR
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using GameBoyEmulator.CustomEFTData;

namespace GameBoyEmulator.Patches
{
    internal class TryProceedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("TryProceed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(Player __instance, Item item, Callback<IHandsController> completeCallback, bool scheduled = true)
        {
            if (item is CustomUsableItem customItem)
            {
                Player.CG_TryProceed1 cg_TryProceed = new Player.CG_TryProceed1();
                cg_TryProceed.completeCallback = completeCallback;
                cg_TryProceed.player_0 = __instance;
                __instance.StopBlindFire();
                __instance.RemoveLeftHandItem();
                __instance.RaiseHandsChanging();
                if (!__instance.InventoryController.IsAtReachablePlace(item))
                {
                    __instance.SetFirstAvailableItem(cg_TryProceed.completeCallback);
                    return false;
                }
                
                Player.MedsController medsController = __instance.HandsController as Player.MedsController;
                if (medsController != null && !__instance.IsAI)
                {
                    medsController.SetOnUsedCallback(new Callback<IQuickUseItem>(Player.CG_Class1318.CG_Class1318.method_24));
                }

                Player.CG_TryProceed cg_TryProceed2 = new Player.CG_TryProceed();
                cg_TryProceed2.CG_TryProceed1 = cg_TryProceed;
                ProceedCustomUsableItem(customItem, cg_TryProceed2.CG_TryProceed1.completeCallback, scheduled);
                return false;
            }

            return true;
        }

        public static void ProceedCustomUsableItem(CustomUsableItem item, Callback<IHandsController> completeCallback, bool scheduled = true)
        {
            Player.CG_ProceedPortableRangeFinder cg_ProceedPortableRangeFinder = new Player.CG_ProceedPortableRangeFinder();
            {
                completeCallback = completeCallback;
            };

            if (KomradeClient.Player is ClientPlayer)
            {
                KomradeClient.Player.Proceed<ClientCustomUsableItemController>(item, cg_ProceedPortableRangeFinder.method_0, scheduled);
                return;
            }
            KomradeClient.Player.Proceed<CustomUsableItemController>(item, cg_ProceedPortableRangeFinder.method_1, scheduled);
        }

    }
}
#endif