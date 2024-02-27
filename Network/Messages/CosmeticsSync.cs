using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("CosmeticsSync", false, true)]
    internal class CosmeticsSync : INamedMessage
    {
        internal int PlayerNum;
        internal string[] Cosmetics;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out int length);
            var list = new List<string>();
            for (var i = 0; i < length; i++)
            {
                reader.ReadValueSafe(out string c);
                list.Add(c);
            }
            Cosmetics = list.ToArray();
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(Cosmetics.Length);
            for (var i = 0; i < Cosmetics.Length; i++)
                writer.WriteValueSafe(Cosmetics[i]);
        }
    }
}
