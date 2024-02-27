using AdvancedCompany.Config;
using AdvancedCompany.Lib.SyncCallbacks;
using AdvancedCompany.Lib.SyncHandler;
using AdvancedCompany.Network.Sync;
using AdvancedCompany.Objects;
using AdvancedCompany.Patches;
using Steamworks.Ugc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static AdvancedCompany.Lib.Sync;
using static AdvancedCompany.Patches.LobbySizePatches;

namespace AdvancedCompany.Lib
{
    public class Sync
    {
        internal NetworkManager.ConnectionApprovalResponse Response;
        internal bool IsFinished = false;

        private static List<Type> Syncers = new List<Type>();
        private List<ISyncHandler> SyncHandlers = new List<ISyncHandler>();
        public static void AddSyncHandler<T>() where T : ISyncHandler, new()
        {
            Syncers.Add(typeof(T));
        }

        internal Sync()
        {
            for (var i = 0; i < Syncers.Count; i++)
            {
                SyncHandlers.Add((ISyncHandler) Activator.CreateInstance(Syncers[i]));
            }
        }

        internal void WriteDataToPlayerJoiningLobby(FastBufferWriter writer)
        {
            Plugin.Log.LogMessage("Writing data to client joining...");
            writer.WriteValueSafe(MagicLong);
            var sizePos = writer.Position;
            writer.WriteValueSafe((int)0);
            writer.WriteValueSafe(SyncHandlers.Count);
            Plugin.Log.LogDebug("Writing data for a total of " + SyncHandlers.Count + " synchronizers.");
            for (var i = 0; i < SyncHandlers.Count; i++)
            {
                var name = SyncHandlers[i].GetIdentifier();
                Plugin.Log.LogDebug("Writing data for " + name);
                writer.WriteValueSafe(name, true);
                var pos = writer.Position;
                writer.WriteValueSafe((int)0);
                SyncHandlers[i].WriteDataToPlayerJoiningLobby(this, writer);
                var pos2 = writer.Position;
                var len = (pos2 - pos) - 4;
                Plugin.Log.LogDebug("Data size: " + len);
                writer.Seek(pos);
                writer.WriteValueSafe(len);
                writer.Seek(pos2);
            }
            var finalPos = writer.Position;
            writer.Seek(sizePos);
            var totalSize = (int)((finalPos - sizePos) - 4);
            Plugin.Log.LogDebug("Total package size: " + totalSize);
            writer.WriteValueSafe(totalSize);
            writer.Seek(finalPos);
        }

        internal void WriteDataToHostBeforeJoining(FastBufferWriter writer)
        {
            Plugin.Log.LogMessage("Writing handshake data before joining...");
            writer.WriteValueSafe(MagicLong);
            var sizePos = writer.Position;
            writer.WriteValueSafe((int)0);

            writer.WriteValueSafe(SyncHandlers.Count);
            Plugin.Log.LogDebug("Writing data for a total of " + SyncHandlers.Count + " synchronizers.");
            for (var i = 0; i < SyncHandlers.Count; i++)
            {
                var name = SyncHandlers[i].GetIdentifier();
                Plugin.Log.LogDebug("Writing data for " + name);
                writer.WriteValueSafe(name, true);
                var pos = writer.Position;
                writer.WriteValueSafe((int)0);
                SyncHandlers[i].WriteDataToHostBeforeJoining(this, writer);
                var pos2 = writer.Position;
                var len = (pos2 - pos) - 4;
                Plugin.Log.LogDebug("Data size: " + len);
                writer.Seek(pos);
                writer.WriteValueSafe(len);
                writer.Seek(pos2);
            }
            var finalPos = writer.Position;
            writer.Seek(sizePos);
            var totalSize = (int)((finalPos - sizePos) - 4);
            Plugin.Log.LogDebug("Total package size: " + totalSize);
            writer.WriteValueSafe(totalSize);
            writer.Seek(finalPos);
        }

        internal const ulong MagicLong = 0x42069420;

