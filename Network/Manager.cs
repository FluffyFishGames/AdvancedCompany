using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static Unity.Netcode.CustomMessagingManager;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using System.Collections;
using HarmonyLib;

namespace AdvancedCompany.Network
{
    [HarmonyPatch]
    internal class Manager
    {
        public const string NamePrefix = "AdvancedCompanyMod_";
        private static Dictionary<string, (Type Type, bool ServerOnly, bool IsRelay)> MessageTypes = new();
        private static Dictionary<string, List<(object, Action<INamedMessage>)>> Listeners = new();
        public delegate void PlayerConnected(ulong clientID);
        public delegate void PlayerDisconnected(ulong clientID, int playerNum);
        public delegate void Connected();
        public delegate void Disconnected();
        public delegate void Kicked(string reason);
        public delegate void LobbyCreated();
        public delegate void ReceivedLobby();

        public static PlayerConnected OnPlayerConnected;
        public static PlayerDisconnected OnPlayerDisconnected;
        public static Connected OnConnected;
        public static Disconnected OnDisconnected;
        public static Kicked OnKicked;
        public static LobbyCreated OnLobbyCreated;
        public static ReceivedLobby OnReceivedLobby;
        public static Lobby Lobby;

        static Manager()
        {
            var types = typeof(Manager).Assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                if (typeof(INamedMessage).IsAssignableFrom(types[i]))
                {
                    var messageAttribute = types[i].GetCustomAttribute<Message>();
                    if (messageAttribute != null)
                    {
                        MessageTypes.Add(messageAttribute.Name, (types[i], messageAttribute.ServerOnly, messageAttribute.IsRelay));
                    }
                }
            }
        }

