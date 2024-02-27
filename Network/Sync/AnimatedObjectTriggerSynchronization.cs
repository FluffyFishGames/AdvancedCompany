using AdvancedCompany.Lib.SyncCallbacks;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Lib.SyncHandler;

namespace AdvancedCompany.Network.Sync
{
    internal class AnimatedObjectTriggerSynchronization : ISyncHandler, INetworkObjectsPlaced
    {
        public ulong[] ObjectNetworkIDs;
        public ushort[] ObjectBehaviourIDs;
        public bool[] BoolValue;

        public string GetIdentifier()
        {
            return "AdvCmpny.AnimatedObjectTrigger";
        }

        public void NetworkObjectsPlaced()
        {
            if (ObjectNetworkIDs == null)
                return;
            for (var i = 0; i < ObjectNetworkIDs.Length; i++)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ObjectNetworkIDs[i]))
                {
                    var doorNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ObjectNetworkIDs[i]];
                    var trigger = doorNetworkObject.GetNetworkBehaviourAtOrderIndex(ObjectBehaviourIDs[i]);
                    if (trigger is AnimatedObjectTrigger animatedTrigger)
                    {
                        animatedTrigger.boolValue = BoolValue[i];
                        if (animatedTrigger.triggerAnimator != null)
                            animatedTrigger.triggerAnimator.SetBool(animatedTrigger.animationString, BoolValue[i]);
                        if (animatedTrigger.triggerAnimatorB != null)
                            animatedTrigger.triggerAnimatorB.SetBool("on", BoolValue[i]);
                    }
                    else
                    {
                        Plugin.Log.LogWarning("Network object with ID " + ObjectNetworkIDs[i] + " had no trigger with ID " + ObjectBehaviourIDs[i]);
                    }
                }
                else
                {
                    Plugin.Log.LogWarning("Network object with ID " + ObjectNetworkIDs[i] + " was not found!");
                }
            }
        }

        public void ReadDataFromClientBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
        }

        public void ReadDataFromJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int len);
            ObjectNetworkIDs = new ulong[len];
            ObjectBehaviourIDs = new ushort[len];
            BoolValue = new bool[len];
            for (var i = 0; i < len; i++)
            {
                reader.ReadValueSafe(out ObjectNetworkIDs[i]);
                reader.ReadValueSafe(out ObjectBehaviourIDs[i]);
                reader.ReadValueSafe(out BoolValue[i]);
            }
        }

        public void WriteDataToHostBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
        }

        public void WriteDataToPlayerJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
            var triggers = GameObject.FindObjectsOfType<AnimatedObjectTrigger>();
            var resultTriggers = new List<ulong>();
            var resultScripts = new List<ushort>();
            var resultBoolValue = new List<bool>();
            for (var i = 0; i < triggers.Length; i++)
            {
                if (triggers[i].isBool)
                {
                    resultTriggers.Add(triggers[i].NetworkObjectId);
                    resultScripts.Add(triggers[i].NetworkBehaviourId);
                    resultBoolValue.Add(triggers[i].boolValue);
                }
            }
            writer.WriteValueSafe((int) resultTriggers.Count);
            for (var i = 0; i < resultTriggers.Count; i++)
            {
                writer.WriteValueSafe(resultTriggers[i]);
                writer.WriteValueSafe(resultScripts[i]);
                writer.WriteValueSafe(resultBoolValue[i]);
            }
        }
    }
}
