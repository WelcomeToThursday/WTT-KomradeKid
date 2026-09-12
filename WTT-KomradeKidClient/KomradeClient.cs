#if !UNITY_EDITOR
using BepInEx;
using BepInEx.Bootstrap;
using Comfort.Common;
using EFT;
using EFT.UI;
using GameBoyEmulator.CustomEFTData;
using GameBoyEmulator.Patches;
using GameBoyEmulator.Utils;
using SPT.Reflection.Patching;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using WTTClientCommonLib;
using WTTClientCommonLib.Helpers;
using WTTClientCommonLib.Services;


namespace GameBoyEmulator
{
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.wtt.commonlib")]
    [BepInPlugin(
    PluginConstants.Guid,
    PluginConstants.Name,
    PluginConstants.Version)]

    internal class KomradeClient : BaseUnityPlugin
    {
        private static CommandProcessor _commandProcessor;
        private static GameWorld _gameWorld;
        public static Player Player;
        private static GameUI _gameUI;
        public static bool FikaInstalled { get; private set; }
        public static readonly string PluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal void Awake()
        {
            //CustomTemplateIdToObjectService.AddNewTemplateIdToObjectMapping(NewTemplateIdToObjectMappingClass.CustomMappings);
            MenuSettings.Init(Config);
            new InstallModPatch().Enable();
#if DEBUG
            // these patches are when/if I want to enable Game Boy load/unload from the item itself

            //new IsActivePatch().Enable();
            //new IsInteractivePatch().Enable();
            //new LoadWeaponPatch().Enable();
            //new UnLoadWeaponPatch().Enable();

            // This one is for if I want to be able to spawn with the Game Boy in my hands
            
            //new ClientPlayerMethod147Patch().Enable();

#endif
            //new GameEndedPatch().Enable();
            new TranslateCommandHideoutPatch().Enable();
            new ClientUsableItemControllerPatch().Enable();
            new UsableAnimationsHandsControllerPatch().Enable();
            new HandsControllerGetWeaponAnimationTypePatch().Enable();
            new IsAtBindablePlacePatch().Enable();
            new IsAtReachablePlace().Enable();
            new PlayerGetWeaponAnimationTypePatch().Enable();
            new CreateItemAsyncPatch().Enable();
            new SetHandsUsableItemPatch().Enable();
            new TryProceedPatch().Enable();
            FikaInstalled = Chainloader.PluginInfos.ContainsKey("com.fika.core");
            if (FikaInstalled)
            {
                LoadFikaModule();
            }

        }
        internal void Start()
        {
            Init();
        }

        internal void Update()
        {
            if (Singleton<GameWorld>.Instantiated && (_gameWorld == null || _gameUI == null || Player == null))
            {
                _gameWorld = Singleton<GameWorld>.Instance;
                _gameUI = MonoBehaviourSingleton<GameUI>.Instance;
                Player = Singleton<GameWorld>.Instance.MainPlayer;
            }
        }

        public void Init()
        {
            if (_commandProcessor == null)
            {
                _commandProcessor = new CommandProcessor();
                _commandProcessor.RegisterCommandProcessor();
            }
        }
        private void LoadFikaModule()
        {
            try
            {
                string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string fikaAssemblyPath = Path.Combine(pluginDir, "WTT-KomradeKidClientFika.dll");
        
                if (!File.Exists(fikaAssemblyPath))
                {
                    LogHelper.LogError($"[WTT-KomradeKid] Fika module not found at: {fikaAssemblyPath}");
                    FikaInstalled = false;
                    return;
                }
        
                var fikaAssembly = Assembly.LoadFrom(fikaAssemblyPath);
                var fikaPatchType = fikaAssembly.GetType("KomradeKidClientFika.Patches.FikaProceedPatch");
        
                if (fikaPatchType != null)
                {
                    var fikaPatchInstance = Activator.CreateInstance(fikaPatchType);
            
                    var enableMethod = fikaPatchType.GetMethod("Enable", BindingFlags.Public | BindingFlags.Instance);
            
                    if (enableMethod != null)
                    {
                        enableMethod.Invoke(fikaPatchInstance, null);
                        LogHelper.LogInfo("[WTT-KomradeKid] Fika module loaded and FikaProceedPatch enabled");
                    }
                    else
                    {
                        LogHelper.LogError("[WTT-KomradeKid] Could not find Enable method on FikaProceedPatch");
                        FikaInstalled = false;
                    }
                }
                else
                {
                    LogHelper.LogError("[WTT-KomradeKid] Could not find FikaProceedPatch type in Fika assembly");
                    FikaInstalled = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"[WTT-KomradeKid] Failed to load Fika module: {ex}");
                FikaInstalled = false;
            }
        }
    }
}
#endif