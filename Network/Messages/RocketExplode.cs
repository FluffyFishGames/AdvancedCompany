using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Network.Messages
{
    [Message("RocketExplode", false, true)]
    internal class RocketExplode : INamedMessage
    {
        internal long ID;
        internal Vector3 Position;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out ID);
            reader.ReadValueSafe(out Position);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(ID);
            writer.WriteValueSafe(Position);
        }
    }
}
