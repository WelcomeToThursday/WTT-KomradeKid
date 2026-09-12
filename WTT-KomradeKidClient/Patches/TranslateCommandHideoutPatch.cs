#if !UNITY_EDITOR
using Comfort.Common;
using Diz.Jobs;
using EFT;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using GameBoyEmulator.CustomEFTData;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameBoyEmulator.Patches
{
    internal class TranslateCommandHideoutPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(HideoutPlayerOwner).GetMethod(nameof(HideoutPlayerOwner.TranslateCommand));
        }

        [PatchPrefix]
        public static bool PatchPrefix(HideoutPlayerOwner __instance, ECommand command, ref InputNode.ETranslateResult __result)
        {
            HideoutPlayer hideoutPlayer = __instance.HideoutPlayer;

            if (hideoutPlayer.HandsController != null && hideoutPlayer.HandsController is CustomUsableItemController customUsableItemController)
            {
                switch (command)
                {
                    case ECommand.ExamineWeapon:
                        customUsableItemController.ExamineWeapon();
                        break;
                    case ECommand.ToggleAlternativeShooting:
                        customUsableItemController.ToggleAim();
                        __result = InputNode.ETranslateResult.Block;
                        return false;
                    case ECommand.Escape:
                        hideoutPlayer.SetEmptyHands(method_2);
                        break;
                }
            }

            if (command is >= ECommand.SelectFastSlot4 and <= ECommand.SelectFastSlot0)
            {
                EBoundItem boundItem = command switch
                {
                    ECommand.SelectFastSlot4 => EBoundItem.Item4,
                    ECommand.SelectFastSlot5 => EBoundItem.Item5,
                    ECommand.SelectFastSlot6 => EBoundItem.Item6,
                    ECommand.SelectFastSlot7 => EBoundItem.Item7,
                    ECommand.SelectFastSlot8 => EBoundItem.Item8,
                    ECommand.SelectFastSlot9 => EBoundItem.Item9,
                    ECommand.SelectFastSlot0 => EBoundItem.Item10,
                    _ => throw new ArgumentOutOfRangeException()
                };

                Item boundItemObj = ResolveLiveBoundItem(hideoutPlayer, boundItem);

                if (boundItemObj is CustomUsableItem)
                {
                    ProceedItemGameBoy(hideoutPlayer, boundItem, boundItemObj);
                    return false;
                }
            }
            return true;
        }

        private static async void ProceedItemGameBoy(
            HideoutPlayer hideoutPlayer,
            EBoundItem boundItem,
            Item boundItemObj)
        {
            if (boundItemObj is not CustomUsableItem gameBoy)
            {
                return;
            }

            await HideoutPlayer.UpdateHideoutBundles(hideoutPlayer.Profile, JobYieldPriority.Immediate);

            Item liveGameBoy = ResolveLiveBoundItem(hideoutPlayer, boundItem) ?? gameBoy;

            Console.WriteLine(
                $"[GameBoy] Owner via OriginalInventory | Id={liveGameBoy.Id} | " +
                $"Address={liveGameBoy.CurrentAddress}");

            InventoryScreenQuickAccessPanel inventoryScreenQuickAccessPanel =
                Singleton<CommonUI>.Instance.EftBattleUIScreen.QuickAccessPanel;

            inventoryScreenQuickAccessPanel.Show(hideoutPlayer.OriginalInventory, ItemUiContext.Instance);
            inventoryScreenQuickAccessPanel.AnimatedShow(true);

            var checkResult = liveGameBoy.CheckAction(null);

            Console.WriteLine(
                $"[GameBoy] CheckAction(null) via OriginalInventory-resolved item | " +
                $"Succeeded={checkResult.Succeeded}");

            bool canProceed =
                checkResult.Succeeded &&
                !hideoutPlayer.OriginalInventory.IsChangingWeapon &&
                (!hideoutPlayer.IsInBufferZone || hideoutPlayer.CanManipulateWithHandsInBufferZone);

            if (canProceed)
            {
                TryProceedPatch.ProceedCustomUsableItem((CustomUsableItem)liveGameBoy, method_131, true);
                return;
            }

            hideoutPlayer.SetItemInHands(liveGameBoy, method_131);
        }

        private static void method_131(Result<IHandsController> result)
        {
            if (result.Failed)
            {
                ConsoleScreen.LogError($"[GameBoy] SetItemInHands failed: {result.Error}");
            }
            else
            {
                ConsoleScreen.LogWarning($"[GameBoy] SetItemInHands succeeded");
            }
        }

        private static void method_2(Result<IEmptyHandsController> callback)
        {
            Complete(null);
        }

        private static void Complete(string error)
        {
        }

        private static Item ResolveLiveBoundItem(HideoutPlayer hideoutPlayer, EBoundItem boundItem)
        {
            Item staleItem = hideoutPlayer.Inventory.FastAccess.GetBoundItem(boundItem);
            if (staleItem == null)
            {
                return null;
            }

            Item liveItem = hideoutPlayer.OriginalInventory.Inventory
                .GetPlayerItems()
                .FirstOrDefault(i => i.Id == staleItem.Id);

            Console.WriteLine(
                $"[GameBoy] Resolve | StaleId={staleItem.Id} | " +
                $"FoundInOriginalInventory={liveItem != null} | " +
                $"StaleAddress={staleItem.CurrentAddress}");

            return liveItem ?? staleItem;
        }
    }
}
#endif