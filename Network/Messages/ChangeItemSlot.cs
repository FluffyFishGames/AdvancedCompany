using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("ChangeItemSlot", false, true)]
    internal class ChangeItemSlot : INamedMessage
    {
        internal int PlayerNum;
        internal int FromSlot;
        internal int ToSlot;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out FromSlot);
            reader.ReadValueSafe(out ToSlot);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(FromSlot);
            writer.WriteValueSafe(ToSlot);
        }
    }
}
