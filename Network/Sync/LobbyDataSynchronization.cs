using AdvancedCompany.Config;
using AdvancedCompany.Lib.SyncCallbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Lib.SyncHandler;
using AdvancedCompany.Patches;
using AdvancedCompany.Game;
using Unity.Collections;
using AdvancedCompany.Lib;
using Steamworks;

namespace AdvancedCompany.Network.Sync
{
    internal class LobbyDataSynchronization : ISyncHandler, IConnectClientToPlayerObject
    {
        public Game.Player Player;
        public Dictionary<string, string> Mods;
        public int PlayerNum;
        public int CurrentLevelID;
        public int CurrentSeed;
        public LevelWeatherType CurrentWeather;
        public int[] DeadPlayers;
        public bool OnMoon;
        public bool GameHasStarted;

        public void ConnectClientToPlayerObject(GameNetcodeStuff.PlayerControllerB player)
        {
            var startOfRound = global::StartOfRound.Instance;
            var roundManager = global::RoundManager.Instance;
            var networkManager = global::GameNetworkManager.Instance;
            if (startOfRound != null && roundManager != null && networkManager != null)
            {
                startOfRound.currentLevel = startOfRound.levels[CurrentLevelID];
                startOfRound.randomMapSeed = CurrentSeed;
                startOfRound.SetPlanetsWeather();
                startOfRound.currentLevel.currentWeather = CurrentWeather;
                startOfRound.currentLevel.overrideWeather = true;
                startOfRound.currentLevel.overrideWeatherType = CurrentWeather;
                roundManager.currentLevel = startOfRound.currentLevel;
                networkManager.gameHasStarted = GameHasStarted;

                for (var i = 0; i < startOfRound.allPlayerScripts.Length; i++)
                {
                    var playerObj = startOfRound.allPlayerScripts[i];
                    bool isDead = false;
                    for (var j = 0; j < DeadPlayers.Length; j++)
                    {
                        if (DeadPlayers[j] == (int)playerObj.playerClientId)
                        {
                            isDead = true;
                            break;
                        }
                    }
                    if (isDead)
                    {
                        Plugin.Log.LogMessage("Player " + playerObj.playerClientId + " is dead!");
                        playerObj.bleedingHeavily = false;
                        playerObj.statusEffectAudio.Stop();
                        playerObj.isPlayerDead = true;
                        playerObj.snapToServerPosition = false;
                        playerObj.isUnderwater = false;
                        playerObj.isHoldingInteract = false;
                        playerObj.isPlayerControlled = false;
                        playerObj.currentlyHeldObject = null;
                        SoundManager.Instance.playerVoicePitchTargets[i] = 1f;
                        SoundManager.Instance.playerVoicePitchLerpSpeed[i] = 3f;
                        playerObj.DisablePlayerModel(player.gameObject);
                    }
                }

                if (OnMoon)
                {
                    startOfRound.inShipPhase = false;
                    roundManager.StartCoroutine(LoadLevel(startOfRound, roundManager));
                }
                else
                {
                    LobbySizePatches.Handshake = null;
                }
            }
        }

        static IEnumerator LoadLevel(global::StartOfRound startOfRound, global::RoundManager roundManager)
        {
            yield return new WaitForSeconds(0.1f);
            roundManager.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
            roundManager.GenerateNewLevelClientRpc(startOfRound.randomMapSeed, startOfRound.currentLevelID);
            roundManager.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
        }

        public string GetIdentifier()
        {
            return "AdvCmpny.Lobby";
        }

        public void ReadDataFromClientBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
            Mods = new Dictionary<string, string>();

            var networkIdentifiers = GetNetworkPrefabIdentifiers();
            
            reader.ReadValueSafe(out int prefabsLen);
            var receivedIdentifiers = new Dictionary<uint, string>();
            for (var i = 0; i < prefabsLen; i++)
            {
                reader.ReadValueSafe(out uint id);
                reader.ReadValueSafe(out string str, true);
                receivedIdentifiers.Add(id, str);
                Plugin.Log.LogDebug("Received network prefab: " + id + ":" + str);
            }

