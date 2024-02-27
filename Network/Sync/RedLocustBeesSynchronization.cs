using AdvancedCompany.Lib.SyncCallbacks;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Lib.SyncHandler;

namespace AdvancedCompany.Network.Sync
{
    internal class RedLocustBeesSynchronization : ISyncHandler, INetworkObjectsPlaced
    {
        public ulong[] BeesNetworkIDs;
        public ulong[] HivesNetworkIDs;

        public string GetIdentifier()
        {
            return "AdvCmpny.RedLocust";
        }

        public void NetworkObjectsPlaced()
        {
            if (BeesNetworkIDs == null)
                return;
            for (var i = 0; i < BeesNetworkIDs.Length; i++)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(BeesNetworkIDs[i]) &&
                    NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(HivesNetworkIDs[i]))
                {
                    var beesNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[BeesNetworkIDs[i]];
                    var hiveNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[HivesNetworkIDs[i]];
                    var bees = beesNetworkObject.GetComponent<RedLocustBees>();
                    bees.hasSpawnedHive = true;
                    bees.hive = hiveNetworkObject.GetComponent<GrabbableObject>();
                }
                else
                {
                    Plugin.Log.LogWarning("Network object with ID " + BeesNetworkIDs[i] + " was not found!");
                }
            }
        }

        public void ReadDataFromClientBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
        }

        public void ReadDataFromJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int len);
            BeesNetworkIDs = new ulong[len];
            HivesNetworkIDs = new ulong[len];
            for (var i = 0; i < len; i++)
            {
                reader.ReadValueSafe(out BeesNetworkIDs[i]);
                reader.ReadValueSafe(out HivesNetworkIDs[i]);
            }
        }

        public void WriteDataToHostBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
        }

        public void WriteDataToPlayerJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
            var bees = GameObject.FindObjectsOfType<RedLocustBees>();
            var resultBees = new List<ulong>();
            var resultHives = new List<ulong>();
            for (var i = 0; i < bees.Length; i++)
            {
                if (bees[i].hive != null)
                {
                    var beeNetworkObject = bees[i].GetComponent<NetworkObject>();
                    resultBees.Add(beeNetworkObject.NetworkObjectId);
                    var hiveNetworkObject = bees[i].hive.GetComponent<NetworkObject>();
                    resultHives.Add(hiveNetworkObject.NetworkObjectId);
                }
            }
            writer.WriteValueSafe(resultBees.Count);
            for (var i = 0; i < resultBees.Count; i++)
            {
                writer.WriteValueSafe(resultBees[i]);
                writer.WriteValueSafe(resultHives[i]);
            }
        }
    }
}
