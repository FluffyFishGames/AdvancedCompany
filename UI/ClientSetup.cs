using AdvancedCompany.Config;
using AdvancedCompany.Patches;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.UI
{
    [Boot.Bootable]
    [Boot.Requires(typeof(ClientConfiguration))]
    [Boot.Requires(typeof(ClientConfigurationProvider))]
    [LoadAssets]
    [HarmonyPatch]
    public class ClientSetup
    {
        private static GameObject ClientScreenPrefab;
        private static MenuManager MenuManagerInstance;
        private static GameObject ClientScreen;
        private static PlayerSettings Settings;
        private static string SelectedPreset = "";

        public static void Boot()
        {
            var selectedPathFile = System.IO.Path.GetFullPath(BepInEx.Paths.PluginPath + "/../presets/advancedcompany/selected");
            if (System.IO.File.Exists(selectedPathFile))
            {
                var preset = System.IO.File.ReadAllText(selectedPathFile);
                SelectedPreset = preset;
                var p = ClientConfigurationProvider.Instance.GetPreset(preset);
                if (p != null)
                    ClientConfiguration.Instance.CopyFrom(p.Configuration);
                else
                    ClientConfiguration.Instance.CopyFrom(ClientConfigurationProvider.Instance.GetPresets()[0].Configuration);
            }
            else
                ClientConfiguration.Instance.CopyFrom(ClientConfigurationProvider.Instance.GetPresets()[0].Configuration);
        }
        public static void LoadAssets(AssetBundle assets)
        {
            ClientScreenPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/ClientScreen.prefab");
        }

        [HarmonyPatch(typeof(QuickMenuManager), "CloseQuickMenu")]
        [HarmonyPostfix]
        public static void CloseQuickMenu(QuickMenuManager __instance)
        {
            Settings.Close();
        }

        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPostfix]
        public static void InitializeInGame(QuickMenuManager __instance)
        {
            ClientScreen = GameObject.Instantiate(ClientScreenPrefab, __instance.mainButtonsPanel.transform.parent);
            ClientScreen.transform.localScale = new Vector3(1f, 1f, 1f);
            
            Settings = ClientScreen.GetComponent<PlayerSettings>();
            ClientScreen.SetActive(false);
            Settings.OnContinue += (configuration, presetName) =>
            {
                ClientConfiguration.Instance.CopyFrom(configuration);
                Game.Player.GetPlayer(GameNetworkManager.Instance.localPlayerController).SetCosmetics(ClientConfiguration.Instance.Cosmetics.ActivatedCosmetics.ToArray());
                InventoryPatches.ArrangeHotbar(Perks.InventorySlots());
                SelectedPreset = presetName;
                try
                {
                    var selectedPathFile = System.IO.Path.GetFullPath(BepInEx.Paths.PluginPath + "/../presets/advancedcompany/selected");
                    System.IO.File.WriteAllText(selectedPathFile, presetName);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while saving selected preset:");
                    Plugin.Log.LogError(e);
                }
            };
            Settings.OnCancel += () =>
            {
                InventoryPatches.ArrangeHotbar(Perks.InventorySlots());
            };

            var t = __instance.mainButtonsPanel.transform; 
            for (var i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i); 
                if (c.name == "Resume" || c.name == "InvitePlayers" || c.name == "Settings")
                    c.transform.localPosition += new Vector3(0f, 97f, 0f);
                if (c.name == "Settings")
                {
                    var newButton = GameObject.Instantiate(c.gameObject, c.parent);
                    newButton.name = "AdvancedCompany settings";
                    newButton.transform.localPosition = c.transform.localPosition + new Vector3(0f, -97f, 0f);
                    var button = newButton.GetComponent<Button>();
                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                    {
                        ClientScreen.SetActive(true);
                        Settings.SetCosmeticsProvider(new PlayerSettings.TestCosmeticProvider());
                        Settings.SetConfiguration(ClientConfigurationProvider.Instance);
                        Settings.SelectPreset(SelectedPreset);
                    }));
                    button.GetComponentInChildren<TextMeshProUGUI>().text = "> AdvancedCompany";
                }
            } 
        }
        [HarmonyPatch(typeof(MenuManager), "Start")]
        [HarmonyPrefix]
        public static void Initialize(MenuManager __instance)
        {
            var canvas = __instance.menuButtons.GetComponentInParent<Canvas>();
            var mainButtons = canvas.transform.Find("MenuContainer").Find("MainButtons");

            if (mainButtons != null)
            {
                ClientScreen = GameObject.Instantiate(ClientScreenPrefab, canvas.transform);
                ClientScreen.transform.localScale = new Vector3(1f, 1f, 1f);
                Settings = ClientScreen.GetComponent<PlayerSettings>();
                ClientScreen.SetActive(false);
                Settings.OnContinue += (configuration, presetName) =>
                {
                    ClientConfiguration.Instance.CopyFrom(configuration); 
                    SelectedPreset = presetName;
                    try
                    {
                        var selectedPathFile = System.IO.Path.GetFullPath(BepInEx.Paths.PluginPath + "/../presets/advancedcompany/selected");
                        System.IO.File.WriteAllText(selectedPathFile, presetName);
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogError("Error while saving selected preset:");
                        Plugin.Log.LogError(e);
                    }
                };
                Settings.OnCancel += () =>
                {
                }; 

                for (var i = 0; i < mainButtons.childCount; i++)
                {
                    var c = mainButtons.GetChild(i);
                    if (c.name == "HostButton" || c.name == "JoinACrew" || c.name == "StartLAN" || c.name == "SettingsButton")
                        c.transform.localPosition += new Vector3(0f, 39f, 0f);
                    if (c.name == "SettingsButton")
                    {
                        var newButton = GameObject.Instantiate(c.gameObject, c.parent);
                        newButton.transform.localPosition = c.transform.localPosition + new Vector3(0f, -39f, 0f);
                        newButton.name = "AdvancedCompany settings";
                        newButton.GetComponentInChildren<TextMeshProUGUI>().text = "> AdvancedCompany settings";
                        var button = newButton.GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                        {
                            ClientScreen.SetActive(true);
                            Settings.SetCosmeticsProvider(new PlayerSettings.TestCosmeticProvider());
                            Settings.SetConfiguration(ClientConfigurationProvider.Instance);
                            Settings.SelectPreset(SelectedPreset);
                        }));
                    }
                }
            }
        }
    }
}