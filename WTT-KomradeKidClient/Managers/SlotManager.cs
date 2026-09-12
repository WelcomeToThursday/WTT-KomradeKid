#if !UNITY_EDITOR
using System;
using EFT;
using EFT.InventoryLogic;
using GameBoyEmulator.CustomEFTData;
using GameBoyEmulator.Utils;
using UnityEngine;

namespace GameBoyEmulator.Managers
{
    public class SlotManager : MonoBehaviour, IItemRelatedView, IAddHandler, IRemoveHandler
    {
        private Slot _cartridgeSlot;
        private Slot _accessorySlot;
        private CustomUsableItem _gameboy;
        public bool isRegistered;

        private GameBoyModItemManager _gameBoyModItemManager;
        private CustomUsableItemController _customUsableItemController;
        private Player _player;

        private CustomUsableItemController _boundController;
        private CustomUsableItem _boundItem;

        public void Init(CustomUsableItemController controller)
        {
            _player = KomradeClient.Player;
            if (!_player)
            {
                Deinit();
                Console.WriteLine("[GameBoy] SlotManager Init aborted: player is null.");
                return;
            }

            if (controller == null)
            {
                Deinit();
                Console.WriteLine("[GameBoy] SlotManager Init aborted: controller is null.");
                return;
            }

            var newItem = controller.Item as CustomUsableItem;
            if (newItem == null)
            {
                Deinit();
                Console.WriteLine("[GameBoy] SlotManager Init aborted: controller item is null.");
                return;
            }

            bool controllerChanged = !ReferenceEquals(_boundController, controller);
            bool itemChanged = !ReferenceEquals(_boundItem, newItem);

            if (isRegistered && !controllerChanged && !itemChanged)
            {
                Console.WriteLine("[GameBoy] SlotManager already bound to this exact controller/item, skipping.");
                return;
            }

            if (isRegistered)
            {
                Deinit();
            }

            _gameBoyModItemManager = gameObject.GetComponent<GameBoyModItemManager>();
            if (_gameBoyModItemManager == null)
            {
                Deinit();
                Console.WriteLine("[GameBoy] SlotManager Init aborted: GameBoyModItemManager missing.");
                return;
            }

            _customUsableItemController = controller;
            _gameboy = newItem;

            _cartridgeSlot = _gameboy.GetCartridgeSlot();
            _accessorySlot = _gameboy.GetAccessorySlot();

            if (_cartridgeSlot == null || _accessorySlot == null)
            {
                Deinit();
                Console.WriteLine("[GameBoy] SlotManager Init aborted: cartridge/accessory slots are null.");
                return;
            }

            var owner = _player.InventoryController as IItemOwner;
            if (owner == null)
            {
                Deinit();
                Console.WriteLine("[GameBoy] SlotManager Init aborted: InventoryController is not IItemOwner.");
                return;
            }

            owner.RegisterView(this);

            _boundController = controller;
            _boundItem = newItem;
            isRegistered = true;

            Console.WriteLine("[GameBoy] SlotManager rebound and registered to InventoryController.");
        }

        public void Deinit()
        {
            if (!isRegistered)
                return;

            try
            {
                var owner = _player?.InventoryController as IItemOwner;
                owner?.UnregisterView(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameBoy] Deinit owner lookup failed: {ex.Message}");
            }

            _customUsableItemController = null;
            _boundController = null;
            _boundItem = null;
            _gameboy = null;
            _cartridgeSlot = null;
            _accessorySlot = null;
            isRegistered = false;
        }

        public void OnItemRemoved(RemoveItemEventArgs obj)
        {
            if (obj.Status == CommandStatus.Succeed && obj.From is SlotItemAddress)
            {
                OnItemAddedOrRemoved((Slot)obj.From.Container, false);
            }
        }

        public void OnItemAdded(AddItemEventArgs eventArgs)
        {
            if (eventArgs.Status == CommandStatus.Succeed && eventArgs.To is SlotItemAddress)
            {
                OnItemAddedOrRemoved((Slot)eventArgs.To.Container, true);
            }
        }

        public void OnItemAddedOrRemoved(Slot slot, bool isAdded)
        {
            if (slot == null)
                return;

            if (slot == _cartridgeSlot)
            {
                if (isAdded)
                    LoadCartridge();
                else
                    UnloadCartridge();
                return;
            }

            if (slot == _accessorySlot)
            {
                if (isAdded)
                    ApplyAccessory();
                else
                    RemoveAccessory();
            }
        }

        private void LoadCartridge()
        {
            var cartridge = _cartridgeSlot?.ContainedItem as GameBoyCartridge;
            if (cartridge == null || !_player)
                return;

            bool isUsingGameBoy = _player.HandsController.Item is CustomUsableItem;
            if (!isUsingGameBoy)
                return;

            bool inventoryOpened = _player.HandsController.IsInventoryOpen();
            _gameBoyModItemManager.OnCartridgeAppeared(_cartridgeSlot, cartridge, !inventoryOpened);
        }

        private void UnloadCartridge()
        {
            if (_player == null)
                return;

            bool isUsingGameBoy = _player.HandsController.Item is CustomUsableItem;
            if (!isUsingGameBoy)
                return;

            bool inventoryOpened = _player.HandsController.IsInventoryOpen();
            _gameBoyModItemManager.RemoveCartridge(!inventoryOpened);
        }

        private void ApplyAccessory()
        {
            var accessory = _accessorySlot?.ContainedItem as GameBoyAccessory;
            if (accessory == null || !_player)
                return;

            bool isUsingGameBoy = _player.HandsController.Item is CustomUsableItem;
            if (!isUsingGameBoy)
                return;

            _gameBoyModItemManager.OnAccessoryAppeared(_accessorySlot, accessory);
        }

        private void RemoveAccessory()
        {
            if (_player == null)
                return;

            bool isUsingGameBoy = _player.HandsController.Item is CustomUsableItem;
            if (!isUsingGameBoy)
                return;

            _gameBoyModItemManager.RemoveAccessory();
        }
    }
}
#endif