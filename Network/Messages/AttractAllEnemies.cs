using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("AttractAllEnemies", false, true)]
    internal class AttractAllEnemies : INamedMessage
    {
        public bool Inside;
        public int PlayerNum;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out Inside);
            reader.ReadValueSafe(out PlayerNum);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(Inside);
            writer.WriteValueSafe(PlayerNum);
        }
    }
}