        internal int SearchMagicLong(FastBufferReader reader)
        {
            var startPos = reader.Position;
            reader.Seek(0);

            byte[] data = new byte[reader.Length];
            reader.ReadBytes(ref data, data.Length, 0);
            reader.Seek(startPos);
            for (var i = 0; i < data.Length - sizeof(ulong); i++)
            {
                var found = System.BitConverter.ToUInt64(data, i);
                if (found == MagicLong)
                {
                    Plugin.Log.LogInfo("Found magic long at position " + i);
                    return i;
                }
            }
            return -1;
        }
        internal void ReadDataFromClientBeforeJoining(FastBufferReader reader)
        {
            Plugin.Log.LogMessage("Reading data from handshake...");

            reader.ReadValue(out ulong magicLong);
            if (magicLong != MagicLong)
            {
                Plugin.Log.LogWarning("Something interferred with the network handshake. Panic mode initiated. Please consider removing conflicting mods. Searching for magic long...");
                var pos = SearchMagicLong(reader);
                if (pos == -1)
                {
                    Plugin.Log.LogError("Handshake message was destroyed by another mod. Please report this to the dev and check your mods. Sorry, we can't continue past this point. :(");
                    throw new Exception("Handshake message was destroyed by another mod. Please report this to the dev and check your mods. Sorry, we can't continue past this point. :(");
                }
                else reader.Seek(pos);
            }
            reader.ReadValue(out int packageLength);
            Plugin.Log.LogDebug("Total package size: " + packageLength);


            reader.ReadValueSafe(out int len);
            if (SyncHandlers.Count != len)
                Plugin.Log.LogWarning("There was a mismatch in synchronizers. Have: " + SyncHandlers.Count + "; Received: " + len);

            Plugin.Log.LogDebug("Reading a total of " + len + " synchronizers.");
            for (var i = 0; i < len; i++)
            {
                if (this.Error != null)
                    break;
                reader.ReadValueSafe(out string name, true);
                reader.ReadValueSafe(out int blockLength);
                Plugin.Log.LogDebug("Reading block " + name + " with size " + blockLength);
                Plugin.Log.LogDebug("Pointer: " + reader.Position);
                bool found = false;
                for (var j = 0; j < SyncHandlers.Count; j++)
                {
                    if (SyncHandlers[i].GetIdentifier() == name)
                    {
                        SyncHandlers[i].ReadDataFromClientBeforeJoining(this, reader);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Plugin.Log.LogWarning("Syncer " + name + " wasn't found!");
                    reader.Seek(reader.Position + blockLength);
                }
            }            
        }

        internal void ReadDataFromJoiningLobby(FastBufferReader reader)
        {
            Plugin.Log.LogMessage("Reading data from lobby...");

            reader.ReadValue(out ulong magicLong);
            if (magicLong != MagicLong)
            {
                Plugin.Log.LogWarning("Something interferred with the network handshake. Panic mode initiated. Please consider removing conflicting mods. Searching for magic long...");
                var pos = SearchMagicLong(reader);
                if (pos == -1)
                {
                    Plugin.Log.LogError("Handshake message was destroyed by another mod. Please report this to the dev and check your mods. Sorry, we can't continue past this point. :(");
                    throw new Exception("Handshake message was destroyed by another mod. Please report this to the dev and check your mods. Sorry, we can't continue past this point. :(");
                }
                else reader.Seek(pos);
            }
            reader.ReadValue(out int packageLength);
            Plugin.Log.LogMessage("Total package size: " + packageLength);

            reader.ReadValueSafe(out int len);
            if (SyncHandlers.Count != len)
                Plugin.Log.LogWarning("There was a mismatch in synchronizers. Have: " + SyncHandlers.Count + "; Received: " + len);

            Plugin.Log.LogMessage("Reading a total of " + len + " synchronizers.");
            for (var i = 0; i < len; i++)
            {
                reader.ReadValueSafe(out string name, true);
                reader.ReadValueSafe(out int blockLength);
                var startPos = reader.Position;
                Plugin.Log.LogMessage("Reading block " + name + " with size " + blockLength);
                Plugin.Log.LogMessage("Pointer: " + reader.Position);

                bool found = false;
                for (var j = 0; j < SyncHandlers.Count; j++)
                {
                    if (SyncHandlers[i].GetIdentifier() == name)
                    {
                        try
                        {
                            SyncHandlers[i].ReadDataFromJoiningLobby(this, reader);
                            found = true;
                            break;
                        }
                        catch (Exception e)
                        {
                            Plugin.Log.LogError("Error while syncing " + name + " data:");
                            Plugin.Log.LogError(e);
                            reader.Seek(startPos + blockLength);
                        }
                    }
                }
                if (!found)
                {
                    Plugin.Log.LogWarning("Syncer " + name + " wasn't found!");
                    reader.Seek(reader.Position + blockLength);
                }
            }
        }

        internal Sync PlayerJoining(ulong clientID)
        {
            return new Sync();
        }

        public T GetSyncHandler<T>() where T : ISyncHandler
        {
            for (var i = 0; i < SyncHandlers.Count; i++)
            {
                if (SyncHandlers[i] is T p)
                    return p;
            }
            return default(T);
        }

        internal void LevelLoaded()
        {
            Plugin.Log.LogMessage("Executing level loaded callback...");
            for (var i = 0; i < SyncHandlers.Count; i++)
            {
                try
                {
                    if (SyncHandlers[i] is ILevelLoaded p)
                        p.LevelLoaded();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while applying " + SyncHandlers[i].GetIdentifier() + " synchronizer!");
                    Plugin.Log.LogError(e);
                }
            }
        }

        internal void NetworkObjectsPlaced()
        {
            Plugin.Log.LogMessage("Executing network objects placed callback...");
            for (var i = 0; i < SyncHandlers.Count; i++)
            {
                try
                {
                    if (SyncHandlers[i] is INetworkObjectsPlaced p)
                        p.NetworkObjectsPlaced();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while applying " + SyncHandlers[i].GetIdentifier() + " synchronizer!");
                    Plugin.Log.LogError(e);
                }
            }
        }
        internal void ConnectClientToPlayerObject(GameNetcodeStuff.PlayerControllerB player)
        {
            Plugin.Log.LogMessage("Executing connect client to player object callback...");
            for (var i = 0; i < SyncHandlers.Count; i++)
            {
                try
                {
                    if (SyncHandlers[i] is IConnectClientToPlayerObject p)
                        p.ConnectClientToPlayerObject(player);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while applying " + SyncHandlers[i].GetIdentifier() + " synchronizer!");
                    Plugin.Log.LogError(e);
                }
            }
        }

        public ulong ClientId;
        internal ulong ConfigHash;
        internal string Error;
        public void SetError(string connectionError)
        {
            Error = connectionError;
        }

        static Sync()
        {
            AddSyncHandler<LobbyDataSynchronization>();
            AddSyncHandler<RedLocustBeesSynchronization>();
            AddSyncHandler<ItemSynchronization>();
            AddSyncHandler<DoorLockSynchronization>();
            AddSyncHandler<AnimatedObjectTriggerSynchronization>();
            AddSyncHandler<OtherSynchronization>();
        }
    }
}
