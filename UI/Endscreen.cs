using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.UI
{
    [LoadAssets]
    [HarmonyPatch]
    public class Endscreen
    {
        private static GameObject Container;
        public static Endscreen Instance;
        private static GameObject PerformanceReportPrefab;
        private static GameObject DeadContainerPrefab;
        private static GameObject MissingContainerPrefab;
        private static GameObject NoteContainerPrefab;
        private static Transform AllDead;
        private static Transform PlayerNoteContainer;
        private static Transform DeadNoteContainer;
        private static Transform MissingTitle;
        private static Transform MissingScrollBox;
        private static Transform MissingNoteContainer;
        private static Transform CollectedLabel;
        private static Transform CollectedLine;
        private static TextMeshProUGUI CollectedText;
        private static TextMeshProUGUI TotalText;
        private static Transform ScrapLost;
        private static TextMeshProUGUI ScrapLostText;
        private static TextMeshProUGUI GradeText;
        private static int CollectedScrap;
        private static int TotalScrap;
        private static string Grade;
        private static bool AreAllDead;

        private static Dictionary<ulong, string> CachedDeathReasons = new();
        [HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
        [HarmonyPrefix]
        public static void GetCoronerDeathMessages(HUDManager __instance)
        {
            CachedDeathReasons.Clear();
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.elitemastereric.coroner"))
            {
                for (int i = 0; i < global::StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    var controller = global::StartOfRound.Instance.allPlayerScripts[i];
                    try
                    {
                        CachedDeathReasons[controller.playerClientId] = CoronerDeathReason(controller);
                    }
                    catch (Exception e) { }
                }
            }
        }

        public static void LoadAssets(AssetBundle assets)
        {
            PerformanceReportPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/PerformanceReport.prefab");
            DeadContainerPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/DeadContainer.prefab");
            MissingContainerPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/MissingContainer.prefab");
            NoteContainerPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/NoteContainer.prefab");
        }

        private static string GetDeathReason(GameNetcodeStuff.PlayerControllerB controller)
        {
            if (Network.Manager.Lobby.LateJoiners.Contains((int)controller.playerClientId))
                return "Joined late";
            if (CachedDeathReasons.ContainsKey(controller.playerClientId))
                return CachedDeathReasons[controller.playerClientId];
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.elitemastereric.coroner"))
            {
                try
                {
                    return CoronerDeathReason(controller);
                }
                catch (Exception e) { }
            }
            switch (controller.causeOfDeath)
            {
                case CauseOfDeath.Unknown:
                    return "Uncertain circumstances";
                case CauseOfDeath.Drowning:
                    return "Drowned";
                case CauseOfDeath.Strangulation:
                    return "Strangulated";
                case CauseOfDeath.Suffocation:
                    return "Suffocation";
                case CauseOfDeath.Mauling:
                    return "Mauling";
                case CauseOfDeath.Blast:
                    return "Blast";
                case CauseOfDeath.Crushing:
                    return "Crushed";
                case CauseOfDeath.Electrocution:
                    return "Electrocuted";
                case CauseOfDeath.Gravity:
                    return "Gravity";
                case CauseOfDeath.Gunshots:
                    return "Gunshots";
                case CauseOfDeath.Kicking:
                    return "Kicked";
                default:
                    return "Uncertain circumstances";
            }
        }

        private static string CoronerDeathReason(GameNetcodeStuff.PlayerControllerB controller)
        {
            return Coroner.AdvancedDeathTracker.StringifyCauseOfDeath(Coroner.AdvancedDeathTracker.GetCauseOfDeath(controller));
        }

        public static void Open()
        {
            try
            {
                var t = HUDManager.Instance.endgameStatsAnimator.gameObject.transform;
                for (var i = 0; i < t.childCount; i++)
                {
                    var ch = t.GetChild(i);
                    if (ch.name == "Text")
                        ch.gameObject.SetActive(false);
                    if (ch.name == "BGBoxes" || ch.name == "Lines")
                        GameObject.Destroy(ch.gameObject);
                }
                bool somebodyMissing = false;
                var c = PlayerNoteContainer.childCount;
                for (var i = c - 1; i >= 0; i--)
                    GameObject.Destroy(PlayerNoteContainer.GetChild(i).gameObject);
                c = DeadNoteContainer.childCount;
                for (var i = c - 1; i >= 0; i--)
                    GameObject.Destroy(DeadNoteContainer.GetChild(i).gameObject);
                c = MissingNoteContainer.childCount;
                for (var i = c - 1; i >= 0; i--)
                    GameObject.Destroy(MissingNoteContainer.GetChild(i).gameObject);

                for (int i = 0; i < global::StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    var player = global::StartOfRound.Instance.allPlayerScripts[i];
                    if (!player.disconnectedMidGame)
                    {
                        var username = HUDManager.Instance.statsUIElements.playerNamesText[i].text;
                        Texture2D avatar = Patches.PlayerControllerB.GetAvatar(player);
                        var notes = HUDManager.Instance.statsUIElements.playerNotesText[i].text;
                        if (notes.StartsWith("Notes:")) notes = notes.Substring(6);
                        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.elitemastereric.coroner"))
                        {
                            var ind = notes.IndexOf("Cause of Death");
                            if (ind > -1)
                                notes = notes.Substring(0, ind);
                        }

                        notes = notes.Trim();
                        string deathReason = GetDeathReason(player);
                        if (notes != "")
                            AddPlayerNote(avatar, username, notes);
                        if (HUDManager.Instance.statsUIElements.playerStates[i].sprite == HUDManager.Instance.statsUIElements.deceasedIcon)
                            AddDeceasedNote(avatar, username, deathReason);
                        if (HUDManager.Instance.statsUIElements.playerStates[i].sprite == HUDManager.Instance.statsUIElements.missingIcon)
                        {
                            somebodyMissing = true;
                            AddMissingNote(avatar, username);
                        }
                    }
                }
                MissingTitle.gameObject.SetActive(somebodyMissing);
                MissingScrollBox.gameObject.SetActive(somebodyMissing);
                CollectedScrap = (int)RoundManager.Instance.scrapCollectedInLevel;
                TotalScrap = (int)RoundManager.Instance.totalScrapValueInLevel;
                //int.TryParse(HUDManager.Instance.statsUIElements.quotaNumerator.text, out CollectedScrap);
                //int.TryParse(HUDManager.Instance.statsUIElements.quotaDenominator.text, out TotalScrap);

                AreAllDead = HUDManager.Instance.statsUIElements.allPlayersDeadOverlay.enabled;
                AllDead.gameObject.SetActive(AreAllDead);

                CollectedText.text = "";
                TotalText.text = "";

                CollectedText.gameObject.SetActive(true);
                TotalText.gameObject.SetActive(true);
                CollectedLine.gameObject.SetActive(true);
                CollectedLabel.gameObject.SetActive(true);
                ScrapLost.gameObject.SetActive(false);

                Grade = HUDManager.Instance.statsUIElements.gradeLetter.text;
                GradeText.text = "";

                Container.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(Container.GetComponent<RectTransform>());
                HUDManager.Instance.StartCoroutine(AnimateMenu());
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error occured while opening end screen!");
                Plugin.Log.LogError(e);
            }
        }

        private static IEnumerator AnimateMenu()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            yield return new WaitForSeconds(1f);
            float p = 0f;
            TotalText.text = TotalScrap + "";
            while (p < 1f)
            {
                CollectedText.text = CollectedScrap + "";
                p += 0.05f;
                yield return new WaitForEndOfFrame();
            }

            if (AreAllDead)
            {
                yield return new WaitForSeconds(1f);
                CollectedText.gameObject.SetActive(false);
                TotalText.gameObject.SetActive(false);
                CollectedLine.gameObject.SetActive(false);
                CollectedLabel.gameObject.SetActive(false);
                ScrapLostText.text = "Lost " + (Mathf.Round((1f - Perks.GetMultiplier("SaveLoot")) * 1000f) / 10f) + "% scrap";
                ScrapLost.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(1f);
            GradeText.text = Grade;

            yield return new WaitForSeconds(5.5f - (AreAllDead ? 1f : 0f));
            Container.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

        public static Texture2D GetTextureFromImage(Steamworks.Data.Image? image)
        {
            if (image.HasValue)
            {
                Texture2D texture2D = new Texture2D((int)image.Value.Width, (int)image.Value.Height);
                for (int i = 0; i < image.Value.Width; i++)
                {
                    for (int j = 0; j < image.Value.Height; j++)
                    {
                        Steamworks.Data.Color pixel = image.Value.GetPixel(i, j);
                        texture2D.SetPixel(i, (int)image.Value.Height - j, new UnityEngine.Color((float)(int)pixel.r / 255f, (float)(int)pixel.g / 255f, (float)(int)pixel.b / 255f, (float)(int)pixel.a / 255f));
                    }
                }
                texture2D.Apply();
                return texture2D;
            }
            return null;
        }

        private static void AddPlayerNote(Texture2D image, string username, string notes)
        {
            var note = GameObject.Instantiate(NoteContainerPrefab, PlayerNoteContainer);
            var playerNameContainer = note.transform.GetChild(0);
            var playerImage = playerNameContainer.GetChild(0);
            if (image != null)
                playerImage.GetComponent<RawImage>().texture = image;
            else
                playerImage.gameObject.SetActive(false);
            playerNameContainer.GetChild(1).GetComponent<TextMeshProUGUI>().text = username;
            note.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = notes;

            note.transform.parent = PlayerNoteContainer;
            note.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private static void AddDeceasedNote(Texture2D image, string username, string deathReason)
        {
            var note = GameObject.Instantiate(DeadContainerPrefab, PlayerNoteContainer);
            var playerNameContainer = note.transform.GetChild(0);
            var playerImage = playerNameContainer.GetChild(0);
            if (image != null)
                playerImage.GetComponent<RawImage>().texture = image;
            else
                playerImage.gameObject.SetActive(false);
            playerNameContainer.GetChild(1).GetComponent<TextMeshProUGUI>().text = username;
            note.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = deathReason;
            //note.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = notes;

            note.transform.parent = DeadNoteContainer;
            note.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private static void AddMissingNote(Texture2D image, string username)
        {
            var note = GameObject.Instantiate(MissingContainerPrefab, PlayerNoteContainer);
            var playerNameContainer = note.transform.GetChild(0);
            var playerImage = playerNameContainer.GetChild(0);
            if (image != null)
                playerImage.GetComponent<RawImage>().texture = image;
            else
                playerImage.gameObject.SetActive(false);
            playerNameContainer.GetChild(1).GetComponent<TextMeshProUGUI>().text = username;
            //note.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = notes;

            note.transform.parent = MissingNoteContainer;
            note.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPostfix]
        public static void Attach(HUDManager __instance)
        {
            Container = GameObject.Instantiate(PerformanceReportPrefab, __instance.endgameStatsAnimator.transform.parent);
            Container.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Container.GetComponent<RectTransform>().sizeDelta = new Vector2(823f, 717f);

            var boxes = Container.transform.GetChild(1);
            var playerNotes = boxes.GetChild(0);
            var deadPlayers = boxes.GetChild(1);
            var bottom = Container.transform.GetChild(2);
            AllDead = playerNotes.GetChild(1);
            PlayerNoteContainer = playerNotes.GetChild(2).GetChild(0).GetChild(0);
            DeadNoteContainer = deadPlayers.GetChild(1).GetChild(0).GetChild(0);
            MissingTitle = deadPlayers.GetChild(2);
            MissingScrollBox = deadPlayers.GetChild(3);
            MissingNoteContainer = MissingScrollBox.GetChild(0).GetChild(0);
            var collectedBox = bottom.GetChild(0);
            CollectedLabel = collectedBox.GetChild(0);
            CollectedText = collectedBox.GetChild(1).GetComponent<TextMeshProUGUI>();
            CollectedLine = collectedBox.GetChild(2);
            TotalText = collectedBox.GetChild(3).GetComponent<TextMeshProUGUI>();
            ScrapLost = collectedBox.GetChild(4);
            ScrapLostText = ScrapLost.GetChild(0).GetComponent<TextMeshProUGUI>();
            GradeText = bottom.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

            Container.SetActive(false);
        }
        
        private static MethodInfo OpenEndgameMenuMethod = typeof(Endscreen).GetMethod("Open", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        [HarmonyPatch(typeof(global::StartOfRound), "EndOfGame", MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchHUDManagerEndOfGame(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->EndOfGame...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Ldstr && inst[i].operand.ToString() == "displayStats"))
                {
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Call, OpenEndgameMenuMethod));
                    i++;
                }
            }
            Plugin.Log.LogDebug("Patched StartOfRound->EndOfGame!");
            return inst.AsEnumerable();
        }
    }
}
