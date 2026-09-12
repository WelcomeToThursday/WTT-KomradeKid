#if !UNITY_EDITOR
using EFT;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using System.Threading.Tasks;
using GameBoyEmulator.CustomEFTData;

namespace GameBoyEmulator.Patches
{
    internal class ClientUsableItemControllerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ClientUsableItemController).GetMethod("CreateAsync", BindingFlags.Static | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix(ref Task<ClientUsableItemController> __result, ClientPlayer player, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                throw new Exception("Invalid itemId");
            }
            CustomUsableItem item = player.InventoryController.FindItem<CustomUsableItem>(itemId);
            if (item != null)
            {
                __result = Player.UsableItemController.CreateControllerAsync<ClientUsableItemController>(player, item);
                return false;
            }

            return true;
        }

    }
}
#endif