using System;
using System.Collections.Generic;
using System.Text;
using static AdvancedCompany.Lib.Sync;
using Unity.Netcode;
using AdvancedCompany.Lib.SyncHandler;
using UnityEngine;

namespace AdvancedCompany.Network.Sync.Items
{
    internal class GrabbableObjectSynchronizationHandler : IItemSynchronisationHandler<global::GrabbableObject>
    {
        public bool HasBattery = false;
        public float BatteryCharge = 0f;
        public Vector3 StartFallingPosition;
        public int FloorYRot;
        public float FallTime;
        public bool HasHitGround;
        public int ScrapValue;
        public bool ItemUsedUp;
        public bool IsPocketed;
        public bool IsInElevator;
        public bool IsBeingUsed;
        public bool IsInShipRoom;
        public bool IsInFactory;
        public bool Deactivated;
        public bool HasBeenHeld;
        public bool ScrapPersistedThroughRounds;
        public bool ReachedFloorTarget;
        public ulong ParentObject;

        public override void Apply(global::GrabbableObject item)
        {
            item.hasHitGround = HasHitGround;
            item.startFallingPosition = StartFallingPosition;
            item.floorYRot = FloorYRot;
            item.fallTime = FallTime;
            item.scrapValue = ScrapValue;
            item.itemUsedUp = ItemUsedUp;
            item.isPocketed = IsPocketed;
            item.isInElevator = IsInElevator;
            item.isBeingUsed = IsBeingUsed;
            item.isInShipRoom = IsInShipRoom;
            item.isInFactory = IsInFactory;
            item.deactivated = Deactivated;
            item.hasBeenHeld = HasBeenHeld;
            item.scrapPersistedThroughRounds = ScrapPersistedThroughRounds;
            item.reachedFloorTarget = ReachedFloorTarget;
            if (ParentObject > 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(ParentObject))
            {
                item.parentObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[ParentObject].transform;
            }
            if (HasBattery)
            {
                if (item.insertedBattery == null)
                    item.insertedBattery = new Battery(BatteryCharge == 0f, BatteryCharge);
                else
                    item.insertedBattery.charge = BatteryCharge;
            }
        }

        public override void Read(GrabbableObject item)
        {
            StartFallingPosition = item.startFallingPosition;
            FloorYRot = item.floorYRot;
            FallTime = item.fallTime;
            HasHitGround = item.hasHitGround;
            ScrapValue = item.scrapValue;
            ItemUsedUp = item.itemUsedUp;
            IsPocketed = item.isPocketed;
            IsInElevator = item.isInElevator;
            IsBeingUsed = item.isBeingUsed;
            IsInShipRoom = item.isInShipRoom;
            IsInFactory = item.isInFactory;
            Deactivated = item.deactivated;
            ScrapPersistedThroughRounds = item.scrapPersistedThroughRounds;
            ReachedFloorTarget = item.reachedFloorTarget;
            if (item.parentObject != null)
            {
                var network = item.GetComponent<NetworkObject>();
                if (network != null)
                    ParentObject = network.NetworkObjectId;
            }
            HasBattery = item.insertedBattery != null;
            if (item.insertedBattery != null)
                BatteryCharge = item.insertedBattery.charge;
        }

        public override void Read(FastBufferReader reader)
        {
            reader.ReadValueSafe(out StartFallingPosition);
            reader.ReadValueSafe(out FloorYRot);
            reader.ReadValueSafe(out FallTime);
            reader.ReadValueSafe(out HasHitGround);
            reader.ReadValueSafe(out ScrapValue);
            reader.ReadValueSafe(out ItemUsedUp);
            reader.ReadValueSafe(out IsPocketed);
            reader.ReadValueSafe(out IsInElevator);
            reader.ReadValueSafe(out IsBeingUsed);
            reader.ReadValueSafe(out IsInShipRoom);
            reader.ReadValueSafe(out IsInFactory);
            reader.ReadValueSafe(out Deactivated);
            reader.ReadValueSafe(out ParentObject);
            reader.ReadValueSafe(out HasBattery);
            reader.ReadValueSafe(out ScrapPersistedThroughRounds);
            reader.ReadValueSafe(out ReachedFloorTarget);
            if (HasBattery)
                reader.ReadValueSafe(out BatteryCharge);
        }
        public override void Write(FastBufferWriter writer)
        {
            writer.WriteValueSafe(StartFallingPosition);
            writer.WriteValueSafe(FloorYRot);
            writer.WriteValueSafe(FallTime);
            writer.WriteValueSafe(HasHitGround);
            writer.WriteValueSafe(ScrapValue);
            writer.WriteValueSafe(ItemUsedUp);
            writer.WriteValueSafe(IsPocketed);
            writer.WriteValueSafe(IsInElevator);
            writer.WriteValueSafe(IsBeingUsed);
            writer.WriteValueSafe(IsInShipRoom);
            writer.WriteValueSafe(IsInFactory);
            writer.WriteValueSafe(Deactivated);
            writer.WriteValueSafe(ParentObject);
            writer.WriteValueSafe(HasBattery);
            writer.WriteValueSafe(ScrapPersistedThroughRounds);
            writer.WriteValueSafe(ReachedFloorTarget);
            if (HasBattery)
                writer.WriteValueSafe(BatteryCharge);
        }
    }

}
