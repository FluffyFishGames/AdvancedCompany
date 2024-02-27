using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("GrantPlayerXP", false, true)]
    internal class GrantPlayerXP : INamedMessage
    {
        internal bool All;
        internal int PlayerNum;
        internal int XP;
        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out All);
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out XP);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(All);
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(XP);
        }
    }
}
