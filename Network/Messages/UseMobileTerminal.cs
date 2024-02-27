using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("UseMobileTerminal", false, true)]
    internal class UseMobileTerminal : INamedMessage
    {
        public int PlayerNum;
        public bool IsUsingTerminal;
        public string ConsoleText;
        public string InputText;
        public float Scroll;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out IsUsingTerminal);
            if (IsUsingTerminal)
            {
                reader.ReadValueSafe(out ConsoleText);
                reader.ReadValueSafe(out InputText);
                reader.ReadValueSafe(out Scroll);
            }
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(IsUsingTerminal);
            if (IsUsingTerminal)
            {
                writer.WriteValueSafe(ConsoleText);
                writer.WriteValueSafe(InputText);
                writer.WriteValueSafe(Scroll);
            }
        }
    }
}
