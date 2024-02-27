using AdvancedCompany;
using AdvancedCompany.Config;
using AdvancedCompany.Unity.Moons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AdvancedCompany.Config.LobbyConfiguration;

public class LobbySettings : BaseSettings<LobbyConfiguration>
{

    [Header("General")]
    public ConfigTabContent GeneralTab;
    private ConfigSlider LobbySize;
    private ConfigToggle KeepOpen;
    private ConfigToggle KeepOpenOnMoon;

    [Header("Game")]
    public ConfigTabContent GameTab;
    public ConfigNumericInput StartCredits;
    public ConfigSlider GlobalValueMultiplier;
    public ConfigSlider GlobalAmountMultiplier;
    public ConfigSlider GlobalMaxPowerMultiplier;
    public ConfigToggle ActivatePortableTerminal;
    private ConfigToggle ExtendDeadline;
    public ConfigNumericInput DeadlineLength;
    private ConfigToggle SaveSuits;
    private ConfigToggle Cosmetics;
    private ConfigToggle SaveProgress;
    private ConfigToggle IndividualXP;
    private ConfigToggle ResetXP;
    private ConfigToggle DeactivateHotbar;
    private ConfigNumericInput StartXP;
    private ConfigNumericInput StartShipXP;
    private ConfigSlider XPMultiplier;
    private ConfigSlider LengthOfDay;

    [Header("Items")]
    public ConfigTabContent ItemsTab;
    private ConfigContainer ItemsContainer;
    private ConfigContainer ScrapContainer;
    private ConfigContainer UnlockablesContainer;
    private Dictionary<string, ConfigItemInput> Items = new();
    private Dictionary<string, ConfigScrapInput> Scrap = new();
    private Dictionary<string, ConfigUnlockableInput> Unlockables = new();
    private ConfigSlider BulletProofVestDamageReductionAtFullHealth;
    private ConfigSlider BulletProofVestDamageReductionAtNoHealth;
    private ConfigNumericInput BulletProofVestTurretDamage;
    private ConfigNumericInput BulletProofVestShotgunDamage;
    private ConfigNumericInput BulletProofVestMaxDamage;
    private ConfigToggle BulletProofVestDestroyOnNoHealth;
    private ConfigNumericInput VisionEnhancerBatteryLife;
    private ConfigNumericInput HelmetLampBatteryLife;
    private ConfigNumericInput HeadsetBatteryLife;
    private ConfigNumericInput TacticalHelmetBatteryLife;
    private ConfigNumericInput TacticalHelmetBatteryLifeWithLight;
    private ConfigSlider FlippersSpeed;

    [Header("Perks")]
    public ConfigTabContent PerksTab;
    private Dictionary<string, ConfigPerkInput> PlayerPerks = new Dictionary<string, ConfigPerkInput>();
    private Dictionary<string, ConfigPerkInput> ShipPerks = new Dictionary<string, ConfigPerkInput>();

    [Header("Enemies")]
    public ConfigTabContent EnemiesTab;
    private Dictionary<string, ConfigEnemyInput> Enemies = new();
    private ConfigContainer EnemiesContainer;

    [Header("Moons")]
    public ConfigTabContent MoonsTab;
    private ConfigToggle EnableWeatherModifiers;
    private Dictionary<string, ConfigWeatherInput> Weathers = new Dictionary<string, ConfigWeatherInput>();
    private ConfigToggle EnableMoonPrices;
    private MoonsContainer MoonsContainer;
    private Dictionary<string, ConfigMoonInput> Moons = new Dictionary<string, ConfigMoonInput>();
    
