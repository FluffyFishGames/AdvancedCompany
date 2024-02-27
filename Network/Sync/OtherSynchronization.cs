using AdvancedCompany.Lib.SyncCallbacks;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Lib.SyncHandler;
using BepInEx;
using AdvancedCompany.Patches;

namespace AdvancedCompany.Network.Sync
{
    internal class OtherSynchronization : ISyncHandler, INetworkObjectsPlaced, ILevelLoaded
    {
        public bool ShipLightsOn;
        public int LivingPlayers;
        public int DaysSpent;
        public int AllStepsTaken;
        public int Deaths;
        public int ScrapValueCollected;
        public int[] DamageTaken;
        public int[] StepsTaken;
        public int[] Jumps;
        public int[] Profitable;
        public int[] TurnAmount;
        public List<string>[] PlayerNotes;
        public float TotalScrapValueInLevel;
        public int ScrapCollectedInLevel;
        public int ValueOfFoundScrapItems;
        public bool PowerOffPermanently;
        public bool BreakerBoxIsPowerOn;
        public int BreakerBoxLeversSwitchedOff;

        public string GetIdentifier()
        {
            return "AdvCmpny.Other";
        }

        public void NetworkObjectsPlaced()
        {
            var shipLights = GameObject.FindObjectOfType<ShipLights>();
            shipLights.SetShipLightsOnLocalClientOnly(ShipLightsOn);
            StartOfRound.Instance.gameStats.daysSpent = DaysSpent;
            StartOfRound.Instance.gameStats.allStepsTaken = AllStepsTaken;
            StartOfRound.Instance.gameStats.deaths = Deaths;
            StartOfRound.Instance.gameStats.scrapValueCollected = ScrapValueCollected;
            for (var i = 0; i < StartOfRound.Instance.gameStats.allPlayerStats.Length; i++)
            {
                StartOfRound.Instance.gameStats.allPlayerStats[i].damageTaken = DamageTaken[i];
                StartOfRound.Instance.gameStats.allPlayerStats[i].stepsTaken = StepsTaken[i];
                StartOfRound.Instance.gameStats.allPlayerStats[i].jumps = Jumps[i];
                StartOfRound.Instance.gameStats.allPlayerStats[i].profitable = Profitable[i];
                StartOfRound.Instance.gameStats.allPlayerStats[i].turnAmount = TurnAmount[i];
                StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes = PlayerNotes[i];
            }
        }

        public void LevelLoaded()
        {
            StartOfRound.Instance.livingPlayers = LivingPlayers + 1;
            RoundManager.Instance.totalScrapValueInLevel = TotalScrapValueInLevel;
            RoundManager.Instance.scrapCollectedInLevel = ScrapCollectedInLevel;
            RoundManager.Instance.valueOfFoundScrapItems = ValueOfFoundScrapItems;
            RoundManager.Instance.powerOffPermanently = PowerOffPermanently;

            BreakerBox breakerBox = GameObject.FindObjectOfType<BreakerBox>();
            if (breakerBox != null)
            {
                breakerBox.isPowerOn = BreakerBoxIsPowerOn;
                breakerBox.leversSwitchedOff = BreakerBoxLeversSwitchedOff;
                RoundManager.Instance.SwitchPower(!RoundManager.Instance.powerOffPermanently && breakerBox.isPowerOn);
            }
        }

        public void ReadDataFromClientBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
        }

        public void ReadDataFromJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ShipLightsOn);
            reader.ReadValueSafe(out LivingPlayers);
            reader.ReadValueSafe(out DaysSpent);
            reader.ReadValueSafe(out AllStepsTaken);
            reader.ReadValueSafe(out Deaths);
            reader.ReadValueSafe(out ScrapValueCollected);
            reader.ReadValueSafe(out int statsLength);
            DamageTaken = new int[statsLength];
            StepsTaken = new int[statsLength];
            Jumps = new int[statsLength];
            Profitable = new int[statsLength];
            TurnAmount = new int[statsLength];
            PlayerNotes = new List<string>[statsLength];
            for (var i = 0; i < statsLength; i++)
            {
                reader.ReadValueSafe(out DamageTaken[i]);
                reader.ReadValueSafe(out StepsTaken[i]);
                reader.ReadValueSafe(out Jumps[i]);
                reader.ReadValueSafe(out Profitable[i]);
                reader.ReadValueSafe(out TurnAmount[i]);
                reader.ReadValueSafe(out int notesCount);
                PlayerNotes[i] = new List<string>();
                for (var j = 0; j < notesCount; j++)
                {
                    reader.ReadValueSafe(out string note, true);
                    PlayerNotes[i].Add(note);
                }
            }
            reader.ReadValueSafe(out TotalScrapValueInLevel);
            reader.ReadValueSafe(out ScrapCollectedInLevel);
            reader.ReadValueSafe(out ValueOfFoundScrapItems);
            reader.ReadValueSafe(out PowerOffPermanently);
            reader.ReadValueSafe(out bool HasBreakerBox);
            if (HasBreakerBox)
            {
                reader.ReadValueSafe(out BreakerBoxIsPowerOn);
                reader.ReadValueSafe(out BreakerBoxLeversSwitchedOff);
            }
        }

        public void WriteDataToHostBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
        }

        public void WriteDataToPlayerJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
            var shipLights = GameObject.FindObjectOfType<ShipLights>();
            writer.WriteValueSafe(shipLights.areLightsOn);

            // sync StartOfRound
            writer.WriteValueSafe(StartOfRound.Instance.livingPlayers);
            writer.WriteValueSafe(StartOfRound.Instance.gameStats.daysSpent);
            writer.WriteValueSafe(StartOfRound.Instance.gameStats.allStepsTaken);
            writer.WriteValueSafe(StartOfRound.Instance.gameStats.deaths);
            writer.WriteValueSafe(StartOfRound.Instance.gameStats.scrapValueCollected);
            writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats.Length);
            for (var i = 0; i < StartOfRound.Instance.gameStats.allPlayerStats.Length; i++)
            {
                writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats[i].damageTaken);
                writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats[i].stepsTaken);
                writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats[i].jumps);
                writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats[i].profitable);
                writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats[i].turnAmount);
                writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Count);
                for (var j = 0; j < StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Count; j++)
                {
                    writer.WriteValueSafe(StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes[j], true);
                }
            }
            // sync RoundManager
            writer.WriteValueSafe(RoundManager.Instance.totalScrapValueInLevel);
            writer.WriteValueSafe(RoundManager.Instance.scrapCollectedInLevel);
            writer.WriteValueSafe(RoundManager.Instance.valueOfFoundScrapItems);
            writer.WriteValueSafe(RoundManager.Instance.powerOffPermanently);

            BreakerBox breakerBox = GameObject.FindObjectOfType<BreakerBox>();
            if (breakerBox != null)
            {
                writer.WriteValueSafe(true);
                writer.WriteValueSafe(breakerBox.isPowerOn);
                writer.WriteValueSafe(breakerBox.leversSwitchedOff);
            }
            else
                writer.WriteValueSafe(false);
        }
    }
}
