using AdvancedCompany.Config;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Patches;
using AdvancedCompany.Terminal.Applications;
using HarmonyLib;
using JetBrains.Annotations;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace AdvancedCompany.Game
{
    [Boot.Bootable]
    [Boot.Requires(typeof(ClientConfiguration))]
    [LoadAssets]
    [HarmonyPatch]
    public class MobileTerminal
    {
        internal static GameObject TerminalPrefab;
        internal bool SkipUpdate;
        internal Coroutine HideMobileTerminalCoroutine;
        internal GameObject SpawnedInstance;
        internal Game.Player Player;
        internal bool IsOpen;
        internal ScrollRect ScrollRect;
        internal TextMeshProUGUI Output;
        internal TMPro.TMP_InputField Input;
        internal GameObject InputContainer;
        internal string Text = "";
        internal bool AddedLines = false;
        internal static InputAction MobileTerminalAction;
        internal IApplication RunningApplication;
        internal bool InputActivated = true;
        internal static IApplication StartApplication;
        internal static Regex ParametersRegex = new Regex(@"(?:(['""])(.*?)(?<!\\)(?>\\\\)*\1|([^\s]+))");
        internal MobileTerminal(Game.Player player)
        {
            Player = player;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "OpenMenu_performed")]
        [HarmonyPrefix]
        internal static bool OpenMenu_performed(GameNetcodeStuff.PlayerControllerB __instance, InputAction.CallbackContext context)
        {
            if (__instance == global::StartOfRound.Instance.localPlayerController)
            {
                var player = Game.Player.GetPlayer(__instance);
                if (player.MobileTerminal.IsOpen && context.performed)
                {
                    player.MobileTerminal.Close();
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Update")]
        [HarmonyPrefix]
        internal static void Update(GameNetcodeStuff.PlayerControllerB __instance)
        {
            var player = Game.Player.GetPlayer(__instance);

            if (player.IsLocal && player.MobileTerminal.IsOpen)
            {
                player.MobileTerminal.Update();
            }
        }

        public static void LoadAssets(AssetBundle assets)
        {
            TerminalPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Objects/MobileTerminal.prefab");
        }

        public static void Boot()
        {
            //MobileTerminalAction = new InputAction("MobileTerminal", InputActionType.Button, ClientConfiguration.Keybinds.PortableTerminal);

            ClientConfiguration.Keybinds.PortableTerminal.performed += (obj) => {
                if (global::StartOfRound.Instance != null && global::StartOfRound.Instance.localPlayerController != null)
                {
                    var player = Game.Player.GetPlayer(global::StartOfRound.Instance.localPlayerController);
                    if (!player.IsCrouching && !ShipBuildModeManager.Instance.InBuildMode && !player.Controller.quickMenuManager.isMenuOpen && ((player.Controller.IsOwner && player.Controller.isPlayerControlled && (!player.Controller.IsServer || player.Controller.isHostPlayerObject)) || player.Controller.isTestingPlayer) && !player.Controller.inSpecialInteractAnimation && !player.Controller.isTypingChat && (player.Controller.isMovementHindered <= 0 || player.Controller.isUnderwater) && (player.Controller.thisController.isGrounded || (!player.Controller.isJumping && player.Controller.IsPlayerNearGround())) && !player.Controller.isJumping && (!player.Controller.isPlayerSliding || player.Controller.playerSlidingTimer > 2.5f) && !player.MobileTerminal.IsOpen)
                    {
                        player.MobileTerminal.ToggleMobileTerminal();
                    }
                }
            };
            //MobileTerminalAction.Enable();

            Network.Manager.AddListener<UseMobileTerminal>((msg) =>
            {
                var player = Game.Player.GetPlayer(msg.PlayerNum);
                if (player != null && !player.IsLocal)
                {
                    if (msg.IsUsingTerminal)
                    {
                        player.MobileTerminal.Open();
                        player.MobileTerminal.Output.text = msg.ConsoleText;
                        player.MobileTerminal.Input.text = msg.InputText;
                        player.MobileTerminal.Output.ForceMeshUpdate();
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(player.MobileTerminal.ScrollRect.GetComponent<RectTransform>());
                        player.MobileTerminal.ScrollRect.verticalNormalizedPosition = msg.Scroll;
                    }
                    else
                    {
                        player.MobileTerminal.Close();
                    }
                }
            });
        }

        internal void ToggleMobileTerminal()
        {
            if (!ServerConfiguration.Instance.General.ActivatePortableTerminal)
                return;

            if (IsOpen)
                Close();
            else
                Open();
        }

        public void Submit(string input, string[] additionalParams = null)
        {
            Input.text = "";
            if (RunningApplication != null)
            {
                RunningApplication.Submit(input);
            }
            else
            {
                var @params = ParametersRegex.Matches(input);
                var p = new List<string>();
                for (var i = 0; i < @params.Count; i++)
                {
                    if (@params[i].Groups[2].Value != "")
                        p.Add(@params[i].Groups[2].Value);
                    else if (@params[i].Groups[3].Value != "")
                        p.Add(@params[i].Groups[3].Value);
                    else
                        Plugin.Log.LogWarning("A parameter wasn't parseable: " + @params[i].Value);
                }
                if (p.Count > 0)
                {
                    var args = new string[p.Count - 1];
                    for (var i = 0; i < args.Length; i++)
                        args[i] = p[i + 1];

                    if (Applications.ContainsKey(p[0]))
                    {
                        if (additionalParams != null)
                        {
                            string[] newArgs = new string[args.Length + additionalParams.Length];
                            Array.Copy(args, 0, newArgs, 0, args.Length);
                            Array.Copy(additionalParams, 0, newArgs, args.Length, additionalParams.Length);
                            args = newArgs;
                        }
                        Start(Applications[p[0]], args);
                    }
                    else if (Links.ContainsKey(p[0]))
                    {
                        Submit(Links[p[0]], args);
                    }
                    else
                    {
                        var highestCertainty = 0f;
                        IFallbackApplication highestApp = null;
                        foreach (var f in FallbackApplications)
                        {
                            var certainty = f.Certainty(input);
                            if (certainty > highestCertainty)
                            {
                                highestCertainty = certainty;
                                highestApp = f;
                            }
                        }
                        if (highestApp != null && highestApp is IApplication app)
                        {
                            RunningApplication = app;
                            SkipUpdate = true;
                            highestApp.Fallback(this, input);
                        }
                        else
                            WriteLine("Command \"" + p[0] + "\" not found.");
                    }
                }

            }
            Player.Controller.StartCoroutine(SelectInput());
        }

        internal IEnumerator SelectInput()
        {
            yield return new WaitForEndOfFrame();
            if (InputActivated)
            {
                Input.interactable = true;
                Input.ActivateInputField();
                Input.Select();
            }
            else
            {
                Input.DeactivateInputField();
                Input.interactable = false;
            }
        }

        static MobileTerminal()
        {
            var help = new HelpApplication();
            RegisterApplication("help", help);
            StartApplication = help;
        }

        internal static List<IFallbackApplication> FallbackApplications = new List<IFallbackApplication>();
        internal static Dictionary<string, IApplication> Applications = new Dictionary<string, IApplication>(StringComparer.OrdinalIgnoreCase);
        public static void RegisterApplication(string applicationName, IApplication application)
        {
            if (!Applications.ContainsKey(applicationName))
            {
                Applications.Add(applicationName, application);
                if (application is IFallbackApplication f)
                    FallbackApplications.Add(f);
            }
            else Plugin.Log.LogWarning("An application with the name \"" + applicationName + "\" was already added!");
        }

        internal static Dictionary<string, string> Links = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static void RegisterLink(string linkName, string linkTo)
        {
            if (!Links.ContainsKey(linkName))
            {
                Links.Add(linkName, linkTo);
            }
            else Plugin.Log.LogWarning("A link with the name \"" + linkName + "\" was already added!");
        }

        public void Open()
        {
            if (IsOpen)
                return;
            IsOpen = true;
            if (Player.IsLocal)
            {
                Plugin.Log.LogDebug("Player " + Player.PlayerNum + " (local) opening terminal");
                var leftHand = Player.GetBone(Player.EgoBone.L_HAND);
                foreach (var vol in global::HUDManager.Instance.playerGraphicsVolume.profile.components)
                    if (vol is DepthOfField dof)
                        dof.nearFocusEnd.value = 0.1f;

                global::HUDManager.Instance.HideHUD(true);
                if (SpawnedInstance == null)
                {
                    SpawnedInstance = GameObject.Instantiate(TerminalPrefab, leftHand);
                    SpawnedInstance.transform.localScale = new Vector3(0.126f, 0.126f, 0.126f);
                    SpawnedInstance.transform.localPosition = new Vector3(-0.248f, 0.029f, -0.262f);
                    SpawnedInstance.transform.localRotation = Quaternion.Euler(new Vector3(72.166f, -67.123f, -242.925f));
                    ScrollRect = SpawnedInstance.transform.Find("UI/Scroll View").GetComponent<ScrollRect>();
                    var content = ScrollRect.transform.Find("Viewport/Content");
                    Output = content.Find("Output").GetComponent<TextMeshProUGUI>();
                    InputContainer = content.Find("Input").gameObject;
                    Input = InputContainer.transform.Find("InputField (TMP)").GetComponent<TMPro.TMP_InputField>();
                    Input.onSubmit.AddListener(new UnityEngine.Events.UnityAction<string>((input) => {
                        this.Submit(input);
                    }));
                }
                else SpawnedInstance.SetActive(true);
                Player.Controller.playingQuickSpecialAnimation = true;
                Player.Controller.inSpecialInteractAnimation = true;
                Player.Controller.inTerminalMenu = true;
                Player.Controller.playerBodyAnimator.ResetTrigger("SA_stopAnimation");
                //Player.Controller.localArmsTransform.localPosition = new Vector3(0f, 2.104f, 0.012f);
                //Player.Controller.localArmsTransform.localRotation = Quaternion.Euler(new Vector3(270f, 0f, 0f));
                if (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability) Player.Controller.playerBodyAnimator.SetTrigger("SA_Typing");
                else Player.Controller.playerBodyAnimator.SetTrigger("MobileTerminal");

                Clear();
                Start(StartApplication);
                Input.text = "";
                Input.interactable = true;
                Input.Select();

                Network.Manager.Send(new UseMobileTerminal() { PlayerNum = Player.PlayerNum, ConsoleText = Text, InputText = Input.text, Scroll = 1f, IsUsingTerminal = true });

                if (Player.Controller.currentlyHeldObject != null)
                    Player.Controller.currentlyHeldObject.EnableItemMeshes(false);
                if (Player.Controller.currentlyHeldObjectServer != null)
                    Player.Controller.currentlyHeldObjectServer.EnableItemMeshes(false);
            }
            else
            {
                Plugin.Log.LogDebug("Player " + Player.PlayerNum + " opening terminal");
                var leftHand = Player.GetBone(Player.Bone.L_HAND);
                if (SpawnedInstance == null)
                {
                    SpawnedInstance = GameObject.Instantiate(TerminalPrefab, leftHand);
                    SpawnedInstance.transform.localScale = new Vector3(0.126f, 0.126f, 0.126f);
                    SpawnedInstance.transform.localPosition = new Vector3(-0.18f, 0.032f, -0.3f);
                    SpawnedInstance.transform.localRotation = Quaternion.Euler(new Vector3(74.532f, -76.956f, -240.481f));
                    ScrollRect = SpawnedInstance.transform.Find("UI/Scroll View").GetComponent<ScrollRect>();
                    var content = ScrollRect.transform.Find("Viewport/Content");
                    Output = content.Find("Output").GetComponent<TextMeshProUGUI>();
                    Input = content.Find("Input/InputField (TMP)").GetComponent<TMPro.TMP_InputField>();
                }
                else SpawnedInstance.SetActive(true);
                Player.Controller.playingQuickSpecialAnimation = true;
                Player.Controller.inSpecialInteractAnimation = true;
                Player.Controller.inTerminalMenu = true;

                if (Player.Controller.currentlyHeldObject != null)
                    Player.Controller.currentlyHeldObject.EnableItemMeshes(false);
                if (Player.Controller.currentlyHeldObjectServer != null)
                    Player.Controller.currentlyHeldObjectServer.EnableItemMeshes(false);
            }
            if (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability)
            {
                Player.AddOverride("TypeOnTerminal", "DrawMobileTerminal");
                Player.AddOverride("TypeOnTerminal2", "HoldMobileTerminal");
            }
        }

        public void Close()
        {
            if (!IsOpen)
                return;
            IsOpen = false;
            Input.interactable = false;
            if (Player.IsLocal)
            {
                Patches.InventoryPatches.ArrangeHotbar(Perks.InventorySlots());

                Plugin.Log.LogDebug("Player " + Player.PlayerNum + " (local) closing terminal");
                Player.Controller.playingQuickSpecialAnimation = false;
                Player.Controller.inSpecialInteractAnimation = false;
                Player.Controller.inTerminalMenu = false;
                if (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability) Player.Controller.playerBodyAnimator.ResetTrigger("SA_Typing");
                else Player.Controller.playerBodyAnimator.ResetTrigger("MobileTerminal");
                Player.Controller.playerBodyAnimator.SetTrigger("SA_stopAnimation");
                if (HideMobileTerminalCoroutine != null)
                    Player.Controller.StopCoroutine(HideMobileTerminalCoroutine);
                HideMobileTerminalCoroutine = Player.Controller.StartCoroutine(HideTerminal());
                Network.Manager.Send(new UseMobileTerminal() { PlayerNum = Player.PlayerNum, IsUsingTerminal = false });

                if (Player.Controller.currentlyHeldObject != null)
                    Player.Controller.currentlyHeldObject.EnableItemMeshes(true);
                if (Player.Controller.currentlyHeldObjectServer != null)
                    Player.Controller.currentlyHeldObjectServer.EnableItemMeshes(true);
            }
            else
            {
                Plugin.Log.LogDebug("Player " + Player.PlayerNum + " closing terminal");
                Player.Controller.playingQuickSpecialAnimation = false;
                Player.Controller.inSpecialInteractAnimation = false;
                Player.Controller.inTerminalMenu = false;
                if (HideMobileTerminalCoroutine != null)
                    Player.Controller.StopCoroutine(HideMobileTerminalCoroutine);
                HideMobileTerminalCoroutine = Player.Controller.StartCoroutine(HideTerminal());

                if (Player.Controller.currentlyHeldObject != null)
                    Player.Controller.currentlyHeldObject.EnableItemMeshes(true);
                if (Player.Controller.currentlyHeldObjectServer != null)
                    Player.Controller.currentlyHeldObjectServer.EnableItemMeshes(true);
            }
        }

        internal IEnumerator HideTerminal()
        {
            yield return new WaitForSeconds(0.2f);
            SpawnedInstance.SetActive(false);
            if (Player.IsLocal)
            {
                foreach (var vol in global::HUDManager.Instance.playerGraphicsVolume.profile.components)
                    if (vol is DepthOfField dof)
                        dof.nearFocusEnd.value = 0.5f;
                global::HUDManager.Instance.HideHUD(false);
            }
            if (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability)
            {
                Player.RemoveOverride("TypeOnTerminal");
                Player.RemoveOverride("TypeOnTerminal2");
            }
        }

        internal const float SendKeyCooldown = 0.5f;
        internal const float KeyCooldown = 0.07f;
        internal float KeyTimer = 0f;
        internal float SendKeyTimer = 0f;
        internal float LastMobileTerminalSync = 0f;
        internal float LastScrollSync = 0f;
        internal string LastMobileTerminalSyncText = "";
        internal string LastMobileTerminalSyncInput = "";
        internal bool ScrollToTop = false;

        public void Update()
        {
            if (RunningApplication != null)
            {
                if (!SkipUpdate)
                    RunningApplication.Update();
                SkipUpdate = false;
            }
            if (KeyTimer > 0f)
                KeyTimer -= Time.deltaTime;

            if (!Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability)
                Player.Controller.playerBodyAnimator.ResetTrigger("MobileTerminalTyping");
            if (Game.Manager.Terminal != null)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    if (!Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability)
                        Player.Controller.playerBodyAnimator.SetTrigger("MobileTerminalTyping");
                    if (KeyTimer <= 0f)
                    {
                        KeyTimer = KeyCooldown;
                        RoundManager.PlayRandomClip(Player.Controller.itemAudio, Game.Manager.Terminal.keyboardClips);
                    }
                }
            }
            if (LastMobileTerminalSync > 0f)
                LastMobileTerminalSync -= Time.deltaTime;
            if (LastMobileTerminalSync <= 0f)
            {
                if (LastMobileTerminalSyncText != Text || LastMobileTerminalSyncInput != Input.text || ScrollRect.verticalNormalizedPosition != LastScrollSync)
                {
                    LastMobileTerminalSync = 1f;
                    LastMobileTerminalSyncText = Text;
                    LastMobileTerminalSyncInput = Input.text;
                    LastScrollSync = ScrollRect.verticalNormalizedPosition;
                    Network.Manager.Send(new UseMobileTerminal() { PlayerNum = Player.PlayerNum, IsUsingTerminal = true, ConsoleText = Text, InputText = Input.text, Scroll = ScrollRect.verticalNormalizedPosition });
                }
            }

            var focusSearch = Regex.Replace(Text, @"<[^>]+?\/?>", m => {
                // here you can exclude specific tags such as `<a>` or maybe `<b>`, etc.
                return m.Value != "<focus>" ? "" : m.Value;
            });
            var focusIndex = focusSearch.IndexOf("<focus>");
            Output.text = Text.Replace("<focus>", "");
            if (focusIndex > -1)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.GetComponent<RectTransform>());
                if (Output.textInfo.characterInfo.Length > focusIndex)
                {
                    var tl = Output.textInfo.characterInfo[focusIndex].lineNumber * (Output.fontSize + Output.lineSpacing);
                    var ahead = (Output.fontSize + Output.lineSpacing) * 4;

                    var viewportHeight = ScrollRect.viewport.rect.height;
                    var contentHeight = ScrollRect.content.rect.height;
                    var visibleBottom = contentHeight - (ScrollRect.verticalNormalizedPosition * (contentHeight - viewportHeight));
                    var visibleTop = visibleBottom - viewportHeight;

                    if (tl - ahead < visibleTop)
                        ScrollRect.verticalNormalizedPosition = -((-contentHeight + (tl - ahead) + viewportHeight) / (contentHeight - viewportHeight));
                    else if (tl + ahead > visibleBottom)
                        ScrollRect.verticalNormalizedPosition = (contentHeight - (tl + ahead)) / (contentHeight - viewportHeight);
                }
            }
            if (AddedLines)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.GetComponent<RectTransform>());
                ScrollRect.verticalNormalizedPosition = 0f;
                AddedLines = false;
            }
            if (ScrollToTop)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.GetComponent<RectTransform>());
                ScrollRect.verticalNormalizedPosition = 1f;
                ScrollToTop = false;
            }
        }

        public void SetText(string text, bool scrollToTop = true)
        {
            Text = text;
            if (scrollToTop)
            {
                ScrollToTop = scrollToTop;
            }
        }

        public void Clear()
        {
            Text = "";
        }

        public void WriteLine(string text)
        {
            Text += text + "\r\n";
            AddedLines = true;
        }

        /// <summary>
        /// Exits the currently running application
        /// </summary>
        public void Exit()
        {
            if (RunningApplication != null)
            {
                RunningApplication.Exit();
                RunningApplication = null;
                if (!InputActivated)
                    ActivateInput();
            }
        }

        public void DeactivateInput()
        {
            InputActivated = false;
            InputContainer.SetActive(false);
            Player.Controller.StartCoroutine(SelectInput());
        }

        public void ActivateInput()
        {
            InputActivated = true;
            InputContainer.SetActive(true);
            Player.Controller.StartCoroutine(SelectInput());
        }

        public void Start(IApplication application, string[] args = null)
        {
            if (application == null) return;
            if (args == null) args = new string[] { };
            SkipUpdate = true;
            RunningApplication = application;
            RunningApplication.Main(this, args);
        }

        internal int ScrollTo = -1;
        public void ScrollToText(int pos)
        {
            ScrollTo = pos;
        }
    }
}
