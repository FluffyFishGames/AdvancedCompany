using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("SyncAnimationOverride", false, true)]
    internal class SyncAnimationOverride : INamedMessage
    {
        public int PlayerNum;
        public string OriginalName;
        public string ReplacementName;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValue(out PlayerNum);
            reader.ReadValue(out OriginalName, true);
            reader.ReadValue(out ReplacementName, true);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValue(PlayerNum);
            writer.WriteValue(OriginalName, true);
            writer.WriteValue(ReplacementName, true);
        }
    }
}
