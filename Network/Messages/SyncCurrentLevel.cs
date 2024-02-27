using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("SyncCurrentLevel", false, false)]
    internal class SyncCurrentLevel : INamedMessage
    {
        internal int CurrentLevelID;
        internal int CurrentWeather;
        internal int CurrentSeed;
        internal int[] DeadPlayers;
        internal int[] JoinedLate;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out CurrentLevelID);
            reader.ReadValueSafe(out CurrentSeed);
            reader.ReadValueSafe(out CurrentWeather);
            reader.ReadValueSafe(out DeadPlayers);
            reader.ReadValueSafe(out JoinedLate);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(CurrentLevelID);
            writer.WriteValueSafe(CurrentSeed);
            writer.WriteValueSafe(CurrentWeather);
            writer.WriteValueSafe(DeadPlayers);
            writer.WriteValueSafe(JoinedLate);
        }
    }
}
