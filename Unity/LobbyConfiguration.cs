using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace AdvancedCompany.Config
{
    public class LobbyConfiguration : Configuration
    {
        internal static AllMoons AllMoonsConfig = new AllMoons();
        internal static AllItems AllItemsConfig = new AllItems();
        internal static AllUnlockables AllUnlockablesConfig = new AllUnlockables();
        internal static AllEnemies AllEnemiesConfig = new AllEnemies();
        internal static AllScrap AllScrapConfig = new AllScrap();

        public class LobbyConfig : Configuration
        {
            [Slider(4f, 32f, ShowValue = true, InputWidth = 35f)]
            public int LobbySize = 8;
            public bool KeepOpen = true;
            public bool KeepOpenOnMoon = false;
        }

        public class GeneralConfig : Configuration
        {
            [Slider(0.1f, 10f, ShowValue = true, Conversion = 100f, InputWidth = 35f)]
            public float GlobalScrapAmountMultiplier = 1f;
            [Slider(0.1f, 10f, ShowValue = true, Conversion = 100f, InputWidth = 35f)]
            public float GlobalScrapValueMultiplier = 1f;
            [Slider(0.1f, 10f, ShowValue = true, Conversion = 100f, InputWidth = 35f)]
            public float GlobalMaxPowerMultiplier = 1f;
            public int DeadlineLength = 3;
            public int StartCredits = 60;
            public bool ActivatePortableTerminal = true;
            public bool EnableCosmetics = true;
            public bool EnableExtendDeadline = true;
            public bool SaveSuitsAfterDeath = true;
            public bool SaveProgress = true;
            public bool IndividualXP = true;
            public bool DeactivateHotbar = false;
            public bool ResetXP = true;
            [Slider(0.1f, 10f, ShowValue = true, Conversion = 100f, InputWidth = 35f)]
            public float XPMultiplier = 1;
            public int StartingXP = 500;
            public int StartingShipXP = 0;
            [Slider(100f, 3500f, ShowValue = true, InputWidth = 35f)]
            public int DayLength = 700;
        }

        public class EnemyConfig : Configuration
        {
            public bool Active = true;
            public bool OverridePowerLevel;
            public int PowerLevel;
        }

        public class MoonConfig : Configuration
        {
            public bool OverridePrice;
            public int Price;
            public bool OverrideDungeonSize;
            public float DungeonSize;
            
            public bool OverrideMinScrapAmount;
            public int MinScrapAmount;
            public bool OverrideMaxScrapAmount;
            public int MaxScrapAmount;
            public bool OverrideScrapAmountModifier;
            public float ScrapAmountModifier = 1f;
            
            public bool OverrideScrapValueModifier;
            public float ScrapValueModifier = 1f;
            
            public Dictionary<string, LootTableItem> LootTable = new Dictionary<string, LootTableItem>();

            public bool OverrideDaytimeEnemiesMaxPower;
            public int DaytimeEnemiesMaxPower;
            public bool OverrideDaytimeEnemiesProbability;
            public float DaytimeEnemiesProbability;
            public Dictionary<string, EnemyTableItem> DaytimeEnemies = new Dictionary<string, EnemyTableItem>();

            public bool OverrideOutsideEnemiesMaxPower;
            public int OutsideEnemiesMaxPower;
            public Dictionary<string, EnemyTableItem> OutsideEnemies = new Dictionary<string, EnemyTableItem>();

            public bool OverrideInsideEnemiesMaxPower;
            public int InsideEnemiesMaxPower;
            public bool OverrideInsideEnemiesProbability;
            public float InsideEnemiesProbability;
            public Dictionary<string, EnemyTableItem> InsideEnemies = new Dictionary<string, EnemyTableItem>();

            public class LootTableItem : Configuration
            {
                public bool Override;
                public int Rarity;
            }
            public class EnemyTableItem : Configuration
            {
                public bool Override;
                public int Rarity;
            }
        }

        public class FreeMoonConfig : MoonConfig
        {
            public string Name;
        }

        public class WeatherConfig : Configuration
        {
            public bool OverrideScrapAmountMultiplier;
            public float ScrapAmountMultiplier;
            public bool OverrideScrapValueMultiplier;
            public float ScrapValueMultiplier;

            protected override void SetDefaults()
            {
                base.SetDefaults();
                this.SetDefault(nameof(OverrideScrapAmountMultiplier), false);
                this.SetDefault(nameof(ScrapAmountMultiplier), 1f);
                this.SetDefault(nameof(OverrideScrapValueMultiplier), false);
                this.SetDefault(nameof(ScrapValueMultiplier), 1f);
            }
        }

        public class MoonsConfig : Configuration
        {
            public MoonsConfig()
            {
                LoadedFromJSON();
            }

            public override void LoadedFromJSON()
            {
                foreach (var kv in AllMoonsConfig.Moons)
                {
                    if (!Moons.ContainsKey(kv.Key))
                        Moons[kv.Key] = (MoonConfig)kv.Value._Clone();

                    Moons[kv.Key].Field("OverrideDaytimeEnemiesMaxPower").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideDaytimeEnemiesProbability").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideDungeonSize").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideInsideEnemiesMaxPower").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideInsideEnemiesProbability").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideMaxScrapAmount").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideMinScrapAmount").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideOutsideEnemiesMaxPower").DefaultValue = false;
                    Moons[kv.Key].Field("OverridePrice").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideScrapAmountModifier").DefaultValue = false;
                    Moons[kv.Key].Field("OverrideScrapValueModifier").DefaultValue = false;

                    Moons[kv.Key].Field("DaytimeEnemiesMaxPower").DefaultValue = kv.Value.DaytimeEnemiesMaxPower;
                    Moons[kv.Key].Field("DaytimeEnemiesProbability").DefaultValue = kv.Value.DaytimeEnemiesProbability;
                    Moons[kv.Key].Field("DungeonSize").DefaultValue = kv.Value.DungeonSize;
                    Moons[kv.Key].Field("InsideEnemiesMaxPower").DefaultValue = kv.Value.InsideEnemiesMaxPower;
                    Moons[kv.Key].Field("InsideEnemiesProbability").DefaultValue = kv.Value.InsideEnemiesProbability;
                    Moons[kv.Key].Field("MaxScrapAmount").DefaultValue = kv.Value.MaxScrapAmount;
                    Moons[kv.Key].Field("MinScrapAmount").DefaultValue = kv.Value.MinScrapAmount;
                    Moons[kv.Key].Field("OutsideEnemiesMaxPower").DefaultValue = kv.Value.OutsideEnemiesMaxPower;
                    Moons[kv.Key].Field("Price").DefaultValue = kv.Value.Price;
                    Moons[kv.Key].Field("ScrapAmountModifier").DefaultValue = kv.Value.ScrapAmountModifier;
                    Moons[kv.Key].Field("ScrapValueModifier").DefaultValue = kv.Value.ScrapValueModifier;

                    foreach (var kv2 in kv.Value.DaytimeEnemies)
                    {
                        if (Moons[kv.Key].DaytimeEnemies.ContainsKey(kv2.Key))
                        {
                            Moons[kv.Key].DaytimeEnemies[kv2.Key].Field("Override").DefaultValue = false;
                            Moons[kv.Key].DaytimeEnemies[kv2.Key].Field("Rarity").DefaultValue = kv2.Value.Rarity;
                        }
                        else
                        {
                            Moons[kv.Key].DaytimeEnemies[kv2.Key] = (MoonConfig.EnemyTableItem)kv2.Value._Clone();
                        }
                    }
                    foreach (var kv2 in kv.Value.OutsideEnemies)
                    {
                        if (Moons[kv.Key].OutsideEnemies.ContainsKey(kv2.Key))
                        {
                            Moons[kv.Key].OutsideEnemies[kv2.Key].Field("Override").DefaultValue = false;
                            Moons[kv.Key].OutsideEnemies[kv2.Key].Field("Rarity").DefaultValue = kv2.Value.Rarity;
                        }
                        else
                        {
                            Moons[kv.Key].OutsideEnemies[kv2.Key] = (MoonConfig.EnemyTableItem)kv2.Value._Clone();
                        }
                    }
                    foreach (var kv2 in kv.Value.InsideEnemies)
                    {
                        if (Moons[kv.Key].InsideEnemies.ContainsKey(kv2.Key))
                        {
                            Moons[kv.Key].InsideEnemies[kv2.Key].Field("Override").DefaultValue = false;
                            Moons[kv.Key].InsideEnemies[kv2.Key].Field("Rarity").DefaultValue = kv2.Value.Rarity;
                        }
                        else
                        {
                            Moons[kv.Key].InsideEnemies[kv2.Key] = (MoonConfig.EnemyTableItem)kv2.Value._Clone();
                        }
                    }
                    foreach (var kv2 in kv.Value.LootTable)
                    {
                        if (Moons[kv.Key].LootTable.ContainsKey(kv2.Key))
                        {
                            Moons[kv.Key].LootTable[kv2.Key].Field("Override").DefaultValue = false;
                            Moons[kv.Key].LootTable[kv2.Key].Field("Rarity").DefaultValue = kv2.Value.Rarity;
                        }
                        else
                        {
                            Moons[kv.Key].LootTable[kv2.Key] = (MoonConfig.LootTableItem)kv2.Value._Clone();
                        }
                    }
                    foreach (var kv2 in LobbyConfiguration.AllScrapConfig.Items)
                    {
                        if (!Moons[kv.Key].LootTable.ContainsKey(kv2.Key))
                        {
                            Moons[kv.Key].LootTable[kv2.Key] = new MoonConfig.LootTableItem()
                            {
                                Override = false,
                                Rarity = 0
                            };
                        }
                    }

                    foreach (var kv2 in LobbyConfiguration.AllEnemiesConfig.Enemies)
                    {
                        if (!Moons[kv.Key].DaytimeEnemies.ContainsKey(kv2.Key))
                            Moons[kv.Key].DaytimeEnemies[kv2.Key] = new MoonConfig.EnemyTableItem() { Override = false, Rarity = 0 };
                        if (!Moons[kv.Key].OutsideEnemies.ContainsKey(kv2.Key))
                            Moons[kv.Key].OutsideEnemies[kv2.Key] = new MoonConfig.EnemyTableItem() { Override = false, Rarity = 0 };
                        if (!Moons[kv.Key].InsideEnemies.ContainsKey(kv2.Key))
                            Moons[kv.Key].InsideEnemies[kv2.Key] = new MoonConfig.EnemyTableItem() { Override = false, Rarity = 0 };
                    }
                }
            }

            public Dictionary<string, MoonConfig> Moons = new Dictionary<string, MoonConfig>();
            public WeatherConfig ClearWeather = new WeatherConfig() { OverrideScrapAmountMultiplier = false, ScrapAmountMultiplier = 1f, OverrideScrapValueMultiplier = false, ScrapValueMultiplier = 1f };
            public WeatherConfig FoggyWeather = new WeatherConfig() { OverrideScrapAmountMultiplier = true, ScrapAmountMultiplier = 1.1f, OverrideScrapValueMultiplier = true, ScrapValueMultiplier = 1.1f };
            public WeatherConfig RainyWeather = new WeatherConfig() { OverrideScrapAmountMultiplier = true, ScrapAmountMultiplier = 1.1f, OverrideScrapValueMultiplier = true, ScrapValueMultiplier = 1.1f };
            public WeatherConfig FloodedWeather = new WeatherConfig() { OverrideScrapAmountMultiplier = true, ScrapAmountMultiplier = 1.3f, OverrideScrapValueMultiplier = true, ScrapValueMultiplier = 1.2f };
            public WeatherConfig StormyWeather = new WeatherConfig() { OverrideScrapAmountMultiplier = true, ScrapAmountMultiplier = 1.5f, OverrideScrapValueMultiplier = true, ScrapValueMultiplier = 1.5f };
            public WeatherConfig EclipsedWeather = new WeatherConfig() { OverrideScrapAmountMultiplier = true, ScrapAmountMultiplier = 1.8f, OverrideScrapValueMultiplier = true, ScrapValueMultiplier = 1.5f };
        }

        public class ScrapConfig : Configuration
        {
            public bool Active;
            public bool OverrideMinValue;
            public int MinValue;
            public bool OverrideMaxValue;
            public int MaxValue;
            public bool OverrideWeight;
            public float Weight;
        }

        public class ItemConfig : Configuration
        {
            public bool Active;
            public bool OverridePrice;
            public int Price;
            public bool OverrideMaxDiscount;
            public int MaxDiscount;
            public bool OverrideWeight;
            public float Weight;
        }

        public class UnlockableConfig : Configuration
        {
            public bool Active;
            public bool OverridePrice;
            public int Price;
        }

        public class NightVisionConfig : Configuration
        {
            public float BatteryTime = 180f;
        }

        public class BulletProofVestConfig : Configuration
        {
            public int MaxDamage = 90;
            public int TurretDamage = 5;
            public int ShotgunDamage = 30;

            [Slider(0f, 1f, ShowValue = true, Conversion = 100f, InputWidth = 35f)]
            public float DamageReductionAtFullHealth = 1f;
            [Slider(0f, 1f, ShowValue = true, Conversion = 100f, InputWidth = 35f)]
            public float DamageReductionAtNoHealth = 0.5f;
            public bool DestroyAtNoHealth = true;
        }

        public class HelmetLampConfig : Configuration
        {
            public float BatteryTime = 360f;
        }
        public class HeadsetConfig : Configuration
        {
            public float BatteryTime = 800f;
        }
        public class TacticalHelmetConfig : Configuration
        {
            public float BatteryTime = 900f;
            public float BatteryTimeWithLight = 500f;
        }
        public class FlippersConfig : Configuration
        {
            [Slider(0.5f, 3f, ShowValue = true, Conversion = 100f, InputWidth = 35f)]
            public float Speed = 1f;
        }

        public class CompabilityConfig : Configuration
        {
            public bool DisableEquipment;
            public bool DisablePortableTerminal;
            public bool DisablePlayerPerks;
            public int InventorySlots;
            public bool DisablePlayerTweaks;
            public bool DisableInventory;
            public bool DisableShipPerks;
            public bool DisableShipTweaks;
            public bool DisableMoonPrices;
            public bool DisableMoonModifiers;
            public bool DisableWeatherModifiers;
            public bool DisableItemWeight;
            public bool DisableItemPrice;
            public bool DisableItemDiscount;
            public bool DisableCosmetics;
            public bool DisableDayLength;
            public bool AnimationCompabilityMode;
        }

        public class ItemsConfig : Configuration
        {
            public ItemsConfig()
            {
                foreach (var kv in AllItemsConfig.Items)
                    Items.Add(kv.Key, (ItemConfig)kv.Value._Clone());
                foreach (var kv in AllScrapConfig.Items)
                    Scrap.Add(kv.Key, (ScrapConfig)kv.Value._Clone());
                foreach (var kv in AllUnlockablesConfig.Unlockables)
                    Unlockables.Add(kv.Key, (UnlockableConfig)kv.Value._Clone());
            }

            public override void LoadedFromJSON()
            {
                foreach (var kv in AllItemsConfig.Items)
                {
                    if (Items.ContainsKey(kv.Key))
                    {
                        Items[kv.Key].Field("OverridePrice").DefaultValue = false;
                        Items[kv.Key].Field("OverrideMaxDiscount").DefaultValue = false;
                        Items[kv.Key].Field("OverrideWeight").DefaultValue = false;
                        Items[kv.Key].Field("Price").DefaultValue = kv.Value.Price;
                        Items[kv.Key].Field("MaxDiscount").DefaultValue = kv.Value.MaxDiscount;
                        Items[kv.Key].Field("Weight").DefaultValue = kv.Value.Weight;
                    }
                    else
                    {
                        Items[kv.Key] = (ItemConfig)kv.Value._Clone();
                    }
                }
                foreach (var kv in AllScrapConfig.Items)
                {
                    if (Scrap.ContainsKey(kv.Key))
                    {
                        Scrap[kv.Key].Field("OverrideMinValue").DefaultValue = false;
                        Scrap[kv.Key].Field("OverrideMaxValue").DefaultValue = false;
                        Scrap[kv.Key].Field("OverrideWeight").DefaultValue = false;
                        Scrap[kv.Key].Field("MinValue").DefaultValue = kv.Value.MinValue;
                        Scrap[kv.Key].Field("MaxValue").DefaultValue = kv.Value.MaxValue;
                        Scrap[kv.Key].Field("Weight").DefaultValue = kv.Value.Weight;
                    }
                    else
                    {
                        Scrap[kv.Key] = (ScrapConfig)kv.Value._Clone();
                    }
                }
                foreach (var kv in AllUnlockablesConfig.Unlockables)
                {
                    if (Unlockables.ContainsKey(kv.Key))
                    {
                        Unlockables[kv.Key].Field("OverridePrice").DefaultValue = false;
                        Unlockables[kv.Key].Field("Price").DefaultValue = kv.Value.Price;
                    }
                    else
                    {
                        Unlockables[kv.Key] = (UnlockableConfig)kv.Value._Clone();
                    }
                }
            }

            public Dictionary<string, ItemConfig> Items = new Dictionary<string, ItemConfig>();
            public Dictionary<string, ScrapConfig> Scrap = new Dictionary<string, ScrapConfig>();
            public Dictionary<string, UnlockableConfig> Unlockables = new Dictionary<string, UnlockableConfig>();
            public NightVisionConfig VisionEnhancer = new();
            public BulletProofVestConfig BulletProofVest = new();
            public HelmetLampConfig HelmetLamp = new();
            public HeadsetConfig Headset = new();
            public TacticalHelmetConfig TacticalHelmet = new();
            public FlippersConfig Flippers = new();

            /*public ItemConfig MissileLauncher = new() { Active = true, Price = 150, MaxDiscount = 50, Weight = 1f + (8f / 105f) };
            public ItemConfig RocketBoots = new() { Active = true, Price = 100, MaxDiscount = 50, Weight = 1f + (5f / 105f) };
            public ItemConfig Flippers = new() { Active = true, Price = 80, MaxDiscount = 40, Weight = 1f + (5f / 105f) };
            public ItemConfig LightningRod = new() { Active = true, Price = 120, MaxDiscount = 50, Weight = 1f + (50f / 105f) };
            public ItemConfig HelmetLamp = new() { Active = true, Price = 50, MaxDiscount = 70, Weight = 1f + (4f / 105f) };
            public ItemConfig Headset = new() { Active = true, Price = 50, MaxDiscount = 70, Weight = 1f + (4f / 105f) };
            public ItemConfig TacticalHelmet = new() { Active = true, Price = 300, MaxDiscount = 70, Weight = 1f + (16f / 105f) };

            public ItemConfig RadarBooster = new() { Active = true, Price = 60, MaxDiscount = 80, Weight = 1f + (19f / 105f) };
            public ItemConfig ZapGun = new() { Active = true, Price = 400, MaxDiscount = 80, Weight = 1f + (10f / 105f) };
            public ItemConfig WalkieTalkie = new() { Active = true, Price = 12, MaxDiscount = 80, Weight = 1f + (0f / 105f) };
            public ItemConfig TZPInhalant = new() { Active = true, Price = 120, MaxDiscount = 80, Weight = 1f + (0f / 105f) };
            public ItemConfig ExtensionLadder = new() { Active = true, Price = 60, MaxDiscount = 60, Weight = 1f + (0f / 105f) };
            public ItemConfig StunGrenade = new() { Active = true, Price = 30, MaxDiscount = 80, Weight = 1f + (5f / 105f) };
            public ItemConfig Shovel = new() { Active = true, Price = 30, MaxDiscount = 80, Weight = 1f + (8f / 105f) };
            public ItemConfig ProFlashlight = new() { Active = true, Price = 25, MaxDiscount = 80, Weight = 1f + (5f / 105f) };
            public ItemConfig Lockpicker = new() { Active = true, Price = 20, MaxDiscount = 80, Weight = 1f + (16f / 105f) };
            public ItemConfig Jetpack = new() { Active = true, Price = 700, MaxDiscount = 80, Weight = 1f + (52f / 105f) };
            public ItemConfig Flashlight = new() { Active = true, Price = 15, MaxDiscount = 80, Weight = 1f + (0f / 105f) };
            public ItemConfig Boombox = new() { Active = true, Price = 60, MaxDiscount = 80, Weight = 1f + (16f / 105f) };
            public ItemConfig SprayPaint = new() { Active = true, Price = 50, MaxDiscount = 80, Weight = 1f + (0f / 105f) };
            
            public List<FreeItemConfig> FreeItems = new List<FreeItemConfig>();*/

            public LobbyConfiguration.ItemConfig GetByItemName(string itemName)
            {
                if (itemName == null) return null;
                if (Items.ContainsKey(itemName))
                    return Items[itemName];
                return null;
            }

            public LobbyConfiguration.UnlockableConfig GetByUnlockableName(string unlockableName)
            {
                if (unlockableName == null) return null;
                if (Unlockables.ContainsKey(unlockableName))
                    return Unlockables[unlockableName];
                return null;
            }

            public LobbyConfiguration.ScrapConfig GetByScrapName(string itemName)
            {
                if (itemName == null) return null;
                if (Scrap.ContainsKey(itemName))
                    return Scrap[itemName];
                return null;
            }
        }

        public class PerkConfiguration : Configuration
        {
            public bool Active;
            public float Base;
            public float Change;
            public int[] Prices;
        }

        public class ShipPerksConfig : Configuration
        {
            public PerkConfiguration ScanDistance = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 50, 100, 150, 200, 300, 400, 500, 600, 700, 800 } };
            public PerkConfiguration ExtraBattery = new() { Active = true, Base = 1f, Change = 0.075f, Prices = new int[] { 100, 150, 200, 250, 300, 350, 400, 500 } };
            public PerkConfiguration ExtendDeadlineDiscount = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 200, 300, 400, 500, 750, 1000, 1500, 2000 } };
            public PerkConfiguration LandingSpeed = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 50, 100, 200, 300, 400, 500, 600, 700, 800, 1000 } };
            public PerkConfiguration DeliverySpeed = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 50, 100, 200, 300, 400, 500, 600, 700, 800, 1000 } };
            public PerkConfiguration SaveLoot = new() { Active = true, Base = 0f, Change = 0.1f, Prices = new int[] { 200, 300, 400, 500, 750, 1000, 1500, 2000 } };
            public PerkConfiguration TravelDiscount = new() { Active = true, Base = 1f, Change = 0.075f, Prices = new int[] { 200, 400, 600, 800, 1000, 1250, 1500, 2000 } };
        }


        public class PlayerPerksConfig : Configuration
        {
            public PerkConfiguration SprintSpeed = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 150, 200, 250, 400, 500 } } ;
            public PerkConfiguration JumpHeight = new() { Active = true, Base = 1f, Change = 0.05f, Prices = new int[] { 100, 100, 150, 200, 250 } };
            public PerkConfiguration JumpStamina = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 100, 100, 150, 200, 250, 300 } };
            public PerkConfiguration SprintStamina = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 150, 300, 500, 750, 1000, 1500 } };
            public PerkConfiguration StaminaRegen = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 200, 400, 600, 800, 1200, 1500 } };
            public PerkConfiguration FallDamage = new() { Active = true, Base = 1f, Change = 0.15f, Prices = new int[] { 100, 200, 300, 400, 500 } };
            public PerkConfiguration Damage = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 150, 300, 500, 750, 1000, 1250, 1500 } };
            public PerkConfiguration Weight = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 150, 300, 500, 750, 1000 } };
            public PerkConfiguration WeightSpeed = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 200, 400, 600, 800, 1000 } };
            public PerkConfiguration DealDamage = new() { Active = true, Base = 0f, Change = 0.1f, Prices = new int[] { 150, 200, 250, 350, 500, 750, 1000, 1250 } };
            public PerkConfiguration ClimbingSpeed = new() { Active = true, Base = 1f, Change = 0.1f, Prices = new int[] { 50, 100, 150, 200, 250, 300, 350, 400 } };
            public PerkConfiguration InventorySlots = new() { Active = true, Base = 3f, Change = 1f, Prices = new int[] { 500, 1000, 1500, 2500, 3500, 5000, 7500 } };
        }

        public class EnemiesConfig : Configuration
        {
            public EnemiesConfig()
            {
                foreach (var kv in AllEnemiesConfig.Enemies)
                    Enemies.Add(kv.Key, (EnemyConfig)kv.Value._Clone());
            }

            public Dictionary<string, EnemyConfig> Enemies = new Dictionary<string, EnemyConfig>();
            
            public LobbyConfiguration.EnemyConfig GetByEnemyName(string enemyName)
            {
                if (Enemies.ContainsKey(enemyName))
                    return Enemies[enemyName];
                return null;
            }
        }
        public LobbyConfig Lobby = new();
        public GeneralConfig General = new();
        public MoonsConfig Moons = new();
        public ItemsConfig Items = new();
        public PlayerPerksConfig PlayerPerks = new();
        public ShipPerksConfig ShipPerks = new();
        public EnemiesConfig Enemies = new();
    }
}