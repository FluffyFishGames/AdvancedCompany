using AdvancedCompany.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using static AdvancedCompany.Lib.Sync;
using Unity.Netcode;
using AdvancedCompany.Lib.SyncHandler;

namespace AdvancedCompany.Network.Sync.Items
{
    internal class BulletProofVestSynchronizationHandler : IItemSynchronisationHandler<BulletProofVest>
    {
        public int Damage;
        public override void Apply(BulletProofVest item)
        {
            item.Damage = Damage;
        }

        public override void Read(BulletProofVest item)
        {
            Damage = item.Damage;
        }

        public override void Read(FastBufferReader reader)
        {
            reader.ReadValueSafe(out Damage);
        }

        public override void Write(FastBufferWriter writer)
        {
            writer.WriteValueSafe(Damage);
        }
    }
}