    public override void ApplyConfiguration()
    {
        LobbySize.UpdateValue();
        KeepOpen.UpdateValue();
        KeepOpenOnMoon.UpdateValue();
        ExtendDeadline.UpdateValue();
        SaveSuits.UpdateValue();
        Cosmetics.UpdateValue();
        IndividualXP.UpdateValue();
        ResetXP.UpdateValue();
        SaveProgress.UpdateValue();
        StartXP.UpdateValue();
        StartShipXP.UpdateValue();
        XPMultiplier.UpdateValue();
        LengthOfDay.UpdateValue();
        StartCredits.UpdateValue();
        DeactivateHotbar.UpdateValue();
        GlobalValueMultiplier.UpdateValue();
        GlobalAmountMultiplier.UpdateValue();
        GlobalMaxPowerMultiplier.UpdateValue();
        DeadlineLength.UpdateValue();
        ActivatePortableTerminal.UpdateValue();

        foreach (var i in Items)
        {
            if (Configuration.Items.Items.ContainsKey(i.Key))
                i.Value.SetValue(Configuration.Items.Items[i.Key]);
            else
                i.Value.SetValue(LobbyConfiguration.AllItemsConfig.Items[i.Key]);
        }
        foreach (var i in Unlockables)
        {
            if (Configuration.Items.Unlockables.ContainsKey(i.Key))
                i.Value.SetValue(Configuration.Items.Unlockables[i.Key]);
            else
                i.Value.SetValue(LobbyConfiguration.AllUnlockablesConfig.Unlockables[i.Key]);
        }
        foreach (var i in Scrap)
        {
            if (Configuration.Items.Scrap.ContainsKey(i.Key))
                i.Value.SetValue(Configuration.Items.Scrap[i.Key]);
            else
                i.Value.SetValue(LobbyConfiguration.AllScrapConfig.Items[i.Key]);
        }
        foreach (var i in Enemies)
        {
            if (Configuration.Enemies.Enemies.ContainsKey(i.Key))
                i.Value.SetValue(Configuration.Enemies.Enemies[i.Key]);
            else
                i.Value.SetValue(LobbyConfiguration.AllEnemiesConfig.Enemies[i.Key]);
        }

        VisionEnhancerBatteryLife.UpdateValue();
        BulletProofVestDestroyOnNoHealth.UpdateValue();
        BulletProofVestDamageReductionAtFullHealth.UpdateValue();
        BulletProofVestDamageReductionAtNoHealth.UpdateValue();
        BulletProofVestTurretDamage.UpdateValue();
        BulletProofVestShotgunDamage.UpdateValue();
        BulletProofVestMaxDamage.UpdateValue();
        HelmetLampBatteryLife.UpdateValue();
        HeadsetBatteryLife.UpdateValue();
        TacticalHelmetBatteryLife.UpdateValue();
        TacticalHelmetBatteryLifeWithLight.UpdateValue();
        FlippersSpeed.UpdateValue();
        PlayerPerks["SprintSpeed"].UpdateValue();
        PlayerPerks["JumpHeight"].UpdateValue();
        PlayerPerks["JumpStamina"].UpdateValue();
        PlayerPerks["SprintStamina"].UpdateValue();
        PlayerPerks["StaminaRegen"].UpdateValue();
        PlayerPerks["FallDamage"].UpdateValue();
        PlayerPerks["Damage"].UpdateValue();
        PlayerPerks["Weight"].UpdateValue();
        PlayerPerks["WeightSpeed"].UpdateValue();
        PlayerPerks["DealDamage"].UpdateValue();
        PlayerPerks["ClimbingSpeed"].UpdateValue();
        PlayerPerks["InventorySlots"].UpdateValue();

        ShipPerks["ScanDistance"].UpdateValue();
        ShipPerks["ExtraBattery"].UpdateValue();
        ShipPerks["ExtendDeadlineDiscount"].UpdateValue();
        ShipPerks["LandingSpeed"].UpdateValue();
        ShipPerks["DeliverySpeed"].UpdateValue();
        ShipPerks["SaveLoot"].UpdateValue();
        ShipPerks["TravelDiscount"].UpdateValue();

        Weathers["Clear"].UpdateValue();
        Weathers["Foggy"].UpdateValue();
        Weathers["Rainy"].UpdateValue();
        Weathers["Flooded"].UpdateValue();
        Weathers["Stormy"].UpdateValue();
        Weathers["Eclipsed"].UpdateValue();

        MoonsContainer.UpdateValues();
        /*
        Moons["Experimentation"].UpdateValue();
        Moons["Vow"].UpdateValue();
        Moons["Assurance"].UpdateValue();
        Moons["Offense"].UpdateValue();
        Moons["March"].UpdateValue();
        Moons["Rend"].UpdateValue();
        Moons["Dine"].UpdateValue();
        Moons["Titan"].UpdateValue();*/
        /*
        for (var i = 0; i < FreeMoons.Count; i++)
            GameObject.Destroy(FreeMoons[i].gameObject);
        FreeMoons.Clear();

        for (var i = 0; i < Configuration.Moons.FreeMoons.Count; i++)
        {
            var configVal = Configuration.Moons.FreeMoons[i];
            AddMoon(configVal);
        }*/
        /*
        for (var i = 0; i < FreeItems.Count; i++)
            GameObject.Destroy(FreeItems[i].gameObject);
        FreeItems.Clear();

        for (var i = 0; i < Configuration.Items.FreeItems.Count; i++)
        {
            var configVal = Configuration.Items.FreeItems[i];
            AddItem(configVal);
        }*/
    }

