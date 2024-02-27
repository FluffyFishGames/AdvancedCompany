using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace AdvancedCompany.Network.Messages
{
    [Message("SpawnRocketFromClient", true)]
    internal class SpawnRocketFromClient : INamedMessage
    {
        internal int PlayerNum;
        internal long TempID;
        internal Vector3 Position;
        internal Quaternion Rotation;
        internal float TurbulenceTime;
        internal float TurbulenceSpeed;
        internal float TurbulenceStrength;
        internal float FlyTime;
        internal Color Color;
        internal double Time;

        public void ReadData(FastBufferReader reader)
        {
            reader.ReadValueSafe(out PlayerNum);
            reader.ReadValueSafe(out TempID);
            reader.ReadValueSafe(out Position);
            reader.ReadValueSafe(out Rotation);
            reader.ReadValueSafe(out TurbulenceTime);
            reader.ReadValueSafe(out TurbulenceSpeed);
            reader.ReadValueSafe(out TurbulenceStrength);
            reader.ReadValueSafe(out FlyTime);
            reader.ReadValueSafe(out Color);
            reader.ReadValueSafe(out Time);
        }

        public void WriteData(FastBufferWriter writer)
        {
            writer.WriteValueSafe(PlayerNum);
            writer.WriteValueSafe(TempID);
            writer.WriteValueSafe(Position);
            writer.WriteValueSafe(Rotation);
            writer.WriteValueSafe(TurbulenceTime);
            writer.WriteValueSafe(TurbulenceSpeed);
            writer.WriteValueSafe(TurbulenceStrength);
            writer.WriteValueSafe(FlyTime);
            writer.WriteValueSafe(Color);
            writer.WriteValueSafe(Time);
        }
    }
}
