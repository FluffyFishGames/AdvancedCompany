using AdvancedCompany.Config;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Patches;
using AdvancedCompany.Terminal.Applications;
using HarmonyLib;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Game
{
    [Boot.Bootable]
    [HarmonyPatch]
    internal partial class Manager
    {
        private static bool FirstTerminalFrame = true;
        
        public static global::Terminal Terminal;
        public delegate void GameOver();
        public static GameOver OnGameOver;
        public delegate void GameStarted();
        public static GameStarted OnGameStarted;
        
        public static Dictionary<int, ItemData> AddedItems = new Dictionary<int, ItemData>();
        public static Dictionary<int, global::Item> ItemProperties = new Dictionary<int, global::Item>();
        public static Dictionary<int, ItemRuntimeData> RuntimeItems = new Dictionary<int, ItemRuntimeData>();

        public static void Boot()
        {
            Network.Manager.AddListener<AttractAllEnemies>((msg) =>
            {
                foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
                {
                    if (enemy.isOutside == msg.Inside && enemy.IsOwner)
                    {
                        enemy.StopSearch(enemy.currentSearch);
                        enemy.movingTowardsTargetPlayer = true;
                        enemy.targetPlayer = StartOfRound.Instance.allPlayerScripts[msg.PlayerNum];
                        /*
                        var node = enemy.ChooseClosestNodeToPosition(msg.Position);
                        enemy.SetDestinationToPosition(node.position);
                        enemy.moveTowardsDestination = true;
                        enemy.targetNode = node;*/
                    }
                }
            });
        }

        public static void AddItem(GrabbableObject grabbableObject)
        {
            grabbableObject.grabbable = true;
            grabbableObject.grabbableToEnemies = true;
            grabbableObject.useCooldown = 0.1f;

            var prefab = grabbableObject.gameObject;
            prefab.layer = 6;
            prefab.tag = "PhysicsProp";

            var itemData = prefab.GetComponent<ItemData>();
            if (itemData != null)
            {
                if (grabbableObject is Shovel s)
                    s.shovelAudio = s.GetComponent<AudioSource>();
                AddedItems.Add(itemData.ID, itemData);
                var itemProperties = ScriptableObject.CreateInstance<global::Item>();
                itemProperties.spawnPrefab = prefab;
                itemProperties.name = itemData.ItemName.Replace(" ", "");
                itemProperties.itemName = itemData.ItemName;
                itemProperties.itemId = itemData.ID;
                itemProperties.itemIcon = itemData.ItemIcon;
                itemProperties.creditsWorth = itemData.Price;
                itemProperties.highestSalePercentage = itemData.MaxDiscount;
                itemProperties.isConductiveMetal = itemData.IsConductive;
                itemProperties.allowDroppingAheadOfPlayer = true;
                itemProperties.twoHanded = itemData.IsTwoHanded;
                itemProperties.syncUseFunction = itemData.SyncUseFunction;
                itemProperties.syncInteractLRFunction = itemData.SyncInteractLRFunction;
                itemProperties.syncGrabFunction = itemData.SyncGrabFunction;
                itemProperties.syncDiscardFunction = itemData.SyncDiscardFunction;
                itemProperties.restingRotation = itemData.GroundRestingRotation;
                itemProperties.verticalOffset = itemData.GroundVerticalOffset;
                itemProperties.weight = 1f + ((float)itemData.Weight / 105f);
                itemProperties.saveItemVariable = itemData.HasSaveData;
                itemProperties.isScrap = itemData.IsScrap;
                itemProperties.dropSFX = itemData.DropSFX;
                itemProperties.pocketSFX = itemData.PocketSFX;
                itemProperties.grabSFX = itemData.GrabSFX;
                itemProperties.requiresBattery = itemData.UsesBattery;
                itemProperties.batteryUsage = itemData.BatteryUsage;

                if (itemProperties.requiresBattery)
                    grabbableObject.insertedBattery = new Battery(false, 1f);

                if (itemData.IsScrap)
                {
                    itemProperties.minValue = (int) (itemData.MinValue / 0.4f);
                    itemProperties.maxValue = (int) (itemData.MaxValue / 0.4f);

                    var scanNode = new GameObject("ScanNode");
                    scanNode.layer = 22;
                    var boxCollider = scanNode.AddComponent<BoxCollider>();
                    var scanProps = scanNode.AddComponent<ScanNodeProperties>();

                    scanProps.minRange = 1;
                    scanProps.maxRange = 13;
                    scanProps.headerText = itemData.ItemName;
                    scanProps.nodeType = 2;
                    scanProps.creatureScanID = -1;
                    scanNode.transform.parent = prefab.transform;
                    scanNode.transform.localPosition = Vector3.zero;

                    GameAssets.Scrap.Add(itemProperties.itemName, itemProperties);
                }

                if (itemData.HoldAnimation != null)
                {
                    if (itemData.HoldIsTwoHanded)
                        itemProperties.twoHandedAnimation = true;
                    //if (!Plugin.UseAnimationOverride)
                        itemProperties.grabAnim = itemData.HoldAnimation;
                    /*else
                        itemProperties.grabAnim = "HoldLungApparatice";*/
                }
                itemProperties.meshVariants = new Mesh[0];
                itemProperties.materialVariants = new Material[0];

                grabbableObject.itemProperties = itemProperties;

                ItemProperties.Add(itemData.ID, itemProperties);

                if (itemData.IsBuyable)
                {
                    var id = itemProperties.itemName.Replace(" ", "-");

                    var buyCompletedNode = ScriptableObject.CreateInstance<TerminalNode>();
                    buyCompletedNode.name = $"{id}BuyNode2";
                    buyCompletedNode.displayText = $"Ordered [variableAmount] {itemProperties.itemName}. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n";
                    buyCompletedNode.clearPreviousText = true;
                    buyCompletedNode.maxCharactersToType = 35;
                    buyCompletedNode.isConfirmationNode = false;
                    buyCompletedNode.itemCost = itemProperties.creditsWorth;
                    buyCompletedNode.playSyncedClip = 0;

                    var buyConfirmationNode = ScriptableObject.CreateInstance<TerminalNode>();
                    buyConfirmationNode.name = $"{id}BuyNode1";
                    buyConfirmationNode.displayText = $"You have requested to order {itemProperties.itemName}. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
                    buyConfirmationNode.clearPreviousText = true;
                    buyConfirmationNode.maxCharactersToType = 35;
                    buyConfirmationNode.isConfirmationNode = true;
                    buyConfirmationNode.overrideOptions = true;
                    buyConfirmationNode.itemCost = itemProperties.creditsWorth;

                    var keyword = TerminalKeyword.CreateInstance<TerminalKeyword>();
                    keyword.word = itemProperties.itemName.Replace(" ", "").ToLowerInvariant();
                    keyword.name = keyword.word + "Keyword";
                    keyword.isVerb = false;

                    var noun = new CompatibleNoun();
                    noun.noun = keyword;
                    noun.result = buyConfirmationNode;

                    RuntimeItems.Add(itemData.ID, new ItemRuntimeData()
                    {
                        Item = itemProperties,
                        BuyCompletedNode = buyCompletedNode,
                        BuyConfirmationNode = buyConfirmationNode,
                        Keyword = keyword,
                        Noun = noun
                    });
                }
                Utils.AddNetworkPrefab(prefab);
            }
            else
            {
                Plugin.Log.LogError("ItemData was missing on " + prefab.name);
            }
        }


        [HarmonyPatch(typeof(global::Terminal), "Update")]
        [HarmonyPostfix]
        private static void Update(global::Terminal __instance)
        {
            if (FirstTerminalFrame)
            {
                FirstTerminalFrame = false;
                Game.Manager.Items.SearchItems();
                Game.Manager.Moons.SearchMoons();
                Game.Manager.Enemies.SearchEnemies();
            }
        }

        [HarmonyPatch(typeof(global::Terminal), "Start")]
        [HarmonyPostfix]
        private static void SetTimeAndPlanetToSavedSettings(global::Terminal __instance)
        {
            if (global::StartOfRound.Instance.gameStats.daysSpent == 0)
            {
                Plugin.Log.LogMessage("Terminal starting credits to: " + ServerConfiguration.Instance.General.StartCredits);
                __instance.groupCredits = ServerConfiguration.Instance.General.StartCredits;
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "SetTimeAndPlanetToSavedSettings")]
        [HarmonyPostfix]
        private static void SetTimeAndPlanetToSavedSettings(global::StartOfRound __instance)
        {
            if (Terminal == null)
                Terminal = GameObject.FindObjectOfType<global::Terminal>();
            if (__instance.gameStats.daysSpent == 0)
            {
                if (Terminal != null)
                {
                    Plugin.Log.LogMessage("Set starting credits to: " + ServerConfiguration.Instance.General.StartCredits);
                    Terminal.groupCredits = ServerConfiguration.Instance.General.StartCredits;
                }
                TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * GetDeadlineDays());
                TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ResetShip")]
        [HarmonyPostfix]
        private static void ResetShip(global::StartOfRound __instance)
        {
            if (Terminal == null)
                Terminal = GameObject.FindObjectOfType<global::Terminal>();
            if (Terminal != null)
                Terminal.groupCredits = ServerConfiguration.Instance.General.StartCredits;
            TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * GetDeadlineDays());
            TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
            TimeOfDay.Instance.OnDayChanged();
        }

        public static float GetDeadlineDays()
        {
            return (float)ServerConfiguration.Instance.General.DeadlineLength * 1f;
        }

        public static void GrantXP()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var xp = TimeOfDay.Instance.profitQuota;
                if (!ServerConfiguration.Instance.General.SaveProgress)
                    xp = (int)(xp * ServerConfiguration.Instance.General.XPMultiplier);

                Network.Manager.Send(new GrantPlayerXP() { All = true, XP = xp });
                Network.Manager.Send(new GrantShipXP() { XP = xp });

                Network.Manager.Send(new ChangeShip() { TotalQuota = Network.Manager.Lobby.CurrentShip.TotalQuota + TimeOfDay.Instance.profitQuota, ExtendedDeadline = false });
            }
        }

        [HarmonyPatch(typeof(global::TimeOfDay), "SetNewProfitQuota")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSetNewProfitQuota(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching TimeOfDay->SetNewProfitQuota...");

            var method = typeof(Manager).GetMethod("GetDeadlineDays", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            var method2 = typeof(Manager).GetMethod("GrantXP", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4 && inst[i].operand is float f && f == 4f)
                {
                    Plugin.Log.LogDebug("Changed 4f to Manager->GetDeadlineDays()");
                    inst[i].opcode = OpCodes.Call;
                    inst[i].operand = method;
                    break;
                }
            }
            inst.Insert(0, new CodeInstruction(OpCodes.Call, method2));
            Plugin.Log.LogDebug("Patched TimeOfDay->SetNewProfitQuota...");

            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::TimeOfDay), "SyncNewProfitQuotaClientRpc")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSyncNewProfitQuotaClientRpc(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching TimeOfDay->SyncNewProfitQuotaClientRpc...");

            var method = typeof(Manager).GetMethod("GetDeadlineDays", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString().Contains("deadlineDaysAmount"))
                {
                    inst[i].opcode = OpCodes.Call;
                    inst[i].operand = method;
                    inst.RemoveAt(i + 1);
                    inst.RemoveAt(i - 2);
                    inst.RemoveAt(i - 2);
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched TimeOfDay->SyncNewProfitQuotaClientRpc...");

            return inst.AsEnumerable();
        }

        public static void DeactivateShipDecor(List<TerminalNode> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].shipUnlockableID > -1 && StartOfRound.Instance.unlockablesList.unlockables.Count > items[i].shipUnlockableID)
                {
                    var unlockable = StartOfRound.Instance.unlockablesList.unlockables[items[i].shipUnlockableID];
                    if (!Items.IsUnlockableBuyable(unlockable))
                    {
                        items.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(global::Terminal), "RotateShipDecorSelection")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchRotateShipDecorSelection(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching Terminal->RotateShipDecorSelection...");

            var method = typeof(Manager).GetMethod("DeactivateShipDecor", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Call && inst[i].operand.ToString().Contains("Next"))
                {
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Stloc_S, 4));
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Call, method));
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Ldloc_S, 4));
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched Terminal->RotateShipDecorSelection...");

            return inst.AsEnumerable();
        }
        

        [HarmonyPatch(typeof(global::Terminal), "Awake")]
        [HarmonyPostfix]
        private static void TerminalAwake(global::Terminal __instance)
        {
            Moons.Reset();
            Terminal = __instance;
            FirstTerminalFrame = true;

            Plugin.Log.LogInfo("Adding own items to Terminal.");

            var perkKeyword = TerminalKeyword.CreateInstance<TerminalKeyword>();
            perkKeyword.isVerb = false;
            perkKeyword.word = "perks";
            perkKeyword.specialKeywordResult = new TerminalNode() { clearPreviousText = true, displayText = "To level up your character leave this terminal and get out your handy portable terminal (default key X).\n\nSponsored by <color=#ffffff>AdvancedTech</color>\n<color=#ff3333>Advancing mankind!</color>\n\n<size=60%>AdvancedTech is not responsible for any damages that may occur in connection with items sponsored by AdvancedTech for evaluation purposes. AdvancedTech reserves the right to take legal action against any statements to the contrary.</size>\n\n<size=100%>" };

            var buyKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
            var keywords = __instance.terminalNodes.allKeywords.ToList();
            var buyNouns = buyKeyword.compatibleNouns.ToList();
            var buyableItemsList = __instance.buyableItemsList.ToList();

            keywords.Add(perkKeyword);

            var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            var infoKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
            var confirmKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "confirm");
            var denyKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "deny");

            foreach (var i in Game.Manager.ItemProperties)
            {
                if (i.Value.itemName == "Vision enhancer")
                    i.Value.batteryUsage = ServerConfiguration.Instance.Items.VisionEnhancer.BatteryTime;
                if (i.Value.itemName == "Helmet lamp")
                    i.Value.batteryUsage = ServerConfiguration.Instance.Items.HelmetLamp.BatteryTime;
                if (i.Value.itemName == "Headset")
                    i.Value.batteryUsage = ServerConfiguration.Instance.Items.Headset.BatteryTime;
                if (i.Value.itemName == "Tactical helmet")
                    i.Value.batteryUsage = ServerConfiguration.Instance.Items.TacticalHelmet.BatteryTime;

                if (Game.Manager.AddedItems[i.Key].IsBuyable)
                {
                    Plugin.Log.LogInfo("Adding " + Game.Manager.AddedItems[i.Key].ItemName);
                    var runtimeItem = Game.Manager.RuntimeItems[i.Key];
                    runtimeItem.Apply(buyKeyword, keywords, buyNouns, cancelPurchaseNode, confirmKeyword, denyKeyword, buyableItemsList);
                }
            }
            __instance.buyableItemsList = buyableItemsList.ToArray();
            buyKeyword.compatibleNouns = buyNouns.ToArray();
            __instance.terminalNodes.allKeywords = keywords.ToArray();
        }

        public static int GetNodeCost(TerminalNode node)
        {
            if (Terminal == null) Terminal = GameObject.FindObjectOfType<global::Terminal>();
            if (Terminal == null) return node.itemCost;

            if (node.displayPlanetInfo > -1)
            {
                var cost = (int) (Moons.GetMoonPrice(node.displayPlanetInfo, node.itemCost) * Perks.GetMultiplier("TravelDiscount"));
                if (cost != -1) return cost;
            }
            if (node.buyRerouteToMoon > -1)
            {
                var cost = (int) (Moons.GetMoonPrice(node.buyRerouteToMoon, node.itemCost) * Perks.GetMultiplier("TravelDiscount"));
                if (cost != -1) return cost;
            }
            if (node.buyItemIndex > -1)
            {
                if (Terminal.buyableItemsList.Length > node.buyItemIndex)
                    return Items.GetItemPrice(Terminal.buyableItemsList[node.buyItemIndex], false);
            }
            if (node.shipUnlockableID > -1)
            {
                if (global::StartOfRound.Instance.unlockablesList.unlockables.Count > node.shipUnlockableID)
                    return Items.GetUnlockablePrice(global::StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID]);
            }
            return node.itemCost;
        }

        internal static bool CompareField(FieldInfo fieldInfo, string name)
        {
            var compare = fieldInfo.DeclaringType.FullName + "." + fieldInfo.Name;
            return compare == name;
        }

        internal static Dictionary<string, MethodInfo> Patches = new Dictionary<string, MethodInfo>()
        {
            {"Item.weight", typeof(Game.Manager.Items).GetMethod("GetItemWeight", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"Item.creditsWorth", typeof(Game.Manager.Items).GetMethod("GetItemPriceTranspiler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"Item.highestSalePercentage", typeof(Game.Manager.Items).GetMethod("GetItemHighestSalePercentage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"Item.maxValue", typeof(Game.Manager.Items).GetMethod("GetItemMaxValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"Item.minValue", typeof(Game.Manager.Items).GetMethod("GetItemMinValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"TerminalNode.itemCost", typeof(Game.Manager).GetMethod("GetNodeCost", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.maxScrap", typeof(Game.Manager.Moons).GetMethod("GetMaxScrapAmount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.minScrap", typeof(Game.Manager.Moons).GetMethod("GetMinScrapAmount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.factorySizeMultiplier", typeof(Game.Manager.Moons).GetMethod("GetDungeonSize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.daytimeEnemiesProbabilityRange", typeof(Game.Manager.Moons).GetMethod("GetDaytimeEnemiesProbability", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.spawnProbabilityRange", typeof(Game.Manager.Moons).GetMethod("GetInsideEnemiesProbability", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.maxDaytimeEnemyPowerCount", typeof(Game.Manager.Moons).GetMethod("GetDaytimeEnemiesMaxPower", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.maxOutsideEnemyPowerCount", typeof(Game.Manager.Moons).GetMethod("GetOutsideEnemiesMaxPower", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.maxEnemyPowerCount", typeof(Game.Manager.Moons).GetMethod("GetInsideEnemiesMaxPower", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.Enemies", typeof(Game.Manager.Moons).GetMethod("GetInsideEnemies", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.OutsideEnemies", typeof(Game.Manager.Moons).GetMethod("GetOutsideEnemies", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.DaytimeEnemies", typeof(Game.Manager.Moons).GetMethod("GetDaytimeEnemies", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"SelectableLevel.spawnableScrap", typeof(Game.Manager.Moons).GetMethod("GetLootTable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"EnemyType.PowerLevel", typeof(Game.Manager.Enemies).GetMethod("GetPowerLevel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"RoundManager.scrapAmountMultiplier", typeof(Game.Manager.Moons).GetMethod("GetScrapAmountModifier", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
            {"RoundManager.scrapValueMultiplier", typeof(Game.Manager.Moons).GetMethod("GetScrapValueModifier", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)},
        };
        internal static IEnumerable<CodeInstruction> PatchFields(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                foreach (var kv in Patches)
                {
                    if (inst[i].IsLdfld(kv.Key))
                    {
                        inst[i].opcode = OpCodes.Call;
                        inst[i].operand = kv.Value;
                        Plugin.Log.LogDebug("Replaced " + kv.Key + " with " + kv.Value.DeclaringType.FullName + "->" + kv.Value.Name);
                    }
                }
            }
            return inst.AsEnumerable();
        }
    }
}
