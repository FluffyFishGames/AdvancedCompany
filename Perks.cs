using System;
using Unity.Netcode;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Config;
using AdvancedCompany.Game;
using UnityEngine;

namespace AdvancedCompany
{
    [Boot.Requires(typeof(ServerConfiguration))]
    [Boot.Bootable]
    internal class Perks : NetworkBehaviour
    {
        internal static Perk SprintSpeedPerk;
        internal static Perk JumpHeightPerk;
        internal static Perk JumpStaminaPerk;
        internal static Perk SprintStaminaPerk;
        internal static Perk StaminaRegenPerk;
        internal static Perk FallDamagePerk;
        internal static Perk DamagePerk;
        internal static Perk WeightPerk;
        internal static Perk WeightSpeedPerk;
        internal static Perk InventorySlotsPerk;
        internal static Perk DealDamagePerk;
        internal static Perk ClimbingSpeedPerk;
        internal static Perk ScanDistancePerk;
        internal static Perk ExtraBatteryPerk;
        internal static Perk ExtendDeadlineDiscountPerk;
        internal static Perk LandingSpeedPerk;
        internal static Perk DeliverySpeedPerk;
        internal static Perk SaveLootPerk;
        internal static Perk TravelDiscountPerk;

        public static void Boot()
        {
            SprintSpeedPerk = new Perk(
                Perk.Type.PLAYER,
                "SprintSpeed",
                "Sprint speed",
                "Increases your sprint speed by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.SprintSpeed,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            JumpHeightPerk = new Perk(
                Perk.Type.PLAYER,
                "JumpHeight",
                "Jump height",
                "Increases your jump height by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.JumpHeight,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            JumpStaminaPerk = new Perk(
                Perk.Type.PLAYER,
                "JumpStamina",
                "Jump endurance",
                "Reduces stamina usage of jumping by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.JumpStamina,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );
            SprintStaminaPerk = new Perk(
                Perk.Type.PLAYER,
                "SprintStamina",
                "Sprint endurance",
                "Reduces stamina usage of sprinting by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.SprintStamina,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            StaminaRegenPerk = new Perk(
                Perk.Type.PLAYER,
                "StaminaRegen",
                "Stamina regen",
                "Increases stamina regeneration by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.StaminaRegen,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            FallDamagePerk = new Perk(
                Perk.Type.PLAYER,
                "FallDamage",
                "Reinforced legs",
                "Increases the depth you can fall without damage by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.FallDamage,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            DamagePerk = new Perk(
                Perk.Type.PLAYER,
                "Damage",
                "Protective skin",
                "Reduces damage from enemies by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.Damage,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );
            WeightPerk = new Perk(
                Perk.Type.PLAYER,
                "Weight",
                "Bodybuilder",
                "Reduces stamina loss impact of weight when sprinting by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.Weight,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );
            WeightSpeedPerk = new Perk(
                Perk.Type.PLAYER,
                "WeightSpeed",
                "Heavy runner",
                "Reduces speed loss impact of weight by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.WeightSpeed,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );
            InventorySlotsPerk = new Perk(
                Perk.Type.PLAYER,
                "InventorySlots",
                "Carry bags",
                "Increases inventory size by 1 per level.",
                ServerConfiguration.Instance.PlayerPerks.InventorySlots,
                (float baseValue, float change, int level) => { return baseValue + level; }
            );
            DealDamagePerk = new Perk(
                Perk.Type.PLAYER,
                "DealDamage",
                "Strong arms",
                "Increases the chance for a critical strike by [change]%. Critical strikes will kill enemies with one hit.",
                ServerConfiguration.Instance.PlayerPerks.DealDamage,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            ClimbingSpeedPerk = new Perk(
                Perk.Type.PLAYER,
                "ClimbingSpeed",
                "Climbing speed",
                "Increases climbing speed by [change]% per level.",
                ServerConfiguration.Instance.PlayerPerks.ClimbingSpeed,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );

            ScanDistancePerk = new Perk(
                Perk.Type.SHIP,
                "ScanDistance",
                "Scanner distance",
                "Increases the distance of the scanner by [change]% per level.",
                ServerConfiguration.Instance.ShipPerks.ScanDistance,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            ExtraBatteryPerk = new Perk(
                Perk.Type.SHIP,
                "ExtraBattery",
                "Batterypack",
                "Increases battery capacity of items by [change]% per level.",
                ServerConfiguration.Instance.ShipPerks.ExtraBattery,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            ExtendDeadlineDiscountPerk = new Perk(
                Perk.Type.SHIP,
                "ExtendDeadlineDiscount",
                "Deadline discount",
                "Decreases the cost to extend the deadline by [change]% per level.",
                ServerConfiguration.Instance.ShipPerks.ExtendDeadlineDiscount,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );
            LandingSpeedPerk = new Perk(
                Perk.Type.SHIP,
                "LandingSpeed",
                "Landing speed",
                "Reduces the time to land and lift off by [change]% per level.",
                ServerConfiguration.Instance.ShipPerks.LandingSpeed,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );
            DeliverySpeedPerk = new Perk(
                Perk.Type.SHIP,
                "DeliverySpeed",
                "Express delivery",
                "Reduces the time for deliveries to arrive by [change]% per level.",
                ServerConfiguration.Instance.ShipPerks.DeliverySpeed,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );
            SaveLootPerk = new Perk(
                Perk.Type.SHIP,
                "SaveLoot",
                "Loot saver",
                "Increases the amount of saved loot by [change]% per level.",
                ServerConfiguration.Instance.ShipPerks.SaveLoot,
                (float baseValue, float change, int level) => { return baseValue + level * change; }
            );
            TravelDiscountPerk = new Perk(
                Perk.Type.SHIP,
                "TravelDiscount",
                "Travel discount",
                "Decreases the price to travel to moons by [change]% per level.",
                ServerConfiguration.Instance.ShipPerks.TravelDiscount,
                (float baseValue, float change, int level) => { return baseValue - level * change; }
            );

            Network.Manager.AddListener<ExtendDeadline>((message) =>
            {
                ExtendDeadline();
            });

            Network.Manager.AddListener<ChangePerk>((message) =>
            {
                var perk = Perk.Perks[message.ID];
                if (perk.PerkType == Perk.Type.PLAYER)
                {
                    var player = Network.Manager.Lobby.Player(message.PlayerNum);
                    player.SetLevel(perk, message.Level);
                    if (message.PlayerNum == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
                        player.SaveLocal();
                    if (!ServerConfiguration.Instance.General.SaveProgress && NetworkManager.Singleton.IsServer)
                        player.SaveServer(global::GameNetworkManager.Instance.currentSaveFileName);
                }
                else if (perk.PerkType == Perk.Type.SHIP)
                {
                    var ship = Network.Manager.Lobby.CurrentShip;
                    ship.SetLevel(perk, message.Level);
                    if (NetworkManager.Singleton.IsServer)
                        ship.Save(global::GameNetworkManager.Instance.currentSaveFileName);
                }
            });

            Network.Manager.AddListener<Respec>((message) =>
            {
                if (message.Type == Perk.Type.PLAYER)
                {
                    var player = Network.Manager.Lobby.Player(message.PlayerNum);
                    player.Reset(message.Reset);
                    if (message.PlayerNum == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
                        player.SaveLocal();
                    if (!ServerConfiguration.Instance.General.SaveProgress && NetworkManager.Singleton.IsServer)
                        player.SaveServer(global::GameNetworkManager.Instance.currentSaveFileName);
                }
                else if (message.Type == Perk.Type.SHIP)
                {
                    var ship = Network.Manager.Lobby.CurrentShip;
                    ship.Reset(message.Reset);
                    if (NetworkManager.Singleton.IsServer)
                        ship.Save(global::GameNetworkManager.Instance.currentSaveFileName);
                }
            });

            Network.Manager.AddListener<GrantPlayerXP>((message) =>
            {
                if (message.All)
                {
                    foreach (var kv in Network.Manager.Lobby.ConnectedPlayers)
                    {
                        kv.Value.XP += message.XP;
                        if (!ServerConfiguration.Instance.General.SaveProgress && NetworkManager.Singleton.IsServer)
                            kv.Value.SaveServer(global::GameNetworkManager.Instance.currentSaveFileName);
                    }
                    HUDManager.Instance.DisplayTip("XP gained!", "You have gained " + message.XP + " for fulfilling the quota. Type \"perks\" on the portable terminal to level up.", isWarning: false, useSave: false);
                }
                else
                {
                    var player = Network.Manager.Lobby.Player(message.PlayerNum);
                    player.XP += message.XP;
                    if (!ServerConfiguration.Instance.General.SaveProgress && NetworkManager.Singleton.IsServer)
                        player.SaveServer(global::GameNetworkManager.Instance.currentSaveFileName);
                }
                if (message.All || message.PlayerNum == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
                    Network.Manager.Lobby.Player().SaveLocal();
                
            });

            Network.Manager.AddListener<GrantShipXP>((message) =>
            {
                Network.Manager.Lobby.CurrentShip.XP += message.XP;
                if (NetworkManager.Singleton.IsServer)
                    Network.Manager.Lobby.CurrentShip.Save(global::GameNetworkManager.Instance.currentSaveFileName);
            });

            Network.Manager.AddListener<ChangeShip>((message) =>
            {
                Network.Manager.Lobby.CurrentShip.TotalQuota = message.TotalQuota;
                Network.Manager.Lobby.CurrentShip.ExtendedDeadline = message.ExtendedDeadline;
                if (NetworkManager.Singleton.IsServer)
                    Network.Manager.Lobby.CurrentShip.Save(global::GameNetworkManager.Instance.currentSaveFileName);
            });

            Network.Manager.AddListener<DeadlineChanged>((message) =>
            {
                HUDManager.Instance.DisplayTip("Deadline extended", "Deadline was extended by a day!", false, false);
                TimeOfDay.Instance.timeUntilDeadline = message.NewDeadline;
                TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
            });
        }
        
        public static int ExtendDeadlinePrice
        {
            get
            {
                if (TimeOfDay.Instance != null)
                {
                    return (int)((float)TimeOfDay.Instance.profitQuota * GetMultiplier(ExtendDeadlineDiscountPerk));
                }
                return 0;
            }
        }

        public static void OnClientExtendDeadline(ulong clientId, FastBufferReader reader)
        {
            ExtendDeadline();
        }

        public static void OnDeadlineSync(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out float deadline);
            TimeOfDay.Instance.timeUntilDeadline = deadline;
            TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
        }

        public static void ExtendDeadline()
        {
            var price = ExtendDeadlinePrice;
            if (price <= Game.Manager.Terminal.groupCredits)
            {
                TimeOfDay.Instance.timeUntilDeadline += TimeOfDay.Instance.totalTime;
                HUDManager.Instance.DisplayTip("Deadline extended", "Deadline was extended by day!", false, false);
                TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
                Game.Manager.Terminal.groupCredits -= price;

                if (NetworkManager.Singleton.IsServer)
                {
                    Game.Manager.Terminal.SyncGroupCreditsServerRpc((int)(Game.Manager.Terminal.groupCredits), (int)Game.Manager.Terminal.numberOfItemsInDropship);
                    Network.Manager.SendToAll(new DeadlineChanged() { NewDeadline = TimeOfDay.Instance.timeUntilDeadline });
                    Network.Manager.Send(new ChangeShip() { TotalQuota = Network.Manager.Lobby.CurrentShip.TotalQuota, ExtendedDeadline = true });
                }
                else
                {
                    Network.Manager.Send(new ExtendDeadline());
                }
            }
        }

        public static float GetMultiplier(Perk perk)
        {
            if (Network.Manager.Lobby != null)
            {
                if (perk.PerkType == Perk.Type.PLAYER)
                {
                    var localPlayer = Network.Manager.Lobby.Player();
                    if (localPlayer != null)
                    {
                        if (localPlayer.MultiplierOverrides.ContainsKey(perk.ID))
                            return localPlayer.MultiplierOverrides[perk.ID];
                        return perk.GetMultiplier(localPlayer);
                    }
                }
                else if (perk.PerkType == Perk.Type.SHIP)
                {
                    var ship = Network.Manager.Lobby.CurrentShip;
                    if (ship != null)
                        return perk.GetMultiplier(ship);
                }
            }
            return 1f;
        }

        public static float GetMultiplier(string perkName)
        {
            if (Perk.Perks.ContainsKey(perkName))
            {
                return GetMultiplier(Perk.Perks[perkName]);
            }
            return 1f;
        }

        public static float GetMultiplier(ulong clientID, string perkName)
        {
            if (Perk.Perks.ContainsKey(perkName) && Network.Manager.Lobby != null)
            {
                var perk = Perk.Perks[perkName];
                if (perk.PerkType == Perk.Type.PLAYER)
                {
                    var player = Network.Manager.Lobby.Player();
                    if (player != null)
                        return Perk.Perks[perkName].GetMultiplier(player);
                }
            }
            return 1f;
        }

        public static float GetMultiplier(GameNetcodeStuff.PlayerControllerB controller, string perkName)
        {
            if (controller != null)
                return GetMultiplier(controller.playerClientId, perkName);
            return 1f;
        }

        public static int InventorySlots()
        {
            var player = Network.Manager.Lobby.Player();
            if (player != null)
                return (int)Mathf.Clamp(Mathf.Round(ServerConfiguration.Instance.PlayerPerks.InventorySlots.Base + Perk.Perks["InventorySlots"].GetLevel(player)), 0f, 10f);
            return 4;
        }

        public static int InventorySlotsOf(GameNetcodeStuff.PlayerControllerB controller)
        {
            if (Perk.Perks.ContainsKey("InventorySlots") && Network.Manager.Lobby != null)
            {
                var player = Network.Manager.Lobby.Player((int) controller.playerClientId);
                if (player != null)
                    return (int) Mathf.Clamp(Mathf.Round(ServerConfiguration.Instance.PlayerPerks.InventorySlots.Base + Perk.Perks["InventorySlots"].GetLevel(player)), 0f, 10f);
            }
            return 10;
        }
    }
}
