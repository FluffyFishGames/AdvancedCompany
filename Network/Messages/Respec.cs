using System;
using System.Collections.Generic;
using System.Text;
using AdvancedCompany.Game;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("Respec", false, true)]
    internal class Respec : INamedMessage
    {
        internal int PlayerNum;
        internal Perk.Type Type;
        internal bool Reset;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out int type);
            Type = (Perk.Type)type;
            reader.ReadValueSafe(out Reset);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe((int)Type);
            writer.WriteValueSafe(Reset);
        }
    }
}
