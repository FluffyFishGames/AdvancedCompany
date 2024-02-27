using AdvancedCompany.Config;
using AdvancedCompany.Terminal.Applications;
using JetBrains.Annotations;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Game
{
    internal partial class Manager
    {
        internal class Items
        {
            public static Dictionary<string, bool> ItemActivation;
            public static List<Item> AllItems;
            public static List<Item> ScrapItems;
            public static Dictionary<string, Item> ScrapItemsByName;
            public static List<Item> BuyableItems;
            public static List<UnlockableItem> ShipUpgrades;
            public static List<UnlockableItem> Decorations;
            public static Dictionary<UnlockableItem, int> UnlockablePrices = new Dictionary<UnlockableItem, int>();
            internal static void SearchItems()
            {
                Plugin.Log.LogMessage("Searching for all items...");
                AllItems = new();
                ScrapItems = new();
                BuyableItems = new();
                ShipUpgrades = new();
                Decorations = new();
                ScrapItemsByName = new();
                ItemActivation = new();

                Plugin.Log.LogMessage("Searching for scrap...");
                for (var i = 0; i < StartOfRound.Instance.levels.Length; i++)
                {
                    var level = StartOfRound.Instance.levels[i];
                    for (var j = 0; j < level.spawnableScrap.Count; j++)
                    {
                        var scrap = level.spawnableScrap[j];
                        if (!ScrapItems.Contains(scrap.spawnableItem))
                        {
                            if (!ScrapItemsByName.ContainsKey(scrap.spawnableItem.itemName))
                            {
                                ScrapItems.Add(scrap.spawnableItem);
                                ScrapItemsByName.Add(scrap.spawnableItem.itemName, scrap.spawnableItem);
                            }
                            else
                            {
                                Plugin.Log.LogWarning("New scrap item found, but the name " + scrap.spawnableItem.itemName + " was already used!");
                            }
                        }
                        if (!AllItems.Contains(scrap.spawnableItem))
                            AllItems.Add(scrap.spawnableItem);
                    }
                }
                Plugin.Log.LogDebug("Found scrap: " + String.Join(", ", ScrapItemsByName.Keys));
                Plugin.Log.LogMessage("Searching for buyable items...");
                HashSet<string> foundBuyables = new HashSet<string>();
                HashSet<string> foundDecorations = new HashSet<string>();
                HashSet<string> foundUpgrades = new HashSet<string>();

                for (var i = 0; i < Game.Manager.Terminal.terminalNodes.allKeywords.Length; i++)
                {
                    var keyword = Game.Manager.Terminal.terminalNodes.allKeywords[i];
                    if (keyword != null && keyword.compatibleNouns != null)
                    {
                        for (var j = 0; j < keyword.compatibleNouns.Length; j++)
                        {
                            var noun = keyword.compatibleNouns[j];
                            if (noun.result != null)
                            {
                                if (noun.result.buyItemIndex >= 0)
                                {
                                    if (noun.result.buyItemIndex < Game.Manager.Terminal.buyableItemsList.Length)
                                    {
                                        var item = Game.Manager.Terminal.buyableItemsList[noun.result.buyItemIndex];
                                        if (item == null)
                                            Plugin.Log.LogWarning("The item " + noun.noun.word + "(" + noun.result.name + ") is null? This is unexpected behaviour. Item wont be added.");
                                        else
                                        {
                                            if (!BuyableItems.Contains(item))
                                            {
                                                if (foundBuyables.Contains(item.itemName))
                                                    foundBuyables.Add(item.itemName);
                                                BuyableItems.Add(item);
                                            }
                                            if (!AllItems.Contains(item))
                                                AllItems.Add(item);
                                        }
                                    }
                                    else Plugin.Log.LogWarning("The item " + noun.noun.word + "(" + noun.result.name + ") wasn't added to buyableItemsList. This is unexpected behaviour. Item wont be added.");
                                }
                                else if (noun.result.shipUnlockableID >= 0)
                                {
                                    if (StartOfRound.Instance.unlockablesList.unlockables.Count > noun.result.shipUnlockableID)
                                    {
                                        var unlockable = StartOfRound.Instance.unlockablesList.unlockables[noun.result.shipUnlockableID];
                                        
                                        UnlockablePrices[unlockable] = noun.result.itemCost;
                                        if (unlockable.alwaysInStock)
                                        {
                                            if (!ShipUpgrades.Contains(unlockable))
                                            {
                                                if (foundUpgrades.Contains(unlockable.unlockableName))
                                                    foundUpgrades.Add(unlockable.unlockableName);
                                                ShipUpgrades.Add(unlockable);
                                            }
                                        }
                                        else
                                        {
                                            if (!Decorations.Contains(unlockable))
                                            {
                                                if (foundDecorations.Contains(unlockable.unlockableName))
                                                    foundDecorations.Add(unlockable.unlockableName);
                                                Decorations.Add(unlockable);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < StartOfRound.Instance.unlockablesList.unlockables.Count; i++)
                {
                    var unlockable = StartOfRound.Instance.unlockablesList.unlockables[i];
                    if (unlockable.shopSelectionNode != null)
                    {
                        UnlockablePrices[unlockable] = unlockable.shopSelectionNode.itemCost;

                        if (unlockable.alwaysInStock)
                        {
                            if (!ShipUpgrades.Contains(unlockable))
                            {
                                if (foundUpgrades.Contains(unlockable.unlockableName))
                                    foundUpgrades.Add(unlockable.unlockableName);
                                ShipUpgrades.Add(unlockable);
                            }
                        }
                        else
                        {
                            if (!Decorations.Contains(unlockable))
                            {
                                if (foundDecorations.Contains(unlockable.unlockableName))
                                    foundDecorations.Add(unlockable.unlockableName);
                                Decorations.Add(unlockable);
                            }
                        }
                    }
                }
                Plugin.Log.LogDebug("Found buyable items: " + String.Join(", ", foundBuyables));
                Plugin.Log.LogDebug("Found decorations: " + String.Join(", ", foundDecorations));
                Plugin.Log.LogDebug("Found upgrades: " + String.Join(", ", foundUpgrades));

                AllItems = AllItems.OrderBy(a => a.itemName).ToList();
                ScrapItems = ScrapItems.OrderBy(a => a.itemName).ToList();
                BuyableItems = BuyableItems.OrderBy(a => a.itemName).ToList();
                Decorations = Decorations.OrderBy(a => a.unlockableName).ToList();
                ShipUpgrades = ShipUpgrades.OrderBy(a => a.unlockableName).ToList();

                LobbyConfiguration.AllItemsConfig.Items.Clear();
                foreach (var item in BuyableItems)
                    LobbyConfiguration.AllItemsConfig.Items.Add(item.itemName, new LobbyConfiguration.ItemConfig()
                    {
                        Active = true,
                        MaxDiscount = item.highestSalePercentage,
                        Price = item.creditsWorth,
                        Weight = item.weight
                    });

                System.IO.File.WriteAllText(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "items.json"), LobbyConfiguration.AllItemsConfig.ToJSON().ToString(Newtonsoft.Json.Formatting.Indented));

                LobbyConfiguration.AllScrapConfig.Items.Clear();
                foreach (var scrap in ScrapItems)
                    LobbyConfiguration.AllScrapConfig.Items.Add(scrap.itemName, new LobbyConfiguration.ScrapConfig()
                    {
                        Active = true,
                        MaxValue = scrap.maxValue,
                        MinValue = scrap.minValue,
                        Weight = scrap.weight
                    });

                System.IO.File.WriteAllText(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "scrap.json"), LobbyConfiguration.AllScrapConfig.ToJSON().ToString(Newtonsoft.Json.Formatting.Indented));
                
                LobbyConfiguration.AllUnlockablesConfig.Unlockables.Clear();
                foreach (var unlockable in Decorations)
                {
                    if (!LobbyConfiguration.AllUnlockablesConfig.Unlockables.ContainsKey(unlockable.unlockableName) && UnlockablePrices[unlockable] > -1)
                        LobbyConfiguration.AllUnlockablesConfig.Unlockables.Add(unlockable.unlockableName, new LobbyConfiguration.UnlockableConfig()
                        {
                            Active = true,
                            Price = UnlockablePrices[unlockable]
                        });
                }
                foreach (var unlockable in ShipUpgrades)
                {
                    if (!LobbyConfiguration.AllUnlockablesConfig.Unlockables.ContainsKey(unlockable.unlockableName) && UnlockablePrices[unlockable] > -1)
                        LobbyConfiguration.AllUnlockablesConfig.Unlockables.Add(unlockable.unlockableName, new LobbyConfiguration.UnlockableConfig()
                        {
                            Active = true,
                            Price = UnlockablePrices[unlockable]
                        });
                }
                System.IO.File.WriteAllText(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "unlockables.json"), LobbyConfiguration.AllUnlockablesConfig.ToJSON().ToString(Newtonsoft.Json.Formatting.Indented));
            }

            public static bool IsSpawnable(Item item)
            {
                try
                {
                    var config = ServerConfiguration.Instance.Items.GetByScrapName(item.itemName);
                    return config == null || config.Active;
                }
                catch (Exception e)
                {
                    return true;
                }
            }

            public static bool IsBuyable(Item item)
            {
                try
                {
                    var config = ServerConfiguration.Instance.Items.GetByItemName(item.itemName);
                    return config == null || config.Active;
                }
                catch (Exception e)
                {
                    return true;
                }
            }

            public static bool IsUnlockableBuyable(UnlockableItem item)
            {
                try
                {
                    var config = ServerConfiguration.Instance.Items.GetByUnlockableName(item.unlockableName);
                    return config == null || config.Active;
                }
                catch (Exception e)
                {
                    return true;
                }
            }

            public static int GetItemSalesPercentage(Item item)
            {
                var ind = -1;
                for (var i = 0; i < Game.Manager.Terminal.buyableItemsList.Length; i++)
                {
                    if (Game.Manager.Terminal.buyableItemsList[i] == item)
                    {
                        ind = i;
                        break;
                    }
                }
                if (ind > -1)
                {
                    return Game.Manager.Terminal.itemSalesPercentages[ind];
                }
                return 100;
            }

            public static float GetItemWeight(Item item)
            {
                if (item.isScrap)
                {
                    var config = ServerConfiguration.Instance.Items.GetByScrapName(item.itemName);
                    return config == null || !config.OverrideWeight ? item.weight : config.Weight;
                }
                else
                {
                    var config = ServerConfiguration.Instance.Items.GetByItemName(item.itemName);
                    return config == null || !config.OverrideWeight ? item.weight : config.Weight;
                }
            }

            public static int GetItemMinValue(Item item)
            {
                var config = ServerConfiguration.Instance.Items.GetByScrapName(item.itemName);
                return config == null || !config.OverrideMinValue ? item.minValue : config.MinValue;
            }

            public static int GetItemMaxValue(Item item)
            {
                var config = ServerConfiguration.Instance.Items.GetByScrapName(item.itemName);
                return config == null || !config.OverrideMaxValue ? item.maxValue : config.MaxValue;
            }

            public static int GetItemHighestSalePercentage(Item item)
            {
                var config = ServerConfiguration.Instance.Items.GetByItemName(item.itemName);
                if (config != null && !config.Active) return 0;
                return config == null || !config.OverrideMaxDiscount ? item.highestSalePercentage : config.MaxDiscount;
            }

            public static int GetItemPrice(Item item, bool discount = false)
            {
                var config = ServerConfiguration.Instance.Items.GetByItemName(item.itemName);
                var factor = discount ? ((float)GetItemSalesPercentage(item)) / 100f : 1f;
                return Mathf.FloorToInt(config == null || !config.OverridePrice ? ((float)item.creditsWorth) * factor : ((float)config.Price) * factor);
            }

            public static int GetUnlockablePrice(UnlockableItem unlockable)
            {
                var config = ServerConfiguration.Instance.Items.GetByUnlockableName(unlockable.unlockableName);
                return config == null || !config.OverridePrice ? UnlockablePrices[unlockable] : config.Price;
            }

            public static int GetItemPriceTranspiler(Item item)
            {
                return GetItemPrice(item, false);
            }
        }
    }
}