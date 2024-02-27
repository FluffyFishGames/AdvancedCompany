using HarmonyLib;
using AdvancedCompany.Boot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace AdvancedCompany
{
    [Bootable]
    public class Assets
    {
        public static AssetBundle AssetBundle;
        public static void Boot()
        {
            string assetsPath = System.IO.Path.Combine(Path.GetDirectoryName(typeof(Assets).Assembly.Location), "advancedcompany/advancedcompanyassets");
            if (!System.IO.File.Exists(assetsPath))
                assetsPath = System.IO.Path.Combine(Path.GetDirectoryName(typeof(Assets).Assembly.Location), "advancedcompanyassets");
            AssetBundle = AssetBundle.LoadFromFile(assetsPath);

            BootManager.AddTypeCallback((type) =>
            {
                if (type.GetCustomAttribute<LoadAssets>() != null)
                {
                    var method = type.GetMethod("LoadAssets", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method != null)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(AssetBundle))
                            method.Invoke(null, new object[] { AssetBundle });
                        else
                            Plugin.Log.LogError("Method " + method.DeclaringType.FullName + "->" + method.Name + " has wrong parameters.");
                    }
                    else
                        Plugin.Log.LogError("Type " + type.FullName + " is missing static LoadAssets method.");
                }
            });
        }
    }
}
