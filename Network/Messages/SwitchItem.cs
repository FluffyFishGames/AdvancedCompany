using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("SwitchItem", false, true)]
    internal class SwitchItem : INamedMessage
    {
        internal int PlayerNum;
        internal int Slot;
        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out this.PlayerNum);
            reader.ReadValueSafe(out this.Slot);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(this.PlayerNum);
            writer.WriteValueSafe(this.Slot);
        }
    }
}
