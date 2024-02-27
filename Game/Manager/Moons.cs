using AdvancedCompany.Config;
using AdvancedCompany.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;

namespace AdvancedCompany.Game
{
    internal partial class Manager
    {
        public class Moons
        {
            internal class Moon
            {
                public SelectableLevel Level;
                public LobbyConfiguration.MoonConfig Config;
                private Dictionary<string, int> _OverrideDaytimeEnemies;
                private Dictionary<string, int> _OverrideOutsideEnemies;
                private Dictionary<string, int> _OverrideInsideEnemies;
                private Dictionary<string, int> _OverrideLoot;
                private List<SpawnableEnemyWithRarity> _CurrentDaytimeEnemies;
                private List<SpawnableEnemyWithRarity> _CurrentOutsideEnemies;
                private List<SpawnableEnemyWithRarity> _CurrentInsideEnemies;
                private List<SpawnableItemWithRarity> _CurrentLoot;

                public float? GetScrapAmountModifier()
                {
                    if (Config == null) return null;

                    if (Config.OverrideScrapAmountModifier)
                        return Config.ScrapAmountModifier;
                    return null;
                }
                public float? GetScrapValueModifier()
                {
                    if (Config == null) return null;

                    if (Config.OverrideScrapValueModifier)
                        return Config.ScrapValueModifier;
                    return null;
                }

                public int GetMinScrapAmount(int found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideMinScrapAmount)
                        return Config.MinScrapAmount;
                    return found;
                }

                public int GetMaxScrapAmount(int found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideMaxScrapAmount)
                        return Config.MaxScrapAmount;
                    return found;
                }

                public float GetDungeonSize(float found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideDungeonSize)
                        return Config.DungeonSize;
                    return found;
                }

                public int GetPrice(int found)
                {
                    if (Config == null) return found;

                    if (Config.OverridePrice)
                        return Config.Price;
                    return found;
                }

