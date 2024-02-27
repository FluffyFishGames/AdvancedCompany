using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

namespace AdvancedCompany.Config
{
    public abstract class BaseConverter
    {

        static BaseConverter()
        {
            BaseConverter.AddType<bool>(new DelegateConverter<bool>((reader, type) => { reader.ReadValueSafe(out bool @bool); return @bool; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<bool>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<sbyte>(new DelegateConverter<sbyte>((reader, type) => { reader.ReadValueSafe(out sbyte @sbyte); return @sbyte; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<sbyte>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<byte>(new DelegateConverter<byte>((reader, type) => { reader.ReadValueSafe(out byte @byte); return @byte; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<byte>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<short>(new DelegateConverter<short>((reader, type) => { reader.ReadValueSafe(out short @short); return @short; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<short>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<ushort>(new DelegateConverter<ushort>((reader, type) => { reader.ReadValueSafe(out ushort @ushort); return @ushort; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<ushort>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<int>(new DelegateConverter<int>((reader, type) => { reader.ReadValueSafe(out int @int); return @int; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<int>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<uint>(new DelegateConverter<uint>((reader, type) => { reader.ReadValueSafe(out uint @uint); return @uint; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<uint>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<long>(new DelegateConverter<long>((reader, type) => { reader.ReadValueSafe(out long @long); return @long; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<long>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<ulong>(new DelegateConverter<ulong>((reader, type) => { reader.ReadValueSafe(out ulong @ulong); return @ulong; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<ulong>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<float>(new DelegateConverter<float>((reader, type) => { reader.ReadValueSafe(out float @float); return @float; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<float>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<double>(new DelegateConverter<double>((reader, type) => { reader.ReadValueSafe(out double @double); return @double; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<double>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<string>(new DelegateConverter<string>((reader, type) => { reader.ReadValueSafe(out string @string); return @string; }, (writer, val) => { writer.WriteValueSafe(val); }, (token, type) => { return token.ToObject<string>(); }, (val) => { return JToken.FromObject(val); }));
            BaseConverter.AddType<IDictionary>(new DelegateConverter<IDictionary>(
                (reader, type) => {
                    var generics = type.GetGenericArguments();
                    if (generics.Length < 2)
                        throw new Exception("Unsupported dictionary given. Missing generics.");
                    var keyConverter = BaseConverter.GetConverter(generics[0]);
                    var valueConverter = BaseConverter.GetConverter(generics[1]);

                    var ret = (IDictionary)Activator.CreateInstance(type);
                    reader.ReadValueSafe(out int count);
                    for (var i = 0; i < count; i++)
                    {
                        object key = keyConverter._Read(reader, generics[0]);
                        object value = valueConverter._Read(reader, generics[1]);
                        ret.Add(key, value);
                    }
                    return ret;
                },
                (writer, val) => {
                    var type = val.GetType();
                    var generics = type.GetGenericArguments();
                    if (generics.Length < 2)
                        throw new Exception("Unsupported dictionary given. Missing generics.");
                    var keyConverter = BaseConverter.GetConverter(generics[0]);
                    var valueConverter = BaseConverter.GetConverter(generics[1]);

                    var dict = (IDictionary)val;
                    writer.WriteValueSafe(dict.Count);
                    foreach (var key in dict.Keys)
                    {
                        keyConverter._Write(writer, key);
                        valueConverter._Write(writer, dict[key]);
                    }
                },
                (token, type) => {
                    if (token is JArray arr)
                    {
                        var generics = type.GetGenericArguments();
                        if (generics.Length < 2)
                            throw new Exception("Unsupported dictionary given. Missing generics.");
                        var keyConverter = BaseConverter.GetConverter(generics[0]);
                        var valueConverter = BaseConverter.GetConverter(generics[1]);

                        var ret = (IDictionary)Activator.CreateInstance(type);
                        for (var i = 0; i < arr.Count; i++)
                        {
                            if (arr[i] is JObject obj && obj.ContainsKey("key") && obj.ContainsKey("value"))
                            {
                                var key = keyConverter._FromJSON(obj["key"], generics[0]);
                                var value = valueConverter._FromJSON(obj["value"], generics[1]);
                                ret.Add(key, value);
                            }
                            else throw new Exception("JSON format invalid. Expected object, got " + arr[i].Type);
                        }
                        return ret;
                    }
                    else throw new Exception("JSON format invalid. Expected array, got " + token.Type);
                },
                (val) => {
                    var type = val.GetType();
                    var generics = type.GetGenericArguments();
                    if (generics.Length < 2)
                        throw new Exception("Unsupported dictionary given. Missing generics.");
                    var keyConverter = BaseConverter.GetConverter(generics[0]);
                    var valueConverter = BaseConverter.GetConverter(generics[1]);

                    var dict = (IDictionary)val;
                    var ret = new JArray();
                    foreach (var key in dict.Keys)
                    {
                        var obj = new JObject();
                        obj["key"] = keyConverter._ToJSON(key);
                        obj["value"] = valueConverter._ToJSON(dict[key]);
                        ret.Add(obj);
                    }
                    return ret;
                }));
            BaseConverter.AddType<IList>(new DelegateConverter<IList>(
                (reader, type) => {
                    if (type.IsArray)
                    {
                        var elementType = type.GetElementType();
                        var valueConverter = BaseConverter.GetConverter(elementType);

                        reader.ReadValueSafe(out int count);
                        var ret = Array.CreateInstance(elementType, count);
                        for (var i = 0; i < count; i++)
                        {
                            object value = valueConverter._Read(reader, elementType);
                            ret.SetValue(value, i);
                        }
                        return ret;
                    }
                    else
                    {
                        var generics = type.GetGenericArguments();
                        if (generics.Length < 1)
                            throw new Exception("Unsupported list given. Missing generics.");
                        var valueConverter = BaseConverter.GetConverter(generics[0]);

                        var ret = (IList)Activator.CreateInstance(type);
                        reader.ReadValueSafe(out int count);
                        for (var i = 0; i < count; i++)
                        {
                            object value = valueConverter._Read(reader, generics[0]);
                            ret.Add(value);
                        }
                        return ret;
                    }
                },
                (writer, val) => {
                    var type = val.GetType();
                    Type elementType = null;
                    if (type.IsArray)
                        elementType = type.GetElementType();
                    else
                    {
                        var generics = type.GetGenericArguments();
                        if (generics.Length < 1)
                            throw new Exception("Unsupported list given. Missing generics.");
                        elementType = generics[0];
                    }
                    var valueConverter = BaseConverter.GetConverter(elementType);

                    var list = (IList)val;
                    writer.WriteValueSafe(list.Count);
                    foreach (var value in list)
                    {
                        valueConverter._Write(writer, value);
                    }
                },
                (token, type) => {
                    if (token is JArray arr)
                    {
                        Type elementType = null;
                        if (type.IsArray)
                            elementType = type.GetElementType();
                        else
                        {
                            var generics = type.GetGenericArguments();
                            if (generics.Length < 1)
                                throw new Exception("Unsupported list given. Missing generics.");
                            elementType = generics[0];
                        }
                        var valueConverter = BaseConverter.GetConverter(elementType);

                        if (type.IsArray)
                        {
                            Array ret = Array.CreateInstance(elementType, arr.Count);
                            for (var i = 0; i < arr.Count; i++)
                            {
                                ret.SetValue(valueConverter._FromJSON(arr[i], elementType), i);
                            }
                            return ret;
                        }
                        else
                        {
                            IList ret = null;
                            ret = (IList)Activator.CreateInstance(type);
                            for (var i = 0; i < arr.Count; i++)
                            {
                                ret.Add(valueConverter._FromJSON(arr[i], elementType));
                            }
                            return ret;
                        }
                    }
                    else throw new Exception("JSON format invalid. Expected array, got " + token.Type);
                },
                (val) => {
                    var type = val.GetType();
                    Type elementType = null;
                    if (type.IsArray)
                        elementType = type.GetElementType();
                    else
                    {
                        var generics = type.GetGenericArguments();
                        if (generics.Length < 1)
                            throw new Exception("Unsupported list given. Missing generics.");
                        elementType = generics[0];
                    }
                    var valueConverter = BaseConverter.GetConverter(elementType);

                    var list = (IList)val;
                    var ret = new JArray();
                    foreach (var value in list)
                    {
                        ret.Add(valueConverter._ToJSON(value));
                    }
                    return ret;
                }));
            BaseConverter.AddType<Configuration>(new DelegateConverter<Configuration>(
                (reader, type) => {
                    var instance = (Configuration)Activator.CreateInstance(type);
                    var fields = Configuration.GetFields(instance);
                    foreach (var field in fields)
                    {
                        if (field.Field.Converter != null)
                            field.Value = field.Field.Converter._Read(reader, field.Field.Field.FieldType);
                    }

                    return instance;
                },
                (writer, val) => {
                    var fields = Configuration.GetFields(val);
                    foreach (var field in fields)
                    {
                        if (field.Field.Converter != null)
                            field.Field.Converter._Write(writer, field.Value);
                    }
                },
                (token, type) => {
                    if (token is JObject obj)
                    {
                        var instance = (Configuration)Activator.CreateInstance(type);
                        var fields = Configuration.GetFields(instance);
                        foreach (var field in fields)
                        {
                            if (obj.ContainsKey(field.Field.JSONName) && field.Field.Converter != null)
                            {
                                field.Value = field.Field.Converter._FromJSON(obj[field.Field.JSONName], field.Field.Field.FieldType);
                                if (!(field.Value is Configuration))
                                    field.DefaultValue = field.CloneValue();
                            }
                        }
                        instance.LoadedFromJSON();
                        return instance;
                    }
                    else throw new Exception("JSON format invalid. Expected object, got " + token.Type);
                },
                (val) => {
                    var fields = Configuration.GetFields(val);
                    var obj = new JObject();
                    foreach (var field in fields)
                    {
                        obj[field.Field.JSONName] = field.Field.Converter._ToJSON(field.Value);
                    }
                    return obj;
                }));
        }

        public static BaseConverter GetConverter(Type t)
        {
            while (t != null)
            {
                if (Converters.ContainsKey(t))
                    return Converters[t];
                var interfaces = t.GetInterfaces();
                foreach (var i in interfaces)
                    if (Converters.ContainsKey(i))
                        return Converters[i];
                t = t.BaseType;
            }
            return null;
        }

        internal static Dictionary<Type, BaseConverter> Converters = new Dictionary<Type, BaseConverter>();

        public static void AddType<T>(Converter<T> converter)
        {
            Converters.Add(typeof(T), converter);
        }

        internal abstract void _Write(FastBufferWriter writer, object val);
        internal abstract object _Read(FastBufferReader reader, Type t);
        internal abstract JToken _ToJSON(object obj);
        internal abstract object _FromJSON(JToken token, Type t);
    }

    public abstract class Converter<T> : BaseConverter
    {
        public abstract T Read(FastBufferReader reader);
        public abstract void Write(FastBufferWriter writer, T val);
        public abstract JToken ToJSON(T obj);
        public abstract T FromJSON(JToken token);

        internal override object _Read(FastBufferReader reader, Type t)
        {
            return Read(reader);
        }

        internal override void _Write(FastBufferWriter writer, object val)
        {
            Write(writer, (T)val);
        }

        internal override JToken _ToJSON(object obj)
        {
            return ToJSON((T)obj);
        }

        internal override object _FromJSON(JToken token, Type t)
        {
            return FromJSON(token);
        }
    }

    public class DelegateConverter<T> : Converter<T>
    {
        public delegate T FromJSONDelegate(JToken token, Type t);
        public delegate JToken ToJSONDelegate(T obj);
        public delegate T ReadDelegate(FastBufferReader reader, Type t);
        public delegate void WriteDelegate(FastBufferWriter writer, T val);
        internal FromJSONDelegate OnFromJSON;
        internal ToJSONDelegate OnToJSON;
        internal ReadDelegate OnRead;
        internal WriteDelegate OnWrite;
        public DelegateConverter(ReadDelegate read, WriteDelegate write, FromJSONDelegate fromJSON, ToJSONDelegate toJSON)
        {
            OnRead = read;
            OnWrite = write;
            OnFromJSON = fromJSON;
            OnToJSON = toJSON;
        }

        internal override object _FromJSON(JToken token, Type t)
        {
            return OnFromJSON(token, t);
        }

        internal override object _Read(FastBufferReader reader, Type t)
        {
            return OnRead(reader, t);
        }

        public override JToken ToJSON(T obj)
        {
            return OnToJSON(obj);
        }

        public override void Write(FastBufferWriter writer, T val)
        {
            OnWrite(writer, val);
        }

        public override T Read(FastBufferReader reader)
        {
            throw new NotImplementedException();
        }

        public override T FromJSON(JToken token)
        {
            throw new NotImplementedException();
        }
    }
}
