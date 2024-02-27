using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using HarmonyLib;
using AdvancedCompany.Config;

namespace AdvancedCompany.PostProcessing
{
    [Boot.Bootable]
    public sealed class VisionEnhancerEffect
    {
        private static GameObject LightAndFog;
        private static bool Active = false;
        private static float Phase = 0f;
        private static VisionEnhancer Instance;
        private static AdvancedCompany.Lib.HDRP.PostProcessInstance PPInstance;

        public static void Boot()
        {
            PPInstance = AdvancedCompany.Lib.HDRP.AddPostProcessing<VisionEnhancer>(Lib.HDRP.InjectionPoint.AFTER_POSTPROCESS);
            Instance = (VisionEnhancer)PPInstance.CurrentInstance;
            PPInstance.OnInstanceChanged += (old, instance) => { Instance = (VisionEnhancer)instance; };
        }

        public static void Initialize()
        {
            Active = false;
            Phase = 0f;

            var player = Game.Player.GetPlayer(GameNetworkManager.Instance.localPlayerController);

            LightAndFog = new GameObject("VisionEnhancerLight");
            LightAndFog.transform.parent = player.GetBone(Game.Player.Bone.METARIG);
            LightAndFog.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            LightAndFog.transform.localRotation = Quaternion.identity;

            var fog = LightAndFog.AddComponent<LocalVolumetricFog>();
            fog.parameters.size = new Vector3(50f, 50f, 50f);
            fog.parameters.falloffMode = LocalVolumetricFogFalloffMode.Exponential;
            fog.parameters.blendingMode = LocalVolumetricFogBlendingMode.Overwrite;
            fog.parameters.anisotropy = 25f;
            fog.parameters.meanFreePath = 25f;

            var light1 = new GameObject("Light1");
            light1.transform.parent = LightAndFog.transform;
            light1.transform.localPosition = new Vector3(8f, 9.1f, -1.5f);
            light1.transform.localRotation = Quaternion.Euler(50f, -61f, -68f);

            var light1Comp = light1.AddComponent<Light>();
            light1Comp.color = new Color(0f, 1f, 0f);
            light1Comp.type = LightType.Spot;
            light1Comp.shape = LightShape.Box;
            light1Comp.cullingMask = int.MaxValue - 128;

            var light1Data = light1.AddComponent<HDAdditionalLightData>();
            light1Data.SetBoxSpotSize(new Vector2(500f, 500f));
            light1Data.SetIntensity(100f, LightUnit.Lux);
            light1Data.spotLightShape = SpotLightShape.Box;
            //light1Data.affectSpecular = false;
            light1Data.affectsVolumetric = true;
            light1Data.volumetricDimmer = 0.1f;
            light1Data.range = 1000f;
            light1Data.lightDimmer = 1f;
            light1Data.SetCullingMask(int.MaxValue - 128);

            var light2 = new GameObject("Light2");
            light2.transform.parent = LightAndFog.transform;
            light2.transform.localPosition = new Vector3(8f, 6.5f, 8f);
            light2.transform.localRotation = Quaternion.Euler(56f, 114f, 110f);


            var light2Comp = light2.AddComponent<Light>();
            light2Comp.color = new Color(0f, 1f, 0f);
            light2Comp.type = LightType.Spot;
            light2Comp.shape = LightShape.Box;
            light2Comp.cullingMask = int.MaxValue - 128;

            var light2Data = light2.AddComponent<HDAdditionalLightData>();
            light2Data.SetBoxSpotSize(new Vector2(500f, 500f));
            light2Data.SetIntensity(100f, LightUnit.Lux);
            light2Data.spotLightShape = SpotLightShape.Box;
            light2Data.SetCullingMask(int.MaxValue - 128);
            //light2Data.affectSpecular = false;
            light2Data.affectsVolumetric = true;
            light2Data.volumetricDimmer = 0.1f;
            light2Data.range = 1000f;
            light2Data.lightDimmer = 1f;

            LightAndFog.SetActive(false);
        }

        public static void Activate()
        {
            Active = true;
        }

        public static void Deactivate()
        {
            Active = false;
        }

        public static void Update()
        {
            if (Active)
                Phase = Mathf.Clamp01(Phase + Time.deltaTime * 2f);
            else
                Phase = Mathf.Clamp01(Phase - Time.deltaTime * 5f);

            if (Instance != null)
            {
                if (Instance.intensity.value != Phase)
                    Instance.intensity.value = Phase;
                var brightVal = 0.3f + Mathf.Clamp01(ClientConfiguration.Instance.Graphics.VisionEnhancerBrightness) * 0.4f;
                if (Instance.brightness.value != brightVal)
                    Instance.brightness.value = brightVal;
                if (Phase > 0f && !LightAndFog.activeSelf)
                {
                    var renderer = GameNetworkManager.Instance.localPlayerController.localVisor.GetChild(0).GetComponent<MeshRenderer>();
                    renderer.materials[2].SetFloat("_Metallic", 1f);
                    renderer.materials[2].SetFloat("_Smoothness", 0f);
                    LightAndFog.SetActive(true);
                }
                if (Phase <= 0f && LightAndFog.activeSelf)
                {
                    var renderer = GameNetworkManager.Instance.localPlayerController.localVisor.GetChild(0).GetComponent<MeshRenderer>();
                    renderer.materials[2].SetFloat("_Metallic", 0.69f);
                    renderer.materials[2].SetFloat("_Smoothness", 0.5f);
                    LightAndFog.SetActive(false);
                }
            }
        }
    }
}