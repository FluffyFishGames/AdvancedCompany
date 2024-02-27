using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("GrantShipXP", false, true)]
    internal class GrantShipXP : INamedMessage
    {
        internal int XP;
        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out XP);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(XP);
        }
    }
}
