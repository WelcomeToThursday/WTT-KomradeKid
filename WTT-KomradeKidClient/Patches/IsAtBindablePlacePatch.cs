#if !UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using GameBoyEmulator.CustomEFTData;
using SPT.Reflection.Patching;

namespace GameBoyEmulator.Patches
{
    internal class IsAtBindablePlacePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(InventoryController).GetMethod("IsAtBindablePlace", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        public static void Postfix(InventoryController __instance, Item item, ref bool __result)
        {
            if (item is CustomUsableItem usableItem)
            {
                InventoryController.CG_IsAtBindablePlace cg_IsAtBindablePlace = new InventoryController.CG_IsAtBindablePlace();
                cg_IsAtBindablePlace.inventoryController_0 = __instance;
                if (usableItem.CurrentAddress != null && !(usableItem.Parent is OwnerItself))
                {
                    ItemAddress currentAddress = usableItem.Parent.Container.ParentItem.CurrentAddress;
                    cg_IsAtBindablePlace.parentSlot = ((currentAddress != null) ? currentAddress.Container : null) as Slot;
                    CompoundItem compoundItem = usableItem as CompoundItem;
                    __result = Inventory.FastAccessSlots.Select(new Func<EquipmentSlot, Slot>(cg_IsAtBindablePlace.method_0)).Any(new Func<Slot, bool>(cg_IsAtBindablePlace.method_1)) && (compoundItem == null || !compoundItem.MissingVitalParts.Any<Slot>()) && __instance.Examined(usableItem);
                }
            }
        }
    }

}
#endif