    public override void Awake()
    {
        base.Awake();
        var lobbyContainer = GeneralTab.AddContainer("Lobby", "");
        LobbySize = lobbyContainer.AddSlider(Configuration.Lobby.Field(nameof(Configuration.Lobby.LobbySize)), "Lobby size");

        var keepOpenContainer = GeneralTab.AddContainer("Keep open - EXPERIMENTAL", "When activated your Steam lobby will be kept open and joinable when you are in orbit. You can also activate to allow joining of players while on a moon. This will put the players into spectator mode.");
        KeepOpen = keepOpenContainer.AddToggle(Configuration.Lobby.Field(nameof(Configuration.Lobby.KeepOpen)), "Enabled");
        KeepOpenOnMoon = keepOpenContainer.AddToggle(Configuration.Lobby.Field(nameof(Configuration.Lobby.KeepOpenOnMoon)), "Allow when landed");

        var dayContainer = GameTab.AddContainer("Day settings", "Settings to change how days behave.");
        LengthOfDay = dayContainer.AddSlider(Configuration.General.Field(nameof(Configuration.General.DayLength)), "Length of day");

        var startContainer = GameTab.AddContainer("Start settings", "Settings to change how a round starts.");
        StartCredits = startContainer.AddNumericInput(Configuration.General.Field(nameof(Configuration.General.StartCredits)), "Start credits", "");

        var globalMultiplierContainer = GameTab.AddContainer("Global multipliers", "Settings to change how much scrap spawns and how valueable it is and other stuff like enemy max power.");
        GlobalValueMultiplier = globalMultiplierContainer.AddSlider(Configuration.General.Field(nameof(Configuration.General.GlobalScrapValueMultiplier)), "Value");
        GlobalAmountMultiplier = globalMultiplierContainer.AddSlider(Configuration.General.Field(nameof(Configuration.General.GlobalScrapAmountMultiplier)), "Amount");
        GlobalMaxPowerMultiplier = globalMultiplierContainer.AddSlider(Configuration.General.Field(nameof(Configuration.General.GlobalMaxPowerMultiplier)), "Max enemy power");

        var extendDeadlineContainer = GameTab.AddContainer("Deadline", "The extend deadline option adds a command to your portable terminal to extend your deadline once per quota. You can also change the length of the deadline in general. Changes to this variable only takes effect when starting a new game or quota");
        ExtendDeadline = extendDeadlineContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.EnableExtendDeadline)), "Enabled");
        DeadlineLength = extendDeadlineContainer.AddNumericInput(Configuration.General.Field(nameof(Configuration.General.DeadlineLength)), "Length of deadline", "");

        var saveSuitsContainer = GameTab.AddContainer("Save suits after being fired", "When activated will save your suits when you are getting fired.");
        SaveSuits = saveSuitsContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.SaveSuitsAfterDeath)), "Enabled");

        var cosmeticsContainer = GameTab.AddContainer("Enable cosmetics", "When activated players will have the ability to wear cosmetics.");
        Cosmetics = cosmeticsContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.EnableCosmetics)), "Enabled");

        var portableTerminalContainer = GameTab.AddContainer("Portable terminal", "PLEASE NOTE: If you deactivate the portable terminal you won't have access to perks as well. There is NO replacement!");
        ActivatePortableTerminal = portableTerminalContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.ActivatePortableTerminal)), "Enabled");

        var saveProgressContainer = GameTab.AddContainer("Save progress", "When deactivated allows you to customize starting XP and XP multipliers. Progress for all players will be saved in the hosts save file and reset when not meeting the quota.");
        SaveProgress = saveProgressContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.SaveProgress)), "Enabled");

        saveProgressContainer.AddEmpty();
        IndividualXP = saveProgressContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.IndividualXP)), "Individual XP", true);
        ResetXP = saveProgressContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.ResetXP)), "Reset XP", true);
        StartXP = saveProgressContainer.AddNumericInput(Configuration.General.Field(nameof(Configuration.General.StartingXP)), "Start XP", "XP", 60f);
        StartShipXP = saveProgressContainer.AddNumericInput(Configuration.General.Field(nameof(Configuration.General.StartingShipXP)), "Start ship XP", "XP", 60f);
        XPMultiplier = saveProgressContainer.AddSlider(Configuration.General.Field(nameof(Configuration.General.XPMultiplier)), "XP multiplier");

        var deactivateHotbarContainer = GameTab.AddContainer("Deactivate hotbar", "WARNING: If you enable this setting, AdvancedCompany won't touch the hotbar and inventories at all. This means that the inventory perk gets auto deactivated, all equippable items, including cursed items, will get forcefully deactivated, the energy bars are removed, customization of the hotbar is removed, there wont be any hotkeys, neither flashlight nor quick access, added and no inventory fixes like flashlight fixes. This mode is experimental and there won't be any support for it whatsoever. If I see you using it and creating a GitHub issue relating to this mode it will get auto closed. Activating this option might result in unwanted results as the hotbar changes are a core function of AdvancedCompany. Spawning in and equipping any equipment item might break your game in unexpected ways.");
        DeactivateHotbar = deactivateHotbarContainer.AddToggle(Configuration.General.Field(nameof(Configuration.General.DeactivateHotbar)), "Deactivate");

        ItemsContainer = ItemsTab.AddItemContainer("Store items", "Here you can configure store items and if they should be activated.");
        foreach (var kv in Configuration.Items.Items)
        {
            Items[kv.Key] = ItemsContainer.AddItem(kv.Value, kv.Key);
        }

        UnlockablesContainer = ItemsTab.AddUnlockableContainer("Unlockables", "Here you can configure unlockables and if they should be purchaseable.");
        foreach (var kv in Configuration.Items.Unlockables)
        {
            Unlockables[kv.Key] = UnlockablesContainer.AddUnlockable(kv.Value, kv.Key);
        }

        ScrapContainer = ItemsTab.AddScrapContainer("Scrap items", "Here you can configure scrap items and if they should spawn.");
        foreach (var kv in Configuration.Items.Scrap)
        {
            Scrap[kv.Key] = ScrapContainer.AddScrap(kv.Value, kv.Key);
        }

        EnemiesContainer = EnemiesTab.AddEnemyContainer("Enemies", "Here you can configure enemies power levels and if they should spawn at all.");
        foreach (var kv in Configuration.Enemies.Enemies)
        {
            Enemies[kv.Key] = EnemiesContainer.AddEnemy(kv.Value, kv.Key);
        }

        var visionEnhancerContainer = ItemsTab.AddContainer("Vision enhancer", "Here you can change parameters for the vision enhancer.");
        VisionEnhancerBatteryLife = visionEnhancerContainer.AddNumericInput(Configuration.Items.VisionEnhancer.Field(nameof(Configuration.Items.VisionEnhancer.BatteryTime)), "Battery life", "secs", 60);

        var bulletProofVestContainer = ItemsTab.AddContainer("Bulletproof vest", "Here you can change parameters for the bulletproof vest.");
        BulletProofVestMaxDamage = bulletProofVestContainer.AddNumericInput(Configuration.Items.BulletProofVest.Field(nameof(Configuration.Items.BulletProofVest.MaxDamage)), "Max damage", "HP", 60);
        BulletProofVestDestroyOnNoHealth = bulletProofVestContainer.AddToggle(Configuration.Items.BulletProofVest.Field(nameof(Configuration.Items.BulletProofVest.DestroyAtNoHealth)), "Destroy on no health");
        BulletProofVestTurretDamage = bulletProofVestContainer.AddNumericInput(Configuration.Items.BulletProofVest.Field(nameof(Configuration.Items.BulletProofVest.TurretDamage)), "Turret damage", "HP", 60);
        BulletProofVestShotgunDamage = bulletProofVestContainer.AddNumericInput(Configuration.Items.BulletProofVest.Field(nameof(Configuration.Items.BulletProofVest.ShotgunDamage)), "Shotgun damage", "HP", 60);
        BulletProofVestDamageReductionAtFullHealth = bulletProofVestContainer.AddSlider(Configuration.Items.BulletProofVest.Field(nameof(Configuration.Items.BulletProofVest.DamageReductionAtFullHealth)), "Damage reduction at full health");
        BulletProofVestDamageReductionAtNoHealth = bulletProofVestContainer.AddSlider(Configuration.Items.BulletProofVest.Field(nameof(Configuration.Items.BulletProofVest.DamageReductionAtNoHealth)), "Damage reduction at no health");

        var helmetLampContainer = ItemsTab.AddContainer("Helmet lamp", "Here you can change parameters for the helmet lamp.");
        HelmetLampBatteryLife = helmetLampContainer.AddNumericInput(Configuration.Items.HelmetLamp.Field(nameof(Configuration.Items.HelmetLamp.BatteryTime)), "Battery life", "secs", 60);

        var headsetContainer = ItemsTab.AddContainer("Headset", "Here you can change parameters for the headset.");
        HeadsetBatteryLife = headsetContainer.AddNumericInput(Configuration.Items.Headset.Field(nameof(Configuration.Items.Headset.BatteryTime)), "Battery life", "secs", 60);

        var tacticalHelmetContainer = ItemsTab.AddContainer("Tactical helmet", "Here you can change parameters for the tactical helmet.");
        TacticalHelmetBatteryLife = tacticalHelmetContainer.AddNumericInput(Configuration.Items.TacticalHelmet.Field(nameof(Configuration.Items.TacticalHelmet.BatteryTime)), "Battery life", "secs", 60);
        TacticalHelmetBatteryLifeWithLight = tacticalHelmetContainer.AddNumericInput(Configuration.Items.TacticalHelmet.Field(nameof(Configuration.Items.TacticalHelmet.BatteryTimeWithLight)), "Battery life with light", "secs", 60);

        var flippersContainer = ItemsTab.AddContainer("Flippers", "Here you can change parameters for the flippers.");
        FlippersSpeed = flippersContainer.AddSlider(Configuration.Items.Flippers.Field(nameof(Configuration.Items.Flippers.Speed)), "Speed");
        
        var playerPerksContainer = PerksTab.AddPerkContainer("Player perks", "Player perks can be purchased by every player individually and will only have an effect for the player who've bought them.");
        PlayerPerks["SprintSpeed"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.SprintSpeed, "Sprint speed");
        PlayerPerks["JumpHeight"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.JumpHeight, "Jump height");
        PlayerPerks["JumpStamina"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.JumpStamina, "Jump stamina");
        PlayerPerks["SprintStamina"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.SprintStamina, "Sprint stamina");
        PlayerPerks["StaminaRegen"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.StaminaRegen, "Stamina regen");
        PlayerPerks["FallDamage"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.FallDamage, "Fall damage");
        PlayerPerks["Damage"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.Damage, "Damage");
        PlayerPerks["Weight"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.Weight, "Weight");
        PlayerPerks["WeightSpeed"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.WeightSpeed, "Weight speed");
        PlayerPerks["DealDamage"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.DealDamage, "Critical strike chance");
        PlayerPerks["ClimbingSpeed"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.ClimbingSpeed, "Climbing speed");
        PlayerPerks["InventorySlots"] = playerPerksContainer.AddPerk(Configuration.PlayerPerks.InventorySlots, "Inventory slots");

        var shipPerksContainer = PerksTab.AddPerkContainer("Ship perks", "Ship perks can be purchased by every player for the whole crew and they will have an effect for the entire team.");
        ShipPerks["ScanDistance"] = shipPerksContainer.AddPerk(Configuration.ShipPerks.ScanDistance, "Scan distance");
        ShipPerks["ExtraBattery"] = shipPerksContainer.AddPerk(Configuration.ShipPerks.ExtraBattery, "Extra battery");
        ShipPerks["ExtendDeadlineDiscount"] = shipPerksContainer.AddPerk(Configuration.ShipPerks.ExtendDeadlineDiscount, "Extend deadline discount");
        ShipPerks["LandingSpeed"] = shipPerksContainer.AddPerk(Configuration.ShipPerks.LandingSpeed, "Landing speed");
        ShipPerks["DeliverySpeed"] = shipPerksContainer.AddPerk(Configuration.ShipPerks.DeliverySpeed, "Delivery speed");
        ShipPerks["SaveLoot"] = shipPerksContainer.AddPerk(Configuration.ShipPerks.SaveLoot, "Save loot");
        ShipPerks["TravelDiscount"] = shipPerksContainer.AddPerk(Configuration.ShipPerks.TravelDiscount, "Travel discount");


        MoonsContainer = MoonsTab.AddMoonContainer("Moons", "Here you can configure moons, their loot tables and enemy spawn table. If a moon is missing from the list, start a game once and come back here after that. They should show up then.", Configuration.Moons);
        var weathersContainer = MoonsTab.AddContainer("Weather modifiers", "Here you can modify the scrap value and amount present on moons during specific weather conditions.");
        var weatherList = MoonsTab.AddWeatherContainer(null, null);
        Weathers["Clear"] = weatherList.AddWeather(Configuration.Moons.ClearWeather, "Clear");
        Weathers["Foggy"] = weatherList.AddWeather(Configuration.Moons.FoggyWeather, "Foggy");
        Weathers["Rainy"] = weatherList.AddWeather(Configuration.Moons.RainyWeather, "Rainy");
        Weathers["Flooded"] = weatherList.AddWeather(Configuration.Moons.FloodedWeather, "Flooded");
        Weathers["Stormy"] = weatherList.AddWeather(Configuration.Moons.StormyWeather, "Stormy");
        Weathers["Eclipsed"] = weatherList.AddWeather(Configuration.Moons.EclipsedWeather, "Eclipsed");

        /*Moons["Experimentation"] = MoonsContainer.AddMoon(Configuration.Moons.Experimentation, "Experimentation");
        Moons["Vow"] = MoonsContainer.AddMoon(Configuration.Moons.Vow, "Vow");
        Moons["Assurance"] = MoonsContainer.AddMoon(Configuration.Moons.Assurance, "Assurance");
        Moons["Offense"] = MoonsContainer.AddMoon(Configuration.Moons.Offense, "Offense");
        Moons["March"] = MoonsContainer.AddMoon(Configuration.Moons.March, "March");
        Moons["Rend"] = MoonsContainer.AddMoon(Configuration.Moons.Rend, "Rend");
        Moons["Dine"] = MoonsContainer.AddMoon(Configuration.Moons.Dine, "Dine");
        Moons["Titan"] = MoonsContainer.AddMoon(Configuration.Moons.Titan, "Titan");*/
    }
}
