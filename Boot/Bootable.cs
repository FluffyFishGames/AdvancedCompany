using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Boot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Bootable : System.Attribute
    {
    }
}
