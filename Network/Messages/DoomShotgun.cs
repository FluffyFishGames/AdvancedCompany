using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace AdvancedCompany.Network.Messages
{
    [Message("DoomShotgun", false, true)]
    public class DoomShotgun : INamedMessage
    {
        public int PlayerNum;
        public Vector3 ShotgunPosition;
        public Vector3 ShotgunForward;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out ShotgunPosition);
            reader.ReadValueSafe(out ShotgunForward);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(ShotgunPosition);
            writer.WriteValueSafe(ShotgunForward);
        }
    }
}
