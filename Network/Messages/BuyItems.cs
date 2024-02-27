using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("BuyItems", false, true)]
    internal class BuyItems : INamedMessage
    {
        public int NewCredits;
        public int[] Items;
        public int[] Unlockables;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out NewCredits);
            reader.ReadValueSafe(out Items);
            reader.ReadValueSafe(out Unlockables);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(NewCredits);
            writer.WriteValueSafe(Items);
            writer.WriteValueSafe(Unlockables);
        }
    }
}
