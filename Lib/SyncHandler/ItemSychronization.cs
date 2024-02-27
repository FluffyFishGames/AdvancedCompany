using AdvancedCompany.Lib.SyncCallbacks;
using AdvancedCompany.Network.Sync.Items;
using System;
using System.Collections.Generic;
using System.Text;
using static AdvancedCompany.Lib.Sync;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

namespace AdvancedCompany.Lib.SyncHandler
{
    public class ItemSynchronization : ISyncHandler, INetworkObjectsPlaced
    {
        private static Dictionary<Type, Type> Types = new Dictionary<Type, Type>();
        private static Dictionary<string, Type> TypesByName = new Dictionary<string, Type>();

        internal List<IItemSynchronizationHandler> Items = new List<IItemSynchronizationHandler>();
        internal List<ulong> NetworkIDs = new List<ulong>();

        static ItemSynchronization()
        {
            AddItemType<GrabbableObjectSynchronizationHandler>();
            AddItemType<RocketLaucherSynchronizationHandler>();
            AddItemType<BulletProofVestSynchronizationHandler>();
        }

        public static void AddItemType<T>() where T : IItemSynchronizationHandler, new()
        {
            var t = typeof(T);
            Type target = null;
            while (t != null)
            {
                var args = t.GetGenericArguments();
                if (args.Length > 0)
                {
                    target = args[0];
                    break;
                }
                t = t.BaseType;
            }

            Types[target] = typeof(T);
            TypesByName[target.Name] = typeof(T);
        }

        public string GetIdentifier()
        {
            return "AdvCmpny.Items";
        }

        public void NetworkObjectsPlaced()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var networkID = NetworkIDs[i];
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(networkID))
                {
                    Plugin.Log.LogMessage("Applying properties for network object " + networkID);
                    var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkID];
                    var grabbableObject = networkObject.GetComponent<GrabbableObject>();
                    Items[i].ApplyObject(grabbableObject);
                }
                else
                {
                    Plugin.Log.LogWarning("Network object with ID " + networkID + " was not found!");
                }
            }
        }

        public void ReadDataFromClientBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
        }

        public void ReadDataFromJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int objectLength);
            for (var i = 0; i < objectLength; i++)
            {
                reader.ReadValueSafe(out ulong networkID);
                reader.ReadValueSafe(out int descriptorsLength);
                for (var j = 0; j < descriptorsLength; j++)
                {
                    reader.ReadValueSafe(out string typeName, true);
                    if (TypesByName.ContainsKey(typeName))
                    {
                        var item = (IItemSynchronizationHandler)Activator.CreateInstance(TypesByName[typeName]);
                        item.Read(reader);
                        Items.Add(item);
                        NetworkIDs.Add(networkID);
                    }
                    else
                    {
                        Plugin.Log.LogError("Lobby data wasn't parseable. Missing type handler for: " + typeName);
                        break;
                    }
                }
            }
        }

        public void WriteDataToHostBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
        }

        public void WriteDataToPlayerJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
            var grabbableObjects = GameObject.FindObjectsOfType<GrabbableObject>();
            Dictionary<ulong, List<(string, IItemSynchronizationHandler)>> results = new Dictionary<ulong, List<(string, IItemSynchronizationHandler)>>();
            for (var i = 0; i < grabbableObjects.Length; i++)
            {
                var type = grabbableObjects[i].GetType();
                var networkObject = grabbableObjects[i].GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    var res = new List<(string, IItemSynchronizationHandler)>();
                    while (type != null)
                    {
                        if (Types.ContainsKey(type))
                        {
                            var item = (IItemSynchronizationHandler)Activator.CreateInstance(Types[type]);
                            res.Add((type.Name, item));
                            item.ReadObject(grabbableObjects[i]);
                        }
                        type = type.BaseType;
                    }
                    results[networkObject.NetworkObjectId] = res;
                }
            }

            writer.WriteValueSafe(results.Count);
            foreach (var kv in results)
            {
                writer.WriteValueSafe(kv.Key);
                writer.WriteValueSafe(kv.Value.Count);
                foreach (var item in kv.Value)
                {
                    writer.WriteValueSafe(item.Item1, true);
                    item.Item2.Write(writer);
                }
            }
        }
    }

}
