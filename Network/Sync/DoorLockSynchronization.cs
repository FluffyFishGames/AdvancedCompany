using AdvancedCompany.Lib.SyncCallbacks;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Lib.SyncHandler;
using AdvancedCompany.Lib;
using UnityEngine.InputSystem;

namespace AdvancedCompany.Network.Sync
{
    internal class DoorLockSynchronization : ISyncHandler, INetworkObjectsPlaced
    {
        public ulong[] DoorNetworkIDs;
        public bool[] IsDoorOpened;
        public bool[] IsLocked;
        public float[] LockPickTimeLeft;
        public float[] EnemyDoorMeter;

        public string GetIdentifier()
        {
            return "AdvCmpny.DoorLocks";
        }

        public void NetworkObjectsPlaced()
        {
            if (DoorNetworkIDs == null)
                return;
            for (var i = 0; i < DoorNetworkIDs.Length; i++)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(DoorNetworkIDs[i]))
                {
                    var doorNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[DoorNetworkIDs[i]];
                    var @lock = doorNetworkObject.gameObject.GetComponentInChildren<DoorLock>();
                    @lock.isDoorOpened = IsDoorOpened[i];
                    @lock.isLocked = IsLocked[i];
                    @lock.lockPickTimeLeft = LockPickTimeLeft[i];
                    @lock.enemyDoorMeter = EnemyDoorMeter[i];
                }
                else
                {
                    Plugin.Log.LogWarning("Network object with ID " + DoorNetworkIDs[i] + " was not found!");
                }
            }
        }

        public void ReadDataFromClientBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
        }

        public void ReadDataFromJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int len);
            DoorNetworkIDs = new ulong[len];
            IsDoorOpened = new bool[len];
            IsLocked = new bool[len];
            LockPickTimeLeft = new float[len];
            EnemyDoorMeter = new float[len];
            for (var i = 0; i < len; i++)
            {
                reader.ReadValueSafe(out DoorNetworkIDs[i]);
                reader.ReadValueSafe(out IsDoorOpened[i]);
                reader.ReadValueSafe(out IsLocked[i]);
                reader.ReadValueSafe(out LockPickTimeLeft[i]);
                reader.ReadValueSafe(out EnemyDoorMeter[i]);
            }
        }

        public void WriteDataToHostBeforeJoining(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
        }

        public void WriteDataToPlayerJoiningLobby(AdvancedCompany.Lib.Sync sync, FastBufferWriter writer)
        {
            var locks = GameObject.FindObjectsOfType<DoorLock>();
            var resultDoors = new List<ulong>();
            var resultIsDoorOpened = new List<bool>();
            var resultIsLocked = new List<bool>();
            var resultLockPickTimeLeft = new List<float>();
            var resultEnemyDoorMeter = new List<float>();
            for (var i = 0; i < locks.Length; i++)
            {
                resultDoors.Add(locks[i].NetworkObjectId);
                resultIsDoorOpened.Add(locks[i].isDoorOpened);
                resultIsLocked.Add(locks[i].isLocked);
                resultLockPickTimeLeft.Add(locks[i].lockPickTimeLeft);
                resultEnemyDoorMeter.Add(locks[i].enemyDoorMeter);
            }
            writer.WriteValueSafe((int) resultDoors.Count);
            for (var i = 0; i < resultDoors.Count; i++)
            {
                writer.WriteValueSafe(resultDoors[i]);
                writer.WriteValueSafe(resultIsDoorOpened[i]);
                writer.WriteValueSafe(resultIsLocked[i]);
                writer.WriteValueSafe(resultLockPickTimeLeft[i]);
                writer.WriteValueSafe(resultEnemyDoorMeter[i]);
            }
        }
    }
}
