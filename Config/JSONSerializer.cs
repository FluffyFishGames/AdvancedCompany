using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AdvancedCompany.Config
{
    internal class JSONSerializer
    {
        /*
        public class CachedType
        {
            internal Type Type;
            public List<CachedField> Fields = new List<CachedField>();

            public CachedType(Type t)
            {
                Type = t;

            }

            public JObject Serialize(object obj)
            {
                if (obj.GetType() != Type)
                    throw new ArgumentException();
                JObject jObj = new JObject();
                foreach (var f in Fields)
                {
                    jObj[f.JSONName] = f.Serialize(jObj);
                }
                return jObj;
            }
        }

        public class CachedField
        {
            internal FieldInfo Field;
            public string JSONName;
            
            public CachedField(FieldInfo field)
            {
                Field = field;

                var name = field.Name;
                var jsonName = "";
                for (var i = 0; i < name.Length; i++)
                {
                    if ((name[i] + "") == (name[i] + "").ToUpperInvariant())
                        jsonName += ("_" + name[i]).ToLowerInvariant();
                    else
                        jsonName += name[i];
                }
                JSONName = jsonName;
            }

            public JValue Serialize(object obj)
            {
                Field.FieldType
            }

        }
        private static Dictionary<Type, CachedType> Types = new Dictionary<Type, CachedType>();
        public static JObject Serialize(object obj)
        {
            var type = obj.GetType();
            if (!Types.ContainsKey(type))
                Types.Add(type, new CachedType(type));
        }*/
    }
}
