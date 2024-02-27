using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

namespace AdvancedCompany
{
    [Boot.Bootable]
    public class HDRPChanges
    {
        public static void Boot()
        {
            Plugin.Log.LogDebug("Patching HDRP...");
            try
            {
                var pipelineAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
                var settings = pipelineAsset.currentPlatformRenderPipelineSettings;
                var globalSettings = typeof(HDRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings");
                var instanceProperty = globalSettings.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var instance = instanceProperty.GetValue(null);
                var afterPostProcess = globalSettings.GetField("afterPostProcessCustomPostProcesses", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                var list = (List<string>)afterPostProcess.GetValue(instance);
                list.Add(typeof(PostProcessing.VisionEnhancer).AssemblyQualifiedName);

                settings.lightLoopSettings.cookieAtlasSize = CookieAtlasResolution.CookieResolution2048;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while patching HDRP!");
                Plugin.Log.LogError(e);
            }
        }
    }
}
