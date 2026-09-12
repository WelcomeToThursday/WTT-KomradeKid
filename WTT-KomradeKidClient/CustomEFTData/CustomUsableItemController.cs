#if !UNITY_EDITOR
using Comfort.Common;
using EFT;
using EFT.InventoryLogic.Operations;
using System;
using System.Collections.Generic;

namespace GameBoyEmulator.CustomEFTData;

public class CustomUsableItemController : Player.UsableItemController
{
    public override Dictionary<Type, Player.ItemHandsController.OperationFactoryDelegate>
        GetOperationFactoryDelegates()
    {
        return new Dictionary<Type, Player.ItemHandsController.OperationFactoryDelegate>
        {
            {
                typeof(CustomSpawnOperation),
                CreateSpawnOperation
            },
            {
                typeof(CustomIdlingOperation),
                CreateIdlingOperation
            },
            {
                typeof(CustomRemoveOperation),
                CreateRemoveOperation
            },
            {
                typeof(CustomDropBackpackOperation),
                CreateDropBackpackOperation
            }
        };
    }

    public override void InitializeController(Player player, WeaponPrefab weaponPrefab)
    {
        base.InitializeController(player, weaponPrefab);

        player.ProceduralWeaponAnimation.ManualSetVariables(2f, 0f, 0f, 0f);

        SetUpOverlaping();

        BaseSoundPlayer soundPlayer =
            _controllerObject.GetComponent<BaseSoundPlayer>();

        if (soundPlayer != null)
        {
            soundPlayer.Init(
                this,
                player.PlayerBones.WeaponRoot,
                player);
        }

        InitializeEmulator();
    }

    public override void StateChangedHandler(
        EPlayerState previousState,
        EPlayerState nextState)
    {
        // Intentionally empty only if Komrade Kid should remain aimed/held
        // through all player-state changes.
        //
        // The base implementation disables aiming in states where aiming
        // is forbidden, so use the following instead if that is desired:
        //
        // base.StateChangedHandler(previousState, nextState);
    }

    public override void InitiateSpawnOperation(Action callback)
    {
        InitiateOperation<CustomSpawnOperation>().Start(callback);
    }

    private void InitializeEmulator()
    {
        DefaultEmulatorManager manager =
            _controllerObject.GetComponentInChildren<DefaultEmulatorManager>();

        if (manager != null)
        {
            manager.Init(this);
        }
    }

    private Player.ObjectInHandsOperation CreateSpawnOperation()
    {
        return new CustomSpawnOperation(this);
    }

    private Player.ObjectInHandsOperation CreateIdlingOperation()
    {
        return new CustomIdlingOperation(this);
    }

    private Player.ObjectInHandsOperation CreateRemoveOperation()
    {
        return new CustomRemoveOperation(this);
    }

    private Player.ObjectInHandsOperation CreateDropBackpackOperation()
    {
        return new CustomDropBackpackOperation(this);
    }

    private sealed class CustomSpawnOperation
        : Player.UsableItemController.SpawnOperation
    {
        public CustomSpawnOperation(CustomUsableItemController controller)
            : base(controller)
        {
        }

        public override void WeaponAppeared()
        {
            SetIdlingOperation();
        }

        public override void SetIdlingOperation()
        {
            CustomIdlingOperation idling =
                Controller.InitiateOperation<CustomIdlingOperation>();

            idling.Start();

            _onWeaponAppear?.Invoke();

            if (_hideAction != null)
            {
                idling.HideWeapon(_hideAction, _fastDrop);
            }
        }

        public override void SetLeftStanceAnimOnStartOperation()
        {
            Player.MovementContext.LeftStanceController
                .DisableLeftStanceAnimFromHandsAction();
        }
    }

    private sealed class CustomIdlingOperation
        : Player.UsableItemController.Idling
    {
        public CustomIdlingOperation(CustomUsableItemController controller)
            : base(controller)
        {
        }

        public override void HideWeapon(Action onHidden, bool fastDrop)
        {
            State = Player.EOperationState.Finished;

            Controller.InitiateOperation<CustomRemoveOperation>()
                .Start(onHidden, fastDrop);
        }

        public override void InitiateDropBackpackOperation(
            IOneItemOperation oneItemOperation,
            Callback callback)
        {
            Controller.InitiateOperation<CustomDropBackpackOperation>()
                .Start(oneItemOperation.Item1, callback);
        }
    }

    private sealed class CustomRemoveOperation
        : Player.UsableItemController.Remove
    {
        public CustomRemoveOperation(CustomUsableItemController controller)
            : base(controller)
        {
        }
    }

    private sealed class CustomDropBackpackOperation
        : Player.UsableItemController.DropBackpackOperation
    {
        public CustomDropBackpackOperation(
            CustomUsableItemController controller)
            : base(controller)
        {
        }

        public override void InitiateIdlingOperation()
        {
            Controller.InitiateOperation<CustomIdlingOperation>().Start();
        }
    }
}
#endif