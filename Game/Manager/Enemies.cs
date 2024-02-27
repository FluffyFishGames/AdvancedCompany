using AdvancedCompany.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedCompany.Game
{
    internal partial class Manager
    {
        internal class Enemies
        {
            internal class Enemy
            {
                public EnemyType EnemyType;
                public LobbyConfiguration.EnemyConfig Config;
                public int GetPowerLevel(int found)
                {
                    if (Config == null) return found;

                    if (Config.OverridePowerLevel)
                        return Config.PowerLevel;
                    return found;
                }
            }

            internal static List<Enemy> AllEnemies = new List<Enemy>();
            internal static Dictionary<string, Enemy> EnemiesByName = new Dictionary<string, Enemy>();

            internal static void SearchEnemies()
            {
                AllEnemies = new();
                EnemiesByName = new();
                Plugin.Log.LogMessage("Searching for enemies...");
                for (var i = 0; i < global::StartOfRound.Instance.levels.Length; i++)
                {
                    var level = global::StartOfRound.Instance.levels[i];
                    foreach (var e in level.Enemies)
                    {
                        if (!EnemiesByName.ContainsKey(e.enemyType.enemyName))
                        {
                            var newEnemy = new Enemy()
                            {
                                Config = ServerConfiguration.AllEnemiesConfig.Enemies.ContainsKey(e.enemyType.enemyName) ? ServerConfiguration.AllEnemiesConfig.Enemies[e.enemyType.enemyName] : null,
                                EnemyType = e.enemyType
                            };
                            AllEnemies.Add(newEnemy);
                            EnemiesByName.Add(e.enemyType.enemyName, newEnemy);
                        }
                    }
                    foreach (var e in level.OutsideEnemies)
                    {
                        if (!EnemiesByName.ContainsKey(e.enemyType.enemyName))
                        {
                            var newEnemy = new Enemy()
                            {
                                Config = ServerConfiguration.AllEnemiesConfig.Enemies.ContainsKey(e.enemyType.enemyName) ? ServerConfiguration.AllEnemiesConfig.Enemies[e.enemyType.enemyName] : null,
                                EnemyType = e.enemyType
                            };
                            AllEnemies.Add(newEnemy);
                            EnemiesByName.Add(e.enemyType.enemyName, newEnemy);
                        }
                    }
                    foreach (var e in level.DaytimeEnemies)
                    {
                        if (!EnemiesByName.ContainsKey(e.enemyType.enemyName))
                        {
                            var newEnemy = new Enemy()
                            {
                                Config = ServerConfiguration.AllEnemiesConfig.Enemies.ContainsKey(e.enemyType.enemyName) ? ServerConfiguration.AllEnemiesConfig.Enemies[e.enemyType.enemyName] : null,
                                EnemyType = e.enemyType
                            };
                            AllEnemies.Add(newEnemy);
                            EnemiesByName.Add(e.enemyType.enemyName, newEnemy);
                        }
                    }
                    Plugin.Log.LogDebug("Found enemies: " + String.Join(", ", EnemiesByName.Keys));
                }

                AllEnemies = AllEnemies.OrderBy(a => a.EnemyType.enemyName).ToList();

                LobbyConfiguration.AllEnemiesConfig.Enemies.Clear();
                foreach (var enemy in AllEnemies)
                {
                    LobbyConfiguration.AllEnemiesConfig.Enemies.Add(enemy.EnemyType.enemyName, new LobbyConfiguration.EnemyConfig()
                    {
                        Active = true,
                        OverridePowerLevel = false,
                        PowerLevel = enemy.EnemyType.PowerLevel
                    });
                }
                System.IO.File.WriteAllText(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "enemies.json"), LobbyConfiguration.AllEnemiesConfig.ToJSON().ToString(Newtonsoft.Json.Formatting.Indented));
            }

            public static bool IsSpawnable(EnemyType enemy)
            {
                try
                {
                    var config = ServerConfiguration.Instance.Enemies.GetByEnemyName(enemy.enemyName);
                    return config == null || config.Active;
                }
                catch (Exception e)
                {
                    return true;
                }
            }

            public static int GetPowerLevel(EnemyType enemy)
            {
                if (EnemiesByName.ContainsKey(enemy.enemyName))
                    return EnemiesByName[enemy.enemyName].GetPowerLevel(enemy.PowerLevel);
                return enemy.PowerLevel;
            }

        }
    }
}
