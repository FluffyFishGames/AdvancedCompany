using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

namespace AdvancedCompany.Network
{
    internal class Message : System.Attribute
    {
        internal string Name;
        internal bool ServerOnly;
        internal bool IsRelay;
        internal Message(string name, bool serverOnly = false, bool isRelay = false)
        {
            Name = name;
            ServerOnly = serverOnly;
            IsRelay = isRelay;
        }
    }
}
