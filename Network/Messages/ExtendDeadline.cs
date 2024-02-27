using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("ExtendDeadline", true)]
    internal class ExtendDeadline : INamedMessage
    {
        public void ReadData(FastBufferReader reader)
        {
        }

        public void WriteData(FastBufferWriter writer)
        {
        }
    }
}
