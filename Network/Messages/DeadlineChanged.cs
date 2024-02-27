using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("DeadlineChanged")]
    internal class DeadlineChanged : INamedMessage
    {
        internal float NewDeadline;
        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out NewDeadline);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(NewDeadline);
        }
    }
}
