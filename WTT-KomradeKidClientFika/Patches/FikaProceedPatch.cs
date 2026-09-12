using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Players;
using Fika.Core.Networking.LiteNetLib;
using Fika.Core.Networking.LiteNetLib.Utils;
using Fika.Core.Networking.Packets.Player.Common;
using Fika.Core.Networking.Packets.Player.Common.SubPackets;
using GameBoyEmulator.CustomEFTData;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace KomradeKidClientFika.Patches
{
    internal class FikaProceedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(FikaPlayer), (x) =>
            {
                var parameters = x.GetParameters();
                return x.Name == "Proceed" &&
                x.IsGenericMethod == true &&
                parameters.Length == 3 &&
                parameters[0].ParameterType == typeof(Item) &&
                parameters[2].ParameterType == typeof(bool);
            }).MakeGenericMethod(typeof(CustomUsableItemController));
        }

        [PatchPrefix]
        public static bool PatchPrefix(FikaPlayer __instance, Item item, Callback<IUsableItemController> callback, bool scheduled)
        { 
            if (item is CustomUsableItem customItem)
            {

                var handler = new CustomUsableItemControllerHandler(__instance, customItem);
                Func<CustomUsableItemController> controllerFunc = new(handler.ReturnController);
                handler.process = new Player.Process<CustomUsableItemController, IUsableItemController>(
                    __instance, controllerFunc, customItem, false);
                handler.confirmCallback = new Action(handler.SendPacket);
                handler.process.Proceed(new(handler.HandleResult), callback, scheduled);
                return false; 
            } 
            return true;
        }

        public static CustomUsableItemController Create(FikaPlayer player, Item item)
        {
            CustomUsableItemController controller = Player.UsableItemController.CreateController<CustomUsableItemController>(player, item);
            return controller;
        }

        // Mirror Fika's handler class structure
        private class CustomUsableItemControllerHandler(FikaPlayer player, Item item)
        {
            private readonly FikaPlayer _player = player;
            private readonly Item _item = item;
            public Player.Process<CustomUsableItemController, IUsableItemController> process;
            public Action confirmCallback;

            internal CustomUsableItemController ReturnController()
            {
                return Create(player, item);
            }

            internal void SendPacket()
            {
                _player.CommonPacket.Type = ECommonSubPacketType.Proceed;
                _player.CommonPacket.SubPacket = ProceedPacket.FromValue(default, _item.Id, 0f, 0, EProceedType.UsableItem, false);
                _player.PacketSender.NetworkManager.SendNetReusable(ref _player.CommonPacket, DeliveryMethod.ReliableOrdered, true);
            }

            internal void HandleResult(IResult result)
            {
                if (result.Succeed)
                {
                    confirmCallback();
                }
            }
        }
    }

    // Network packet structure mirroring Fika's implementation
    public struct CustomUsableItemPacket(int netId) : INetSerializable
    {
        public int NetId = netId;
        public bool HasCompassState;
        public bool CompassState;
        public bool ExamineWeapon;
        public bool HasAim;
        public bool AimState;

        public void Deserialize(NetDataReader reader)
        {
            NetId = reader.GetInt();
            HasCompassState = reader.GetBool();
            if (HasCompassState)
            {
                CompassState = reader.GetBool();
            }
            ExamineWeapon = reader.GetBool();
            HasAim = reader.GetBool();
            if (HasAim)
            {
                AimState = reader.GetBool();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetId);
            writer.Put(HasCompassState);
            if (HasCompassState)
            {
                writer.Put(CompassState);
            }
            writer.Put(ExamineWeapon);
            writer.Put(HasAim);
            if (HasAim)
            {
                writer.Put(AimState);
            }
        }
    }

    public class CoopClientCustomUsableItemController : CustomUsableItemController
    {
        protected FikaPlayer player;

        public static CoopClientCustomUsableItemController Create(FikaPlayer player, Item item)
        {
            CoopClientCustomUsableItemController controller = CreateController<CoopClientCustomUsableItemController>(player, item);
            controller.player = player;
            return controller;
        }

        public override void CompassStateHandler(bool isActive)
        {
            base.CompassStateHandler(isActive);
            player.CommonPacket.Type = ECommonSubPacketType.UsableItem;
            player.CommonPacket.SubPacket = UsableItemPacket.FromValue(true, isActive, false, false, false);
            player.PacketSender.NetworkManager.SendNetReusable(ref player.CommonPacket, DeliveryMethod.ReliableOrdered, true);
        }

        public override bool ExamineWeapon()
        {
            bool flag = base.ExamineWeapon();
            if (flag)
            {
                player.CommonPacket.Type = ECommonSubPacketType.UsableItem;
                player.CommonPacket.SubPacket = UsableItemPacket.FromValue(false, false, true, false, false);
                player.PacketSender.NetworkManager.SendNetReusable(ref player.CommonPacket, DeliveryMethod.ReliableOrdered, true);
            }
            return flag;
        }

        public override void SetAim(bool value)
        {
            bool isAiming = IsAiming;
            base.SetAim(value);

            if (IsAiming != isAiming)
            {
                player.CommonPacket.Type = ECommonSubPacketType.UsableItem;
                player.CommonPacket.SubPacket = UsableItemPacket.FromValue(false, false, false, true, isAiming);
                player.PacketSender.NetworkManager.SendNetReusable(ref player.CommonPacket, DeliveryMethod.ReliableOrdered, true);
            }
        }
    }
}
