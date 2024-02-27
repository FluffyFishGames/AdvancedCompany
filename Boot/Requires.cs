using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Boot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class Requires : System.Attribute
    {
        public Type Type;
        public Requires(Type type)
        {
            Type = type;
        }
    }
}
