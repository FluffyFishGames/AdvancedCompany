using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("ActivateTacticalHelmet", false, true)]
    internal class ActivateTacticalHelmet : INamedMessage
    {
        internal int PlayerNum;
        internal bool IsUsed;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out IsUsed);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(IsUsed);
        }
    }
}
