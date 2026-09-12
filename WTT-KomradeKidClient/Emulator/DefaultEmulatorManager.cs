using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityGB;
using UnityEngine.UI;
using EFT.Communications;
using static ObjectInHandsAnimator;




#if !UNITY_EDITOR
using UnityEngine.Serialization;
using System.Threading.Tasks;
using EFT.UI;
using GameBoyEmulator.Utils;
using GameBoyEmulator.Managers;
using EFT;
using BepInEx.Configuration;
using EFT.InventoryLogic;
using GameBoyEmulator.CustomEFTData;
#endif

internal abstract class MenuSettings
{
#if !UNITY_EDITOR
    private static ConfigEntry<KeyboardShortcut> _upKey;
    private static ConfigEntry<KeyboardShortcut> _downKey;
    private static ConfigEntry<KeyboardShortcut> _leftKey;
    private static ConfigEntry<KeyboardShortcut> _rightKey;
    private static ConfigEntry<KeyboardShortcut> _aKey;
    private static ConfigEntry<KeyboardShortcut> _bKey;
    private static ConfigEntry<KeyboardShortcut> _startKey;
    private static ConfigEntry<KeyboardShortcut> _selectKey;
    private static ConfigEntry<KeyboardShortcut> _powerKey;
    private static ConfigEntry<KeyboardShortcut> _unloadCartridgeKey;
    private static ConfigEntry<KeyboardShortcut> _loadCartridgeKey;
    public static ConfigEntry<KeyboardShortcut> UpdateKeybindings;

    public static void Init(ConfigFile config)
    {
        _upKey = config.Bind(
            "Keybindings",
            "Up",
            new KeyboardShortcut(KeyCode.UpArrow),
            new ConfigDescription("Key to move Up", null,
                new ConfigurationManagerAttributes { Order = 12 })
        );

        _downKey = config.Bind(
            "Keybindings",
            "Down",
            new KeyboardShortcut(KeyCode.DownArrow),
            new ConfigDescription("Key to move Down", null,
                new ConfigurationManagerAttributes { Order = 11 })
        );

        _leftKey = config.Bind(
            "Keybindings",
            "Left",
            new KeyboardShortcut(KeyCode.LeftArrow),
            new ConfigDescription("Key to move Left", null,
                new ConfigurationManagerAttributes { Order = 10 })
        );

        _rightKey = config.Bind(
            "Keybindings",
            "Right",
            new KeyboardShortcut(KeyCode.RightArrow),
            new ConfigDescription("Key to move Right", null,
                new ConfigurationManagerAttributes { Order = 9 })
        );

        _aKey = config.Bind(
            "Keybindings",
            "A Button",
            new KeyboardShortcut(KeyCode.Comma),
            new ConfigDescription("Key for A button", null,
                new ConfigurationManagerAttributes { Order = 8 })
        );

        _bKey = config.Bind(
            "Keybindings",
            "B Button",
            new KeyboardShortcut(KeyCode.Period),
            new ConfigDescription("Key for B button", null,
                new ConfigurationManagerAttributes { Order = 7 })
        );

        _startKey = config.Bind(
            "Keybindings",
            "Start",
            new KeyboardShortcut(KeyCode.Quote),
            new ConfigDescription("Key for Start button", null,
                new ConfigurationManagerAttributes { Order = 6 })
        );

        _selectKey = config.Bind(
            "Keybindings",
            "Select",
            new KeyboardShortcut(KeyCode.Colon),
            new ConfigDescription("Key for Select button", null,
                new ConfigurationManagerAttributes { Order = 5 })
        );

        _powerKey = config.Bind(
            "Keybindings",
            "Power",
            new KeyboardShortcut(KeyCode.P),
            new ConfigDescription("Key to toggle power on/off", null,
                new ConfigurationManagerAttributes { Order = 4 })
        );

        _unloadCartridgeKey = config.Bind(
            "Keybindings",
            "Unload Cartridge",
            new KeyboardShortcut(KeyCode.U),
            new ConfigDescription("Key to unload the cartridge", null,
                new ConfigurationManagerAttributes { Order = 3 })
        );

        _loadCartridgeKey = config.Bind(
            "Keybindings",
            "Load Cartridge",
            new KeyboardShortcut(KeyCode.K),
            new ConfigDescription("Key to load a new cartridge", null,
                new ConfigurationManagerAttributes { Order = 2 })
        );

        UpdateKeybindings = config.Bind(
            "Keybindings",
            "Update Keybindings",
            new KeyboardShortcut(KeyCode.Backslash),
            new ConfigDescription("Key to refresh any keybinding changes to current GameBoy", null,
                new ConfigurationManagerAttributes { Order = 1 })
        );
    }
#endif
#if UNITY_EDITOR
    public static Dictionary<KeyCode, EmulatorBase.Button> KeyMapping;
    public static void UnityKeymappings()
    {
        KeyMapping = new Dictionary<KeyCode, EmulatorBase.Button>();
        KeyMapping.Add(KeyCode.UpArrow, EmulatorBase.Button.Up);
        KeyMapping.Add(KeyCode.DownArrow, EmulatorBase.Button.Down);
        KeyMapping.Add(KeyCode.LeftArrow, EmulatorBase.Button.Left);
        KeyMapping.Add(KeyCode.RightArrow, EmulatorBase.Button.Right);
        KeyMapping.Add(KeyCode.Z, EmulatorBase.Button.A);
        KeyMapping.Add(KeyCode.X, EmulatorBase.Button.B);
        KeyMapping.Add(KeyCode.Space, EmulatorBase.Button.Start);
        KeyMapping.Add(KeyCode.LeftShift, EmulatorBase.Button.Select);
    }
#endif
#if !UNITY_EDITOR
    public static Dictionary<KeyboardShortcut, EmulatorBase.Button> GetButtonMappings()
    {
        return new Dictionary<KeyboardShortcut, EmulatorBase.Button>
        {
            { _aKey.Value, EmulatorBase.Button.A },
            { _bKey.Value, EmulatorBase.Button.B },
            { _startKey.Value, EmulatorBase.Button.Start },
            { _selectKey.Value, EmulatorBase.Button.Select }
        };
    }

