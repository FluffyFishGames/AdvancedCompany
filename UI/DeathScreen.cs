using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.UI
{
    [HarmonyPatch]
    [LoadAssets]
    public class DeathScreen
    {
        private static GameObject PlayerBoxPrefab;
        private static GridLayoutGroup GridLayout;
        public static void LoadAssets(AssetBundle assets)
        {
            PlayerBoxPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/PlayerBox.prefab");
        }

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPrefix]
        public static void Init(HUDManager __instance)
        {
            var boxesContainer = __instance.SpectateBoxesContainer.GetComponent<RectTransform>();
            var spectateUI = boxesContainer.parent.GetComponent<RectTransform>();
            var deathScreen = spectateUI.parent.GetComponent<RectTransform>();

            spectateUI.anchorMin = new Vector2(0f, 0f);
            spectateUI.anchorMax = new Vector2(1f, 1f);
            spectateUI.offsetMin = new Vector2(0f, 0f);
            spectateUI.offsetMax = new Vector2(0f, 0f);

            deathScreen.anchorMin = new Vector2(0f, 0f);
            deathScreen.anchorMax = new Vector2(1f, 1f);
            deathScreen.offsetMin = new Vector2(0f, 0f);
            deathScreen.offsetMax = new Vector2(0f, 0f);

            boxesContainer.anchorMin = new Vector2(0f, 0f);
            boxesContainer.anchorMax = new Vector2(1f, 0f);
            boxesContainer.pivot = new Vector2(0f, 0f);
            boxesContainer.offsetMin = new Vector2(15f, 15f);
            boxesContainer.offsetMax = new Vector2(-15f, 115f);

            GridLayout = boxesContainer.gameObject.AddComponent<GridLayoutGroup>();
            GridLayout.spacing = new Vector2(5f, 0f);
            GridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            GridLayout.constraintCount = 1;
            GridLayout.startCorner = GridLayoutGroup.Corner.LowerLeft;
            GridLayout.childAlignment = TextAnchor.LowerLeft;
        }

        public class SpectatorBox
        {
            public PlayerControllerB Player;
            public GameObject Container;
            public RawImage Avatar;
            public Animator Animator;
        }

        private static Dictionary<ulong, SpectatorBox> Spectators = new();

        [HarmonyPatch(typeof(HUDManager), "RemoveSpectateUI")]
        [HarmonyPrefix]
        public static void RemoveSpectateUI()
        {
            foreach (var kv in Spectators)
                GameObject.Destroy(kv.Value.Container);
            Spectators.Clear();
        }


        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPrefix]
        private static void Update()
        {
            if (StartOfRound.Instance.voiceChatModule == null)
            {
                return;
            }
            bool refreshed = false;
            var remove = new List<ulong>();
            foreach (var kv in Spectators)
            {
                if (kv.Value.Container == null)
                {
                    remove.Add(kv.Key);
                    continue;
                }
                PlayerControllerB player = kv.Value.Player;
                if (!player.isPlayerControlled && !player.isPlayerDead)
                    continue;

                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    if (!string.IsNullOrEmpty(StartOfRound.Instance.voiceChatModule.LocalPlayerName))
                    {
                        VoicePlayerState voicePlayerState = StartOfRound.Instance.voiceChatModule.FindPlayer(StartOfRound.Instance.voiceChatModule.LocalPlayerName);
                        if (voicePlayerState != null)
                        {
                            kv.Value.Animator.SetFloat("Volume", StartOfRound.Instance.voiceChatModule.IsMuted || !voicePlayerState.IsSpeaking || voicePlayerState.Amplitude < 0.005f ? 0f : voicePlayerState.Amplitude * 2f);
                        }
                    }
                }
                else if (player.voicePlayerState == null)
                {
                    if (!refreshed)
                    {
                        refreshed = true;
                        StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
                    }
                }
                else
                {
                    VoicePlayerState voicePlayerState = player.voicePlayerState;
                    kv.Value.Animator.SetFloat("Volume", !voicePlayerState.IsSpeaking || voicePlayerState.IsLocallyMuted || voicePlayerState.Amplitude < 0.005f ? 0f : (voicePlayerState.Amplitude / Mathf.Max(voicePlayerState.Volume, 0.01f)) * 2f);
                }
            }
            foreach (var r in remove)
                Spectators.Remove(r);
            UpdateLayoutSize();
        }

        private static void UpdateLayoutSize()
        {
            var childs = GridLayout.transform.childCount - 4;
            var widthPerBox = GridLayout.GetComponent<RectTransform>().rect.width / (float)childs;
            widthPerBox = widthPerBox - 5f;
            if (widthPerBox > 70)
                widthPerBox = 70;
            GridLayout.cellSize = new Vector2(widthPerBox, widthPerBox);
        }

        [HarmonyPatch(typeof(HUDManager), "UpdateBoxesSpectateUI")]
        [HarmonyPrefix] 
        public static bool UpdateBoxes(HUDManager __instance)
        {
            PlayerControllerB playerScript;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                playerScript = StartOfRound.Instance.allPlayerScripts[i];
                if (!playerScript.isPlayerDead)
                {
                    if (playerScript.isPlayerControlled || !Spectators.ContainsKey(playerScript.playerClientId))
                        continue;
                    GameObject.Destroy(Spectators[playerScript.playerClientId].Container);
                    Spectators.Remove(playerScript.playerClientId);
                }
                else if (Spectators.ContainsKey(playerScript.playerClientId))
                {
                    if (Spectators[playerScript.playerClientId].Container == null)
                        Spectators.Remove(playerScript.playerClientId);
                    else if (!Spectators[playerScript.playerClientId].Container.activeSelf)
                        Spectators[playerScript.playerClientId].Container.SetActive(true);
                }
                else
                {
                    GameObject gameObject = GameObject.Instantiate(PlayerBoxPrefab, __instance.SpectateBoxesContainer, worldPositionStays: false);
                    gameObject.transform.localScale = Vector3.one;
                    gameObject.SetActive(true);

                    var spectatorBox = new SpectatorBox()
                    {
                        Container = gameObject,
                        Animator = gameObject.GetComponent<Animator>(),
                        Player = playerScript,
                        Avatar = gameObject.transform.GetChild(0).GetChild(2).GetComponent<RawImage>()
                    };
                    Spectators[playerScript.playerClientId] = spectatorBox;

                    if (!GameNetworkManager.Instance.disableSteam)
                    {
                        var avatar = Patches.PlayerControllerB.GetAvatar(playerScript);
                        if (avatar != null)
                            spectatorBox.Avatar.texture = Patches.PlayerControllerB.GetAvatar(playerScript);
                    }
                }
            }
            UpdateLayoutSize();

            return false;
        }
    }
}
