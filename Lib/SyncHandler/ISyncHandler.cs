using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace AdvancedCompany.Lib.SyncHandler
{
    public interface ISyncHandler
    {
        string GetIdentifier();
        void WriteDataToPlayerJoiningLobby(Sync sync, FastBufferWriter writer);
        void ReadDataFromJoiningLobby(Sync sync, FastBufferReader reader);
        void WriteDataToHostBeforeJoining(Sync sync, FastBufferWriter writer);
        void ReadDataFromClientBeforeJoining(Sync sync, FastBufferReader reader);
    }
}