            var addedIdentifiers = new Dictionary<uint, string>();
            var missingIdentifiers = new Dictionary<uint, string>();
            var mismatchedIdentifiers = new Dictionary<uint, string>();
            foreach (var kv in receivedIdentifiers)
            {
                if (!networkIdentifiers.ContainsKey(kv.Key))
                    missingIdentifiers.Add(kv.Key, kv.Value);
            }
            foreach (var kv in networkIdentifiers)
            {
                if (receivedIdentifiers.ContainsKey(kv.Key))
                {
                    if (receivedIdentifiers[kv.Key] != networkIdentifiers[kv.Key])
                        mismatchedIdentifiers.Add(kv.Key, receivedIdentifiers[kv.Key] + "|" + networkIdentifiers[kv.Key]);
                }
                else addedIdentifiers.Add(kv.Key, kv.Value);
            }

            var modsAdded = new Dictionary<string, int>();
            var modsMissing = new Dictionary<string, int>();
            foreach (var kv in mismatchedIdentifiers)
                Plugin.Log.LogWarning("Mismatched network identifier: " + kv.Key + ":" + kv.Value);
            foreach (var kv in missingIdentifiers)
            {
                Plugin.Log.LogWarning("Client was missing network identifier: " + kv.Key + ":" + kv.Value);
                var p = kv.Value.Split(":");
                if (p.Length == 2)
                {
                    if (!modsMissing.ContainsKey(p[1]))
                        modsMissing.Add(p[1], 1);
                    else
                        modsMissing[p[1]]++;
                }
            }
            foreach (var kv in addedIdentifiers)
            {
                Plugin.Log.LogWarning("Host was missing network identifier: " + kv.Key + ":" + kv.Value);
                var p = kv.Value.Split(":");
                if (p.Length == 2)
                {
                    if (!modsAdded.ContainsKey(p[1]))
                        modsAdded.Add(p[1], 1);
                    else
                        modsAdded[p[1]]++;
                }
            }

            if (missingIdentifiers.Count > 0 || addedIdentifiers.Count > 0)
            {
                var missingText = "";
                foreach (var kv in modsMissing)
                    missingText += kv.Key + ": " + kv.Value + " prefabs\n";
                var addedText = "";
                foreach (var kv in modsAdded)
                    addedText += kv.Key + ": " + kv.Value + " prefabs\n";
                sync.SetError("NetworkConfig mismatch.\n"+ (missingText != "" ? "Missing:\n" + missingText : "") + (addedText != "" ? "Host doesn't support:\n" + addedText : ""));
            }
            else
            {
                reader.ReadValueSafe(out int modCount);

                for (var i = 0; i < modCount; i++)
                {
                    reader.ReadValueSafe(out string modName, true);
                    reader.ReadValueSafe(out string modVersion, true);
                    Mods.Add(modName, modVersion);
                }
                Plugin.Log.LogDebug("Mods installed: " + String.Join(", " , Mods.Keys));
                Player = new Game.Player();// Game.Player.GetPlayer(nextID);
                Player.ClientID = sync.ClientId;
                Player.ReadData(reader);

                var missingMods = new List<string>();
                var wrongVersionMods = new List<string>();
                foreach (var kv in Lib.Mod.RequiredMods)
                {
                    if (!this.Mods.ContainsKey(kv.Key))
                        missingMods.Add(kv.Key + " (" + kv.Value + ")");
                    else if (this.Mods[kv.Key] != kv.Value)
                        wrongVersionMods.Add(kv.Key + " (" + kv.Value + ")");
                }

                var unsupportedMods = new List<string>();
                foreach (var kv in Mods)
                {
                    if (!Lib.Mod.RequiredMods.ContainsKey(kv.Key))
                        unsupportedMods.Add(kv.Key + " (" + kv.Value + ")");
                }

                if (missingMods.Count > 0 || unsupportedMods.Count > 0 || wrongVersionMods.Count > 0)
                {
                    string error = ((missingMods.Count > 0 ? "Missing mods: " + String.Join(", ", missingMods) : "") + (unsupportedMods.Count > 0 ? " Unsupported mods: " + String.Join(", ", unsupportedMods) : "") + (unsupportedMods.Count > 0 ? " Mismatched versions: " + String.Join(", ", wrongVersionMods) : "")).Trim();
                    sync.SetError(error);
                    Plugin.Log.LogWarning("Client has mismatching mods: " + error);
                }
                else if (!global::Unity.Netcode.NetworkManager.Singleton.NetworkConfig.CompareConfig(sync.ConfigHash))
                {
                    sync.SetError("Unity NetworkConfig mismatch.");
                    Plugin.Log.LogWarning("NewtorkConfig mismatch.");
                }
                else
                {
                    if (Player == null)
                    {
                        sync.SetError("There was a problem with parsing a handshake from connecting client. Player was null.");
                    }
                    else
                    {
                        bool isLate = false;
                        var startOfRound = global::StartOfRound.Instance;
                        if (!startOfRound.inShipPhase)
                        {
                            isLate = true;
                            Plugin.Log.LogMessage("We are not in ship phase. Adding " + Player.PlayerNum + " to late joiners.");
                        }

                        Player.JoinedLate = isLate;
                        Player.LoadServer(global::GameNetworkManager.Instance.currentSaveFileName);
                        bool reset = Player.RemainingXP < 0;
                        if (!reset)
                        {
                            foreach (var perk in Perk.PerksByType(Perk.Type.PLAYER))
                            {
                                if (Player.Levels.ContainsKey(perk.ID) && Player.Levels[perk.ID] > 0 && (!perk.IsActive || Player.Levels[perk.ID] > perk.Levels))
                                    reset = true;
                            }
                        }
                        if (reset)
                            Player.Levels = new Dictionary<string, int>();
                    }
                }
            }
        }

