using AdvancedCompany.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using static AdvancedCompany.Lib.Sync;
using Unity.Netcode;
using AdvancedCompany.Lib.SyncHandler;

namespace AdvancedCompany.Network.Sync.Items
{
    internal class RocketLaucherSynchronizationHandler : IItemSynchronisationHandler<MissileLauncher>
    {
        public int Ammo;
        public override void Apply(MissileLauncher item)
        {
            item.Ammo = Ammo;
        }

        public override void Read(MissileLauncher item)
        {
            Ammo = item.Ammo;
        }

        public override void Read(FastBufferReader reader)
        {
            reader.ReadValueSafe(out Ammo);
        }

        public override void Write(FastBufferWriter writer)
        {
            writer.WriteValueSafe(Ammo);
        }
    }
}
