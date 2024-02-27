using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("RocketJump", false, true)]
    internal class RocketJump : INamedMessage
    {
        internal int PlayerNum;
        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out this.PlayerNum);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(this.PlayerNum);
        }
    }
}
