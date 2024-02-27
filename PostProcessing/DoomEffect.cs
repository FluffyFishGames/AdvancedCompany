using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using HarmonyLib;

namespace AdvancedCompany.PostProcessing
{
    [LoadAssets]
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Doom")]
    public sealed class DoomEffect : CustomPostProcessVolumeComponent, IPostProcessComponent
    {

        [Tooltip("Controls the intensity of the effect.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        static Material Material;
        private Material m_Material;
        public bool IsActive() => m_Material != null && intensity.value > 0f;

        // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public static void LoadAssets(AssetBundle assets)
        {
            Material = assets.LoadAsset<Material>("Assets/Shaders/Doom.mat");
        }

        public override void Setup()
        {
            m_Material = new Material(Material);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null)
                return;

            m_Material.SetFloat("_Intensity", intensity.value);
            if (intensity.value <= 0f)
                return;
            float pixelWidth = 640f;
            m_Material.SetFloat("_ScreenWidth", pixelWidth);
            m_Material.SetFloat("_ScreenHeight", pixelWidth * 3f / 4f);
            cmd.Blit(source, destination, m_Material, 0);
            //HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_Material);
        }
    }
}