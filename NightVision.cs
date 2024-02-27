/*using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace LethalCompanyPlus.PostProcessing
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/NightVision")]
    public sealed class NightVision : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        [Tooltip("Controls the intensity of the effect.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(1f, 0f, 1f);

        public static Material Material;
        private Material m_Material;
        public bool IsActive() => m_Material != null && intensity.value > 0f;

        // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public static void LoadAssets(AssetBundle assets)
        {
            Material = assets.LoadAsset<Material>("Assets/Shaders/NightVision.mat");
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
            m_Material.SetTexture("_MainTex", source);
            HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_Material);
        }
    }
}*/