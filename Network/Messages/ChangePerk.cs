using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("ChangePerk", false, true)]
    internal class ChangePerk : INamedMessage
    {
        internal int PlayerNum;
        internal string ID;
        internal int Level;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out ID);
            reader.ReadValueSafe(out Level);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(ID);
            writer.WriteValueSafe(Level);
        }
    }
}
