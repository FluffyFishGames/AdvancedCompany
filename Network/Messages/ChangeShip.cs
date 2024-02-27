using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("ChangeShip", false, true)]
    internal class ChangeShip : INamedMessage
    {
        internal int TotalQuota;
        internal bool ExtendedDeadline;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out TotalQuota);
            reader.ReadValueSafe(out ExtendedDeadline);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(TotalQuota);
            writer.WriteValueSafe(ExtendedDeadline);
        }
    }
}
