#if !UNITY_EDITOR
using EFT.NetworkPackets;

namespace GameBoyEmulator.CustomEFTData
{
    public class ClientCustomUsableItemController : CustomUsableItemController
    {
        public UsableItemPacket UsableItemPacket;

        public override void CompassStateHandler(bool isActive)
        {
            UsableItemPacket.CompassPacket = new CompassPacket(isActive);
            base.CompassStateHandler(isActive);
        }

        public override void ShowGesture(EInteraction gesture)
        {
            UsableItemPacket.Gesture = gesture;
            base.ShowGesture(gesture);
        }

        public override bool ExamineWeapon()
        {
            bool result = base.ExamineWeapon();
            if (result)
            {
                UsableItemPacket.ExamineWeapon = true;
            }
            return result;
        }

        public override void SetInventoryOpened(bool opened)
        {
            UsableItemPacket.EnableInventoryPacket = new EnableInventoryPacket
            {
                EnableInventory = true,
                InventoryStatus = opened
            };
            base.SetInventoryOpened(opened);
        }

        public override void SetAim(bool value)
        {
            bool isAiming = IsAiming;
            base.SetAim(value);

            if (IsAiming != isAiming)
            {
                UsableItemPacket.ToggleAim = true;
                UsableItemPacket.IsAiming = IsAiming;
            }
        }

        public override void Hide()
        {
            base.Hide();
            UsableItemPacket.HideItem = true;
        }
    }
}
#endif