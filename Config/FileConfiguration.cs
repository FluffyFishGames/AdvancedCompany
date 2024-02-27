using AdvancedCompany.Objects;
using BepInEx.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace AdvancedCompany.Config
{
    /*
    [Boot.Bootable]
    internal class FileConfiguration
    {
        public class LobbyConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            //public ConfigEntry<bool> Active;
            public ConfigEntry<int> MaxPlayers;
            public ConfigEntry<bool> EnableCosmetics;
            public ConfigEntry<bool> KeepOpen;
            public ConfigEntry<bool> KeepOpenOnMoon;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Server/Lobby.cfg"), false);

                    //Active = ConfigFile.Bind<bool>(new ConfigDefinition("Lobby", "Active"), true, new ConfigDescription("Activates the bigger lobby functionality. Please ensure that all connecting clients have the same settings or else this will lead to desyncs. Other bigger lobby solutions might not work at all or suffer from bugs when playing with more than 4 players."));
                    MaxPlayers = ConfigFile.Bind<int>(new ConfigDefinition("Lobby", "Max players"), 32, new ConfigDescription("The maximum size for the lobby. 32 is the max."));
                    EnableCosmetics = ConfigFile.Bind<bool>(new ConfigDefinition("Lobby", "Enable cosmetics"), true, new ConfigDescription("When deactivated players wont show their cosmetics."));
                    KeepOpen = ConfigFile.Bind<bool>(new ConfigDefinition("Lobby", "Keep open"), true, new ConfigDescription("When activated will keep the Steam lobby open and allow players to join when in orbit."));
                    KeepOpenOnMoon = ConfigFile.Bind<bool>(new ConfigDefinition("Lobby", "Keep open on moons"), false, new ConfigDescription("When activated will keep the Steam lobby even open on moons and allow players to join as spectators."));
                }
            }

            public void Save(LobbyConfiguration.LobbyConfig config)
            {
                MaxPlayers.Value = config.LobbySize;
                EnableCosmetics.Value = config.EnableCosmetics;
                KeepOpen.Value = config.KeepOpen;
                KeepOpenOnMoon.Value = config.KeepOpenOnMoon;
                ConfigFile.Save();
            }
        }

        public class FileConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<bool> SaveInProfile;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Client/File.cfg"), false);

                    SaveInProfile = ConfigFile.Bind<bool>(new ConfigDefinition("Local progress", "Save in profile"), false, new ConfigDescription("If the progress of your player should be saved in the profile folder or globally."));
                }
            }

            public void Save(PlayerConfiguration.FileConfig config)
            {
                SaveInProfile.Value = config.SaveInProfile;
                ConfigFile.Save();
            }
        }

        public class CosmeticsConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<string> Cosmetics;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(BepInEx.Paths.ConfigPath), "plugins/advancedcompany/Cosmetics.cfg"), false);

                    Cosmetics = ConfigFile.Bind<string>(new ConfigDefinition("Cosmetics", "Applied cosmetics"), "", new ConfigDescription("Comma-separated list of cosmetic IDs to apply. It's recommended to use the in-game configuration."));
                }
            }

            public void Save(PlayerConfiguration.CosmeticsConfig config)
            {
                Cosmetics.Value = string.Join(", ", config.ActivatedCosmetics);
                ConfigFile.Save();
            }
        }

        public class UIConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<float> HotbarAlpha;
            public ConfigEntry<float> HotbarScale;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Client/UI.cfg"), false);

                    HotbarAlpha = ConfigFile.Bind<float>(new ConfigDefinition("Hotbar", "Alpha"), 13, new ConfigDescription("How transparent the hotbar becomes when not used or carrying a two handed item. 0% = invisible, 100% = opaque."));
                    HotbarScale = ConfigFile.Bind<float>(new ConfigDefinition("Hotbar", "Scale"), 100, new ConfigDescription("Scale for hotbar slots."));
                }
            }

            public void Save(PlayerConfiguration.HotbarConfig config)
            {
                HotbarAlpha.Value = Mathf.RoundToInt(config.HotbarAlpha * 100f);
                HotbarScale.Value = Mathf.RoundToInt(config.HotbarScale * 100f);
                ConfigFile.Save();
            }
        }
        public class KeybindsConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<bool> InvertScroll;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Client/Keybinds.cfg"), false);

                    InvertScroll = ConfigFile.Bind<bool>(new ConfigDefinition("Inventory", "Invert scroll direction"), true, new ConfigDescription("Invert the scroll direction."));

                }
            }
            public void Save(PlayerConfiguration.HotbarConfig config)
            {
                InvertScroll.Value = config.InvertScroll;
                ConfigFile.Save();
            }
        }

        public class GeneralConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<bool> EnableExtendDeadline;
            //public ConfigEntry<int> DeadlineLength;
            public ConfigEntry<bool> SaveSuitsAfterDeath;
            public ConfigEntry<bool> SaveProgress;
            public ConfigEntry<float> XPMultiplier;
            public ConfigEntry<int> StartingXP;
            public ConfigEntry<int> DayLength;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Server/General.cfg"), false);
                     
                    EnableExtendDeadline = ConfigFile.Bind<bool>(new ConfigDefinition("General", "Enable extend quota"), true, new ConfigDescription("Enables the ability to extend the deadline on the terminal by one day one time per quota."));
                    //DeadlineLength = ConfigFile.Bind<int>(new ConfigDefinition("General", "Deadline length"), 3, new ConfigDescription("The length of the deadline."));
                    SaveSuitsAfterDeath = ConfigFile.Bind<bool>(new ConfigDefinition("General", "Save suits after death"), true, new ConfigDescription("Determinse if bought suits should be saved after not meeting the quota."));
                    SaveProgress = ConfigFile.Bind<bool>(new ConfigDefinition("General", "Save progress"), true, new ConfigDescription("Determinse if progress should be saved by clients and server. When deactivated every player starts with 500XP and every session starts with 0 Ship XP."));
                    XPMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("General", "XP multiplier"), 100f, new ConfigDescription("Defines the multiplier for gained XP. NOTE: Will only be used when Save progress is off."));
                    StartingXP = ConfigFile.Bind<int>(new ConfigDefinition("General", "Starting XP"), 500, new ConfigDescription("Defines the starting XP. NOTE: Will only be used when Save progress is off."));
                    DayLength = ConfigFile.Bind<int>(new ConfigDefinition("General", "Day length"), 700, new ConfigDescription("The length of a day in real-time seconds."));
                }
            }

            public void Save(LobbyConfiguration.GeneralConfig config)
            {
                EnableExtendDeadline.Value = config.EnableExtendDeadline;
                SaveSuitsAfterDeath.Value = config.SaveSuitsAfterDeath;
                SaveProgress.Value = config.SaveProgress;
                XPMultiplier.Value = (int) (config.XPMultiplier * 100f);
                StartingXP.Value = config.StartingXP;
                DayLength.Value = config.DayLength;

                ConfigFile.Save();
            }
        }


        public class GraphicsConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<float> VisionEnhancerBrightness;
            
            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Client/Graphics.cfg"), false);

                    VisionEnhancerBrightness = ConfigFile.Bind<float>(new ConfigDefinition("Vision enhancer", "Brightness"), 70f, new ConfigDescription("Defines the brightness of the vision enhancer in percent."));
                }
            }

            public void Save(PlayerConfiguration.GraphicsConfig config)
            {
                VisionEnhancerBrightness.Value = Mathf.RoundToInt(config.VisionEnhancerBrightness * 100f);
                
                ConfigFile.Save();
            }
        }
        public class MoonConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<int> Price;
            public ConfigEntry<float> ScrapAmountMultiplier;
            public ConfigEntry<float> ScrapValueMultiplier;
            public MoonConfig(BepInEx.Configuration.ConfigFile configFile, string name, int defaultPrice, float defaultScrapAmount, float defaultScrapValue)
            {
                ConfigFile = configFile;
                Price = ConfigFile.Bind<int>(new ConfigDefinition(name, "Price"), defaultPrice, new ConfigDescription("The price for " + name + ".", null));
                ScrapAmountMultiplier = ConfigFile.Bind<float>(new ConfigDefinition(name, "Scrap amount multiplier"), defaultScrapAmount, new ConfigDescription("The scrap amount multiplier for " + name + " in percent.", null));
                ScrapValueMultiplier = ConfigFile.Bind<float>(new ConfigDefinition(name, "Scrap value multiplier"), defaultScrapValue, new ConfigDescription("The scrap value multiplier for " + name + " in percent.", null));
            }

            public virtual void Set(LobbyConfiguration.MoonConfig config)
            {
                Price.Value = config.Price;
                ScrapAmountMultiplier.Value = Mathf.RoundToInt(config.ScrapAmountModifier * 100f);
                ScrapValueMultiplier.Value = Mathf.RoundToInt(config.ScrapValueModifier * 100f);
            }
        }
        public class MoonsConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            public ConfigEntry<bool> ActivateMoonPrices;
            public MoonConfig Experimentation;
            public MoonConfig Vow;
            public MoonConfig Assurance;
            public MoonConfig Offense;
            public MoonConfig March;
            public MoonConfig Rend;
            public MoonConfig Dine;
            public MoonConfig Titan;
            public MoonConfig ActivateMoon;
            public ConfigEntry<string> FreeMoons;

            public ConfigEntry<float> ClearScrapAmountMultiplier;
            public ConfigEntry<float> ClearScrapValueMultiplier;
            public ConfigEntry<float> FoggyScrapAmountMultiplier;
            public ConfigEntry<float> FoggyScrapValueMultiplier;
            public ConfigEntry<float> RainyScrapAmountMultiplier;
            public ConfigEntry<float> RainyScrapValueMultiplier;
            public ConfigEntry<float> FloodedScrapAmountMultiplier;
            public ConfigEntry<float> FloodedScrapValueMultiplier;
            public ConfigEntry<float> StormyScrapAmountMultiplier;
            public ConfigEntry<float> StormyScrapValueMultiplier;
            public ConfigEntry<float> EclipsedScrapAmountMultiplier;
            public ConfigEntry<float> EclipsedScrapValueMultiplier;
            public ConfigEntry<bool> ActivateMultipliers;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Server/Moons.cfg"), false);

                    ActivateMoonPrices = ConfigFile.Bind<bool>(new ConfigDefinition("Moon base prices", "Activate custom prices"), true, new ConfigDescription("Deactivating this option will remove all moon price logic from the mod. (Travel discount perk will automatically be removed)"));

                    Experimentation = new MoonConfig(ConfigFile, "Experimentation", 0, 100, 100);
                    Vow = new MoonConfig(ConfigFile, "Vow", 0, 100, 100);
                    Assurance = new MoonConfig(ConfigFile, "Assurance", 0, 100, 100);
                    Offense = new MoonConfig(ConfigFile, "Offense", 0, 100, 100);
                    March = new MoonConfig(ConfigFile, "March", 0, 100, 100);
                    Rend = new MoonConfig(ConfigFile, "Rend", 550, 100, 100);
                    Dine = new MoonConfig(ConfigFile, "Dine", 600, 100, 100);
                    Titan = new MoonConfig(ConfigFile, "Titan", 700, 100, 100);

                    FreeMoons = ConfigFile.Bind<string>(new ConfigDefinition("Free moons", "JSON"), "[]", new ConfigDescription("Please configure in-game.", null));

                    ActivateMultipliers = ConfigFile.Bind<bool>(new ConfigDefinition("Weather multipliers", "Activate multipliers"), true, new ConfigDescription("Deactivating this option will remove all multiplier logic from the mod."));

                    ClearScrapAmountMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Clear scrap amount"), 100f, new ConfigDescription("The amount of scrap in percent. 100% being vanilla.", null));
                    ClearScrapValueMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Clear scrap value"), 100f, new ConfigDescription("The value of scrap in percent. 100% being vanilla.", null));

                    FoggyScrapAmountMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Foggy scrap amount"), 110f, new ConfigDescription("The amount of scrap in percent. 100% being vanilla.", null));
                    FoggyScrapValueMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Foggy scrap value"), 110f, new ConfigDescription("The value of scrap in percent. 100% being vanilla.", null));

                    RainyScrapAmountMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Rainy scrap amount"), 110f, new ConfigDescription("The amount of scrap in percent. 100% being vanilla.", null));
                    RainyScrapValueMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Rainy scrap value"), 110f, new ConfigDescription("The value of scrap in percent. 100% being vanilla.", null));

                    FloodedScrapAmountMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Flooded scrap amount"), 130f, new ConfigDescription("The amount of scrap in percent. 100% being vanilla.", null));
                    FloodedScrapValueMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Flooded scrap value"), 120f, new ConfigDescription("The value of scrap in percent. 100% being vanilla.", null));

                    StormyScrapAmountMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Stormy scrap amount"), 150f, new ConfigDescription("The amount of scrap in percent. 100% being vanilla.", null));
                    StormyScrapValueMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Stormy scrap value"), 150f, new ConfigDescription("The value of scrap in percent. 100% being vanilla.", null));

                    EclipsedScrapAmountMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Eclipsed scrap amount"), 180f, new ConfigDescription("The amount of scrap in percent. 100% being vanilla.", null));
                    EclipsedScrapValueMultiplier = ConfigFile.Bind<float>(new ConfigDefinition("Weather multipliers", "Eclipsed scrap value"), 150f, new ConfigDescription("The value of scrap in percent. 100% being vanilla.", null));

                }

            }

            public void Save(LobbyConfiguration.MoonsConfig config)
            {
                ActivateMoonPrices.Value = config.ActivateMoonPrices;

                Experimentation.Set(config.Experimentation);
                Vow.Set(config.Vow);
                Assurance.Set(config.Assurance);
                Offense.Set(config.Offense);
                March.Set(config.March);
                Rend.Set(config.Rend);
                Dine.Set(config.Dine);
                Titan.Set(config.Titan);
                var arr = new JArray();
                foreach (var moon in config.FreeMoons)
                {
                    var obj = new JObject();
                    obj["name"] = moon.Name;
                    obj["price"] = moon.Price;
                    obj["scrap_value_modifier"] = moon.ScrapAmountModifier;
                    obj["scrap_amount_modifier"] = moon.ScrapValueModifier;
                    arr.Add(obj);
                }
                FreeMoons.Value = arr.ToString();

                ActivateMultipliers.Value = config.ActivateWeatherMultipliers;
                ClearScrapAmountMultiplier.Value = Mathf.RoundToInt(config.ClearScrapAmountMultiplier * 100f);
                ClearScrapValueMultiplier.Value = Mathf.RoundToInt(config.ClearScrapValueMultiplier * 100f);
                FoggyScrapAmountMultiplier.Value = Mathf.RoundToInt(config.FoggyScrapAmountMultiplier * 100f);
                FoggyScrapValueMultiplier.Value = Mathf.RoundToInt(config.FoggyScrapValueMultiplier * 100f);
                RainyScrapAmountMultiplier.Value = Mathf.RoundToInt(config.RainyScrapAmountMultiplier * 100f);
                RainyScrapValueMultiplier.Value = Mathf.RoundToInt(config.RainyScrapValueMultiplier * 100f);
                FloodedScrapAmountMultiplier.Value = Mathf.RoundToInt(config.FloodedScrapAmountMultiplier * 100f);
                FloodedScrapValueMultiplier.Value = Mathf.RoundToInt(config.FloodedScrapValueMultiplier * 100f);
                StormyScrapAmountMultiplier.Value = Mathf.RoundToInt(config.StormyScrapAmountMultiplier * 100f);
                StormyScrapValueMultiplier.Value = Mathf.RoundToInt(config.StormyScrapValueMultiplier * 100f);
                EclipsedScrapAmountMultiplier.Value = Mathf.RoundToInt(config.EclipsedScrapAmountMultiplier * 100f);
                EclipsedScrapValueMultiplier.Value = Mathf.RoundToInt(config.EclipsedScrapValueMultiplier * 100f);

                ConfigFile.Save();
            }
        }

        public class ItemsConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;
            
            public ItemConfig MissileLauncher;
            public ItemConfig RocketBoots;
            public ItemConfig Flippers;
            public NightVisionConfig VisionEnhancer;
            public BulletProofVestConfig BulletProofVest;
            public ItemConfig LightningRod;
            public ItemConfig HelmetLamp;
            public ItemConfig Headset;

            // vanilla items
            public ItemConfig RadarBooster;
            public ItemConfig ZapGun;
            public ItemConfig WalkieTalkie;
            public ItemConfig TZPInhalant;
            public ItemConfig ExtensionLadder;
            public ItemConfig StunGrenade;
            public ItemConfig Shovel;
            public ItemConfig ProFlashlight;
            public ItemConfig Lockpicker;
            public ItemConfig Jetpack;
            public ItemConfig Flashlight;
            public ItemConfig Boombox;
            public ItemConfig SprayPaint;

            public ConfigEntry<string> FreeItems;

            public class ItemConfig 
            {
                public ConfigEntry<bool> Active;
                public ConfigEntry<int> Price;
                public ConfigEntry<int> MaxDiscount;
                public ConfigEntry<int> Weight;

                public ItemConfig(BepInEx.Configuration.ConfigFile configFile, string section, int defaultPrice, int defaultMaxDiscount, int defaultWeight)
                {
                    Active = configFile.Bind<bool>(new ConfigDefinition(section, "Active"), true, new ConfigDescription("Determines if this item is purchaseable.", null));
                    Price = configFile.Bind<int>(new ConfigDefinition(section, "Price"), defaultPrice, new ConfigDescription("The price of this item.", null));
                    MaxDiscount = configFile.Bind<int>(new ConfigDefinition(section, "Max discount"), defaultMaxDiscount, new ConfigDescription("The max discount of this item in percent.", null));
                    Weight = configFile.Bind<int>(new ConfigDefinition(section, "Weight"), defaultWeight, new ConfigDescription("The weight of this item in lbs.", null));
                }

                public virtual void Set(LobbyConfiguration.ItemConfig config)
                {
                    Active.Value = config.Active;
                    Price.Value = config.Price;
                    MaxDiscount.Value = config.MaxDiscount;
                    Weight.Value = Mathf.RoundToInt((config.Weight - 1f) * 105f);
                }
            }

            public class NightVisionConfig : ItemConfig
            {
                public ConfigEntry<float> BatteryTime;

                public NightVisionConfig(BepInEx.Configuration.ConfigFile configFile, string section, int defaultPrice, int defaultMaxDiscount, int defaultWeight) : base(configFile, section, defaultPrice, defaultMaxDiscount, defaultWeight)
                {
                    BatteryTime = configFile.Bind<float>(new ConfigDefinition(section, "Battery time"), 180f, new ConfigDescription("The time in seconds of charge.", null));
                }

                public override void Set(LobbyConfiguration.ItemConfig config)
                {
                    base.Set(config);
                    if (config is LobbyConfiguration.NightVisionConfig nightVision)
                        BatteryTime.Value = nightVision.BatteryTime;
                }
            }

            public class BulletProofVestConfig : ItemConfig
            {
                public ConfigEntry<int> MaxDamage;
                public ConfigEntry<int> TurretDamage;
                public ConfigEntry<int> ShotgunDamage;
                public ConfigEntry<float> DamageReductionAtFullHealth;
                public ConfigEntry<float> DamageReductionAtNoHealth;
                public ConfigEntry<bool> DestroyAtNoHealth;

                public BulletProofVestConfig(BepInEx.Configuration.ConfigFile configFile, string section, int defaultPrice, int defaultMaxDiscount, int defaultWeight) : base(configFile, section, defaultPrice, defaultMaxDiscount, defaultWeight)
                {
                    MaxDamage = configFile.Bind<int>(new ConfigDefinition(section, "Max damage"), 90, new ConfigDescription("The maximum amount of damage the vest can withstand.", null));
                    TurretDamage = configFile.Bind<int>(new ConfigDefinition(section, "Turret damage"), 5, new ConfigDescription("The damage a turret bullet will deal to the vest.", null));
                    ShotgunDamage = configFile.Bind<int>(new ConfigDefinition(section, "Shotgun damage"), 30, new ConfigDescription("The damage a shotgun bullet will deal to the vest.", null));
                    DamageReductionAtFullHealth = configFile.Bind<float>(new ConfigDefinition(section, "Damage reduction at full health"), 100f, new ConfigDescription("The damage reduction the vest will have when at full health in percent.", null));
                    DamageReductionAtNoHealth = configFile.Bind<float>(new ConfigDefinition(section, "Damage reduction at no health"), 50f, new ConfigDescription("The damage reduction the vest will have when at zero health in percent.", null));
                    DestroyAtNoHealth = configFile.Bind<bool>(new ConfigDefinition(section, "Destroy at no health"), true, new ConfigDescription("Should the vest get destroyed when reaching its damage reaches max damage.", null));
                }

                public override void Set(LobbyConfiguration.ItemConfig config)
                {
                    base.Set(config);
                    if (config is LobbyConfiguration.BulletProofVestConfig bulletProof)
                    {
                        MaxDamage.Value = bulletProof.MaxDamage;
                        TurretDamage.Value = bulletProof.TurretDamage;
                        ShotgunDamage.Value = bulletProof.ShotgunDamage;
                        DamageReductionAtFullHealth.Value = Mathf.RoundToInt(bulletProof.DamageReductionAtFullHealth * 100f);
                        DamageReductionAtNoHealth.Value = Mathf.Round(bulletProof.DamageReductionAtNoHealth * 100f);
                        DestroyAtNoHealth.Value = bulletProof.DestroyAtNoHealth;
                    }
                }
            }

            // vanilla items

            private void AddItem(string section, int defaultPrice, int defaultMaxDiscount, int defaultWeight, ref ConfigEntry<bool> activeEntry, ref ConfigEntry<int> priceEntry, ref ConfigEntry<int> maxDiscountEntry, ref ConfigEntry<int> weightEntry)
            {
                activeEntry = ConfigFile.Bind<bool>(new ConfigDefinition(section, "Active"), true, new ConfigDescription("Determines if this item is purchaseable.", null));
                priceEntry = ConfigFile.Bind<int>(new ConfigDefinition(section, "Price"), defaultPrice, new ConfigDescription("The price of this item.", null));
                maxDiscountEntry = ConfigFile.Bind<int>(new ConfigDefinition(section, "Max discount"), defaultMaxDiscount, new ConfigDescription("The max discount of this item in percent.", null));
                weightEntry = ConfigFile.Bind<int>(new ConfigDefinition(section, "Weight"), defaultWeight, new ConfigDescription("The weight of this item in lbs.", null));
            }

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Server/Items.cfg"), false);

                    MissileLauncher = new ItemConfig(ConfigFile, "Missile launcher", 150, 50, 8);
                    RocketBoots = new ItemConfig(ConfigFile, "Rocket boots", 100, 50, 5);
                    Flippers = new ItemConfig(ConfigFile, "Flippers", 80, 40, 5);
                    VisionEnhancer = new NightVisionConfig(ConfigFile, "Vision enhancer", 200, 50, 8);
                    BulletProofVest = new BulletProofVestConfig(ConfigFile, "Bulletproof vest", 100, 50, 15);
                    LightningRod = new ItemConfig(ConfigFile, "Lightning rod", 120, 50, 50);
                    HelmetLamp = new ItemConfig(ConfigFile, "Helmet lamp", 50, 70, 4);
                    Headset = new ItemConfig(ConfigFile, "Headset", 50, 70, 4);

                    Boombox = new ItemConfig(ConfigFile, "Boombox", 60, 80, 16);
                    Flashlight = new ItemConfig(ConfigFile, "Flashlight", 15, 80, 0);
                    Jetpack = new ItemConfig(ConfigFile, "Jetpack", 700, 80, 52);
                    Lockpicker = new ItemConfig(ConfigFile, "Lockpicker", 20, 80, 16);
                    ProFlashlight = new ItemConfig(ConfigFile, "Pro flashlight", 25, 80, 5);
                    Shovel = new ItemConfig(ConfigFile, "Shovel", 30, 80, 8);
                    StunGrenade = new ItemConfig(ConfigFile, "Stun grenade", 30, 80, 5);
                    ExtensionLadder = new ItemConfig(ConfigFile, "Extension ladder", 60, 60, 0);
                    TZPInhalant = new ItemConfig(ConfigFile, "TZP inhalant", 120, 80, 0);
                    WalkieTalkie = new ItemConfig(ConfigFile, "Walkie talkie", 12, 80, 0);
                    ZapGun = new ItemConfig(ConfigFile, "Zap gun", 400, 80, 10);
                    RadarBooster = new ItemConfig(ConfigFile, "Radar booster", 60, 80, 19);
                    SprayPaint = new ItemConfig(ConfigFile, "Spray paint", 50, 80, 0);

                    FreeItems = ConfigFile.Bind<string>(new ConfigDefinition("Free items", "JSON"), "[]", new ConfigDescription("Please configure in-game.", null));
                }
            }

            public void Save(LobbyConfiguration.ItemsConfig config)
            {
                MissileLauncher.Set(config.MissileLauncher);
                RocketBoots.Set(config.RocketBoots);
                Flippers.Set(config.Flippers);
                VisionEnhancer.Set(config.VisionEnhancer);
                BulletProofVest.Set(config.BulletProofVest);
                LightningRod.Set(config.LightningRod);
                HelmetLamp.Set(config.HelmetLamp);
                Headset.Set(config.Headset);

                Boombox.Set(config.Boombox);
                Flashlight.Set(config.Flashlight);
                Jetpack.Set(config.Jetpack);
                Lockpicker.Set(config.Lockpicker);
                ProFlashlight.Set(config.ProFlashlight);
                Shovel.Set(config.Shovel);
                StunGrenade.Set(config.StunGrenade);
                ExtensionLadder.Set(config.ExtensionLadder);
                TZPInhalant.Set(config.TZPInhalant);
                WalkieTalkie.Set(config.WalkieTalkie);
                ZapGun.Set(config.ZapGun);
                RadarBooster.Set(config.RadarBooster);
                SprayPaint.Set(config.SprayPaint);

                var arr = new JArray();
                foreach (var item in config.FreeItems)
                {
                    var obj = new JObject();
                    obj["active"] = item.Active;
                    obj["name"] = item.Name;
                    obj["price"] = item.Price;
                    obj["weight"] = item.Weight;
                    obj["max_discount"] = item.MaxDiscount;
                    arr.Add(obj);
                }
                FreeItems.Value = arr.ToString();
                ConfigFile.Save();
            }
        }

        public class PerkConfig
        {
            public ConfigEntry<bool> Active;
            public ConfigEntry<float> Base;
            public ConfigEntry<float> Change;
            public ConfigEntry<string> Prices;

            public PerkConfig(BepInEx.Configuration.ConfigFile configFile, string section, int[] prices, float defaultBase, float defaultChange, string changeDescription, bool overrideValues = false)
            {
                Active = configFile.Bind<bool>(new ConfigDefinition(section, "Active"), true, new ConfigDescription("Determines if this perk is purchaseable.", null));
                Base = configFile.Bind<float>(new ConfigDefinition(section, "Base value"), defaultBase, new ConfigDescription("The base value every player starts with in percent.", null));

                Change = configFile.Bind<float>(new ConfigDefinition(section, "Change"), defaultChange, new ConfigDescription(changeDescription, null));
                Prices = configFile.Bind<string>(new ConfigDefinition(section, "Costs"), String.Join(", ", prices), new ConfigDescription("Comma separated list: The XP price per level. The number of values determine the max level.", null));

                if (overrideValues)
                {
                    Active.Value = (bool) Active.DefaultValue;
                    Base.Value = (float) Base.DefaultValue;
                    Change.Value = (float) Change.DefaultValue;
                    Prices.Value = (string) Prices.DefaultValue;
                }
            }

            public void Set(LobbyConfiguration.PerkConfiguration config)
            {
                Active.Value = config.Active;
                Base.Value = config.Base * 100f;
                Change.Value = config.Change * 100f;
                Prices.Value = string.Join(", ", config.Prices);
            }
        }

        public class PlayerPerksConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;

            public PerkConfig SprintSpeed;
            public PerkConfig JumpHeight;
            public PerkConfig JumpStamina;
            public PerkConfig SprintStamina;
            public PerkConfig FallDamage;
            public PerkConfig Damage;
            public PerkConfig Weight;
            public PerkConfig DealDamage;
            public PerkConfig ClimbingSpeed;

            public ConfigEntry<bool> InventoryActive;
            public ConfigEntry<int> InventoryBase;
            public ConfigEntry<string> InventoryPrices;
            public ConfigEntry<string> Version;
            
            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Server/PlayerPerks.cfg"), false);

                    Version = ConfigFile.Bind<string>(new ConfigDefinition("Z", "Version"), "", new ConfigDescription("Do not change this value. Its for updating the configuration when there is a bug."));
                    var oldVersion = Version.Value;
                    var newVersion = Plugin.Version;
                    Version.Value = newVersion;

                    SprintSpeed = new PerkConfig(ConfigFile,
                        "Sprint speed", 
                        new int[] { 150, 200, 250, 400, 500 }, 
                        100f,
                        10f, 
                        "The change in max sprint speed per perk level in percent. (Result: 100% + value * level)");
                    JumpHeight = new PerkConfig(ConfigFile,
                        "Jump height", 
                        new int[] { 100, 100, 150, 200, 250 }, 
                        100f,
                        5f, 
                        "The change in jump force per perk level in percent. (Result: 100% + value * level)");
                    JumpStamina = new PerkConfig(ConfigFile,
                        "Jump stamina", 
                        new int[] { 100, 100, 150, 200, 250, 300 }, 
                        100f,
                        10f, 
                        "The change in stamina cost per perk level in percent. (Result: 100% - value * level)");
                    SprintStamina = new PerkConfig(ConfigFile,
                        "Sprint stamina", 
                        new int[] { 150, 300, 500, 750, 1000, 1500 }, 
                        100f,
                        10f, 
                        "The change in stamina cost per perk level in percent. (Result: 100% - value * level)");
                    FallDamage = new PerkConfig(ConfigFile,
                        "Fall damage reduction", 
                        new int[] { 100, 200, 300, 400, 500 }, 
                        100f,
                        15f, 
                        "The change in surviveable fall height per perk level in percent. (Result: 100% + value * level)");
                    Damage = new PerkConfig(ConfigFile,
                        "Damage reduction", 
                        new int[] { 150, 300, 500, 750, 1000, 1250, 1500 }, 
                        100f,
                        10f, 
                        "The change in damage received (not including fall damage) per perk level in percent. (Result: 100% - value * level)");
                    Weight = new PerkConfig(ConfigFile,
                        "Weight influence", 
                        new int[] { 150, 300, 500, 750, 1000 }, 
                        100f,
                        10f, 
                        "The change in weight influencing speed per perk level in percent. (Result: 100% - value * level)");
                    DealDamage = new PerkConfig(ConfigFile,
                        "Critical strike chance", 
                        new int[] { 150, 200, 250, 350, 500, 750, 1000, 1250 }, 
                        0f,
                        10f, 
                        "The change in chance of hitting an enemy with a critical strike (one-hit kill) per perk level in percent. (Result: 0% + value * level)", oldVersion == "" && newVersion != "");
                    ClimbingSpeed = new PerkConfig(ConfigFile,
                        "Climbing speed",
                        new int[] { 50, 100, 150, 200, 250, 300, 350, 400 },
                        100f,
                        10f,
                        "The change in climbing speed per perk level in percent. (Result: 100% + value * level)");

                    InventoryActive = ConfigFile.Bind<bool>(new ConfigDefinition("Inventory size", "Active"), true, new ConfigDescription("Determines if this perk is purchaseable.", null));
                    InventoryBase = ConfigFile.Bind<int>(new ConfigDefinition("Inventory size", "Start size"), 3, new ConfigDescription("The starting inventory size.", null));
                    InventoryPrices = ConfigFile.Bind<string>(new ConfigDefinition("Inventory size", "Costs"), "500, 1000, 1500, 2500, 3500, 5000, 7500", new ConfigDescription("Comma separated list: The XP price per level. The number of values determine the max level. Please note: Starting inventory size + max inventory upgrade can not exceed 7 slots.", null));
                }

            }

            public void Save(LobbyConfiguration.PlayerPerksConfig config)
            {
                SprintSpeed.Set(config.SprintSpeed);
                JumpHeight.Set(config.JumpHeight);
                JumpStamina.Set(config.JumpStamina);
                SprintStamina.Set(config.SprintStamina);
                FallDamage.Set(config.FallDamage);
                Damage.Set(config.Damage);
                Weight.Set(config.Weight);
                DealDamage.Set(config.DealDamage);
                ClimbingSpeed.Set(config.ClimbingSpeed);
                InventoryActive.Value = config.InventorySlots.Active;
                InventoryBase.Value = (int) config.InventorySlots.Base;
                InventoryPrices.Value = string.Join(", ", config.InventorySlots.Prices);

                ConfigFile.Save();
            }
        }
        public class ShipPerksConfig
        {
            private BepInEx.Configuration.ConfigFile ConfigFile;

            public PerkConfig ScanDistance;
            public PerkConfig ExtraBattery;
            public PerkConfig ExtendDeadlineDiscount;
            public PerkConfig LandingSpeed;
            public PerkConfig DeliverySpeed;
            public PerkConfig SaveLoot;
            public PerkConfig TravelDiscount;
            public ConfigEntry<string> Version;

            public void LoadFromConfig()
            {
                if (ConfigFile == null)
                {
                    ConfigFile = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany/Server/ShipPerks.cfg"), false);

                    Version = ConfigFile.Bind<string>(new ConfigDefinition("Z", "Version"), "", new ConfigDescription("Do not change this value. Its for updating the configuration when there is a bug."));
                    var oldVersion = Version.Value;
                    var newVersion = Plugin.Version;
                    Version.Value = newVersion;

                    ScanDistance = new PerkConfig(ConfigFile,
                        "Scan distance",
                        new int[] { 50, 100, 150, 200, 300, 400, 500, 600, 700, 800 },
                        100f,
                        10f,
                        "The change in max scan distance per perk level in percent. (Result: 100% + value * level)");
                    ExtraBattery = new PerkConfig(ConfigFile,
                        "Extra battery",
                        new int[] { 100, 150, 200, 250, 300, 350, 400, 500 },
                        100f,
                        7.5f,
                        "The change in battery charge per perk level in percent. (Result: 100% + value * level)");
                    ExtendDeadlineDiscount = new PerkConfig(ConfigFile,
                        "Extend deadline discount",
                        new int[] { 200, 300, 400, 500, 750, 1000, 1500, 2000 },
                        100f,
                        10f,
                        "The change in price to extend deadline in percent. (Result: 100% - value * level)");
                    LandingSpeed = new PerkConfig(ConfigFile,
                        "Landing speed",
                        new int[] { 50, 100, 200, 300, 400, 500, 600, 700, 800, 1000 },
                        100f,
                        10f,
                        "The change in duration to land per perk level in percent. (Result: 100% - value * level)");
                    DeliverySpeed = new PerkConfig(ConfigFile,
                        "Delivery speed",
                        new int[] { 50, 100, 200, 300, 400, 500, 600, 700, 800, 1000 },
                        100f,
                        10f,
                        "The change in duration for deliveries per perk level in percent. (Result: 100% - value * level)");
                    SaveLoot = new PerkConfig(ConfigFile,
                        "Loot saver",
                        new int[] { 200, 300, 400, 500, 750, 1000, 1500, 2000 },
                        0f,
                        10f,
                        "The change in loot saved in case of all players dying per perk level in percent. (Result: 0% + value * level)",
                        oldVersion == "" && newVersion != "");
                    TravelDiscount = new PerkConfig(ConfigFile,
                        "Travel discount",
                        new int[] { 200, 400, 600, 800, 1000, 1250, 1500, 2000 },
                        100f,
                        7.5f,
                        "The change in price to travel to moons per perk level in percent. (Result: 100% - value * level)");
                }
            }

            public void Save(LobbyConfiguration.ShipPerksConfig config)
            {
                ScanDistance.Set(config.ScanDistance);
                ExtraBattery.Set(config.ExtraBattery);
                ExtendDeadlineDiscount.Set(config.ExtendDeadlineDiscount);
                LandingSpeed.Set(config.LandingSpeed);
                DeliverySpeed.Set(config.DeliverySpeed);
                SaveLoot.Set(config.SaveLoot);
                TravelDiscount.Set(config.TravelDiscount);

                ConfigFile.Save();
            }
        }

        public static FileConfiguration Instance;

        public UIConfig UI = new();
        public FileConfig File = new();
        public KeybindsConfig Keybinds = new();
        public GeneralConfig General = new();
        public MoonsConfig Moons = new();
        public PlayerPerksConfig PlayerPerks = new();
        public ShipPerksConfig ShipPerks = new();
        public ItemsConfig Items = new();
        public LobbyConfig Lobby = new();
        public CosmeticsConfig Cosmetics = new();
        public GraphicsConfig Graphics = new();
        public static void Boot()
        {
            Instance = new FileConfiguration();
            Instance.Lobby.LoadFromConfig();
            Instance.PlayerPerks.LoadFromConfig();
            Instance.ShipPerks.LoadFromConfig();
            Instance.Items.LoadFromConfig();
            Instance.Moons.LoadFromConfig();
            Instance.General.LoadFromConfig();
            Instance.Keybinds.LoadFromConfig();
            Instance.UI.LoadFromConfig();
            Instance.File.LoadFromConfig();
            Instance.Cosmetics.LoadFromConfig();
            Instance.Graphics.LoadFromConfig();
            //Plugin.Instance.Config.Bind<int[]>(new BepInEx.Configuration.ConfigDefinition("PlayerPerks", "SprintSpeed"), new int[] { 100 }, new BepInEx.Configuration.ConfigDescription());
        }

        public static void Save(LobbyConfiguration config)
        {
            Instance.Lobby.Save(config.Lobby);
            Instance.PlayerPerks.Save(config.PlayerPerks);
            Instance.ShipPerks.Save(config.ShipPerks);
            Instance.Items.Save(config.Items);
            Instance.Moons.Save(config.Moons);
            Instance.General.Save(config.General);
        }


        public static void Save(PlayerConfiguration config)
        {
            Instance.UI.Save(config.Hotbar);
            Instance.Keybinds.Save(config.Hotbar);
            Instance.Cosmetics.Save(config.Cosmetics);
            Instance.File.Save(config.File);
            Instance.Graphics.Save(config.Graphics);
        }
    }*/
}