    public static Dictionary<KeyboardShortcut, EmulatorBase.Button> GetDPadMappings()
    {
        return new Dictionary<KeyboardShortcut, EmulatorBase.Button>
        {
            { _upKey.Value, EmulatorBase.Button.Up },
            { _downKey.Value, EmulatorBase.Button.Down },
            { _leftKey.Value, EmulatorBase.Button.Left },
            { _rightKey.Value, EmulatorBase.Button.Right },
        };
    }

    public static Dictionary<KeyboardShortcut, string> GetAnimatorKeyBindings()
    {
        return new Dictionary<KeyboardShortcut, string>
        {
            { _upKey.Value, "DPadUpPress" },
            { _downKey.Value, "DPadDownPress" },
            { _leftKey.Value, "DPadLeftPress" },
            { _rightKey.Value, "DPadRightPress" },
            { _aKey.Value, "APress" },
            { _bKey.Value, "BPress" },
            { _startKey.Value, "StartPress" },
            { _selectKey.Value, "SelectPress" }
        };
    }

    public static Dictionary<KeyboardShortcut, string> GetPowerKeyBindings()
    {
        return new Dictionary<KeyboardShortcut, string>
        {
            { _powerKey.Value, "PowerOn" }
        };
    }

    public static Dictionary<KeyboardShortcut, string> GetCartridgeKeyBindings()
    {
        return new Dictionary<KeyboardShortcut, string>
        {
            { _unloadCartridgeKey.Value, "UnloadCartridge" },
            { _loadCartridgeKey.Value, "LoadCartridge" }
        };
    }
#endif
}

public class DefaultEmulatorManager : MonoBehaviour
{
    private ISaveMemory _saveMemory;

