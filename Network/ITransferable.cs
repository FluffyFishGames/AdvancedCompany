using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Network
{
    internal interface ITransferable
    {
        void WriteData(FastBufferWriter writer);
        void ReadData(FastBufferReader reader);
    }
}
