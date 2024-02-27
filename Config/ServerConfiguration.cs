using AdvancedCompany.Game;
using AdvancedCompany.Objects;
using BepInEx.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static Mono.Security.X509.X520;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace AdvancedCompany.Config
{
    [Boot.Bootable]
    internal class ServerConfiguration : LobbyConfiguration
    {
        public static ServerConfiguration Instance;

        public void WasLoaded()
        {
            if (General.DeactivateHotbar)
            {
                Plugin.Log.LogError("╔══════════════════════════════════════════════════╗");
                Plugin.Log.LogError("║ HOTBAR CHANGES DEACTIVATED!                      ║");
                Plugin.Log.LogError("╟──────────────────────────────────────────────────╢");
                Plugin.Log.LogError("║ This notice serves as an indicator that you or   ║");
                Plugin.Log.LogError("║ the host of the game you've joined have          ║");
                Plugin.Log.LogError("║ deactivated a core module of AdvancedCompany and ║");
                Plugin.Log.LogError("║ won't get support or help from AdvancedCompanys  ║");
                Plugin.Log.LogError("║ developer nor are eligible to submit issues on   ║");
                Plugin.Log.LogError("║ AdvancedCompanys GitHub repository.              ║");
                Plugin.Log.LogError("╟──────────────────────────────────────────────────╢");
                Plugin.Log.LogError("║ Removing this notice and opening an issue will   ║");
                Plugin.Log.LogError("║ result in a block on GitHub.                     ║");
                Plugin.Log.LogError("╚══════════════════════════════════════════════════╝");

                Items.Items["Headset"].Active = false;
                Items.Items["Tactical helmet"].Active = false;
                Items.Items["Helmet lamp"].Active = false;
                Items.Items["Vision enhancer"].Active = false;
                Items.Items["Flippers"].Active = false;
                Items.Items["Rocket boots"].Active = false;
                Items.Items["Bulletproof vest"].Active = false;

                Items.Scrap["Light shoes"].Active = false;
                Items.Scrap["Bunny ears"].Active = false;

                PlayerPerks.InventorySlots.Active = false;
            }
        }
        public static void Boot()
        {
            var moonsFile = System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "moons.json");
            var itemsFile = System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "items.json");
            var enemiesFile = System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "enemies.json");
            var scrapFile = System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "scrap.json");
            var unlockablesFile = System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "unlockables.json");

            if (System.IO.File.Exists(itemsFile))
            {
                try
                {
                    LobbyConfiguration.AllItemsConfig.FromJSON(JObject.Parse(System.IO.File.ReadAllText(itemsFile)), true);
                    LobbyConfiguration.AllItemsConfig.Build();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while loading items:");
                    Plugin.Log.LogError(e);
                }
            }
            if (System.IO.File.Exists(enemiesFile))
            {
                try
                {
                    LobbyConfiguration.AllEnemiesConfig.FromJSON(JObject.Parse(System.IO.File.ReadAllText(enemiesFile)), true);
                    LobbyConfiguration.AllEnemiesConfig.Build();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while loading enemies:");
                    Plugin.Log.LogError(e);
                }
            }
            if (System.IO.File.Exists(scrapFile))
            {
                try
                {
                    LobbyConfiguration.AllScrapConfig.FromJSON(JObject.Parse(System.IO.File.ReadAllText(scrapFile)), true);
                    LobbyConfiguration.AllScrapConfig.Build();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while loading scrap:");
                    Plugin.Log.LogError(e);
                }
            }
            if (System.IO.File.Exists(unlockablesFile))
            {
                try
                {
                    LobbyConfiguration.AllUnlockablesConfig.FromJSON(JObject.Parse(System.IO.File.ReadAllText(unlockablesFile)), true);
                    LobbyConfiguration.AllUnlockablesConfig.Build();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while loading unlockables:");
                    Plugin.Log.LogError(e);
                }
            }
            if (System.IO.File.Exists(moonsFile))
            {
                try
                {
                    LobbyConfiguration.AllMoonsConfig.FromJSON(JObject.Parse(System.IO.File.ReadAllText(moonsFile)), true);
                    foreach (var kv in LobbyConfiguration.AllMoonsConfig.Moons)
                    {
                        foreach (var kv2 in LobbyConfiguration.AllScrapConfig.Items)
                        {
                            if (!kv.Value.LootTable.ContainsKey(kv2.Key))
                            {
                                kv.Value.LootTable.Add(kv2.Key, new MoonConfig.LootTableItem() { Override = false, Rarity = 0 });
                            }
                        }
                        foreach (var kv2 in LobbyConfiguration.AllEnemiesConfig.Enemies)
                        {
                            if (!kv.Value.DaytimeEnemies.ContainsKey(kv2.Key))
                            {
                                kv.Value.DaytimeEnemies.Add(kv2.Key, new MoonConfig.EnemyTableItem() { Override = false, Rarity = 0 });
                            }
                            if (!kv.Value.OutsideEnemies.ContainsKey(kv2.Key))
                            {
                                kv.Value.OutsideEnemies.Add(kv2.Key, new MoonConfig.EnemyTableItem() { Override = false, Rarity = 0 });
                            }
                            if (!kv.Value.InsideEnemies.ContainsKey(kv2.Key))
                            {
                                kv.Value.InsideEnemies.Add(kv2.Key, new MoonConfig.EnemyTableItem() { Override = false, Rarity = 0 });
                            }
                        }
                    }
                    LobbyConfiguration.AllMoonsConfig.Build();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while loading moons:");
                    Plugin.Log.LogError(e);
                }
            }

            Instance = new ServerConfiguration();
        }
        /*
        public override void CopyFrom(BaseConfiguration baseConfig)
        {
            base.CopyFrom(baseConfig);
            for (var i = 0; i < this.Items.FreeItems.Count; i++)
            {
                if (this.Items.FreeItems[i].Name.Trim() == "")
                {
                    this.Items.FreeItems.RemoveAt(i);
                    i++;
                }
            }
            for (var i = 0; i < this.Moons.FreeMoons.Count; i++)
            {
                if (this.Moons.FreeMoons[i].Name.Trim() == "")
                {
                    this.Moons.FreeMoons.RemoveAt(i);
                    i++;
                }
            }
        }*/
    }
}
