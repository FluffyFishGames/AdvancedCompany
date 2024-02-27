using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

namespace AdvancedCompany.Lib
{
    internal class HDRP
    {
        internal static Dictionary<string, Dictionary<PostProcessingFlags, VolumeProfile>> Profiles = new Dictionary<string, Dictionary<PostProcessingFlags, VolumeProfile>>();
        internal static Dictionary<string, Dictionary<PostProcessingFlags, List<(Type, PostProcessInstance)>>> Components = new Dictionary<string, Dictionary<PostProcessingFlags, List<(Type, PostProcessInstance)>>>();
        internal static List<string> AfterPostProcessList;
        internal static List<string> AfterPostProcessBlursList;
        internal static List<string> BeforePostProcessList;
        internal static List<string> BeforeTAAList;
        internal static List<string> BeforeTransparentList;
        static HDRP()
        {
            var pipelineAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
            var globalSettings = typeof(HDRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings");
            var instanceProperty = globalSettings.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var instance = instanceProperty.GetValue(null);
            var afterPostProcess = globalSettings.GetField("afterPostProcessCustomPostProcesses", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            AfterPostProcessList = (List<string>)afterPostProcess.GetValue(instance);
            var afterPostProcessBlurs = globalSettings.GetField("afterPostProcessBlursCustomPostProcesses", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            AfterPostProcessBlursList = (List<string>)afterPostProcessBlurs.GetValue(instance);
            var beforePostProcess = globalSettings.GetField("beforePostProcessCustomPostProcesses", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            BeforePostProcessList = (List<string>)beforePostProcess.GetValue(instance);
            var beforeTAA = globalSettings.GetField("beforeTAACustomPostProcesses", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            BeforeTAAList = (List<string>)beforeTAA.GetValue(instance);
            var beforeTransparent = globalSettings.GetField("beforeTransparentCustomPostProcesses", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            BeforeTransparentList = (List<string>)beforeTransparent.GetValue(instance);
        }

        public enum InjectionPoint
        {
            AFTER_POSTPROCESS,
            AFTER_POSTPROCESS_BLURS,
            BEFORE_POSTPROCESS,
            BEFORE_TAA,
            BEFORE_TRANSPARENT
        };

        public static void AddCustomPostProcessing(InjectionPoint point, object obj)
        {
            if (obj == null) return;
            AddCustomPostProcessing(point, obj.GetType().AssemblyQualifiedName);
        }

        public static void AddCustomPostProcessing(InjectionPoint point, Type t)
        {
            if (t == null) return;
            AddCustomPostProcessing(point, t.AssemblyQualifiedName);
        }

        public static void AddCustomPostProcessing(InjectionPoint point, string className)
        {
            switch (point)
            {
                case InjectionPoint.AFTER_POSTPROCESS:
                    if (!AfterPostProcessList.Contains(className))
                        AfterPostProcessList.Add(className);
                    break;
                case InjectionPoint.AFTER_POSTPROCESS_BLURS:
                    if (!AfterPostProcessBlursList.Contains(className))
                        AfterPostProcessBlursList.Add(className);
                    break;
                case InjectionPoint.BEFORE_POSTPROCESS:
                    if (!BeforePostProcessList.Contains(className))
                        BeforePostProcessList.Add(className);
                    break;
                case InjectionPoint.BEFORE_TAA:
                    if (!BeforeTAAList.Contains(className))
                        BeforeTAAList.Add(className);
                    break;
                case InjectionPoint.BEFORE_TRANSPARENT:
                    if (!BeforeTransparentList.Contains(className))
                        BeforeTransparentList.Add(className);
                    break;
            }
        }

        [Flags]
        public enum PostProcessingFlags
        {
            OUTSIDE = 1,
            INSIDE = 2,
            ORBIT = 4,
            UNDERWATER = 8,
            ALL = OUTSIDE | INSIDE | ORBIT | UNDERWATER
        }

        public class PostProcessInstance
        {
            public VolumeComponent CurrentInstance { get; internal set; }
            public delegate void InstanceChanged(VolumeComponent old, VolumeComponent component);
            public InstanceChanged OnInstanceChanged;
        }

        public static PostProcessInstance AddPostProcessing<T>(InjectionPoint point, PostProcessingFlags flags = PostProcessingFlags.ALL, string moonID = null) where T : IPostProcessComponent
        {
            AddCustomPostProcessing(point, typeof(T).AssemblyQualifiedName);

            if (moonID == null)
                moonID = "_ALL";
            if (!Components.ContainsKey(moonID))
                Components.Add(moonID, new Dictionary<PostProcessingFlags, List<(Type, PostProcessInstance)>>() {
                    { PostProcessingFlags.OUTSIDE, new() },
                    { PostProcessingFlags.INSIDE, new() },
                    { PostProcessingFlags.ORBIT, new() },
                    { PostProcessingFlags.UNDERWATER, new() },
                });

            var instance = new PostProcessInstance();
            if ((flags & PostProcessingFlags.OUTSIDE) == PostProcessingFlags.OUTSIDE)
                Components[moonID][PostProcessingFlags.OUTSIDE].Add((typeof(T), instance));
            if ((flags & PostProcessingFlags.INSIDE) == PostProcessingFlags.INSIDE)
                Components[moonID][PostProcessingFlags.INSIDE].Add((typeof(T), instance));
            if ((flags & PostProcessingFlags.ORBIT) == PostProcessingFlags.ORBIT)
                Components[moonID][PostProcessingFlags.ORBIT].Add((typeof(T), instance));
            if ((flags & PostProcessingFlags.UNDERWATER) == PostProcessingFlags.UNDERWATER)
                Components[moonID][PostProcessingFlags.UNDERWATER].Add((typeof(T), instance));
            return instance;
        }

        internal static VolumeProfile GetProfile(string moonID, PostProcessingFlags type)
        {
            if (moonID == null)
                moonID = "_ALL";
            if (!Profiles.ContainsKey(moonID))
                Profiles.Add(moonID, new Dictionary<PostProcessingFlags, VolumeProfile>());
            VolumeProfile profile = null;
            if (!Profiles[moonID].ContainsKey(type))
            {
                profile = VolumeProfile.CreateInstance<VolumeProfile>();
                profile.hideFlags = UnityEngine.HideFlags.DontUnloadUnusedAsset;
                Profiles[moonID].Add(type, profile);

                if (moonID != "_ALL" && Components.ContainsKey("_ALL") && Components["_ALL"].ContainsKey(type))
                    foreach (var t in Components["_ALL"][type])
                        profile.Add(t.Item1, true);
                if (Components.ContainsKey(moonID) && Components[moonID].ContainsKey(type))
                {
                    foreach (var t in Components[moonID][type])
                        profile.Add(t.Item1, true);
                }
            }
            else profile = Profiles[moonID][type];

            if (moonID != "_ALL" && Components.ContainsKey("_ALL") && Components["_ALL"].ContainsKey(type))
            {
                foreach (var t in Components["_ALL"][type])
                {
                    if (profile.TryGet<VolumeComponent>(t.Item1, out var n))
                    {
                        var old = t.Item2.CurrentInstance;
                        t.Item2.CurrentInstance = n;
                        if (t.Item2.OnInstanceChanged != null)
                            t.Item2.OnInstanceChanged(old, n);
                    }
                }
            }
            if (Components.ContainsKey(moonID) && Components[moonID].ContainsKey(type))
            {
                foreach (var t in Components[moonID][type])
                {
                    if (profile.TryGet<VolumeComponent>(t.Item1, out var n))
                    {
                        var old = t.Item2.CurrentInstance;
                        t.Item2.CurrentInstance = n;
                        if (t.Item2.OnInstanceChanged != null)
                            t.Item2.OnInstanceChanged(old, n);
                    }
                }
            }
            return Profiles[moonID][type];
        }
    }
}
