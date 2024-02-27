using System;
using System.Collections.Generic;
using System.Text;
using AdvancedCompany.Config;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Sync;
using AdvancedCompany.Patches;
using Unity.Netcode;

namespace AdvancedCompany.Network
{
    internal class Lobby : ITransferable
    {
        public static int LocalPlayerNum = 0;
        public bool IsJoinable = true;
        public Ship CurrentShip;
        public Dictionary<int, Game.Player> ConnectedPlayers = new Dictionary<int, Game.Player>();
        public List<int> LateJoiners = new List<int>();

        public Game.Player Player(int id)
        {
            if (ConnectedPlayers.ContainsKey(id))
                return ConnectedPlayers[id];
            else return null;
        }
        public Game.Player Player(ulong clientId)
        {
            foreach (var p in ConnectedPlayers)
            {
                if (p.Value.ClientID == clientId)
                    return p.Value;
            }
            return null;
        }

        public Game.Player Player(string name)
        {
            foreach (var kv in ConnectedPlayers)
                if (kv.Value.Controller.playerUsername.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            return null;
        }

        public Game.Player Player()
        {
            if (ConnectedPlayers.ContainsKey(LocalPlayerNum))
                return ConnectedPlayers[LocalPlayerNum];
            return null;
        }

        public void ReadData(FastBufferReader reader)
        {
            ConnectedPlayers.Clear();

            //ServerConfiguration.Instance.ReadData(reader);
            CurrentShip = new Game.Ship();
            CurrentShip.ReadData(reader);
            reader.ReadValueSafe(out IsJoinable);
            reader.ReadValueSafe(out int players);
            for (var i = 0; i < players; i++)
            {
                reader.ReadValueSafe(out int clientID);
                Plugin.Log.LogMessage("Reading lobby data. Player " + clientID);
                var player = Game.Player.GetPlayer(clientID);
                player.ReadData(reader);
                ConnectedPlayers[player.PlayerNum] = player;
            }
            reader.ReadValueSafe(out int lateJoiners);
            Plugin.Log.LogMessage("Reading " + lateJoiners + " late joiners.");
            LateJoiners = new List<int>();
            for (var i = 0; i < lateJoiners; i++)
            {
                reader.ReadValueSafe(out int id);
                Plugin.Log.LogMessage("Reading late joiner: " + id);
                LateJoiners.Add(id);
            }
        }

        public void WriteData(FastBufferWriter writer)
        {
            //ServerConfiguration.Instance.WriteData(writer);
            CurrentShip.WriteData(writer);
            writer.WriteValueSafe(IsJoinable);
            writer.WriteValueSafe(ConnectedPlayers.Count);
            foreach (var kv in ConnectedPlayers)
            {
                writer.WriteValueSafe(kv.Value.PlayerNum);
                kv.Value.WriteData(writer);
            }
            writer.WriteValueSafe(LateJoiners.Count);
            foreach (var kv in LateJoiners)
            {
                writer.WriteValueSafe(kv);
            }
        }

        public void PlayerConnected(Player player, bool isLateJoiner = false)
        {
            Plugin.Log.LogInfo("New player (" + player.PlayerNum + ") joined lobby.");
            ConnectedPlayers[player.PlayerNum] = player;
            if (isLateJoiner)
            {
                Plugin.Log.LogInfo("Player " + player.PlayerNum + " joined late.");
                LateJoiners.Add((int)player.PlayerNum);
            }
        }

        public static int GetPlayerNum(ulong clientId)
        {
            if (LobbySizePatches.ConnectingClients.ContainsKey(clientId))
            {
                var lobbySync = LobbySizePatches.ConnectingClients[clientId].GetSyncHandler<LobbyDataSynchronization>();
                return lobbySync.PlayerNum;
            }
            else throw new Exception("Error while receiving player num. There was no connecting client with ID: " + clientId);
        }
        /*
        public string PlayerConnected(Handshake handshake)
        {
            Plugin.Log.LogMessage("Player connected with Client ID: " + handshake.Player.ClientID);

                if (!ServerConfiguration.Instance.General.SaveProgress)
                {
                    if (NetworkManager.Singleton.IsServer)
                        handshake.Player.LoadServer(global::GameNetworkManager.Instance.currentSaveFileName);
                }
                ConnectedPlayers[handshake.Player.ClientID] = handshake.Player;
            //}
            return null;
        }
        */

        public void PlayerDisconnected(int playerNum)
        {
            ConnectedPlayers.Remove(playerNum);
        }
    }
}
