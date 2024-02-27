using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Unity.Netcode;

namespace AdvancedCompany.Config
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class Slider : System.Attribute
    {
        public float MinValue;
        public float MaxValue;
        public float Conversion { get; set; } = 1f;
        public bool ShowValue { get; set; } = false;
        public float InputWidth { get; set; } = 35f;

        public Slider(float minValue, float maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

    public abstract class Configuration
    {
        public class ConfigField
        {
            public Configuration Configuration;
            internal FieldDescriptor Field;
            public object DefaultValue;

            internal ConfigField(Configuration configuration, FieldDescriptor descriptor)
            {
                Configuration = configuration;
                Field = descriptor;
                var val = descriptor.Field.GetValue(Configuration);
                if (typeof(Configuration).IsAssignableFrom(descriptor.Field.FieldType))
                {
                    if (val == null)
                        val = Activator.CreateInstance(descriptor.Field.FieldType);
                    if (val is Configuration c)
                        c.Build();
                }
                else 
                {
                    if (val == null && Field.Field.FieldType != typeof(string))
                    {
                        if (Field.Field.FieldType.IsArray)
                            val = Array.CreateInstance(Field.Field.FieldType.GetElementType(), 0);
                        else
                            val = Activator.CreateInstance(Field.Field.FieldType);
                    }
                    DefaultValue = DeepClone(val);
                }
            }

            public object Value
            {
                get
                {
                    return Field.Field.GetValue(Configuration);
                }
                set
                {
                    Field.Field.SetValue(Configuration, value);
                }
            }

            public void Reset()
            {
                if (Value is Configuration c)
                    throw new ArgumentException("Can't clone configuration values!");
                Value = CloneDefault();
            }

            public object CloneValue()
            {
                if (Value is Configuration c)
                    throw new ArgumentException("Can't clone configuration values!");
                return DeepClone(Value);
            }

            public object CloneDefault()
            {
                if (typeof(Configuration).IsAssignableFrom(Field.Field.FieldType) || DefaultValue is Configuration c)
                    throw new ArgumentException("Can't clone configuration values!");
                return DeepClone(DefaultValue);
            }
        }

        internal List<ConfigField> Fields;

        public void Build()
        {
            if (Fields == null)
                Fields = GetFields(this);
        }

        public void SetDefault(string fieldName, object value)
        {
            var f = Field(fieldName);
            if (f != null)
                f.DefaultValue = DeepClone(value);
        }
        protected virtual void SetDefaults()
        {
            var t = this.GetType();
            var ret = new List<ConfigField>();
            foreach (var f in AllFields[t])
            {
                ret.Add(new ConfigField(this, f));
            }
            Fields = ret;
        }


        internal class FieldDescriptor
        {
            public FieldInfo Field;
            public string JSONName;
            public BaseConverter Converter;
            public Slider SliderAttribute;

            public FieldDescriptor(FieldInfo fieldInfo)
            {
                Field = fieldInfo;
                SliderAttribute = fieldInfo.GetCustomAttribute<Slider>();
                var name = fieldInfo.Name;
                var jsonName = "";
                var lastUpper = false;
                for (var i = 0; i < name.Length; i++)
                {
                    var isUpper = (name[i] + "") != (name[i] + "").ToLowerInvariant();
                    if (i > 0 && ((!lastUpper && isUpper)))
                        jsonName += ("_" + name[i]).ToLowerInvariant();
                    else
                        jsonName += ("" + name[i]).ToLowerInvariant();
                    lastUpper = isUpper;
                }
                JSONName = jsonName;
            }

            public FieldDescriptor(FieldInfo fieldInfo, BaseConverter converter) : this(fieldInfo)
            {
                Converter = converter;
            }
        }

        internal static object DeepClone(object obj)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            if (t.IsValueType || t == typeof(string))
                return obj;
            else if (obj is Array arr)
            {
                var newArr = Array.CreateInstance(t.GetElementType(), arr.Length);
                for (var i = 0; i < arr.Length; i++)
                    newArr.SetValue(DeepClone(arr.GetValue(i)), i);
                return newArr;
            }
            else
            {
                if (obj is Configuration configuration)
                {
                    var ret = configuration._Clone();
                    ret.Build();
                    return ret;
                }
                else
                {
                    var copy = Activator.CreateInstance(t);
                    if (obj is IList l1 && copy is IList l2)
                    {
                        foreach (var i in l1)
                            l2.Add(DeepClone(i));
                        return l2;
                    }
                    else if (obj is IDictionary d1 && copy is IDictionary d2)
                    {
                        foreach (var i in d1.Keys)
                        {
                            d2.Add(DeepClone(i), DeepClone(d1[i]));
                        }
                        return d2;
                    }
                    else throw new Exception("Cant clone " + t.FullName);
                }
            }
        }

        internal static Dictionary<Type, List<FieldDescriptor>> AllFields = new Dictionary<Type, List<FieldDescriptor>>();

        public Configuration() { }

        internal virtual Configuration _Clone()
        {
            var ret = (Configuration)Activator.CreateInstance(this.GetType());
            ret._CopyFrom(this);
            return ret;
        }

        internal static List<ConfigField> GetFields(Configuration instance)
        {
            if (instance.Fields != null)
                return instance.Fields;

            var t = instance.GetType();
            if (!AllFields.ContainsKey(t))
            {
                var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                var newFields = new List<FieldDescriptor>();
                foreach (var f in fields)
                {
                    var converter = BaseConverter.GetConverter(f.FieldType);
                    if (converter != null)
                    {
                        newFields.Add(new FieldDescriptor(f, converter));
                    }
                }
                AllFields[t] = newFields;
            }

            instance.SetDefaults();
            return instance.Fields;
        }

        public ConfigField Field(string fieldName)
        {
            for (var i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].Field.Field.Name == fieldName)
                {
                    return Fields[i];
                }
            }
            return null;
        }

        public virtual object Default(string fieldName)
        {
            for (var i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].Field.Field.Name == fieldName)
                {
                    return Fields[i].DefaultValue;
                }
            }
            return null;
        }


        public virtual void Reset(string fieldName)
        {
            for (var i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].Field.Field.Name == fieldName)
                {
                    Fields[i].Reset();
                    return;
                }
            }
        }

        public virtual void Reset()
        {
            for (var i = 0; i < Fields.Count; i++)
            {
                Fields[i].Reset();
            }
        }

        public virtual void CopyFrom(Configuration configuration)
        {
            _CopyFrom(configuration);
        }

        internal static Type FindCommonType(Type typeA, Type typeB)
        {
            if (typeA == typeB)
                return typeA;

            var pA = typeA.BaseType;
            while (pA != null)
            {
                if (pA == typeB)
                    return pA;
                pA = pA.BaseType;
            }
            var pB = typeB.BaseType;
            while (pB != null)
            {
                if (pB == typeA)
                    return pB;
                pB = pB.BaseType;
            }
            return null;
        }
        internal virtual void _CopyFrom(Configuration configuration, bool copyDefault = true)
        {
            var type = FindCommonType(this.GetType(), configuration.GetType());
            if (type != null)
            {
                var ownFields = GetFields(this);
                var otherFields = GetFields(configuration);

                for (var i = 0; i < ownFields.Count; i++)
                {
                    for (var j = 0; j < otherFields.Count; j++)
                    {
                        if (ownFields[i].Field.Field.Name == otherFields[j].Field.Field.Name)
                        {
                            //UnityEngine.Debug.Log(ownFields[i].Field.Field.Name + " | " + ownFields[i].Value + " | " + otherFields[j].Value);
                            if (ownFields[i].Value is Configuration config1 && otherFields[j].Value is Configuration config2)
                            {
                                config1._CopyFrom(config2, copyDefault);
                            }
                            else
                            {
                                if (copyDefault)
                                    ownFields[i].DefaultValue = otherFields[j].CloneDefault();
                                ownFields[i].Value = otherFields[j].CloneValue();
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Only same types can be copied.");
            }
        }

        public virtual void WriteData(FastBufferWriter writer)
        {
            var converter = BaseConverter.GetConverter(this.GetType());
            converter._Write(writer, this);
        }
        public virtual void ReadData(FastBufferReader reader)
        {
            var converter = BaseConverter.GetConverter(this.GetType());
            var copy = (Configuration) converter._Read(reader, this.GetType());
            _CopyFrom(copy, false);
        }

        public virtual void LoadedFromJSON()
        {

        }

        public virtual JObject ToJSON()
        {
            var converter = BaseConverter.GetConverter(this.GetType());
            return (JObject)converter._ToJSON(this);
        }

        public virtual void FromJSON(JObject obj, bool setDefault = false)
        {
            var converter = BaseConverter.GetConverter(this.GetType());
            var copy = (Configuration)converter._FromJSON(obj, this.GetType());
            _CopyFrom(copy, setDefault);
        }
    }
}