                public int GetDaytimeEnemiesMaxPower(int found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideDaytimeEnemiesMaxPower)
                        return Config.DaytimeEnemiesMaxPower;
                    return found;
                }
                public int GetOutsideEnemiesMaxPower(int found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideOutsideEnemiesMaxPower)
                        return Config.OutsideEnemiesMaxPower;
                    return found;
                }
                public int GetInsideEnemiesMaxPower(int found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideInsideEnemiesMaxPower)
                        return Config.InsideEnemiesMaxPower;
                    return found;
                }

                public float GetDaytimeEnemiesProbabilityRange(float found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideDaytimeEnemiesProbability)
                        return Config.DaytimeEnemiesProbability;
                    return found;
                }

                public float GetInsideEnemiesProbabilityRange(float found)
                {
                    if (Config == null) return found;

                    if (Config.OverrideInsideEnemiesProbability)
                        return Config.InsideEnemiesProbability;
                    return found;
                }

                public List<SpawnableItemWithRarity> GetLootTable(List<SpawnableItemWithRarity> found)
                {
                    if (Config == null) return found;
                    if (_CurrentLoot == null)
                    {
                        var ret = found.ToList();
                        if (_OverrideLoot == null)
                        {
                            _OverrideLoot = new();
                            foreach (var e in Config.LootTable)
                            {
                                if (Items.ScrapItemsByName.ContainsKey(e.Key))
                                {
                                    if (!Items.IsSpawnable(Items.ScrapItemsByName[e.Key]))
                                        _OverrideLoot.Add(e.Key, 0);
                                    else if (e.Value.Override)
                                    {
                                        _OverrideLoot.Add(e.Key, e.Value.Rarity);
                                    }
                                }
                            }
                        }
                        HashSet<string> foundScrap = new HashSet<string>();
                        for (var i = 0; i < ret.Count; i++)
                        {
                            var element = ret[i];
                            foundScrap.Add(element.spawnableItem.itemName);
                            if (_OverrideLoot.ContainsKey(element.spawnableItem.itemName))
                            {
                                var rarity = _OverrideLoot[element.spawnableItem.itemName];
                                if (rarity == 0)
                                {
                                    ret.RemoveAt(i);
                                    i--;
                                    continue;
                                }
                                ret[i] = new SpawnableItemWithRarity() { spawnableItem = element.spawnableItem, rarity = rarity };
                            }
                        }
                        foreach (var e in _OverrideLoot)
                        {
                            if (!foundScrap.Contains(e.Key))
                            {
                                var rarity = e.Value;
                                if (rarity > 0)
                                    ret.Add(new SpawnableItemWithRarity() { spawnableItem = Items.ScrapItemsByName[e.Key], rarity = rarity });
                            }
                        }
                        _CurrentLoot = ret;
                    }
                    return _CurrentLoot;
                }

                public List<SpawnableEnemyWithRarity> GetDaytimeEnemies(List<SpawnableEnemyWithRarity> found)
                {
                    if (Config == null) return found;
                    if (_CurrentDaytimeEnemies == null)
                    {
                        var ret = found.ToList();
                        if (_OverrideDaytimeEnemies == null)
                        {
                            _OverrideDaytimeEnemies = new();
                            foreach (var e in Config.DaytimeEnemies)
                            {
                                if (Enemies.EnemiesByName.ContainsKey(e.Key))
                                {
                                    if (Enemies.EnemiesByName.ContainsKey(e.Key))
                                    {
                                        if (!Enemies.IsSpawnable(Enemies.EnemiesByName[e.Key].EnemyType))
                                            _OverrideDaytimeEnemies.Add(e.Key, 0);
                                        else if (e.Value.Override)
                                        {
                                            _OverrideDaytimeEnemies.Add(e.Key, e.Value.Rarity);
                                        }
                                    }
                                }
                            }
                        }
                        HashSet<string> foundEnemies = new HashSet<string>();
                        for (var i = 0; i < ret.Count; i++)
                        {
                            var element = ret[i];
                            foundEnemies.Add(element.enemyType.enemyName);
                            if (_OverrideDaytimeEnemies.ContainsKey(element.enemyType.enemyName))
                            {
                                var rarity = _OverrideDaytimeEnemies[element.enemyType.enemyName];
                                if (rarity == 0)
                                {
                                    ret.RemoveAt(i);
                                    i--;
                                    continue;
                                }
                                ret[i] = new SpawnableEnemyWithRarity() { enemyType = element.enemyType, rarity = rarity };
                            }
                        }
                        foreach (var e in _OverrideDaytimeEnemies)
                        {
                            if (!foundEnemies.Contains(e.Key))
                            {
                                var rarity = e.Value;
                                if (rarity > 0)
                                    ret.Add(new SpawnableEnemyWithRarity() { enemyType = Enemies.EnemiesByName[e.Key].EnemyType, rarity = rarity });
                            }
                        }
                        _CurrentDaytimeEnemies = ret;
                    }
                    return _CurrentDaytimeEnemies;
                }

                public List<SpawnableEnemyWithRarity> GetOutsideEnemies(List<SpawnableEnemyWithRarity> found)
                {
                    if (Config == null) return found;
                    if (_CurrentOutsideEnemies == null)
                    {
                        var ret = found.ToList();
                        if (_OverrideOutsideEnemies == null)
                        {
                            _OverrideOutsideEnemies = new();
                            foreach (var e in Config.OutsideEnemies)
                            {
                                if (Enemies.EnemiesByName.ContainsKey(e.Key))
                                {
                                    if (!Enemies.IsSpawnable(Enemies.EnemiesByName[e.Key].EnemyType))
                                        _OverrideOutsideEnemies.Add(e.Key, 0);
                                    else if (Enemies.EnemiesByName.ContainsKey(e.Key))
                                    {
                                        if (e.Value.Override)
                                        {
                                            _OverrideOutsideEnemies.Add(e.Key, e.Value.Rarity);
                                        }
                                    }
                                }
                            }
                        }
                        HashSet<string> foundEnemies = new HashSet<string>();
                        for (var i = 0; i < ret.Count; i++)
                        {
                            var element = ret[i];
                            foundEnemies.Add(element.enemyType.enemyName);
                            if (_OverrideOutsideEnemies.ContainsKey(element.enemyType.enemyName))
                            {
                                var rarity = _OverrideOutsideEnemies[element.enemyType.enemyName];
                                if (rarity == 0)
                                {
                                    ret.RemoveAt(i);
                                    i--;
                                    continue;
                                }
                                ret[i] = new SpawnableEnemyWithRarity() { enemyType = element.enemyType, rarity = rarity };
                            }
                        }
                        foreach (var e in _OverrideOutsideEnemies)
                        {
                            if (!foundEnemies.Contains(e.Key))
                            {
                                var rarity = e.Value;
                                if (rarity > 0)
                                    ret.Add(new SpawnableEnemyWithRarity() { enemyType = Enemies.EnemiesByName[e.Key].EnemyType, rarity = rarity });
                            }
                        }
                        _CurrentOutsideEnemies = ret;
                    }
                    return _CurrentOutsideEnemies;
                }

                public List<SpawnableEnemyWithRarity> GetInsideEnemies(List<SpawnableEnemyWithRarity> found)
                {
                    if (Config == null) return found;
                    if (_CurrentInsideEnemies == null)
                    {
                        var ret = found.ToList();
                        if (_OverrideInsideEnemies == null)
                        {
                            _OverrideInsideEnemies = new();
                            foreach (var e in Config.InsideEnemies)
                            {
                                if (Enemies.EnemiesByName.ContainsKey(e.Key))
                                {
                                    if (!Enemies.IsSpawnable(Enemies.EnemiesByName[e.Key].EnemyType))
                                        _OverrideInsideEnemies.Add(e.Key, 0);
                                    else if (Enemies.EnemiesByName.ContainsKey(e.Key))
                                    {
                                        if (e.Value.Override)
                                        {
                                            _OverrideInsideEnemies.Add(e.Key, e.Value.Rarity);
                                        }
                                    }
                                }
                            }
                        }
                        HashSet<string> foundEnemies = new HashSet<string>();
                        for (var i = 0; i < ret.Count; i++)
                        {
                            var element = ret[i];
                            foundEnemies.Add(element.enemyType.enemyName);
                            if (_OverrideInsideEnemies.ContainsKey(element.enemyType.enemyName))
                            {
                                var rarity = _OverrideInsideEnemies[element.enemyType.enemyName];
                                if (rarity == 0)
                                {
                                    ret.RemoveAt(i);
                                    i--;
                                    continue;
                                }
                                ret[i] = new SpawnableEnemyWithRarity() { enemyType = element.enemyType, rarity = rarity };
                            }
                        }
                        foreach (var e in _OverrideInsideEnemies)
                        {
                            if (!foundEnemies.Contains(e.Key))
                            {
                                var rarity = e.Value;
                                if (rarity > 0)
                                    ret.Add(new SpawnableEnemyWithRarity() { enemyType = Enemies.EnemiesByName[e.Key].EnemyType, rarity = rarity });
                            }
                        }
                        _CurrentInsideEnemies = ret;
                    }
                    return _CurrentInsideEnemies;
                }
            }

            private static Dictionary<string, int> NewPrices = null;
            private static Dictionary<string, int> OriginalPrices = new Dictionary<string, int>();
            private static List<Moon> AllMoons = new List<Moon>();
            private static Dictionary<string, Moon> MoonsByName = new Dictionary<string, Moon>();
            public static void Reset()
            {
                NewPrices = null;
            }

            internal static void SearchMoons()
            {
                AllMoons = new();
                MoonsByName = new();
                OriginalPrices = new();

                Plugin.Log.LogMessage("Searching for all moons...");
                var routeKeyword = Manager.Terminal.terminalNodes.allKeywords.First(keyword => keyword.name == "Route");
                foreach (var noun in routeKeyword.compatibleNouns)
                {
                    var confirmNoun = noun.result.terminalOptions.First(option => option.noun.name == "Confirm");
                    if (confirmNoun.result.buyRerouteToMoon < global::StartOfRound.Instance.levels.Length)
                    {
                        var name = global::StartOfRound.Instance.levels[confirmNoun.result.buyRerouteToMoon].PlanetName;
                        if (!OriginalPrices.ContainsKey(name))
                        {
                            Plugin.Log.LogDebug("Found price " + noun.result.itemCost + " for moon " + name);
                            OriginalPrices.Add(name, noun.result.itemCost);
                        }
                    }
                }
                
                var moonNames = new HashSet<string>();
                for (var i = 0; i < global::StartOfRound.Instance.levels.Length; i++)
                {
                    var level = global::StartOfRound.Instance.levels[i];
                    if (!MoonsByName.ContainsKey(level.PlanetName))
                    {
                        var newMoon = new Moon() { Level = level, Config = ServerConfiguration.Instance.Moons.Moons.ContainsKey(level.PlanetName) ? ServerConfiguration.Instance.Moons.Moons[level.PlanetName] : null };
                        AllMoons.Add(newMoon);
                        MoonsByName.Add(level.PlanetName, newMoon);
                    }
                }

                Plugin.Log.LogDebug("Found moons: " + String.Join(", ", MoonsByName.Keys));

                AllMoons = AllMoons.OrderBy(a => a.Level.PlanetName).ToList();

                LobbyConfiguration.AllMoonsConfig.Moons.Clear();
                foreach (var moon in AllMoons)
                {
                    var lootTable = new Dictionary<string, LobbyConfiguration.MoonConfig.LootTableItem>();
                    foreach (var item in moon.Level.spawnableScrap)
                    {
                        if (!lootTable.ContainsKey(item.spawnableItem.itemName))
                            lootTable.Add(item.spawnableItem.itemName, new LobbyConfiguration.MoonConfig.LootTableItem()
                            {
                                Override = false,
                                Rarity = item.rarity
                            });
                        else
                        {
                            lootTable[item.spawnableItem.itemName].Rarity += item.rarity;
                            Plugin.Log.LogWarning("Double loot table entry: " + item.spawnableItem.itemName);
                        }
                    }
                    var daytimeEnemies = new Dictionary<string, LobbyConfiguration.MoonConfig.EnemyTableItem>();
                    foreach (var enemy in moon.Level.DaytimeEnemies)
                    {
                        if (!daytimeEnemies.ContainsKey(enemy.enemyType.enemyName))
                            daytimeEnemies.Add(enemy.enemyType.enemyName, new LobbyConfiguration.MoonConfig.EnemyTableItem()
                            {
                                Override = false,
                                Rarity = enemy.rarity
                            });
                        else
                        {
                            daytimeEnemies[enemy.enemyType.enemyName].Rarity += enemy.rarity;
                            Plugin.Log.LogWarning("Duplicate in inside enemies of moon \"" + moon.Level.PlanetName + "\" found: " + enemy.enemyType.enemyName);
                        }
                    }
                    var outsideEnemies = new Dictionary<string, LobbyConfiguration.MoonConfig.EnemyTableItem>();
                    foreach (var enemy in moon.Level.OutsideEnemies)
                    {
                        if (!outsideEnemies.ContainsKey(enemy.enemyType.enemyName))
                            outsideEnemies.Add(enemy.enemyType.enemyName, new LobbyConfiguration.MoonConfig.EnemyTableItem()
                            {
                                Override = false,
                                Rarity = enemy.rarity
                            });
                        else
                        {
                            outsideEnemies[enemy.enemyType.enemyName].Rarity += enemy.rarity;
                            Plugin.Log.LogWarning("Duplicate in outside enemies of moon \"" + moon.Level.PlanetName + "\" found: " + enemy.enemyType.enemyName);
                        }
                    }
                    var insideEnemies = new Dictionary<string, LobbyConfiguration.MoonConfig.EnemyTableItem>();
                    foreach (var enemy in moon.Level.Enemies)
                    {
                        if (!insideEnemies.ContainsKey(enemy.enemyType.enemyName))
                            insideEnemies.Add(enemy.enemyType.enemyName, new LobbyConfiguration.MoonConfig.EnemyTableItem()
                            {
                                Override = false,
                                Rarity = enemy.rarity
                            });
                        else
                        {
                            insideEnemies[enemy.enemyType.enemyName].Rarity += enemy.rarity;
                            Plugin.Log.LogWarning("Duplicate in inside enemies of moon \"" + moon.Level.PlanetName + "\" found: " + enemy.enemyType.enemyName);
                        }
                    }
                    LobbyConfiguration.AllMoonsConfig.Moons.Add(moon.Level.PlanetName, new LobbyConfiguration.MoonConfig()
                    {
                        ScrapValueModifier = 1f,
                        ScrapAmountModifier = 1f,
                        DaytimeEnemiesMaxPower = moon.Level.maxDaytimeEnemyPowerCount,
                        InsideEnemiesMaxPower = moon.Level.maxEnemyPowerCount,
                        OutsideEnemiesMaxPower = moon.Level.maxOutsideEnemyPowerCount,
                        DaytimeEnemiesProbability = moon.Level.daytimeEnemiesProbabilityRange,
                        InsideEnemiesProbability = moon.Level.spawnProbabilityRange,
                        DungeonSize = moon.Level.factorySizeMultiplier,
                        MaxScrapAmount = moon.Level.maxScrap,
                        MinScrapAmount = moon.Level.minScrap,
                        Price = OriginalPrices[moon.Level.PlanetName],
                        DaytimeEnemies = daytimeEnemies,
                        InsideEnemies = insideEnemies,
                        OutsideEnemies = outsideEnemies,
                        LootTable = lootTable
                    });
                }
                System.IO.File.WriteAllText(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "moons.json"), LobbyConfiguration.AllMoonsConfig.ToJSON().ToString(Newtonsoft.Json.Formatting.Indented));
            }

            private static void Initialize()
            {
                if (NewPrices == null)
                {
                    NewPrices = new Dictionary<string, int>()
                    { };/*

                        {"41-experimentation", ServerConfiguration.Instance.Moons.Experimentation.Price},
                        {"220-assurance", ServerConfiguration.Instance.Moons.Assurance.Price},
                        {"56-vow", ServerConfiguration.Instance.Moons.Vow.Price},
                        {"21-offense", ServerConfiguration.Instance.Moons.Offense.Price},
                        {"61-march", ServerConfiguration.Instance.Moons.March.Price},
                        {"85-rend", ServerConfiguration.Instance.Moons.Rend.Price},
                        {"7-dine", ServerConfiguration.Instance.Moons.Dine.Price},
                        {"8-titan", ServerConfiguration.Instance.Moons.Titan.Price}
                    };*/
                    /*foreach (var moon in ServerConfiguration.Instance.Moons.FreeMoons)
                        NewPrices.Add(moon.Name.Replace(" ", "-").ToLowerInvariant(), moon.Price);*/

                }
            }

            public static List<SpawnableEnemyWithRarity> GetDaytimeEnemies(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetDaytimeEnemies(level.DaytimeEnemies);
                return level.DaytimeEnemies;
            }

            public static List<SpawnableEnemyWithRarity> GetOutsideEnemies(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetOutsideEnemies(level.OutsideEnemies);
                return level.OutsideEnemies;
            }

            public static List<SpawnableEnemyWithRarity> GetInsideEnemies(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetInsideEnemies(level.Enemies);
                return level.Enemies;
            }

            public static List<SpawnableItemWithRarity> GetLootTable(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetLootTable(level.spawnableScrap);
                return level.spawnableScrap;
            }

            public static float GetDungeonSize(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetDungeonSize(level.factorySizeMultiplier);
                return level.factorySizeMultiplier;
            }

            public static float GetScrapValueModifierOnly(global::RoundManager roundManager)
            {
                var multiplier = 1f;
                if (MoonsByName.ContainsKey(roundManager.currentLevel.PlanetName))
                {
                    var m = MoonsByName[roundManager.currentLevel.PlanetName].GetScrapValueModifier();
                    if (m.HasValue)
                        multiplier *= m.Value;
                }
                if (roundManager.currentLevel.currentWeather == LevelWeatherType.Rainy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.RainyWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Foggy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.FoggyWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Flooded && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.FloodedWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Stormy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.StormyWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Eclipsed && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.EclipsedWeather.ScrapValueMultiplier;
                else if (ServerConfiguration.Instance.Moons.ClearWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.ClearWeather.ScrapValueMultiplier;

                multiplier *= ServerConfiguration.Instance.General.GlobalScrapValueMultiplier;
                return multiplier;
            }

            public static float GetScrapValueModifier(global::RoundManager roundManager)
            {
                var multiplier = roundManager.scrapValueMultiplier;
                if (MoonsByName.ContainsKey(roundManager.currentLevel.PlanetName))
                {
                    var m = MoonsByName[roundManager.currentLevel.PlanetName].GetScrapValueModifier();
                    if (m.HasValue)
                        multiplier *= m.Value;
                }
                if (roundManager.currentLevel.currentWeather == LevelWeatherType.Rainy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.RainyWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Foggy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.FoggyWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Flooded && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.FloodedWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Stormy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.StormyWeather.ScrapValueMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Eclipsed && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.EclipsedWeather.ScrapValueMultiplier;
                else if (ServerConfiguration.Instance.Moons.ClearWeather.OverrideScrapValueMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.ClearWeather.ScrapValueMultiplier;

                multiplier *= ServerConfiguration.Instance.General.GlobalScrapValueMultiplier;
                return multiplier;
            }

            public static float GetScrapAmountModifier(global::RoundManager roundManager)
            {
                var multiplier = roundManager.scrapAmountMultiplier;
                if (MoonsByName.ContainsKey(roundManager.currentLevel.PlanetName))
                {
                    var m = MoonsByName[roundManager.currentLevel.PlanetName].GetScrapAmountModifier();
                    if (m.HasValue)
                        multiplier *= m.Value;
                }
                if (roundManager.currentLevel.currentWeather == LevelWeatherType.Rainy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapAmountMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.RainyWeather.ScrapAmountMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Foggy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapAmountMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.FoggyWeather.ScrapAmountMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Flooded && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapAmountMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.FloodedWeather.ScrapAmountMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Stormy && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapAmountMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.StormyWeather.ScrapAmountMultiplier;
                else if (roundManager.currentLevel.currentWeather == LevelWeatherType.Eclipsed && ServerConfiguration.Instance.Moons.RainyWeather.OverrideScrapAmountMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.EclipsedWeather.ScrapAmountMultiplier;
                else if (ServerConfiguration.Instance.Moons.ClearWeather.OverrideScrapAmountMultiplier)
                    multiplier *= ServerConfiguration.Instance.Moons.ClearWeather.ScrapAmountMultiplier;

                multiplier *= ServerConfiguration.Instance.General.GlobalScrapAmountMultiplier;
                return multiplier;
            }

            public static int GetMaxScrapAmount(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetMaxScrapAmount(level.maxScrap);
                return level.maxScrap;
            }

            public static int GetMinScrapAmount(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetMinScrapAmount(level.minScrap);
                return level.minScrap;
            }

            public static int GetDaytimeEnemiesMaxPower(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetDaytimeEnemiesMaxPower(level.maxDaytimeEnemyPowerCount);
                return (int) (level.maxDaytimeEnemyPowerCount * ServerConfiguration.Instance.General.GlobalMaxPowerMultiplier);
            }

            public static int GetOutsideEnemiesMaxPower(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetOutsideEnemiesMaxPower(level.maxOutsideEnemyPowerCount);
                return (int) (level.maxOutsideEnemyPowerCount * ServerConfiguration.Instance.General.GlobalMaxPowerMultiplier);
            }

            public static int GetInsideEnemiesMaxPower(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetInsideEnemiesMaxPower(level.maxEnemyPowerCount);
                return (int) (level.maxEnemyPowerCount * ServerConfiguration.Instance.General.GlobalMaxPowerMultiplier);
            }

            public static float GetDaytimeEnemiesProbability(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetDaytimeEnemiesProbabilityRange(level.daytimeEnemiesProbabilityRange);
                return level.daytimeEnemiesProbabilityRange;
            }

            public static float GetInsideEnemiesProbability(SelectableLevel level)
            {
                if (MoonsByName.ContainsKey(level.PlanetName))
                    return MoonsByName[level.PlanetName].GetInsideEnemiesProbabilityRange(level.spawnProbabilityRange);
                return level.spawnProbabilityRange;
            }

            public static int GetMoonPrice(int moon, int defaultPrice = 0)
            {
                if (global::StartOfRound.Instance.levels.Length > moon)
                {
                    var planetName = global::StartOfRound.Instance.levels[moon].PlanetName;
                    if (MoonsByName.ContainsKey(planetName))
                        return MoonsByName[planetName].GetPrice(defaultPrice);
                }
                return defaultPrice;
            }
        }
    }
}