        public static void Initialize()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null)
                throw new Exception("NetworkManager is not initialized yet!");

            Plugin.Log.LogMessage("Initialize NetworkManager...");

            networkManager.OnClientStarted += NetworkManager_OnClientStarted;
            networkManager.OnClientStopped += NetworkManager_OnClientStopped;
            networkManager.OnServerStarted += NetworkManager_OnServerStarted;
            networkManager.OnServerStopped += NetworkManager_OnServerStopped;
            //networkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        public static void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsServer)
            {
                Plugin.Log.LogMessage("Client connected!");
                /*
                if (obj == networkManager.LocalClientId)
                {
                    if (OnConnected != null)
                        OnConnected.Invoke();
                }*/
            }
        }

        private static void AddMessageHandlers()
        {
            Plugin.Log.LogMessage("Adding message handlers.");

            var networkManager = NetworkManager.Singleton;
            foreach (var message in MessageTypes)
            {
                var name = message.Key;
                if (message.Value.IsRelay && networkManager.IsServer) // is relay
                {
                    Plugin.Log.LogMessage($"{NamePrefix}{name}Relay");
                    networkManager.CustomMessagingManager.UnregisterNamedMessageHandler($"{NamePrefix}{name}Relay");
                    networkManager.CustomMessagingManager.RegisterNamedMessageHandler($"{NamePrefix}{name}Relay", (clientId, reader) =>
                    {
                        RelayMessage(name, clientId, reader);
                    });
                }
                else if (!message.Value.ServerOnly || networkManager.IsServer)
                {
                    Plugin.Log.LogMessage(NamePrefix + name);
                    networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NamePrefix + name);
                    networkManager.CustomMessagingManager.RegisterNamedMessageHandler(NamePrefix + name, (clientId, reader) =>
                    {
                        ReceiveMessage(name, clientId, reader);
                    });
                }
            }
            if (!networkManager.IsServer)
            {
                //networkManager.CustomMessagingManager.RegisterNamedMessageHandler($"{NamePrefix}ClientConnected", ReceiveClientConnected);
                //networkManager.CustomMessagingManager.RegisterNamedMessageHandler($"{NamePrefix}ClientDisconnected", ReceiveClientDisconnected);
                networkManager.CustomMessagingManager.RegisterNamedMessageHandler($"{NamePrefix}NewPlayer", ReceiveNewPlayer);
                //networkManager.CustomMessagingManager.RegisterNamedMessageHandler($"{NamePrefix}Lobby", ReceiveLobby);
            }
            networkManager.CustomMessagingManager.RegisterNamedMessageHandler($"{NamePrefix}Kick", ReceiveKick);
        }

        private static void RemoveMessageHandlers()
        {
            Plugin.Log.LogMessage("Removing message handlers.");

            var networkManager = NetworkManager.Singleton;
            if (networkManager != null)
            {
                if (networkManager.CustomMessagingManager != null)
                {
                    if (MessageTypes != null)
                    {
                        foreach (var message in MessageTypes)
                        {
                            var name = message.Key;
                            networkManager.CustomMessagingManager.UnregisterNamedMessageHandler($"{NamePrefix}{name}Relay");
                            networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(NamePrefix + name);
                        }
                    }
                    networkManager.CustomMessagingManager.UnregisterNamedMessageHandler($"{NamePrefix}ClientDisconnected");
                    networkManager.CustomMessagingManager.UnregisterNamedMessageHandler($"{NamePrefix}Handshake");
                    networkManager.CustomMessagingManager.UnregisterNamedMessageHandler($"{NamePrefix}Kick");
                }
            }
        }

        private static void NetworkManager_OnServerStarted()
        {
            Plugin.Log.LogMessage("Server started!");

            AddMessageHandlers();
            var networkManager = NetworkManager.Singleton;
            //networkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            //networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }

        private static void NetworkManager_OnClientStarted()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogMessage("Client started!");

                AddMessageHandlers();
                //if (OnConnected != null)
                //    OnConnected.Invoke();
            }
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void GameStartedPatch()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (OnLobbyCreated != null)
                    OnLobbyCreated();
            }
            else
            {
                if (OnConnected != null)
                    OnConnected();
            }
        }

        private static void NetworkManager_OnServerStopped(bool obj)
        {
            Plugin.Log.LogMessage("Server stopped!");

            RemoveMessageHandlers();
        }

        private static void NetworkManager_OnClientStopped(bool obj)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogMessage("Client stopped!");

                RemoveMessageHandlers();
                if (OnDisconnected != null)
                    OnDisconnected.Invoke();
            }
        }

        [HarmonyPatch(typeof(global::GameNetworkManager), "Singleton_OnClientDisconnectCallback")]
        [HarmonyPrefix]
        private static void NetworkManager_OnClientDisconnectCallback(global::GameNetworkManager __instance, ulong clientId)
        {
            Plugin.Log.LogMessage($"OnClientDisconnect for {clientId}");

            if (clientId == NetworkManager.Singleton.LocalClientId && __instance.localClientWaitingForApproval)
            {
                return;
            }
            
            try
            {
                /*
                var startOfRound = global::StartOfRound.Instance;
                if (startOfRound.ClientPlayerList.ContainsKey(clientId))
                {
                    var player = Lobby.Player(clientId);
                    var playerID = player.PlayerNum;
                    Lobby.PlayerDisconnected(playerID);
                    if (OnPlayerDisconnected != null)
                        OnPlayerDisconnected.Invoke(clientId, playerID);

                    var networkManager = NetworkManager.Singleton;
                    if (networkManager.IsServer && networkManager.CustomMessagingManager != null)
                    {
                        using (FastBufferWriter bufferWriter = new FastBufferWriter(1024, Allocator.Temp, 65536))
                        {
                            bufferWriter.WriteValueSafe(playerID);
                            var messageName = $"{NamePrefix}ClientDisconnected";
                            var recipients = new List<ulong>(networkManager.ConnectedClientsIds);
                            recipients.Remove(NetworkManager.ServerClientId);
                            networkManager.CustomMessagingManager.SendNamedMessage(messageName, recipients, bufferWriter, NetworkDelivery.ReliableSequenced);
                        }
                    }
                }
                else
                {
                    Plugin.Log.LogWarning("Wasn't able to remove client " + clientId);
                }*/
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while disconnecting client:");
                Plugin.Log.LogError(e);
            }
        }
        
        /*
        private static void ReceiveClientConnected(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong clientID);
            if (OnPlayerConnected != null)
                OnPlayerConnected.Invoke(clientID);
        }*/
        /*
        private static void ReceiveClientDisconnected(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong clientID);
            Plugin.Log.LogMessage($"ClientDisconnected received for {clientID}.");

            var p = Lobby.Player(clientId);
            if (p != null)
            {
                Lobby.PlayerDisconnected(p.PlayerNum);
                if (OnPlayerDisconnected != null)
                    OnPlayerDisconnected.Invoke(clientID, p.PlayerNum);
            }
            else
            {
                Plugin.Log.LogWarning("Received disconnect for player not in lobby: " + clientId);
            }
        }
        */
        private static void ReceiveKick(ulong clientId, FastBufferReader reader)
        {
            Plugin.Log.LogMessage("Kick received.");

            reader.ReadValueSafe(out string reason);

            if (OnKicked != null)
                OnKicked.Invoke(reason);
        }

        private static IEnumerator KickPlayer(ulong clientId, string kickReason)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            var networkManager = NetworkManager.Singleton;
            networkManager.DisconnectClient(clientId, kickReason);
        }

        private static IEnumerator WaitForHandshake(ulong clientId)
        {
            yield return new WaitForSeconds(2.0f);
            if (Handshakes.Contains(clientId))
                Handshakes.Remove(clientId);
            else
            {
                var networkManager = NetworkManager.Singleton;
                networkManager.DisconnectClient(clientId, "You are missing the AdvancedCompany mod!");
            }
        }

        private static HashSet<ulong> Handshakes = new HashSet<ulong>();

        private static void ReceiveNewPlayer(ulong clientId, FastBufferReader reader)
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || networkManager.IsServer)
                return;
            Plugin.Log.LogInfo("New player connected to lobby.");
            try
            {
                reader.ReadValueSafe(out int playerNum);
                Plugin.Log.LogInfo("Player num: " + playerNum);
                var player = Game.Player.GetPlayer(playerNum);
                player.ReadData(reader);
                Network.Manager.Lobby.PlayerConnected(player, player.JoinedLate);
            } catch (Exception e)
            {
                Plugin.Log.LogError("Error while receiving handshake!");
                Plugin.Log.LogError(e);
            }
        }
        /*
        private static void ReceiveLobby(ulong clientId, FastBufferReader reader)
        {
            Plugin.Log.LogMessage("Lobby received.");

            Lobby = new Lobby();
            Lobby.ReadData(reader);

            if (OnReceivedLobby != null)
                OnReceivedLobby();
        }
        */
        private static INamedMessage ReceiveMessage(string name, ulong clientId, FastBufferReader reader)
        {
            Plugin.Log.LogMessage($"Message received '{name}' from {clientId}.");

            var networkManager = NetworkManager.Singleton;
            var m = MessageTypes[name]; // we can assume its set.
            var message = (INamedMessage)Activator.CreateInstance(m.Type);
            message.ReadData(reader);

            if (Listeners.ContainsKey(name))
            {
                for (var i = 0; i < Listeners[name].Count; i++)
                    Listeners[name][i].Item2(message);
            }
            return message;
        }

        private static void RelayMessage(string name, ulong clientId, FastBufferReader reader)
        {
            Plugin.Log.LogMessage($"Relaying message '{name}' from {clientId}.");

            var networkManager = NetworkManager.Singleton;
            var message = ReceiveMessage(name, clientId, reader);
            var recipients = new List<ulong>(networkManager.ConnectedClientsIds);

            recipients.Remove(0); // networkManager.LocalClientId
            recipients.Remove(clientId);
            Send(message, recipients);
            /*
            using (FastBufferWriter bufferWriter = new FastBufferWriter(1024, Allocator.Temp, 65536))
            {
                message.WriteData(bufferWriter);
                var recipients = new List<ulong>(networkManager.ConnectedClientsIds);
                recipients.Remove(networkManager.LocalClientId);
                recipients.Remove(clientId);
                networkManager.CustomMessagingManager.SendNamedMessage(NamePrefix + name, recipients, bufferWriter, NetworkDelivery.Reliable);
            }*/
        }

        public static void Send(INamedMessage message, ulong clientID = 0)
        {
            var attribute = message.GetType().GetCustomAttribute<Message>();
            var name = attribute.Name;
            var networkManager = NetworkManager.Singleton;
            if (clientID == 0 && attribute.IsRelay && networkManager.IsServer)
            {
                if (Listeners.ContainsKey(name))
                {
                    for (var i = 0; i < Listeners[name].Count; i++)
                        Listeners[name][i].Item2(message);
                }

                using (FastBufferWriter bufferWriter = new FastBufferWriter(1024, Allocator.Temp, int.MaxValue))
                {
                    message.WriteData(bufferWriter);
                    var messageName = NamePrefix + name;
                    var recipients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
                    recipients.Remove(0);
                    networkManager.CustomMessagingManager.SendNamedMessage(messageName, recipients, bufferWriter, NetworkDelivery.ReliableFragmentedSequenced);
                }
            }
            else
            {
                if (attribute.IsRelay)
                {
                    if (Listeners.ContainsKey(name))
                    {
                        for (var i = 0; i < Listeners[name].Count; i++)
                            Listeners[name][i].Item2(message);
                    }
                }
                using (FastBufferWriter bufferWriter = new FastBufferWriter(1024, Allocator.Temp, int.MaxValue))
                {
                    message.WriteData(bufferWriter);
                    var messageName = NamePrefix + name;
                    if (attribute.IsRelay)
                        messageName += "Relay";
                    networkManager.CustomMessagingManager.SendNamedMessage(messageName, clientID, bufferWriter, NetworkDelivery.ReliableFragmentedSequenced);
                }
            }
        }

        public static void Send(INamedMessage message, IReadOnlyList<ulong> recipients)
        {
            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsServer)
                throw new InvalidOperationException("Only server can send messages to clients.");
            var attribute = message.GetType().GetCustomAttribute<Message>();
            var name = attribute.Name;
            using (FastBufferWriter bufferWriter = new FastBufferWriter(1024, Allocator.Temp, int.MaxValue))
            {
                message.WriteData(bufferWriter);
                networkManager.CustomMessagingManager.SendNamedMessage(NamePrefix + name, recipients, bufferWriter, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public static void SendToAll(INamedMessage message)
        {
            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsServer)
                throw new InvalidOperationException("Only server can send messages to clients.");
            var attribute = message.GetType().GetCustomAttribute<Message>();
            var name = attribute.Name;
            using (FastBufferWriter bufferWriter = new FastBufferWriter(1024, Allocator.Temp, int.MaxValue))
            {
                message.WriteData(bufferWriter);
                var recipients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
                recipients.Remove(0);
                networkManager.CustomMessagingManager.SendNamedMessage(NamePrefix + name, recipients, bufferWriter, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public static void AddListener<T>(Action<T> listener) where T : INamedMessage
        {
            var name = typeof(T).GetCustomAttribute<Message>().Name;
            if (!Listeners.ContainsKey(name))
                Listeners.Add(name, new());
            Listeners[name].Add((listener, (msg) => listener((T)msg)));
        }

        public static bool RemoveListener<T>(Action<T> listener) where T : INamedMessage
        {
            var name = typeof(T).GetCustomAttribute<Message>().Name;
            if (Listeners.ContainsKey(name))
            {
                for (var i = 0; i < Listeners[name].Count; i++)
                {
                    if (Listeners[name][i].Item1 == (object) listener)
                    {
                        Listeners[name].RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void RemoveAllListeners<T>()
        {
            var name = typeof(T).GetCustomAttribute<Message>().Name;
            Listeners.Remove(name);
        }
    }
}
