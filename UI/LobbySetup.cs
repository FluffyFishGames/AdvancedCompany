using AdvancedCompany.Config;
using AdvancedCompany.Game;
using AdvancedCompany.Network;
using AdvancedCompany.Patches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedCompany.UI
{
    [LoadAssets]
    [HarmonyPatch]
    public class LobbySetup
    {
        private static GameObject LobbyScreenPrefab;
        private static MenuManager MenuManagerInstance;
        private static GameObject LobbyScreen;
        private static LobbySettings Settings;

        public static void LoadAssets(AssetBundle assets)
        {
            LobbyScreenPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/LobbyScreen.prefab");
        }

        [HarmonyPatch(typeof(MenuManager), "Start")]
        [HarmonyPrefix]
        public static void Initialize(MenuManager __instance)
        {
            var canvas = __instance.menuButtons.GetComponentInParent<Canvas>();

            LobbyScreen = GameObject.Instantiate(LobbyScreenPrefab, canvas.transform);
            LobbyScreen.transform.localScale = new Vector3(1f, 1f, 1f);
            Settings = LobbyScreen.GetComponent<LobbySettings>();
            LobbyScreen.SetActive(false);
            Settings.OnContinue += (configuration, presetName) =>
            {
                ServerConfiguration.Instance.CopyFrom(configuration);
                ServerConfiguration.Instance.WasLoaded();
                try
                {
                    ES3.Save<string>("preset", presetName, global::GameNetworkManager.Instance.currentSaveFileName);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while saving preset in game save file. Save file most likely corrupted?");
                    Plugin.Log.LogError(e);
                }
                StartHost();
            };
            Settings.OnCancel += () =>
            {
                if (!__instance.menuButtons.gameObject.activeSelf)
                    __instance.menuButtons.gameObject.SetActive(true);
            };
        }
        
        [HarmonyPatch(typeof(MenuManager), "ConfirmHostButton")]
        [HarmonyPrefix]
        public static bool StartHosting(MenuManager __instance)
        {
            if (string.IsNullOrEmpty(__instance.lobbyNameInputField.text))
            {
                __instance.tipTextHostSettings.text = "Enter a lobby name!";
                return false;
            }
            __instance.HostSettingsScreen.SetActive(value: false);
            if (__instance.lobbyNameInputField.text.Length > 40)
            {
                __instance.lobbyNameInputField.text = __instance.lobbyNameInputField.text.Substring(0, 40);
            }
            ES3.Save("HostSettings_Name", __instance.lobbyNameInputField.text, "LCGeneralSaveData");
            ES3.Save("HostSettings_Public", __instance.hostSettings_LobbyPublic, "LCGeneralSaveData");
            MenuManagerInstance = __instance;

            var preset = "";
            if (!ModpackConfig.Instance.SkipPresets.Value && ES3.KeyExists("preset", global::GameNetworkManager.Instance.currentSaveFileName))
                preset = ES3.Load<string>("preset", global::GameNetworkManager.Instance.currentSaveFileName);
            else
                preset = ModpackConfig.Instance.StandardPreset.Value;
            if (preset == "" || ModpackConfig.Instance.SkipPresets.Value)
                preset = ModpackConfig.Instance.StandardPreset.Value;
            LobbyConfigurationProvider.Instance = new LobbyConfigurationProvider();
            Settings.SetConfiguration(LobbyConfigurationProvider.Instance);
            if (preset != "")
                Settings.SelectPreset(preset);

            if (ModpackConfig.Instance.SkipPresets.Value)
            {
                ServerConfiguration.Instance.CopyFrom(Settings.SelectedPreset.Configuration);
                ServerConfiguration.Instance.WasLoaded();
                StartHost();
            }
            else
                LobbyScreen.SetActive(true);
            return false;
        }

        public static void StartHost()
        {
            LobbySizePatches.Handshake = null;
            LobbySizePatches.ConnectingClients = new();

            Game.Player.Reset();
            var localPlayer = Game.Player.GetPlayer(0);
            localPlayer.LoadLocal();
            localPlayer.LoadServer(global::GameNetworkManager.Instance.currentSaveFileName);
            localPlayer.EquipConfigurationCosmetics();

            ES3.Save("preset", Settings.SelectedPreset.Name);
            Network.Manager.Lobby = new Lobby()
            {
                CurrentShip = Ship.Load(global::GameNetworkManager.Instance.currentSaveFileName),
                ConnectedPlayers = new Dictionary<int, Player>() { { 0, localPlayer } }
            };

            GameNetworkManager.Instance.lobbyHostSettings = new HostSettings(MenuManagerInstance.lobbyNameInputField.text, MenuManagerInstance.hostSettings_LobbyPublic);
            GameNetworkManager.Instance.StartHost();
        }
    }
}