    public RawImage screenUI;
    public GameObject batteryIndicator;
    public AudioClip blowAirSfx;

    public DefaultAudioOutput defaultAudioOutput;
    public AudioSource emulatorAudioSource;
    private CustomUsableItemController _boundController;
    private CustomUsableItem _boundItem;
    public Emulator Emulator { get; set; }
    private bool _emulatorInitialized;
    public bool emulatorOn;

#if UNITY_EDITOR
        public string filename;
#endif
#if !UNITY_EDITOR
    private Dictionary<KeyboardShortcut, EmulatorBase.Button> _buttonMapping;
    private Dictionary<KeyboardShortcut, EmulatorBase.Button> _dPadMapping;
    private Dictionary<KeyboardShortcut, string> _animatorKeyBindings;
    private Dictionary<KeyboardShortcut, string> _powerKeyBindings;
    private Dictionary<KeyboardShortcut, string> _cartridgeKeyBindings;
    private CustomUsableItemController _controller;

    private CompoundItem[] _compoundItem;
    private InventoryController _inventoryControllerClass;
    private ItemUiContext _itemUiContext;

    private GameBoyModItemManager _modItemManager;
    AnimationManager _animationManager;

    WeaponPrefab _weaponPrefab;
    GameObject _weaponGameObject;
    GameObject _gameboyEmulatorGameObject;
    SlotManager _slotManager;
    private Player _player;

    private IAnimator _animator;
    private GameObject _gameObject;

#endif
    void Start()
    {
#if UNITY_EDITOR
        MenuSettings.UnityKeymappings();
#endif
        //Init();
    }

    private void OnDisable()
    {
        Emulator?.Save();
        ResetSessionState();
    }

    private void OnDestroy()
    {
        ResetSessionState();
    }

    private void ResetSessionState()
    {
        StopAllCoroutines();

        emulatorOn = false;
        _emulatorInitialized = false;

        try
        {
            Emulator?.Stop();
        }
        catch
        {
        }

        if (batteryIndicator)
            batteryIndicator.SetActive(false);

        if (screenUI)
        {
            screenUI.texture = null;
            Color c = screenUI.color;
            screenUI.color = new Color(c.r, c.g, c.b, 0f);
        }

        _slotManager?.Deinit();

        _controller = null;
        _boundController = null;
        _boundItem = null;
        _weaponPrefab = null;
        _weaponGameObject = null;
        _inventoryControllerClass = null;
        _itemUiContext = null;
        _compoundItem = null;
        _animator = null;
    }
#if !UNITY_EDITOR
    private void InitializeKeyBindings()
    {
        _buttonMapping = MenuSettings.GetButtonMappings();
        _dPadMapping = MenuSettings.GetDPadMappings();
        _animatorKeyBindings = MenuSettings.GetAnimatorKeyBindings();
        _powerKeyBindings = MenuSettings.GetPowerKeyBindings();
        _cartridgeKeyBindings = MenuSettings.GetCartridgeKeyBindings();
    }
#endif
#if UNITY_EDITOR
        private void Init()
        {
            TurnOnEmulator();
        }
#endif

#if !UNITY_EDITOR
    private bool IsAnimatorPlaying()
    {
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(1);

        return stateInfo is { normalizedTime: < 1, length: > 0 };
    }

