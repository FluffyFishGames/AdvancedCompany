using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AdvancedCompany.Boot
{
    public static class BootManager
    {
        public delegate void TypeCallback(Type t);
        private static List<TypeCallback> TypeCallbacks = new List<TypeCallback>();
        public static void AddTypeCallback(TypeCallback callback)
        {
            TypeCallbacks.Add(callback);
        }

        public static void Boot()
        {
            List<Type> bootables = new();
            Dictionary<Type, int> bootPlacement = new();
            Dictionary<Type, List<Type>> bootRequires = new();

            var types = typeof(BootManager).Assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var bootable = type.GetCustomAttribute<Bootable>();
                if (bootable != null)
                {
                    bootables.Add(type);
                    var requires = type.GetCustomAttributes<Requires>().ToList();
                    if (requires.Count() == 0)
                        bootPlacement.Add(type, 0);
                    else
                    {
                        bootRequires.Add(type, new List<Type>());
                        foreach (var require in requires)
                            bootRequires[type].Add(require.Type);
                    }
                }
            }

            int currentPlacement = 1;
            while (bootRequires.Count > 0)
            {
                var found = false;
                var foundTypes = new List<Type>();
                var newBootPlacements = new Dictionary<Type, int>();
                foreach (var b in bootRequires)
                {
                    var allFound = true;
                    foreach (var b2 in b.Value)
                    {
                        if (!bootPlacement.ContainsKey(b2))
                        {
                            allFound = false;
                            break;
                        }
                    }
                    if (allFound)
                    {
                        found = true;
                        foundTypes.Add(b.Key);
                        newBootPlacements.Add(b.Key, currentPlacement);
                    }
                }
                foreach (var b in foundTypes)
                    bootRequires.Remove(b);
                foreach (var kv in newBootPlacements)
                    bootPlacement.Add(kv.Key, kv.Value);
                if (found)
                    currentPlacement++;
                else
                    throw new Exception("There seems to be a circular boot requirement.");
            }

            var bootPhases = new List<List<Type>>();
            for (var i = 0; i < currentPlacement; i++)
            {
                var l = new List<Type>();
                foreach (var b in bootPlacement)
                {
                    if (b.Value == i)
                        l.Add(b.Key);
                }
                bootPhases.Add(l);
            }

            foreach (var phase in bootPhases)
            {
                foreach (var type in phase)
                {
                    var bootMethod = type.GetMethod("Boot", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    if (bootMethod != null && bootMethod.GetParameters().Length == 0)
                    {
                        bootMethod.Invoke(null, new object[0]);
                    }
                }
            }

            for (var i = 0; i < types.Length; i++)
            {
                for (var j = 0; j < TypeCallbacks.Count; j++)
                {
                    TypeCallbacks[j](types[i]);
                }
            }
        }
    }
}
