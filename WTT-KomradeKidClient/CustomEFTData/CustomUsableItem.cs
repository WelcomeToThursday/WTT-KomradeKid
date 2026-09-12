#if !UNITY_EDITOR
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WTTClientCommonLib.Attributes;

namespace GameBoyEmulator.CustomEFTData;

[CustomParent("66e42bd851fa456a1ee37885", // Template ID
            typeof(CustomUsableItem),   // Item type
            typeof(CompoundItemTemplate))] // Template type
public sealed class CustomUsableItem : CompoundItem
{
    private Slot _cartridgeSlotCache;

    private Slot _accessorySlotCache;

    public CustomUsableItem(string id, CompoundItemTemplate template) : base(id, template)
    {
        Slots = Array.ConvertAll(template.Slots, CG_Ctor);
    }
    [StructLayout(LayoutKind.Auto)]
    private struct CG_Apply
    {
        public ItemController itemController;
    }
    public override IEnumerable<EItemInfoButton> ItemInteractionButtons
    {
        get
        {
            return base.ItemInteractionButtons;
        }
    }

    public override OperationResult Apply([NotNull] ItemController itemController, [NotNull] Item item, int count, bool simulate)
    {
        CG_Apply @struct;
        @struct.itemController = itemController;
        if (!@struct.itemController.Examined(item))
        {
            return new ItemNotExaminedError(item);
        }
        if (!@struct.itemController.Examined(this))
        {
            return new ItemNotExaminedError(this);
        }
        Slot cartridgeSlot = GetCartridgeSlot();
        Slot accessorySlot = GetAccessorySlot();
        GameBoyCartridge cartridge;
        GameBoyAccessory accessory;
        Error error;
        if ((cartridge = (item as GameBoyCartridge)) != null && cartridgeSlot != null)
        {
            if (!cartridgeSlot.CanAccept(cartridge))
            {
                return new Slot.ItemFiltersWontAllowError(cartridge, cartridgeSlot);
            }
            ItemAddress itemAddress = cartridgeSlot.CreateItemAddress();
            IResult result = smethod_1(itemAddress, ref @struct);
            if (result.Failed)
            {
                return new StringError(result.Error);
            }
            OperationResult<MoveResult> value = ItemManipulator.Move(cartridge, itemAddress, @struct.itemController, simulate);
            if (value.Succeeded)
            {
                return value;
            }
            Item containedItem = cartridgeSlot.ContainedItem;
            if (!UnityUtils.DisabledForNow && containedItem != null && SlotManipulator.CanSwap(cartridge, cartridgeSlot))
            {
                return new OperationResult((Error)null);
            }
        }
        else
        {
            OperationResult result3 = base.Apply(@struct.itemController, item, count, simulate);
            if (result3.Succeeded)
            {
                return result3;
            }
            IResult result4 = smethod_1(cartridgeSlot?.CreateItemAddress(), ref @struct);
            if (result4.Failed)
            {
                return new StringError(result4.Error);
            }
        }
        if ((accessory = (item as GameBoyAccessory)) != null && accessorySlot != null)
        {
            if (!accessorySlot.CanAccept(accessory))
            {
                return new Slot.ItemFiltersWontAllowError(accessory, accessorySlot);
            }
            ItemAddress itemAddress = accessorySlot.CreateItemAddress();
            IResult result = smethod_1(itemAddress, ref @struct);
            if (result.Failed)
            {
                return new StringError(result.Error);
            }
            OperationResult<MoveResult> value = ItemManipulator.Move(accessory, itemAddress, @struct.itemController, simulate);
            if (value.Succeeded)
            {
                return value;
            }
            error = value.Error;
            Item containedItem = accessorySlot.ContainedItem;
            if (!UnityUtils.DisabledForNow && containedItem != null && SlotManipulator.CanSwap(accessory, accessorySlot))
            {
                return new OperationResult((Error)null);
            }
        }
        else
        {
            OperationResult result3 = base.Apply(@struct.itemController, item, count, simulate);
            if (result3.Succeeded)
            {
                return result3;
            }
            error = result3.Error;
            IResult result4 = smethod_1(accessorySlot?.CreateItemAddress(), ref @struct);
            if (result4.Failed)
            {
                return new StringError(result4.Error);
            }
        }
        return error;
    }


    private static IResult smethod_1(ItemAddress slotItemAddress, ref CG_Apply CG_Apply)
    {
        InventoryController inventoryControllerClass;
        if ((inventoryControllerClass = (CG_Apply.itemController as InventoryController)) != null && inventoryControllerClass.Inventory.Equipment.ContainerSlots.Contains(slotItemAddress.Container) && CG_Apply.itemController.SelectEvents(null).Any())
        {
            return new FailedResult("Inventory/PlayerIsBusy");
        }
        return SuccessfulResult.New;
    }

    private bool FindCartridgeSlot(Slot x)
    {
        return x.ID.Contains("Cartridge");
    }

    private bool FindAccessorySlot(Slot x)
    {
        return x.ID.Contains("Cable");
    }

    [CanBeNull]
    public Slot GetCartridgeSlot()
    {
        if (_cartridgeSlotCache != null)
        {
            return _cartridgeSlotCache;
        }


        Slot foundSlot = Slots.FirstOrDefault(FindCartridgeSlot);

        if (foundSlot != null)
        {
            _cartridgeSlotCache = foundSlot;
        }

        return foundSlot;
    }
    [CanBeNull]
    public Slot GetAccessorySlot()
    {
        if (_accessorySlotCache != null)
        {
            return _accessorySlotCache;
        }


        Slot foundSlot = Slots.FirstOrDefault(FindAccessorySlot);

        if (foundSlot != null)
        {
            _accessorySlotCache = foundSlot;
        }

        return foundSlot;
    }

    [CanBeNull]
    public GameBoyCartridge GetCurrentCartridge()
    {
        Slot cartridgeSlot = GetCartridgeSlot();

        if (cartridgeSlot is { ContainedItem: GameBoyCartridge cartridge })
        {
            return cartridge;
        }
        else
        {
            return null;
        }
    }
    [CanBeNull]
    public GameBoyAccessory GetCurrentAccessory()
    {
        Slot accessorySlot = GetAccessorySlot();

        if (accessorySlot is { ContainedItem: GameBoyAccessory accessory })
        {
            return accessory;
        }
        else
        {
            return null;
        }
    }
    public bool CanStartCartridgeReload()
    {

        GameBoyCartridge currentCartridge = GetCurrentCartridge();
        if (currentCartridge != null && !KomradeClient.Player.InventoryController.Examined(currentCartridge))
        {
            NotificationManager.DisplaySingletonWarningNotification("Attached cartridge is not examined.".Localized());
            return false;
        }
        return true;
    }
    public bool IsAccessorySuitable(GameBoyAccessory accessory)
    {
        if (accessory == null)
        {
            return false;
        }

        Slot[] suitableSlots = AllSlots.Where(FindAccessorySlot).ToArray();

        if (suitableSlots.Length == 0)
        {
            return false;
        }

        foreach (var slot in suitableSlots)
        {
            if (slot.CanAccept(accessory))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCartridgeSuitable(GameBoyCartridge cartridge)
    {
        if (cartridge == null)
        {
            return false;
        }

        Slot[] suitableSlots = AllSlots.Where(FindCartridgeSlot).ToArray();

        if (suitableSlots.Length == 0)
        {
            return false;
        }

        // Check if any slot can accept the cartridge
        foreach (var slot in suitableSlots)
        {
            if (slot.CanAccept(cartridge))
            {
                return true;
            }
        }

        return false;
    }

}
#endif