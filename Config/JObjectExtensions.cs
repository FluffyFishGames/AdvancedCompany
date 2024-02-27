using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Config
{
    static class JObjectExtensions
    {
        public static bool TryGetFloat(this JObject obj, string key, out float val)
        {
            if (obj.TryGetValue(key, out var token) && token is JValue value)
            {
                if (value.Type == JTokenType.Float)
                {
                    if (value.Value is float f)
                    {
                        val = f;
                        return true; 
                    }
                    else if (value.Value is double d)
                    {
                        val = (float)d;
                        return true;
                    }
                }
                if (float.TryParse(value.ToString(System.Globalization.CultureInfo.InvariantCulture), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var pf))
                {
                    val = pf;
                    return true;
                }
            }
            val = 0f;
            return false;
        }
        public static bool TryGetInt(this JObject obj, string key, out int val)
        {
            if (obj.TryGetValue(key, out var token) && token is JValue value)
            {
                if (value.Type == JTokenType.Integer && value.Value is int f)
                {
                    val = f;
                    return true;
                }
                else if (int.TryParse(value.ToString(), out int n))
                {
                    val = n;
                    return true;
                }
            }
            val = 0;
            return false;
        }
    }
}
