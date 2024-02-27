using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Lib.SyncCallbacks
{
    public interface IConnectClientToPlayerObject
    {
        public void ConnectClientToPlayerObject(global::GameNetcodeStuff.PlayerControllerB player);
    }
}