    /// <summary>
    /// KeyboardShortcut default behavior is awful and doesn't allow other buttons to be pressed during
    /// </summary>
    private static bool BetterIsDown(KeyboardShortcut key)
    {
        if (!Input.GetKeyDown(key.MainKey))
        {
            return false;
        }

        foreach (var modifier in key.Modifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// KeyboardShortcut default behavior is awful and doesn't allow other buttons to be pressed during
    /// </summary>
    private static bool BetterIsPressed(KeyboardShortcut key)
    {
        if (!Input.GetKey(key.MainKey))
        {
            return false;
        }

        foreach (var modifier in key.Modifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }

        return true;
    }
#endif
    private void Update()
    {
#if UNITY_EDITOR
            UnityUpdate();
#endif
        if (emulatorOn)
        {
            Emulator.RunNextStep();
        }
#if !UNITY_EDITOR
        if (MenuSettings.UpdateKeybindings.Value.IsDown())
        {
            InitializeKeyBindings();
            ConsoleScreen.Log("Keybindings updated.");
        }

        Player player = GameBoyEmulator.KomradeClient.Player;
        if (!player)
        {
            return;
        }

        var currentItemInHands = player.HandsController?.HandsHierarchy?.gameObject;
        if (!_weaponGameObject || _weaponGameObject != currentItemInHands || player.IsInventoryOpened ||
            MonoBehaviourSingleton<PreloaderUI>.Instance.Console.IsConsoleVisible) return;
        if (!IsAnimatorPlaying())
        {
            HandlePowerInput();
            HandleRomLoadingInput();

            if (emulatorOn)
            {
                HandleDPadInputs();
                HandleButtonInputs();
                HandleAnimatorInputs();
            }
        }
#endif
    }
#if !UNITY_EDITOR
    private void HandlePowerInput()
    {
        foreach (var keyBinding in _powerKeyBindings)
        {
            if (BetterIsDown(keyBinding.Key))
            {
                switch (keyBinding.Value)
                {
                    case "PowerOn":
                        if (emulatorOn)
                        {
                            TurnOffEmulator();
                        }
                        else
#if !UNITY_EDITOR
                        if (IsCartridgeLoaded())
#endif
                        {
                            TurnOnEmulator();
                        }
                        else
                        {
                            NotificationManager.DisplaySingletonWarningNotification(
                                "Cannot turn on the GameBoy without a loaded ROM.".Localized());
                        }

                        break;
                }
            }
        }
    }

    private async void HandleRomLoadingInput()
    {
        foreach (var keyBinding in _cartridgeKeyBindings)
        {
            if (BetterIsDown(keyBinding.Key))
            {
                if (!emulatorOn)
                {
                    switch (keyBinding.Value)
                    {
                        case "LoadCartridge":
                            await LoadCartridgeAsync();
                            break;
                        case "UnloadCartridge":
                            await UnloadRomAsync();
                            break;
                    }
                }
            }
        }
    }

    private void HandleDPadInputs()
    {
        foreach (var keyMapping in _dPadMapping)
        {
            Emulator.SetInput(keyMapping.Value, BetterIsPressed(keyMapping.Key));
        }
    }

    private void HandleButtonInputs()
    {
        foreach (var keyMapping in _buttonMapping)
        {
            Emulator.SetInput(keyMapping.Value, BetterIsPressed(keyMapping.Key));
        }
    }

    private void HandleAnimatorInputs()
    {
        foreach (var keyBinding in _animatorKeyBindings)
        {
            _animator.SetBool(keyBinding.Value, BetterIsPressed(keyBinding.Key));
        }
    }
#endif
#if UNITY_EDITOR
        private void UnityUpdate()
        {
            foreach (KeyValuePair<KeyCode, EmulatorBase.Button> entry in MenuSettings.KeyMapping)
            {
                if (Input.GetKeyDown(entry.Key))
                    Emulator.SetInput(entry.Value, true);
                else if (Input.GetKeyUp(entry.Key))
                    Emulator.SetInput(entry.Value, false);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                byte[] screenshot = ((DefaultVideoOutput)Emulator.Video).Texture.EncodeToPNG();
                File.WriteAllBytes("./screenshot.png", screenshot);
                Debug.Log("Screenshot saved.");
            }
        }
#endif
    private void InitializeEmulator()
    {
        if (_emulatorInitialized)
        {
            return;
        }

        // Validate required components
        if (!screenUI)
        {
#if DEBUG
                Console.WriteLine("ERROR: screenUI is null. Cannot initialize emulator.");
#endif
            return;
        }

        if (!defaultAudioOutput)
        {
#if DEBUG
                Console.WriteLine("ERROR: defaultAudioOutput is null. Cannot initialize emulator.");
#endif
            return;
        }

        IVideoOutput drawable = new DefaultVideoOutput();
        IAudioOutput audio = defaultAudioOutput;

        if (_saveMemory == null)
        {
            ISaveMemory saveMemory = new DefaultSaveMemory();
            _saveMemory = saveMemory;
        }

        Emulator = new Emulator(drawable, audio, _saveMemory);
        screenUI.texture = ((DefaultVideoOutput)Emulator.Video).Texture;

        _emulatorInitialized = true;

#if DEBUG
            Console.WriteLine("Emulator initialized successfully.");
#endif
    }

#if !UNITY_EDITOR
    public void Init(CustomUsableItemController controller)
    {
        _player = GameBoyEmulator.KomradeClient.Player;
        if (!_player || controller == null)
            return;

        var newItem = controller.Item as CustomUsableItem;

        bool controllerChanged = !ReferenceEquals(_boundController, controller);
        bool itemChanged = !ReferenceEquals(_boundItem, newItem);

        if (controllerChanged || itemChanged)
        {
            ResetSessionState();
        }

        _controller = controller;
        _boundController = controller;
        _boundItem = newItem;

        _inventoryControllerClass = _player.InventoryController;
        if (_inventoryControllerClass == null)
            return;

        GameObject controllerObject = _controller.ControllerGameObject;
        if (controllerObject == null)
            return;

        _weaponPrefab = controllerObject.GetComponent<WeaponPrefab>();
        if (_weaponPrefab == null)
            return;

        _weaponGameObject = _weaponPrefab._objectInstance;
        if (_weaponGameObject == null)
            return;

        _itemUiContext = ItemUiContext.Instance;
        if (_itemUiContext == null)
            return;

        _compoundItem = _itemUiContext.PlayerStash;
        if (_compoundItem == null)
            return;

        if (_controller.FirearmsAnimator == null)
            return;

        _animator = _controller.FirearmsAnimator.Animator;
        if (_animator == null)
            return;

        _gameboyEmulatorGameObject = CommonUtils.GetGameBoyEmulatorObject(_controller);
        if (_gameboyEmulatorGameObject == null)
            return;

        InitializeEmulator();
        InitializeModItemManager();
        InitializeAnimationManager();
        InitializeSlotManager(_controller);
        InitializeKeyBindings();
    }
    private void InitializeAnimationManager()
    {
#if DEBUG
        Console.WriteLine($"GameBoyEmulatorGameObject is null: {!_gameboyEmulatorGameObject}");
#endif

        _animationManager = _gameboyEmulatorGameObject.GetComponent<AnimationManager>();

#if DEBUG
        Console.WriteLine($"AnimationManager component found: {_animationManager}");
#endif

        if (!_animationManager)
        {
            _animationManager = _gameboyEmulatorGameObject.AddComponent<AnimationManager>();

#if DEBUG
            Console.WriteLine("AnimationManager component added to GameBoyEmulatorGameObject.");
#endif
        }

        _animationManager.Init(_animator, this);

#if DEBUG
        Console.WriteLine("AnimationManager initialized.");
#endif
    }

    private void InitializeModItemManager()
    {
#if DEBUG
        Console.WriteLine($"GameBoyEmulatorGameObject is null: {!_gameboyEmulatorGameObject}");
#endif

        _modItemManager = _gameboyEmulatorGameObject.GetComponent<GameBoyModItemManager>();

#if DEBUG
        Console.WriteLine($"GameBoyModItemManager component found: {_modItemManager}");
#endif

        if (!_modItemManager)
        {
            _modItemManager = _gameboyEmulatorGameObject.AddComponent<GameBoyModItemManager>();

#if DEBUG
            Console.WriteLine("GameBoyModItemManager component added to GameBoyEmulatorGameObject.");
#endif
        }
    }

    private void InitializeSlotManager(CustomUsableItemController controller)
    {
        _slotManager = _gameboyEmulatorGameObject?.GetComponent<SlotManager>();

        if (!_slotManager)
        {
            _slotManager = _gameboyEmulatorGameObject?.AddComponent<SlotManager>();
            Console.WriteLine("[GameBoy] SlotManager component added to GameBoyEmulatorGameObject.");
        }

        _slotManager.Init(controller);

        Console.WriteLine($"[GameBoy] SlotManager Init complete. Registered: {_slotManager.isRegistered}");
    }

    private GameBoyCartridge GetCurrentCartridge()
    {
        CustomUsableItem item = (CustomUsableItem)_controller.GetItem();

        return item?.GetCurrentCartridge();
    }

    private GameBoyAccessory GetCurrentAccessory()
    {
        CustomUsableItem item = (CustomUsableItem)_controller.GetItem();

        return item?.GetCurrentAccessory();
    }

    private bool IsCartridgeLoaded()
    {
        return GetCurrentCartridge() != null;
    }

    private async Task LoadCartridgeAsync()
    {
#if DEBUG
        Console.WriteLine($"Emulator initialized: {_emulatorInitialized}");
#endif

        if (!_emulatorInitialized)
        {
            InitializeEmulator();
#if DEBUG
            Console.WriteLine("Emulator was not initialized. Initializing emulator...");
#endif
        }

        CustomUsableItem item = _controller.GetItem() as CustomUsableItem;

#if DEBUG
        Console.WriteLine($"CustomUsableItem retrieved from controller: {item != null}");
#endif

        if (item == null)
        {
#if DEBUG
            Console.WriteLine("CustomUsableItem is null. Returning...");
#endif
            return;
        }

        GameBoyCartridge currentCartridge = item.GetCurrentCartridge();

#if DEBUG
        Console.WriteLine($"Current cartridge found: {currentCartridge != null}");
#endif

        if (currentCartridge != null)
        {
            NotificationManager.DisplaySingletonWarningNotification("Cartridge is already slotted into the item"
                .Localized());
#if DEBUG
            Console.WriteLine("A cartridge is already slotted. Displaying notification and returning...");
#endif
            return;
        }

#if DEBUG
        Console.WriteLine($"ModItemManager exists: {_modItemManager != null}");
#endif

        if (!_modItemManager)
        {
#if DEBUG
            Console.WriteLine("ModItemManager is null. Returning...");
#endif
            return;
        }

#if DEBUG
        Console.WriteLine($"CompoundItem exists: {_compoundItem != null}");
#endif

        if (_compoundItem == null)
        {
#if DEBUG
            Console.WriteLine("CompoundItem is null. Returning...");
#endif
            return;
        }

#if DEBUG
        Console.WriteLine($"InventoryControllerClass exists: {_inventoryControllerClass != null}");
#endif

        if (_inventoryControllerClass == null)
        {
#if DEBUG
            Console.WriteLine("InventoryControllerClass is null. Returning...");
#endif
            return;
        }

#if DEBUG
        Console.WriteLine($"ItemUiContext exists: {_itemUiContext != null}");
#endif

        if (!_itemUiContext)
        {
#if DEBUG
            Console.WriteLine("ItemUiContext is null. Returning...");
#endif
            return;
        }

#if DEBUG
        Console.WriteLine($"Animator exists: {_animator != null}");
#endif

        if (_animator == null)
        {
#if DEBUG
            Console.WriteLine("Animator is null. Returning...");
#endif
            return;
        }

#if DEBUG
        Console.WriteLine("All checks passed. Attempting to load cartridge asynchronously...");
#endif

        await _modItemManager.LoadCartridgeAsync(item, _compoundItem, _inventoryControllerClass, _itemUiContext,
            _animationManager);

#if DEBUG
        Console.WriteLine("Cartridge loaded successfully.");
#endif
    }

    private async Task UnloadRomAsync()
    {
        if (IsCartridgeLoaded())
        {
            if (_controller.Item is CustomUsableItem item && item.GetCurrentCartridge() != null)
            {
                await _modItemManager.UnloadCartridgeAsync(item, _compoundItem, _inventoryControllerClass,
                    _itemUiContext, _animationManager);
            }
        }
        else
        {
            NotificationManager.DisplaySingletonWarningNotification("No cartridge present in the slot to unload"
                .Localized());
        }
    }
#endif
    public void TurnOnEmulator()
    {
        if (!_emulatorInitialized)
        {
            InitializeEmulator();
        }
#if !UNITY_EDITOR
        CustomUsableItem item = _controller.GetItem() as CustomUsableItem;
        if (item == null)
        {
            return;
        }

        GameBoyCartridge currentCartridge = GetCurrentCartridge();
        if (currentCartridge == null)
        {
            NotificationManager.DisplayWarningNotification("Cannot turn on the GameBoy without a cartridge loaded."
                .Localized());
            return;
        }

        string filename = currentCartridge.RomName;

        if (string.IsNullOrEmpty(filename))
        {
            NotificationManager.DisplayWarningNotification("No RomName defined for the current cartridge.."
                .Localized());
            return;
        }
#endif

        string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string path = Path.Combine(pluginPath ?? throw new InvalidOperationException(), "Roms", filename);

        if (!File.Exists(path))
        {
#if !UNITY_EDITOR
            NotificationManager.DisplayWarningNotification(
                "No Rom found for the current Cartridge".Localized());
#endif
            return;
        }

        byte[] data = File.ReadAllBytes(path);

        Emulator.LoadRom(data);

#if !UNITY_EDITOR
        _animationManager.PlayAnimation("GameboyPower", 4);
#endif
        StartCoroutine(WaitAndRun());
    }

    private IEnumerator WaitAndRun()
    {
        yield return new WaitForSeconds(1.5f);
        Color currentColor = screenUI.color;
        screenUI.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.09f);
        screenUI.texture = ((DefaultVideoOutput)Emulator.Video).Texture;
        batteryIndicator.SetActive(true);
#if !UNITY_EDITOR
        GameBoyAccessory accessory = GetCurrentAccessory();
        if (accessory is { AccessoryType: "Light" })
        {
            ToggleLightAndCover(true);
        }
#endif
        Emulator.Start();
        emulatorOn = true;
    }

    public void TurnOffEmulator()
    {
        if (!emulatorOn)
        {
            return;
        }

        emulatorOn = false;

#if !UNITY_EDITOR
        _animationManager.PlayAnimation("GameboyPower", 4);
#endif
        _emulatorInitialized = false;
        StartCoroutine(FadeOutAndTurnOff());
    }

    private IEnumerator FadeOutAndTurnOff()
    {
        yield return new WaitForSeconds(.95f);
        batteryIndicator.SetActive(false);
        Emulator.Stop();
#if !UNITY_EDITOR
        GameBoyAccessory accessory = GetCurrentAccessory();
        if (accessory is { AccessoryType: "Light" })
        {
            ToggleLightAndCover(false);
        }
#endif
        float fadeDuration = .25f;
        Color originalColor = screenUI.color;

        for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0.0f, t / fadeDuration);
            screenUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        screenUI.texture = null;

        screenUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
#if !UNITY_EDITOR
    public void ToggleLightAndCover(bool enable)
    {
        Light lightComponent = _modItemManager.FindLightComponent();
        if (lightComponent != null)
        {
            lightComponent.enabled = enable;
        }

        var lightCoverOn = _modItemManager.FindLightCoverOnGameObject();
        var lightCoverOff = _modItemManager.FindLightCoverOffGameObject();
        if (lightCoverOn && lightCoverOff)
        {
            var lightCoverOnRenderer = lightCoverOn.GetComponent<Renderer>();
            var lightCoverOffRenderer = lightCoverOff.GetComponent<Renderer>();

            if (lightCoverOnRenderer)
                lightCoverOnRenderer.enabled = enable;

            if (lightCoverOffRenderer)
                lightCoverOffRenderer.enabled = !enable;
        }
    }
#endif
}