        public void ReadDataFromJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
            try
            {
                ServerConfiguration.Instance.ReadData(reader);
                ServerConfiguration.Instance.WasLoaded();

                Network.Manager.Lobby = new Network.Lobby();
                Network.Manager.Lobby.ReadData(reader);
                reader.ReadValueSafe(out int playerID);
                Plugin.Log.LogMessage("Server sent our player ID: " + playerID);
                Lobby.LocalPlayerNum = playerID;
                Player = Network.Manager.Lobby.Player(playerID);
                reader.ReadValueSafe(out CurrentLevelID);
                reader.ReadValueSafe(out CurrentSeed);
                reader.ReadValueSafe(out string weather, true);
                Enum.TryParse<LevelWeatherType>(weather, out CurrentWeather);

                reader.ReadValueSafe(out int deadPlayersLen);
                DeadPlayers = new int[deadPlayersLen];
                for (var i = 0; i < DeadPlayers.Length; i++)
                {
                    reader.ReadValueSafe(out int deadID);
                    DeadPlayers[i] = deadID;
                }
                reader.ReadValueSafe(out OnMoon);
                reader.ReadValueSafe(out GameHasStarted);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while reading lobby handshake:");
                Plugin.Log.LogError(e);
                sync.Error = "Error while reading lobby handshake";
            }
        }

        private static Dictionary<uint, string> GetNetworkPrefabIdentifiers()
        {
            Dictionary<uint, string> prefabIdentifiers = new();
            foreach (KeyValuePair<uint, NetworkPrefab> item in global::Unity.Netcode.NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabOverrideLinks.OrderBy((KeyValuePair<uint, NetworkPrefab> x) => x.Key))
            {
                string pluginName = null;
                var identifier = "";
                if (item.Value.Prefab != null)
                {
                    identifier = item.Value.Prefab.name;
                    var components = item.Value.Prefab.GetComponents<Component>();
                    foreach (var c in components)
                    {
                        try
                        {
                            var aName = c.GetType().Assembly.GetName().Name;
                            if (!aName.StartsWith("Assembly") && !aName.StartsWith("Unity"))
                            {
                                foreach (var plugin in BepInEx.Bootstrap.Chainloader.PluginInfos)
                                {
                                    if (plugin.Value.Instance.GetType().Assembly == c.GetType().Assembly)
                                    {
                                        pluginName = plugin.Value.Metadata.Name;
                                        break;
                                    }
                                }
                                if (pluginName == null)
                                    pluginName = aName;
                            }
                        } catch (Exception)
                        {
                            Plugin.Log.LogWarning("Couldnt determine mod for component " + c);
                        }
                    }
                    identifier += ":" + (pluginName == null ? "Vanilla" : pluginName);
                }    
                prefabIdentifiers.Add(item.Key, identifier);
            }
            return prefabIdentifiers;
        }

        public void WriteDataToHostBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
            var networkIdentifiers = GetNetworkPrefabIdentifiers();
            writer.WriteValueSafe(networkIdentifiers.Count);
            foreach (var kv in networkIdentifiers)
            {
                Plugin.Log.LogDebug("Writing network prefab: " + kv.Key + ":" + kv.Value);
                writer.WriteValueSafe(kv.Key);
                writer.WriteValueSafe(kv.Value, true);
            }

            writer.WriteValueSafe(Lib.Mod.RequiredMods.Count);
            foreach (var kv in Lib.Mod.RequiredMods)
            {
                writer.WriteValueSafe(kv.Key, true);
                writer.WriteValueSafe(kv.Value, true);
            }

            var player = new Game.Player();
            player.LoadLocal();
            player.WriteData(writer);
        }

        public void WriteDataToPlayerJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
            try
            {
                SteamNetworkingUtils.SendBufferSize = 2 * 1024 * 1024;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while increasing send buffer:");
                Plugin.Log.LogError(e);
            }
            int nextID = 999;

            for (int i = 1; i < ServerConfiguration.Instance.Lobby.LobbySize; i++)
            {
                if (!Network.Manager.Lobby.ConnectedPlayers.ContainsKey(i))
                {
                    nextID = i;
                    break;
                }
            }
            if (nextID != 999)
            {
                Plugin.Log.LogInfo("Next free spot: " + nextID);
                var player = Game.Player.GetPlayer(nextID);
                player.CopyFrom(Player);
                player.PlayerNum = nextID;
                PlayerNum = player.PlayerNum;

                var startOfRound = global::StartOfRound.Instance;
                var gameNetworkManager = global::GameNetworkManager.Instance;

                Network.Manager.Lobby.PlayerConnected(player, player.JoinedLate);
                Plugin.Log.LogInfo("Syncing new player to other players...");
                using (FastBufferWriter bufferWriter = new FastBufferWriter(1024, Allocator.Temp, 65536))
                {
                    bufferWriter.WriteValueSafe(player.PlayerNum);
                    player.WriteData(bufferWriter);

                    var recipients = new List<ulong>(global::Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds);
                    recipients.Remove(global::Unity.Netcode.NetworkManager.Singleton.LocalClientId);
                    recipients.Remove(sync.ClientId);
                    global::Unity.Netcode.NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(Network.Manager.NamePrefix + "NewPlayer", recipients, bufferWriter, NetworkDelivery.Reliable);
                }

                ServerConfiguration.Instance.WriteData(writer);
                Network.Manager.Lobby.WriteData(writer);
                writer.WriteValueSafe(player.PlayerNum);
                writer.WriteValueSafe(startOfRound.currentLevelID);
                writer.WriteValueSafe(startOfRound.randomMapSeed);
                var weather = Enum.GetName(typeof(LevelWeatherType), startOfRound.currentLevel.currentWeather);
                writer.WriteValueSafe(weather, true);
                var deadPlayers = new List<int>();
                for (var i = 0; i < startOfRound.allPlayerScripts.Length; i++)
                {
                    var p = startOfRound.allPlayerScripts[i];
                    if (p.isPlayerDead)
                    {
                        deadPlayers.Add((int)p.playerClientId);
                    }
                }
                writer.WriteValueSafe(deadPlayers.Count);
                for (var i = 0; i < deadPlayers.Count; i++)
                    writer.WriteValueSafe(deadPlayers[i]);
                writer.WriteValueSafe(!startOfRound.inShipPhase);
                writer.WriteValueSafe(gameNetworkManager.gameHasStarted);
            }
            else
            {
                sync.SetError("Lobby full when it shouldn't be!?");
            }
        }
    }
}
