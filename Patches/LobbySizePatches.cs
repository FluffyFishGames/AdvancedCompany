using AdvancedCompany.Config;
using AdvancedCompany.Game;
using AdvancedCompany.Lib;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Network.Sync;
using AdvancedCompany.Objects;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    [LoadAssets]
    [Boot.Bootable]
    internal class LobbySizePatches
    {
        public const int MAX_SUPPORTED_PLAYERS = 32;
        public static int GetLobbySize()
        {
            var size = ServerConfiguration.Instance.Lobby.LobbySize;
            return size;
        }

        internal static Lib.Sync Handshake;
        internal static Dictionary<ulong, Lib.Sync> ConnectingClients = new();
        private static GameObject JoiningLateScreenPrefab;
        private static GameObject JoiningLateScreen;

        [HarmonyPatch(typeof(global::HUDManager), "ApplyPenalty")]
        [HarmonyPostfix]
        static void ApplyPenalty(global::HUDManager __instance, ref int playersDead, ref int bodiesInsured)
        {
            playersDead -= Network.Manager.Lobby.LateJoiners.Count;
            Network.Manager.Lobby.LateJoiners.Clear();
        }
        [HarmonyPatch(typeof(global::HUDManager), "SetPlayerLevelSmoothly")]
        [HarmonyPostfix]
        static void SetPlayerLevelSmoothly(global::HUDManager __instance, ref int XPGain)
        {
            if (Network.Manager.Lobby.LateJoiners.Contains((int) global::StartOfRound.Instance.localPlayerController.playerClientId))
                XPGain = 0;
        }

        [HarmonyPatch(typeof(global::GameNetcodeStuff.PlayerControllerB), "Update")]
        [HarmonyPostfix]
        static void DebugUpdate()
        {
            if (DebugText != null)
            {
                var text = "";
                for (var i = 0; i < global::StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    var player = global::StartOfRound.Instance.allPlayerScripts[i];
                    text += "Player " + i + ":\r\n";
                    text += "isPlayerControlled: " + player.isPlayerControlled + "\r\n";
                    text += "isDead: " + player.isPlayerDead + "\r\n";
                    text += "clientId: " + player.actualClientId + "\r\n";
                }
                DebugText.text = text;
            }
        }
        [HarmonyPatch(typeof(global::StartOfRound), "SyncAlreadyHeldObjectsClientRpc")]
        [HarmonyPostfix]
        static void StartOfRoundSyncAlreadyHeldObjectsClientRpc(global::StartOfRound __instance)
        {
            for (var i = 0; i < __instance.allPlayerScripts.Length; i++)
            {
                var player = __instance.allPlayerScripts[i];
                for (var j = 0; j < player.ItemSlots.Length; j++)
                {
                    if (player.ItemSlots[j] is FlashlightItem flashlight)
                    {
                        InventoryPatches.UpdateFlashlightState(flashlight, !flashlight.isPocketed);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(global::QuickMenuManager), "OpenQuickMenu")]
        [HarmonyPostfix]
        static void OpenMenu_performed(global::QuickMenuManager __instance)
        {
            __instance.inviteFriendsTextAlpha.alpha = (ServerConfiguration.Instance.Lobby.KeepOpenOnMoon) || (ServerConfiguration.Instance.Lobby.KeepOpen && global::StartOfRound.Instance.inShipPhase) || !global::GameNetworkManager.Instance.gameHasStarted ? 1f : 0.2f;
        }

        [HarmonyPatch(typeof(global::HUDManager), "Start")]
        [HarmonyPostfix]
        static void AddDebug(global::HUDManager __instance)
        {
            var canvas = __instance.gameOverAnimator.gameObject.GetComponentInParent<Canvas>();

            if (Handshake != null)
            {
                var lobby = Handshake.GetSyncHandler<LobbyDataSynchronization>();
                if (lobby.OnMoon)
                    JoiningLateScreen = GameObject.Instantiate(JoiningLateScreenPrefab, canvas.transform);
            }

            if (DebugText == null)
            {
                var debug = new GameObject("Debug text");
                debug.transform.parent = canvas.transform;
                debug.transform.localScale = Vector3.one;

                var rect = debug.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.anchoredPosition3D = Vector3.zero;
                rect.pivot = new Vector2(0f, 0f);
                rect.offsetMax = new Vector2(0f, 0f);
                rect.offsetMin = new Vector2(0f, 0f);
                var c = debug.AddComponent<CanvasGroup>();
                c.interactable = false;
                DebugText = debug.AddComponent<TextMeshProUGUI>();
                DebugText.fontSize = 12;
                debug.SetActive(false);
            }
        }

        private static TMPro.TextMeshProUGUI DebugText;
        [HarmonyPatch(typeof(global::GameNetcodeStuff.PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        static void ConnectClientToPlayerObject(global::GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (Handshake != null)
            {
                Handshake.ConnectClientToPlayerObject(__instance);
            }
            for (var i = 0; i < global::StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                var player = Game.Player.GetPlayer(global::StartOfRound.Instance.allPlayerScripts[i]);
                if (player != null)
                {
                    player.SetCosmetics(player.Cosmetics, false);
                }
            }
        }
        [HarmonyPatch(typeof(global::RoundManager), "GenerateNewLevelClientRpc")]
        [HarmonyPostfix]
        static void FinishedGeneratingLevelServerRpc(global::RoundManager __instance, int randomSeed, int levelID)
        {
            if (Handshake != null)
            {
                if (!__instance.currentLevel.spawnEnemiesAndScrap)
                {
                    global::RoundManager.Instance.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
                    global::RoundManager.Instance.FinishGeneratingNewLevelClientRpc();
                    global::RoundManager.Instance.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;

                    LobbySizePatches.LateJoinSpectate();

                    Handshake.LevelLoaded();
                    Handshake = null;
                }
            }
        }
                            
        [HarmonyPatch(typeof(global::RoundManager), "FinishedGeneratingLevelServerRpc")]
        [HarmonyPrefix]
        static bool FinishedGeneratingLevelServerRpc(global::RoundManager __instance)
        {
            if (Handshake != null)
            {
                var lobby = Handshake.GetSyncHandler<LobbyDataSynchronization>();
                if (lobby.OnMoon)
                {
                    global::RoundManager.Instance.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
                    global::RoundManager.Instance.FinishGeneratingNewLevelClientRpc();
                    global::RoundManager.Instance.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;

                    LateJoinSpectate();

                    Handshake.LevelLoaded();
                    Handshake = null;
                    return false;
                }

                Handshake = null;
            }
            return true;
        }

        internal static void LateJoinSpectate()
        {
            var localPlayer = global::StartOfRound.Instance.localPlayerController;
            localPlayer.isPlayerDead = true;
            localPlayer.isPlayerControlled = false;
            localPlayer.thisPlayerModelArms.enabled = false;
            localPlayer.localVisor.position = localPlayer.playersManager.notSpawnedPosition.position;
            localPlayer.isInsideFactory = false;
            localPlayer.IsInspectingItem = false;
            localPlayer.inTerminalMenu = false;
            localPlayer.twoHanded = false;
            localPlayer.carryWeight = 1f;
            localPlayer.fallValue = 0f;
            localPlayer.fallValueUncapped = 0f;
            localPlayer.takingFallDamage = false;
            localPlayer.isSinking = false;
            localPlayer.isUnderwater = false;
            global::StartOfRound.Instance.drowningTimer = 1f;
            global::HUDManager.Instance.setUnderwaterFilter = false;
            localPlayer.wasUnderwaterLastFrame = false;
            localPlayer.sourcesCausingSinking = 0;
            localPlayer.sinkingValue = 0f;
            localPlayer.hinderedMultiplier = 1f;
            localPlayer.isMovementHindered = 0;
            localPlayer.inAnimationWithEnemy = null;
            GameObject.FindObjectOfType<global::Terminal>().terminalInUse = false;
            localPlayer.ChangeAudioListenerToObject(localPlayer.playersManager.spectateCamera.gameObject);
            SoundManager.Instance.SetDiageticMixerSnapshot();
            global::HUDManager.Instance.gameOverAnimator.speed = 1000f;
            global::HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
            global::HUDManager.Instance.HideHUD(hide: true);
            global::StartOfRound.Instance.shipAnimator.speed = 1000f;
            localPlayer.StopHoldInteractionOnTrigger();
            localPlayer.KillPlayerServerRpc((int)localPlayer.playerClientId, false, Vector3.zero, -1, -1, Vector3.zero);
            global::StartOfRound.Instance.SwitchCamera(global::StartOfRound.Instance.spectateCamera);
            localPlayer.isInGameOverAnimation = 0.01f;
            localPlayer.cursorTip.text = "";
            localPlayer.cursorIcon.enabled = false;
            localPlayer.DropAllHeldItems(true);
            localPlayer.DisableJetpackControlsLocally();
            global::HUDManager.Instance.StartCoroutine(HideLateJoinScreen());
            global::HUDManager.Instance.UpdateBoxesSpectateUI();
        }

        public static IEnumerator HideLateJoinScreen()
        {
            yield return new WaitForSeconds(2f);
            global::HUDManager.Instance.gameOverAnimator.speed = 1f;
            global::StartOfRound.Instance.shipAnimator.speed = 1f;
            GameObject.Destroy(JoiningLateScreen);
        }

        private static MethodInfo GetLobbySizeMethod = typeof(LobbySizePatches).GetMethod("GetLobbySize", BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static);
        private static MethodInfo GameObjectSetActive = typeof(GameObject).GetMethod("SetActive", BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
        private static PropertyInfo StartOfRoundInstance = typeof(global::StartOfRound).GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        private static FieldInfo StartOfRoundAllPlayerScripts = typeof(global::StartOfRound).GetField("allPlayerScripts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        private static FieldInfo NetworkObjectGlobalObjectIdHash = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        private static FieldInfo NetworkSceneManagerScenePlacedObjects = typeof(NetworkSceneManager).GetField("ScenePlacedObjects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        private static AudioMixer NewAudioMixer32;
        private static AudioMixer NewAudioMixer28;
        private static AudioMixer NewAudioMixer24;
        private static AudioMixer NewAudioMixer20;
        private static AudioMixer NewAudioMixer16;
        private static AudioMixer NewAudioMixer12;
        private static AudioMixer NewAudioMixer8;
        private static AudioMixer NewAudioMixer4;
        private static AudioMixer OriginalMixer;
        private static GameObject ScrollViewPrefab;

        public static void LoadAssets(AssetBundle assets)
        {
            NewAudioMixer32 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic32Players.mixer");
            NewAudioMixer28 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic28Players.mixer");
            NewAudioMixer24 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic24Players.mixer");
            NewAudioMixer20 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic20Players.mixer");
            NewAudioMixer16 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic16Players.mixer");
            NewAudioMixer12 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic12Players.mixer");
            NewAudioMixer8 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic8Players.mixer");
            NewAudioMixer4 = assets.LoadAsset<AudioMixer>("Assets/AudioMixer/Diagetic4Players.mixer");
            ScrollViewPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/ScrollView.prefab");
            JoiningLateScreenPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/LateJoin.prefab");
        }

        public static void PatchAsync()
        {
            var startHostMethod = typeof(global::GameNetworkManager).GetMethod("StartHost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var asyncStateMachine = startHostMethod.GetCustomAttribute<AsyncStateMachineAttribute>();
            var moveNext = asyncStateMachine.StateMachineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            Harmony harmony = new Harmony("AdvancedCompany");
            var harmonyMethod = new HarmonyMethod(typeof(LobbySizePatches).GetMethod("TranspileStartHost"));
            harmony.Patch(moveNext, null, null, harmonyMethod);

            var loadServerListMethod = typeof(global::SteamLobbyManager).GetMethod("LoadServerList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            asyncStateMachine = loadServerListMethod.GetCustomAttribute<AsyncStateMachineAttribute>();
            moveNext = asyncStateMachine.StateMachineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            harmonyMethod = new HarmonyMethod(typeof(LobbySizePatches).GetMethod("TranspileLoadServerList"));
            harmony.Patch(moveNext, null, null, harmonyMethod);
        }

        public static IEnumerable<CodeInstruction> TranspileStartHost(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching GameNetworkManager->StartHost...");

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Call && inst[i - 1].opcode == OpCodes.Ldc_I4_4))
                {
                    Plugin.Log.LogDebug("Replacing SteamMatchmaking.CreateLobbyAsync(4) with SteamMatchmaking.CreateLobbyAsync(LobbySizePatches.GetLobbySize())");
                    inst[i - 1].opcode = OpCodes.Call;
                    inst[i - 1].operand = GetLobbySizeMethod;
                }
            }
            Plugin.Log.LogDebug("Patched GameNetworkManager->StartHost!");
            return inst.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> TranspileLoadServerList(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching SteamLobbyManager->LoadServerList...");

            var inst = new List<CodeInstruction>(instructions);
            int j = 0;
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Call && inst[i].operand.ToString() == "Steamworks.Data.LobbyQuery WithKeyValue(System.String, System.String)") && inst[i-4].opcode == OpCodes.Ldstr && inst[i-4].operand.ToString() == "vers")
                {
                    Plugin.Log.LogDebug("Adding .WithKeyValue(\"advcmpny\", Lib.Mod.GetHash()) to lobby search.");
                    inst.Insert(i + 1, inst[i - 0]);
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(Lib.Mod).GetMethod("GetHash", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)));
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Ldstr, "advcmpny"));
                    inst.Insert(i + 1, inst[i - 5]);
                    inst.Insert(i + 1, inst[i - 6]);
                    i += 5;
                    j++;
                    if (j == 3)
                        break;
                }
            }
            Plugin.Log.LogDebug("Patched SteamLobbyManager->LoadServerList...");
            return inst.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> ReplaceLowerThanFourWithPlayerCount(IEnumerable<CodeInstruction> instructions, bool lookForArray = false)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (((inst[i].opcode == OpCodes.Blt || inst[i].opcode == OpCodes.Blt_S) && inst[i - 1].opcode == OpCodes.Ldc_I4_4) ||
                    (lookForArray && inst[i].opcode == OpCodes.Newarr && inst[i - 1].opcode == OpCodes.Ldc_I4_4))
                {
                    Plugin.Log.LogDebug("Replacing < 4 with < StartOfRound.Instance.allPlayerScripts.length");
                    inst[i - 1].opcode = OpCodes.Call;
                    inst[i - 1].operand = StartOfRoundInstance.GetMethod;
                    inst.Insert(i, new CodeInstruction(OpCodes.Conv_I4));
                    inst.Insert(i, new CodeInstruction(OpCodes.Ldlen));
                    inst.Insert(i, new CodeInstruction(OpCodes.Ldfld, StartOfRoundAllPlayerScripts));
                }
            }
            return inst.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> ReplaceGreaterThanOrEqualFourWithMaxPlayers(IEnumerable<CodeInstruction> instructions, bool lookForArray = false)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (((inst[i].opcode == OpCodes.Blt || inst[i].opcode == OpCodes.Blt_S || inst[i].opcode == OpCodes.Bge || inst[i].opcode == OpCodes.Bge_S) && inst[i - 1].opcode == OpCodes.Ldc_I4_4))
                {
                    Plugin.Log.LogDebug("Replacing < 4 with < LobbyPatches.GetLobbySize()");
                    inst[i - 1].opcode = OpCodes.Call;
                    inst[i - 1].operand = GetLobbySizeMethod;
                }
            }
            return inst.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> SkipGameStarted(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (((inst[i].opcode == OpCodes.Brfalse || inst[i].opcode == OpCodes.Brfalse_S) && inst[i - 1].opcode == OpCodes.Ldfld && inst[i - 1].operand.ToString().Contains("gameHasStarted")))
                {
                    Plugin.Log.LogDebug("Skipping GameNetworkManager->gameHasStarted");
                    inst[i - 2].opcode = OpCodes.Ldc_I4_0;
                    inst[i - 2].operand = null;
                    inst.RemoveAt(i - 1);
                }
                if (((inst[i].opcode == OpCodes.Brtrue || inst[i].opcode == OpCodes.Brtrue_S) && inst[i - 1].opcode == OpCodes.Ldfld && inst[i - 1].operand.ToString().Contains("gameHasStarted")))
                {
                    Plugin.Log.LogDebug("Skipping GameNetworkManager->gameHasStarted");
                    inst[i - 2].opcode = OpCodes.Ldc_I4_1;
                    inst[i - 2].operand = null;
                    inst.RemoveAt(i - 1);
                }
            }
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::StartOfRound), "OnPlayerConnectedClientRpc")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchStartOfRoundOnPlayerConnectedClientRpc(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->OnPlayerConnectedClientRpc...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Callvirt && inst[i].operand.ToString().Contains("get_IsOwnedByServer")) && inst[i + 1].opcode == OpCodes.Brtrue)
                {
                    var branchTarget = inst[i + 1].operand;

                    var insts = new List<CodeInstruction>();
                    insts.Add(inst[i - 4]);
                    insts.Add(inst[i - 3]);
                    insts.Add(inst[i - 2]);
                    insts.Add(inst[i - 1]);
                    insts.Add(new CodeInstruction(OpCodes.Ldfld, typeof(global::GameNetcodeStuff.PlayerControllerB).GetField("isPlayerDead")));
                    insts.Add(inst[i + 1]);

                    inst.Insert(i - 5, new CodeInstruction(inst[i - 6].opcode, inst[i - 6].operand));
                    inst[i - 6].opcode = insts[0].opcode;
                    inst[i - 6].operand = insts[0].operand;
                    for (var j = insts.Count - 1; j >= 1; j--)
                        inst.Insert(i - 5, insts[j]);
                    Plugin.Log.LogDebug("Adding && !StartOfRound::allPlayerScripts[j].isPlayerDead");
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched StartOfRound->OnPlayerConnectedClientRpc!");
            return inst.AsEnumerable();
        }

        private static PropertyInfo LobbyMaxMembers = typeof(Steamworks.Data.Lobby).GetProperty("MaxMembers", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        [HarmonyPatch(typeof(global::GameNetworkManager), "LobbyDataIsJoinable")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchGameNetworkManagerLobbyDataIsJoinable(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching GameNetworkManager->LobbyDataIsJoinable...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (((inst[i].opcode == OpCodes.Bge || inst[i].opcode == OpCodes.Bge_S) && inst[i - 1].opcode == OpCodes.Ldc_I4_4))
                {
                    Plugin.Log.LogDebug("Replacing >= 4 with >=4 lobby.MaxMembers");
                    inst[i - 1].opcode = OpCodes.Call;
                    inst[i - 1].operand = LobbyMaxMembers.GetMethod;
                    inst.Insert(i - 1, inst[i - 3]);
                }
            }
            Plugin.Log.LogDebug("Patched GameNetworkManager->LobbyDataIsJoinable!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "ConnectionApproval")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchGameNetworkManagerConnectionApproval(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching GameNetworkManager->ConnectionApproval...");
            instructions = SkipGameStarted(instructions);
            instructions = ReplaceGreaterThanOrEqualFourWithMaxPlayers(instructions);
            Plugin.Log.LogDebug("Patched GameNetworkManager->ConnectionApproval!");
            return instructions;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "StartGame")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchStartOfRoundStartGame(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->StartGame...");
            //instructions = SkipGameStarted(instructions);
            Plugin.Log.LogDebug("Patched StartOfRound->StartGame!");
            return instructions;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "openingDoorsSequence", MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchStartOfRoundopeningDoorsSequence(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->openingDoorsSequence...");
            //instructions = SkipGameStarted(instructions);
            Plugin.Log.LogDebug("Patched StartOfRound->openingDoorsSequence!");
            return instructions;
        }

        #region EnemyAI patches
        [HarmonyPatch(typeof(global::DressGirlAI), "ChoosePlayerToHaunt")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchDressGirlAIChoosePlayerToHaunt(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching DressGirlAI->ChoosePlayerToHunt...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions, true);
            Plugin.Log.LogDebug("Patched DressGirlAI->ChoosePlayerToHunt!");
            return instructions;
        }

        [HarmonyPatch(typeof(global::EnemyAI), "GetClosestPlayer")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchEnemyAIGetClosestPlayer(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching EnemyAI->GetClosestPlayer...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions);
            Plugin.Log.LogDebug("Patched EnemyAI->GetClosestPlayer!");
            return instructions;
        }

        [HarmonyPatch(typeof(global::ButlerEnemyAI), "Start")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchButlerEnemyAIStart(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching ButlerEnemyAI->Start...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions, true);
            Plugin.Log.LogDebug("Patched ButlerEnemyAI->Start!");
            return instructions;
        }

        [HarmonyPatch(typeof(SpringManAI), "DoAIInterval")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSpringManAIDoAIInterval(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching SpringManAI->DoAIInterval...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions);
            Plugin.Log.LogDebug("Patched SpringManAI->DoAIInterval!");
            return instructions;
        }

        [HarmonyPatch(typeof(SpringManAI), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSpringManAIUpdate(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching SpringManAI->Update...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions);
            Plugin.Log.LogDebug("Patched SpringManAI->Update!");
            return instructions;
        }
        #endregion

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "SendNewPlayerValuesServerRpc")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSendNewPlayerValuesServerRpc(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->SendNewPlayerValuesServerRpc...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions);
            Plugin.Log.LogDebug("Patched PlayerControllerB->SendNewPlayerValuesServerRpc!");
            return instructions;
        }


        [HarmonyPatch(typeof(QuickMenuManager), "ConfirmKickUserFromServer")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchQuickMenuManagerConfirmKickUserFromServer(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching QuickMenuManager->ConfirmKickUserFromServer...");

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (((inst[i].opcode == OpCodes.Bgt || inst[i].opcode == OpCodes.Bgt_S) && inst[i - 1].opcode == OpCodes.Ldc_I4_3))
                {
                    Plugin.Log.LogDebug("Replacing > 4 with > LobbyPatches.GetLobbySize()");
                    inst[i - 1].opcode = OpCodes.Call;
                    inst[i - 1].operand = GetLobbySizeMethod;
                }
            }
            Plugin.Log.LogDebug("Patched QuickMenuManager->ConfirmKickUserFromServer!");
            return inst.AsEnumerable(); 
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "SpectateNextPlayer")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSpectateNextPlayer(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->SpectateNextPlayer...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Rem && inst[i - 1].opcode == OpCodes.Ldc_I4_4))
                {
                    Plugin.Log.LogDebug("Replacing % 4 with % LobbyPatches.GetLobbySize()");
                    inst[i - 1].opcode = OpCodes.Call;
                    inst[i - 1].operand = GetLobbySizeMethod;
                }
            }
            Plugin.Log.LogDebug("Patched PlayerControllerB->SpectateNextPlayer!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::HUDManager), "SyncAllPlayerLevelsServerRpc", new Type[] { })]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchHUDManagerSyncAllPlayerLevelsServerRpc(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching HUDManager->SyncAllPlayerLevelsServerRpc...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions, true);
            Plugin.Log.LogDebug("Patched HUDManager->SyncAllPlayerLevelsServerRpc!");
            return instructions;
        }


        [HarmonyPatch(typeof(global::StartOfRound), "OnClientConnect")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchStartOfRoundOnClientConnect(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->OnClientConnect...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions, true);

            var method = typeof(Network.Lobby).GetMethod("GetPlayerNum", BindingFlags.Static | BindingFlags.Public);
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Stfld && inst[i].operand.ToString().Contains("actualClientId")))
                {
                    Plugin.Log.LogDebug("Adding num = Network.Lobby.GetPlayerNum(clientId)");
                    var insts = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, method),
                        new CodeInstruction(OpCodes.Stloc_1),
                    };

                    inst.Insert(i - 4, new CodeInstruction(inst[i - 5].opcode, inst[i - 5].operand));
                    inst[i - 5].opcode = insts[0].opcode;
                    inst[i - 5].operand = insts[0].operand;

                    for (var j = insts.Count - 1; j >= 1; j--)
                        inst.Insert(i - 4, insts[j]);
                    i += 2;
                    break;
                }
            }
            instructions = inst.AsEnumerable();

            Plugin.Log.LogDebug("Patched StartOfRound->OnClientConnect!");
            return instructions;
        }


        [HarmonyPatch(typeof(global::GameNetworkManager), "Singleton_OnClientDisconnectCallback")]
        [HarmonyPostfix]
        static void PostfixOnClientConnect(global::GameNetworkManager __instance, ulong clientId)
        {
            if (ConnectingClients.ContainsKey(clientId))
            {
                Plugin.Log.LogInfo("Connection disconnected. Removing " + clientId + " from connecting clients list.");
                ConnectingClients[clientId].IsFinished = true;
                PendingRequests.Remove(ConnectingClients[clientId]);
                ConnectingClients.Remove(clientId);
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "OnClientConnect")]
        [HarmonyPostfix]
        static void PostfixOnClientConnect(global::StartOfRound __instance, ulong clientId)
        {
            if (ConnectingClients.ContainsKey(clientId))
            {
                Plugin.Log.LogInfo("Connection complete. Removing " + clientId + " from connecting clients list.");
                ConnectingClients[clientId].IsFinished = true;
                PendingRequests.Remove(ConnectingClients[clientId]);
                ConnectingClients.Remove(clientId);
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "SyncShipUnlockablesServerRpc")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchStartOfRoundSyncShipUnlockablesServerRpc(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->SyncShipUnlockablesServerRpc...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions, true);
            Plugin.Log.LogDebug("Patched StartOfRound->SyncShipUnlockablesServerRpc!");
            return instructions;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "SyncShipUnlockablesClientRpc")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchStartOfRoundSyncShipUnlockablesClientRpc(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->SyncShipUnlockablesClientRpc...");
            instructions = ReplaceLowerThanFourWithPlayerCount(instructions);
            Plugin.Log.LogDebug("Patched StartOfRound->SyncShipUnlockablesClientRpc!");
            return instructions;
        }

        [HarmonyPatch(typeof(QuickMenuManager), "AddUserToPlayerList")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchQuickMenuManagerAddUserToPlayerList(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching QuickMenuManager->AddUserToPlayerList...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Ble && inst[i - 1].opcode == OpCodes.Ldc_I4_4))
                {
                    Plugin.Log.LogDebug("Replacing <= 4 with <= LobbyPatches->GetLobbySize() - 1");
                    inst[i - 1].opcode = OpCodes.Call;
                    inst[i - 1].operand = GetLobbySizeMethod;
                    inst.Insert(i, new CodeInstruction(OpCodes.Sub));
                    inst.Insert(i, new CodeInstruction(OpCodes.Ldc_I4_1));
                }
            }
            Plugin.Log.LogDebug("Patched QuickMenuManager->AddUserToPlayerList!");
            return inst.AsEnumerable();
        }

        public static AudioMixer PlayerMixer;
        protected static FieldInfo PlayerMixerField = typeof(LobbySizePatches).GetField("PlayerMixer", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

        [HarmonyPatch(typeof(global::SoundManager), "SetPlayerPitch")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSetPlayerPitch(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching SoundManager->PatchSetPlayerPitch...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "UnityEngine.Audio.AudioMixer diageticMixer"))
                {
                    Plugin.Log.LogDebug("Replacing SoundManger->diageticMixer with LobbyPatches->PlayerMixer");
                    inst[i].opcode = OpCodes.Ldsfld;
                    inst[i].operand = PlayerMixerField;
                    inst.RemoveAt(i - 1);
                    i++;
                }
            }
            Plugin.Log.LogDebug("Patched SoundManager->PatchSetPlayerPitch!");
            return inst.AsEnumerable();
        }

        public static string ChatReplace(string message)
        {
            var startOfRound = global::StartOfRound.Instance;
            for (var i = 4; i < startOfRound.allPlayerScripts.Length; i++)
            {
                message = message.Replace("[playerNum" + i + "]", startOfRound.allPlayerScripts[i].playerUsername);
            }
            return message;
        }

        [HarmonyPatch(typeof(global::HUDManager), "AddChatMessage")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchHUDManagerAddChatMessage(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching HUDManager->AddChatMessage...");
            var inst = new List<CodeInstruction>(instructions);
            var method = typeof(LobbySizePatches).GetMethod("ChatReplace", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Callvirt && inst[i].operand.ToString().Contains("ToString")))
                {
                    Plugin.Log.LogDebug("Adding LobbySizePatches->ChatReplace");
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Call, method));
                    i++;
                }
            }
            Plugin.Log.LogDebug("Patched HUDManager->AddChatMessage!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::SoundManager), "SetPlayerVoiceFilters")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSetPlayerVoiceFilters(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching SoundManager->SetPlayerVoiceFilters...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "UnityEngine.Audio.AudioMixer diageticMixer"))
                {
                    Plugin.Log.LogDebug("Replacing SoundManger->diageticMixer with LobbyPatches->PlayerMixer");
                    inst[i - 1].opcode = OpCodes.Ldsfld;
                    inst[i - 1].operand = PlayerMixerField;
                    inst.RemoveAt(i);
                    i++;
                }
            }
            Plugin.Log.LogDebug("Patched SoundManager->SetPlayerVoiceFilters!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::SoundManager), "Start")]
        [HarmonyPrefix]
        public static bool ChangeMixerSetting()
        {
            try
            {
                var playerCount = ServerConfiguration.Instance.Lobby.LobbySize;
                Plugin.Log.LogInfo("Changing audio mixer and expanding voice chat slots for " + playerCount + " players.");
                if (OriginalMixer == null)
                    OriginalMixer = SoundManager.Instance.diageticMixer;
                var mixer = NewAudioMixer32;
                if (playerCount > 28)
                    mixer = NewAudioMixer32;
                else if (playerCount > 24)
                    mixer = NewAudioMixer28;
                else if (playerCount > 20)
                    mixer = NewAudioMixer24;
                else if (playerCount > 16)
                    mixer = NewAudioMixer20;
                else if (playerCount > 12)
                    mixer = NewAudioMixer16;
                else if (playerCount > 8)
                    mixer = NewAudioMixer12;
                else if (playerCount > 4)
                    mixer = NewAudioMixer8;
                else
                    mixer = NewAudioMixer4;

                Plugin.Log.LogInfo("New AudioMixer: " + mixer.name);

                mixer.outputAudioMixerGroup = OriginalMixer.FindMatchingGroups("Master")[0];
                PlayerMixer = mixer;

                var newPlayerVoicePitches = new float[playerCount];
                var newPlayerVoicePitchTargets = new float[playerCount];
                var newPlayerVoiceVolumes = new float[playerCount];
                var newPlayerVoicePitchLerpSpeed = new float[playerCount];
                var newPlayerVoiceMixers = new AudioMixerGroup[playerCount];

                for (var i = 0; i < playerCount; i++)
                {
                    newPlayerVoicePitches[i] = 1f;
                    newPlayerVoicePitchTargets[i] = 1f;
                    newPlayerVoiceVolumes[i] = 0.5f;
                    newPlayerVoicePitchLerpSpeed[i] = 3f;
                    newPlayerVoiceMixers[i] = mixer.FindMatchingGroups("VoicePlayer" + i)[0];
                }

                SoundManager.Instance.playerVoicePitches = newPlayerVoicePitches;
                SoundManager.Instance.playerVoicePitchTargets = newPlayerVoicePitchTargets;
                SoundManager.Instance.playerVoiceVolumes = newPlayerVoiceVolumes;
                SoundManager.Instance.playerVoicePitchLerpSpeed = newPlayerVoicePitchLerpSpeed;
                SoundManager.Instance.playerVoiceMixers = newPlayerVoiceMixers;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while initializing SoundManager. Disconnecting...");
                Plugin.Log.LogError(e);
                if (global::GameNetworkManager.Instance != null)
                {
                    global::GameNetworkManager.Instance.disconnectionReasonMessage = "SoundManager initialization failed!";
                    global::GameNetworkManager.Instance.Disconnect();
                }
            }
            return false;
        }

        private static ulong CurrentHandshakePlayer = 0;

        [HarmonyPatch(typeof(global::Unity.Netcode.NetworkConnectionManager), "SendConnectionRequest")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchNetworkConnectionManagerSendConnectionRequest(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching NetworkConnectionManager->SendConnectionRequest...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Call && inst[i].operand.ToString().Contains("SendMessage")))
                {
                    Plugin.Log.LogDebug("Making connection request fragmented.");
                    inst[i - 3].opcode = OpCodes.Ldc_I4_4;
                }
            }
            Plugin.Log.LogDebug("Patched NetworkConnectionManager->SendConnectionRequest!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(ConnectionRequestMessage), "Deserialize")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchConnectionRequestMessageDeserialize(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching ConnectionRequestMessage->Deserialize...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Callvirt && inst[i].operand.ToString().Contains("CompareConfig")))
                {
                    Plugin.Log.LogDebug("Skipping config mismatch as we handle this ourself.");
                    inst[i + 1].opcode = OpCodes.Br;
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Pop));
                    i++;
                }
            }
            Plugin.Log.LogDebug("Patched ConnectionRequestMessage->Deserialize!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(NetworkConnectionManager), "HandleConnectionApproval")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchNetworkConnectionManagerHandleConnectionApproval(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching NetworkConnectionManager->HandleConnectionApproval...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Call && inst[i].operand.ToString().Contains("SendMessage")) && inst[i - 2].opcode == OpCodes.Ldc_I4_2)
                {
                    Plugin.Log.LogDebug("Making message fragmented.");
                    inst[i - 2].opcode = OpCodes.Ldc_I4_4;
                }
            }
            Plugin.Log.LogDebug("Patched NetworkConnectionManager->HandleConnectionApproval!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "LeaveLobbyAtGameStart")]
        [HarmonyPrefix]
        static bool LeaveLobbyAtGameStart(global::GameNetworkManager __instance)
        {
            Network.Manager.Lobby.IsJoinable = false;
            if (ServerConfiguration.Instance.Lobby.KeepOpen)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    if (!ServerConfiguration.Instance.Lobby.KeepOpenOnMoon && __instance.currentLobby != null && __instance.currentLobby.HasValue)
                        __instance.currentLobby.Value.SetJoinable(false);
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(global::Unity.Netcode.NetworkConnectionManager), "ApproveConnection")]
        [HarmonyPostfix]
        static void ApproveConnection(global::Unity.Netcode.NetworkConnectionManager __instance, ref ConnectionRequestMessage connectionRequestMessage, ref NetworkContext context)
        {
            if (ConnectingClients.ContainsKey(context.SenderId) && ConnectingClients[context.SenderId].Error != null)
            {
                __instance.ClientsToApprove[context.SenderId].Approved = false;
                __instance.ClientsToApprove[context.SenderId].Reason = ConnectingClients[context.SenderId].Error;
                ConnectingClients[context.SenderId].IsFinished = true;
                ConnectingClients.Remove(context.SenderId);
            }
        }

        [HarmonyPatch(typeof(global::Unity.Netcode.ConnectionApprovedMessage), "Serialize")]
        [HarmonyPostfix]
        static void WriteLobbyData(global::Unity.Netcode.ConnectionApprovedMessage __instance, ref FastBufferWriter writer, int targetVersion)
        {
            if (ConnectingClients.ContainsKey(__instance.OwnerClientId))
            {
                Plugin.Log.LogInfo("Adding data to ConnectionApprovedMessage.");
                var handshake = ConnectingClients[__instance.OwnerClientId];
                var handshakePos = writer.Position;
                handshake.WriteDataToPlayerJoiningLobby(writer);
                writer.WriteValueSafe(handshakePos);

                var handshakeData = writer.ToArray();
                using (var istream = new MemoryStream(handshakeData))
                using (var mstream = new MemoryStream())
                {
                    using (var dstream = new GZipStream(mstream, System.IO.Compression.CompressionMode.Compress))
                    {
                        istream.CopyTo(dstream);
                        dstream.Flush();
                    }
                    handshakeData = mstream.ToArray();
                }
                var secondWriter = new FastBufferWriter(handshakeData.Length, Allocator.Temp, -1);
                secondWriter.WriteBytesSafe(handshakeData, handshakeData.Length);
                unsafe
                {
                    SwitchHandles(ref writer.Handle, ref secondWriter.Handle);
                }
                secondWriter.Dispose();
            }
        }

        private static unsafe void SwitchHandles(ref FastBufferWriter.WriterHandle* handleA, ref FastBufferWriter.WriterHandle* handleB)
        {
            Allocator allocator = handleB->Allocator;
            bool bufferGrew = handleB->BufferGrew;
            byte* bufferPointer = handleB->BufferPointer;
            int capacity = handleB->Capacity;
            int length = handleB->Length;
            int maxCapacity = handleB->MaxCapacity;
            int position = handleB->Position;

            handleB->Allocator = handleA->Allocator;
            handleB->BufferGrew = handleA->BufferGrew;
            handleB->BufferPointer = handleA->BufferPointer;
            handleB->Capacity = handleA->Capacity;
            handleB->Length = handleA->Length;
            handleB->MaxCapacity = handleA->MaxCapacity;
            handleB->Position = handleA->Position;

            handleA->Allocator = allocator;
            handleA->BufferGrew = bufferGrew;
            handleA->BufferPointer = bufferPointer;
            handleA->Capacity = capacity;
            handleA->Length = length;
            handleA->MaxCapacity = maxCapacity;
            handleA->Position = position;
        }

        [HarmonyPatch(typeof(global::Unity.Netcode.ConnectionApprovedMessage), "Deserialize")]
        [HarmonyPrefix]
        static void DecodeLobbyData(global::Unity.Netcode.ConnectionApprovedMessage __instance, ref bool __result, ref FastBufferReader reader, NetworkContext context, int receivedMessageVersion)
        {
            Plugin.Log.LogDebug("Len: " + reader.Length);
            byte[] data = new byte[reader.Length];
            reader.ReadBytesSafe(ref data, reader.Length);
            for (var i = 0; i < 32; i++)
                Plugin.Log.LogMessage(data[i]);

            using (var mstream = new MemoryStream(data))
            using (var dstream = new GZipStream(mstream, System.IO.Compression.CompressionMode.Decompress))
            using (var ostream = new MemoryStream())
            {
                dstream.CopyTo(ostream);
                data = ostream.ToArray();
            }
            reader = new FastBufferReader(data, Allocator.Temp, data.Length, 0);
        }

        [HarmonyPatch(typeof(global::Unity.Netcode.ConnectionApprovedMessage), "Deserialize")]
        [HarmonyPostfix]
        static void ReadLobbyData(global::Unity.Netcode.ConnectionApprovedMessage __instance, ref bool __result, FastBufferReader reader, NetworkContext context, int receivedMessageVersion)
        {
            if (__result)
            {
                Game.Player.Reset();
                Plugin.Log.LogMessage("Reading ApprovedMessage.");
                var prevPos = reader.Position;
                reader.Seek(reader.Length - 4);
                reader.ReadValueSafe(out int handshakePos);
                reader.Seek(handshakePos);
                Handshake.ReadDataFromJoiningLobby(reader);
                reader.Seek(prevPos);
            }
        }


        [HarmonyPatch(typeof(global::Unity.Netcode.ConnectionRequestMessage), "Serialize")]
        [HarmonyPostfix]
        static void WriteConnectData(global::Unity.Netcode.ConnectionRequestMessage __instance, FastBufferWriter writer, int targetVersion)
        {
            Plugin.Log.LogMessage("Doing a handshake with the server.");

            Handshake = new Sync();
            Handshake.WriteDataToHostBeforeJoining(writer);
        }

        [HarmonyPatch(typeof(global::Unity.Netcode.ConnectionRequestMessage), "Deserialize")]
        [HarmonyPostfix]
        static void ReadConnectData(global::Unity.Netcode.ConnectionRequestMessage __instance, ref bool __result, FastBufferReader reader, NetworkContext context, int receivedMessageVersion)
        {
            NetworkManager networkManager = (NetworkManager)context.SystemOwner;
            var handshake = new Lib.Sync();
            handshake.ClientId = context.SenderId;
            handshake.ConfigHash = __instance.ConfigHash;
            ConnectingClients.Add(context.SenderId, handshake);

            try
            {
                if (__result)
                {
                    if (reader.Length > reader.Position)
                    {
                        if ((Network.Manager.Lobby.IsJoinable || ServerConfiguration.Instance.Lobby.KeepOpenOnMoon) &&
                            (!global::GameNetworkManager.Instance.gameHasStarted || ServerConfiguration.Instance.Lobby.KeepOpen))
                        {
                            handshake.ReadDataFromClientBeforeJoining(reader);
                        }
                        else
                        {
                            handshake.SetError("Lobby is currently not joinable. Game has started.");
                        }
                    }
                    else
                    {
                        handshake.SetError("Missing AdvancedCompany mod.");
                        Plugin.Log.LogWarning("Client is missing AdvancedCompany!");
                    }
                }
            }
            catch (Exception x)
            {
                handshake.SetError("Unknown error");
                Plugin.Log.LogError("Unknown error while connecting:");
                Plugin.Log.LogError(x.ToString());
            }
        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "SteamMatchmaking_OnLobbyCreated")]
        [HarmonyPrefix]
        static void SteamMatchmaking_OnLobbyCreated(global::GameNetworkManager __instance, Result result, Lobby lobby)
        {
            lobby.SetData("advcmpny", Lib.Mod.GetHash());
        }

        public static bool LobbyOpen = true;
        private static List<Sync> PendingRequests = new();
        private static int LastConnectingClientsCount = 0;
        [HarmonyPatch(typeof(global::GameNetworkManager), "ConnectionApproval")]
        [HarmonyPostfix]
        static void GameNetworkManagerConnectionApproval(global::GameNetworkManager __instance, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (!ServerConfiguration.Instance.Lobby.KeepOpen)
            {
                if (__instance.gameHasStarted || !Network.Manager.Lobby.IsJoinable)
                {
                    response.Reason = "Game has already started!";
                    response.Approved = false;
                    return;
                }
            }
            if (ConnectingClients.ContainsKey(request.ClientNetworkId))
            {
                if (response.Approved)
                {
                    Plugin.Log.LogInfo("Connecting client added to queue... Currently there are " + ConnectingClients.Count + " clients connecting...");
                    response.Pending = true;
                    ConnectingClients[request.ClientNetworkId].Response = response;
                    PendingRequests.Add(ConnectingClients[request.ClientNetworkId]);
                }
            }
        }

        [HarmonyPatch(typeof(global::RoundManager), "Update")]
        [HarmonyPostfix]
        static void WorkQueue()
        {
            if (PendingRequests.Count > 0)
            {
                var request = PendingRequests[0];
                request.Response.Pending = false;

                if (request.IsFinished)
                {
                    Plugin.Log.LogInfo("Client finished. Working on connection queue. Pending clients: " + PendingRequests.Count);
                    PendingRequests.RemoveAt(0);
                }
            }
        }


        [HarmonyPatch(typeof(global::QuickMenuManager), "InviteFriendsButton")]
        [HarmonyPrefix]
        static bool QuickMenuManagerInviteFriendsButton(global::QuickMenuManager __instance)
        {
            if (ServerConfiguration.Instance.Lobby.KeepOpenOnMoon)
            {
                global::GameNetworkManager.Instance.InviteFriendsUI();
                return false;
            }
            else if (ServerConfiguration.Instance.Lobby.KeepOpen)
            {
                if (Network.Manager.Lobby.IsJoinable)
                    global::GameNetworkManager.Instance.InviteFriendsUI();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "openingDoorsSequence", MethodType.Enumerator)]
        [HarmonyPrefix]
        static void openingDoorsSequenceCloseLobby(global::StartOfRound __instance)
        {
            Network.Manager.Lobby.IsJoinable = false;
            if (!ServerConfiguration.Instance.Lobby.KeepOpenOnMoon)
            {
                if (global::GameNetworkManager.Instance.currentLobby != null && global::GameNetworkManager.Instance.currentLobby.HasValue)
                {
                    global::GameNetworkManager.Instance.currentLobby.Value.SetJoinable(false);
                }
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "StartGame")]
        [HarmonyPrefix]
        static void StartOfRoundStartGame(global::StartOfRound __instance)
        {
            Network.Manager.Lobby.IsJoinable = false;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ReviveDeadPlayers")]
        [HarmonyPrefix]
        static void StartOfRoundReviveDeadPlayers(global::StartOfRound __instance)
        {
            Network.Manager.Lobby.IsJoinable = true;
            if (ServerConfiguration.Instance.Lobby.KeepOpen)
            {
                if (global::GameNetworkManager.Instance.currentLobby != null && global::GameNetworkManager.Instance.currentLobby.HasValue)
                {
                    global::GameNetworkManager.Instance.currentLobby.Value.SetJoinable(true);
                }
            }
        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "Singleton_OnClientConnectedCallback")]
        [HarmonyPostfix]
        static void Singleton_OnClientConnectedCallback()
        {
            if (Handshake != null)
            {
                Handshake.NetworkObjectsPlaced();
            }
        }

        [HarmonyPatch(typeof(NetworkSceneManager), "PopulateScenePlacedObjects")]
        [HarmonyPostfix]
        static void CreatePlayers(NetworkSceneManager __instance)
        {
            try
            {
                Plugin.Log.LogMessage("PopulateScenePlacedObjects");

                var playerCount = ServerConfiguration.Instance.Lobby.LobbySize;

                Plugin.Log.LogInfo("Extending Lobby. Adding new player objects for a lobby size of " + playerCount);
                Dictionary<uint, Dictionary<int, NetworkObject>> scenePlacedObjects = (Dictionary<uint, Dictionary<int, NetworkObject>>)NetworkSceneManagerScenePlacedObjects.GetValue(__instance);
                var startOfRound = global::StartOfRound.Instance;
                var soundManager = global::SoundManager.Instance;

                if (startOfRound.allPlayerScripts.Length < playerCount)
                {
                    var oldLength = startOfRound.allPlayerObjects.Length;

                    var newPlayerScriptsArray = new GameNetcodeStuff.PlayerControllerB[playerCount];
                    var newPlayerObjectsArray = new GameObject[playerCount];
                    var newPlayerStatsArray = new PlayerStats[playerCount];
                    var newPlayerSpawnPositionsArray = new Transform[playerCount];

                    Array.Copy(startOfRound.allPlayerScripts, 0, newPlayerScriptsArray, 0, startOfRound.allPlayerScripts.Length);
                    Array.Copy(startOfRound.allPlayerObjects, 0, newPlayerObjectsArray, 0, startOfRound.allPlayerObjects.Length);
                    Array.Copy(startOfRound.gameStats.allPlayerStats, 0, newPlayerStatsArray, 0, startOfRound.gameStats.allPlayerStats.Length);
                    Array.Copy(startOfRound.playerSpawnPositions, 0, newPlayerSpawnPositionsArray, 0, startOfRound.playerSpawnPositions.Length);

                    var prefab = startOfRound.allPlayerObjects[startOfRound.allPlayerObjects.Length - 1];
                    uint hash = 12345;
                    prefab.SetActive(false);

                    var setNetwork = (uint hash, NetworkObject networkObject) =>
                    {
                        while (scenePlacedObjects.ContainsKey(hash))
                            hash++;
                        if (!scenePlacedObjects.ContainsKey(hash))
                            scenePlacedObjects.Add(hash, new Dictionary<int, NetworkObject>());
                        if (!scenePlacedObjects[hash].ContainsKey(networkObject.gameObject.scene.handle))
                            scenePlacedObjects[hash].Add(networkObject.gameObject.scene.handle, networkObject);
                        networkObject.GlobalObjectIdHash = hash;
                        return hash + 1;
                    };

                    for (var i = oldLength; i < playerCount; i++)
                    {
                        var newObject = GameObject.Instantiate(prefab, prefab.transform.parent);
                        var newScript = newObject.GetComponent<GameNetcodeStuff.PlayerControllerB>();
                        var networkObject = newObject.GetComponent<NetworkObject>();
                        var physicsNetworkObject = newObject.transform.Find("PlayerPhysicsBox").GetComponent<NetworkObject>();
                        var localItemNetworkObject = newObject.transform.Find("ScavengerModel/metarig/ScavengerModelArmsOnly/metarig/spine.003/shoulder.R/arm.R_upper/arm.R_lower/hand.R/LocalItemHolder").GetComponent<NetworkObject>();
                        var serverItemNetworkObject = newObject.transform.Find("ScavengerModel/metarig/spine/spine.001/spine.002/spine.003/shoulder.R/arm.R_upper/arm.R_lower/hand.R/ServerItemHolder").GetComponent<NetworkObject>();

                        hash = setNetwork(hash, networkObject);
                        hash = setNetwork(hash, physicsNetworkObject);
                        hash = setNetwork(hash, localItemNetworkObject);
                        hash = setNetwork(hash, serverItemNetworkObject);

                        newObject.name = "Player (" + i + ")";
                        newScript.playerClientId = (ulong)i;
                        newScript.playerUsername = "Player #" + (i + 1);

                        newPlayerObjectsArray[i] = newObject;
                        newPlayerScriptsArray[i] = newScript;
                        newPlayerStatsArray[i] = new PlayerStats();
                        newPlayerSpawnPositionsArray[i] = newPlayerSpawnPositionsArray[3];
                        global::StartOfRound.Instance.mapScreen.radarTargets.Add(new TransformAndName(newObject.transform, newScript.playerUsername));

                        hash += 4;
                    }
                    prefab.SetActive(true);

                    startOfRound.allPlayerScripts = newPlayerScriptsArray;
                    startOfRound.allPlayerObjects = newPlayerObjectsArray;
                    startOfRound.playerSpawnPositions = newPlayerSpawnPositionsArray;
                    startOfRound.gameStats.allPlayerStats = newPlayerStatsArray;

                    for (var i = 0; i < playerCount; i++)
                    {
                        var p = Game.Player.GetPlayer(startOfRound.allPlayerScripts[i]);
                        if (p.Username == null || p.Username == "")
                            p.Username = startOfRound.allPlayerScripts[i].playerUsername;
                        p.RadarTarget = global::StartOfRound.Instance.mapScreen.radarTargets[i];
                        startOfRound.allPlayerObjects[i].SetActive(true);
                    }

                    if (NetworkManager.Singleton.IsServer)
                        startOfRound.StartCoroutine(DeactivatePlayerObjectsNextFrame());
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while extending Lobby:");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while extending Lobby.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }

        [HarmonyPatch(typeof(ForestGiantAI), "Start")]
        [HarmonyPrefix]
        static void ForestGiantStart(ForestGiantAI __instance)
        {
            try
            {
                var playerCount = ServerConfiguration.Instance.Lobby.LobbySize;
                Plugin.Log.LogInfo("Extending ForestGiantAI->playerSteahlMeters for " + playerCount + " players.");
                __instance.playerStealthMeters = new float[playerCount];
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while extending ForestGiantAI:");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while extending ForestGiantAI.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }

        private static FieldInfo CrawlerAINearPlayerColliders = typeof(CrawlerAI).GetField("nearPlayerColliders", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        [HarmonyPatch(typeof(CrawlerAI), "Start")]
        [HarmonyPostfix]
        static void CrawlerAIStart(CrawlerAI __instance)
        {
            try
            {
                var playerCount = ServerConfiguration.Instance.Lobby.LobbySize;
                Plugin.Log.LogInfo("Extending CrawlerAI->nearPlayerColliders for " + playerCount + " players.");
                CrawlerAINearPlayerColliders.SetValue(__instance, new Collider[playerCount]);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while extending CrawlerAI:");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while extending CrawlerAI.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }

        private static FieldInfo ShipTeleporterPlayersBeingTeleported = typeof(ShipTeleporter).GetField("playersBeingTeleported", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        [HarmonyPatch(typeof(ShipTeleporter), "Awake")]
        [HarmonyPostfix]
        static void ShipTeleporterAwake(ShipTeleporter __instance)
        {
            try
            {
                var playerCount = ServerConfiguration.Instance.Lobby.LobbySize;
                Plugin.Log.LogInfo("Extending ShipTeleporter->playersBeingTeleported for " + playerCount + " players.");
                var newValue = new int[playerCount];
                for (var i = 0; i < newValue.Length; i++)
                    newValue[i] = -1;
                ShipTeleporterPlayersBeingTeleported.SetValue(__instance, newValue);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while extending ShipTeleporter:");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while extending ShipTeleporter.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }

        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPrefix]
        static void ReworkQuickMenuManagerUI(QuickMenuManager __instance)
        {
            try
            {
                var playerCount = ServerConfiguration.Instance.Lobby.LobbySize;
                if (__instance.playerListSlots.Length < playerCount)
                {
                    Plugin.Log.LogInfo("Extending QuickMenuManager player slots for " + playerCount + " players.");

                    var newPlayerListSlots = new PlayerListSlot[playerCount];

                    var imageTransform = __instance.playerListPanel.transform.GetChild(0);

                    var scrollArea = GameObject.Instantiate(ScrollViewPrefab, imageTransform);
                    var scrollAreaRect = scrollArea.transform.GetComponent<RectTransform>();
                    scrollAreaRect.anchorMin = new Vector2(0f, 0f);
                    scrollAreaRect.anchorMax = new Vector2(1f, 1f);
                    scrollAreaRect.pivot = new Vector2(0f, 1f);
                    scrollAreaRect.offsetMax = new Vector2(-1f, -40f);
                    scrollAreaRect.offsetMin = new Vector2(0f, 1.5f);
                    scrollArea.GetComponent<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;

                    var content = scrollArea.transform.GetChild(0).GetChild(0).gameObject;
                    var verticalLayout = content.AddComponent<VerticalLayoutGroup>();
                    verticalLayout.childAlignment = TextAnchor.UpperLeft;
                    verticalLayout.childControlHeight = false;
                    verticalLayout.childControlWidth = false;
                    verticalLayout.childForceExpandHeight = false;
                    verticalLayout.childForceExpandWidth = false;
                    verticalLayout.padding = new RectOffset(10, 0, 50, -30);

                    var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
                    contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

                    for (var i = 0; i < playerCount; i++)
                    {
                        var playerListEntry = GameObject.Instantiate(imageTransform.GetChild(1).gameObject, content.transform).transform;
                        playerListEntry.parent = content.transform;
                        var playerListRect = playerListEntry.GetComponent<RectTransform>();
                        playerListRect.sizeDelta = new Vector2(200f, 50f);
                        playerListRect.pivot = new Vector2(0f, 0f);

                        var kickButton = playerListEntry.GetChild(3).gameObject;
                        var playerNum = i;
                        kickButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                        {
                            __instance.KickUserFromServer(playerNum);
                        }));
                        var profileIcon = playerListEntry.GetChild(2).gameObject;
                        profileIcon.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                        {
                            __instance.OpenUserSteamProfile(playerNum);
                        }));

                        newPlayerListSlots[i] = new PlayerListSlot()
                        {
                            slotContainer = playerListEntry.gameObject,
                            volumeSliderContainer = playerListEntry.GetChild(1).gameObject,
                            KickUserButton = playerListEntry.GetChild(3).gameObject,
                            usernameHeader = playerListEntry.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>(),
                            volumeSlider = playerListEntry.GetChild(1).GetChild(1).GetComponent<UnityEngine.UI.Slider>(),
                            isConnected = __instance.playerListSlots.Length > i ? __instance.playerListSlots[i].isConnected : false,
                            playerSteamId = __instance.playerListSlots.Length > i ? __instance.playerListSlots[i].playerSteamId : 0,
                        };
                    }

                    for (var i = 0; i < imageTransform.childCount; i++)
                    {
                        var c = imageTransform.GetChild(i);
                        if (c.name.StartsWith("PlayerListSlot"))
                            c.gameObject.SetActive(false);
                    }

                    __instance.playerListSlots = newPlayerListSlots;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while extending QuickMenuManager:");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while extending QuickMenuManager.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }

        public static int NextPlayerSlot = 1;

        [HarmonyPatch(typeof(global::HUDManager), "Start")]
        [HarmonyPrefix]
        static void FixEndscreen(global::HUDManager __instance)
        {
            try
            {
                var playerCount = ServerConfiguration.Instance.Lobby.LobbySize;
                if (__instance.statsUIElements.playerStates.Length < playerCount)
                {
                    Plugin.Log.LogInfo("Extending HUDManager stats for " + playerCount + " players.");
                    var newPlayerStates = new UnityEngine.UI.Image[playerCount];
                    var newPlayerNotesText = new TextMeshProUGUI[playerCount];
                    var newPlayerNamesText = new TextMeshProUGUI[playerCount];

                    Array.Copy(__instance.statsUIElements.playerStates, 0, newPlayerStates, 0, __instance.statsUIElements.playerStates.Length);
                    Array.Copy(__instance.statsUIElements.playerNotesText, 0, newPlayerNotesText, 0, __instance.statsUIElements.playerNotesText.Length);
                    Array.Copy(__instance.statsUIElements.playerNamesText, 0, newPlayerNamesText, 0, __instance.statsUIElements.playerNamesText.Length);

                    for (var i = __instance.statsUIElements.playerStates.Length; i < playerCount; i++)
                    {
                        GameObject go = new GameObject("dummyImage" + i);
                        go.transform.parent = __instance.endgameStatsAnimator.transform;
                        var img = go.AddComponent<UnityEngine.UI.Image>();
                        go.SetActive(false);

                        go = new GameObject("dummyNotes" + i);
                        go.transform.parent = __instance.endgameStatsAnimator.transform;
                        var notes = go.AddComponent<TextMeshProUGUI>();
                        go.SetActive(false);

                        go = new GameObject("dummyName" + i);
                        go.transform.parent = __instance.endgameStatsAnimator.transform;
                        var name = go.AddComponent<TextMeshProUGUI>();
                        go.SetActive(false);

                        newPlayerStates[i] = img;
                        newPlayerNamesText[i] = name;
                        newPlayerNotesText[i] = notes;
                    }

                    __instance.statsUIElements.playerNotesText = newPlayerNotesText;
                    __instance.statsUIElements.playerStates = newPlayerStates;
                    __instance.statsUIElements.playerNamesText = newPlayerNamesText;

                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while extending HUDManager to lobby size.");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while extending HUDManager.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }

        [HarmonyPatch(typeof(global::InteractTrigger), "StopSpecialAnimation")]
        [HarmonyPrefix]
        static bool InteractTriggerStopSpecialAnimation(global::InteractTrigger __instance)
        {
            /** FIXING A BUG IN THE GAME **/
            if (__instance.lockedPlayer != null)
            {
                var p = __instance.lockedPlayer.GetComponent<global::GameNetcodeStuff.PlayerControllerB>();
                if (p != null && p.isPlayerDead)
                    return false;
            }
            return true;
        }


        #region Player object activation
        public static IEnumerator DeactivatePlayerObjectsNextFrame()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            ActivatePlayerObjects(global::StartOfRound.Instance, new ulong[] { 0 });
        }

        [HarmonyPatch(typeof(global::HUDManager), "FillEndGameStats")]
        [HarmonyPrefix]
        static void HUDManagerFillEndGameStats(global::HUDManager __instance)
        {
            /*for (var i = 0; i < __instance.playersManager.allPlayerScripts.Length; i++)
                if (__instance.playersManager.allPlayerScripts[i].isPlayerDead && LateJoiners.Contains((int)__instance.playersManager.allPlayerScripts[i].playerClientId))
                    __instance.playersManager.allPlayerScripts[i].isPlayerDead = false;*/
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "DisablePlayerModel")]
        [HarmonyPostfix]
        static void DisablePlayerModel(GameNetcodeStuff.PlayerControllerB __instance, GameObject playerObject, bool enable = false, bool disableLocalArms = false)
        {
            if (!enable)
                Game.Player.GetPlayer(__instance).HideCosmetics();
            else
                Game.Player.GetPlayer(__instance).ShowCosmetics();
        }

        [HarmonyPatch(typeof(global::GameNetcodeStuff.PlayerControllerB), "SendNewPlayerValuesClientRpc")]
        [HarmonyPostfix]
        static void SendNewPlayerValuesClientRpc(global::GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (!global::GameNetworkManager.Instance.disableSteam)
            {
                for (var i = 0; i < global::StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
                {
                    var target = global::StartOfRound.Instance.mapScreen.radarTargets[i];
                    for (var j = 0; j < global::StartOfRound.Instance.allPlayerObjects.Length; j++)
                    {
                        if (target.transform == global::StartOfRound.Instance.allPlayerObjects[j].transform)
                            target.name = global::StartOfRound.Instance.allPlayerScripts[j].playerUsername ?? "";
                    }
                }
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "OnPlayerConnectedClientRpc")]
        [HarmonyPostfix]
        static void OnPlayerConnectedClientRpc(global::StartOfRound __instance, ulong clientId, int connectedPlayers, ulong[] connectedPlayerIdsOrdered, int assignedPlayerObjectId, int serverMoneyAmount, int levelID, int profitQuota, int timeUntilDeadline, int quotaFulfilled, int randomSeed)
        {
            ActivatePlayerObjects(__instance, connectedPlayerIdsOrdered);

            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                if (!Network.Manager.Lobby.IsJoinable)
                    __instance.allPlayerScripts[assignedPlayerObjectId].DisablePlayerModel(__instance.allPlayerScripts[assignedPlayerObjectId].gameObject, false, true);
                else
                    __instance.allPlayerScripts[assignedPlayerObjectId].DisablePlayerModel(__instance.allPlayerScripts[assignedPlayerObjectId].gameObject, true, true);
            }
            // keep players dead
            __instance.allPlayerScripts[assignedPlayerObjectId].disconnectedMidGame = false;
            int livingPlayers = 0;
            for (int j = 0; j < __instance.allPlayerScripts.Length; j++)
            {
                if (j == 0)
                {
                    if (!__instance.allPlayerScripts[j].isPlayerDead)
                        livingPlayers++;
                }
                else
                {
                    var controlled = j == assignedPlayerObjectId || (!__instance.allPlayerScripts[j].IsOwnedByServer && !__instance.allPlayerScripts[j].isPlayerDead && connectedPlayerIdsOrdered[j] != 999);
                    __instance.allPlayerScripts[j].isPlayerControlled = controlled;
                    if (connectedPlayerIdsOrdered[j] != 999)
                        __instance.allPlayerScripts[j].actualClientId = connectedPlayerIdsOrdered[j];
                    if (controlled) livingPlayers++;
                }
            }
            if (global::GameNetworkManager.Instance.disableSteam)
            {
                for (var i = 0; i < global::StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
                {
                    var target = global::StartOfRound.Instance.mapScreen.radarTargets[i];
                    for (var j = 0; j < global::StartOfRound.Instance.allPlayerObjects.Length; j++)
                    {
                        if (target.transform == global::StartOfRound.Instance.allPlayerObjects[j].transform)
                            target.name = global::StartOfRound.Instance.allPlayerScripts[j].playerUsername ?? "";
                    }
                }
            }

            __instance.livingPlayers = livingPlayers;
            for (var i = 0; i < global::StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                var p = Game.Player.GetPlayer(global::StartOfRound.Instance.allPlayerScripts[i]);
                if (p != null)
                {
                    p.SetCosmetics(p.Cosmetics, false);
                }
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "OnPlayerDC")]
        [HarmonyPostfix]
        static void OnPlayerDC(global::StartOfRound __instance, int playerObjectNumber, ulong clientId)
        {
            try
            {
                Plugin.Log.LogInfo("Player (" + playerObjectNumber + ") disconnected. Changing activation state of player objects.");
                Network.Manager.Lobby.PlayerDisconnected(playerObjectNumber);
                
                var playerIds = __instance.ClientPlayerList.Values.ToList();
                var connectedPlayerIds = new ulong[ServerConfiguration.Instance.Lobby.LobbySize];
                for (var i = 0; i < connectedPlayerIds.Length; i++)
                    connectedPlayerIds[i] = (ulong)(playerIds.Contains(i) ? i : 999);
                ActivatePlayerObjects(__instance, connectedPlayerIds);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while activating player objects:");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while activating player objects.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }


        protected static List<ulong> ConnectedIDs = new List<ulong>();
        private static void ActivatePlayerObjects(global::StartOfRound __instance, ulong[] connectedPlayerIdsOrdered)
        {
            try
            {
                ConnectedIDs = new List<ulong>();

                Plugin.Log.LogInfo("Activating necessary player objects for next player to join.");
                bool first = true;
                for (var i = 0; i < __instance.allPlayerObjects.Length; i++)
                {
                    if (connectedPlayerIdsOrdered.Length > i && connectedPlayerIdsOrdered[i] != 999)
                    {
                        ConnectedIDs.Add((ulong)i);
                        if (!__instance.allPlayerObjects[i].activeSelf)
                            __instance.allPlayerObjects[i].SetActive(true);
                    }
                    else if (first)
                    {
                        // next available player should also be activated.
                        if (!__instance.allPlayerObjects[i].activeSelf)
                            __instance.allPlayerObjects[i].SetActive(true);
                        Plugin.Log.LogInfo("Next player object with ID " + __instance.allPlayerScripts[i].playerClientId + " activated.");
                        first = false;
                    }
                    else if (__instance.allPlayerObjects[i].activeSelf) 
                        __instance.allPlayerObjects[i].SetActive(false);
                }
                if (first)
                {
                    Plugin.Log.LogInfo("No remaining free player objects. Lobby is full!");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while activating player objects:");
                Plugin.Log.LogError(e);
                global::GameNetworkManager.Instance.disconnectionReasonMessage = "Error while activating player objects.";
                global::GameNetworkManager.Instance.Disconnect();
            }
        }
        #endregion
    }
}
