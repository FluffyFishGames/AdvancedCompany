using UnityEngine;

namespace AdvancedCompany
{
    [LoadAssets]
    public class PlayerRocketBoots : MonoBehaviour
    {
        public GameObject LeftRocket;
        public GameObject RightRocket;
        private ParticleSystem LeftParticles;
        private ParticleSystem RightParticles;
        private bool Initialized = false;
        private static GameObject LeftRocketBootPrefab;
        private static GameObject RightRocketBootPrefab;

        public static void LoadAssets(AssetBundle assets)
        {
            LeftRocketBootPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Objects/RocketBootsLeft.prefab");
            RightRocketBootPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Objects/RocketBootsRight.prefab");
        }

        public void Awake()
        {
            var spine = transform.Find("ScavengerModel").Find("metarig").Find("spine");
            var leftHeel = spine.Find("thigh.L").Find("shin.L").Find("foot.L");
            var rightHeel = spine.Find("thigh.R").Find("shin.R").Find("foot.R");

            if (LeftRocket == null)
            {
                LeftRocket = GameObject.Instantiate(LeftRocketBootPrefab, leftHeel);
                LeftRocket.transform.localPosition = new Vector3(-0.0786f, -0.0302f, 0.0056f);
                LeftRocket.transform.localEulerAngles = new Vector3(-54.256f, -170.885f, 169.978f);
                LeftRocket.transform.localScale = new Vector3(0.5319018f, 0.5319018f, 0.5319018f);
                LeftParticles = LeftRocket.transform.Find("Particles").GetComponent<ParticleSystem>();
            }
            if (RightRocket == null)
            { 
                RightRocket = GameObject.Instantiate(RightRocketBootPrefab, rightHeel);
                RightRocket.transform.localPosition = new Vector3(0.0833f, -0.0401f, 0.0086f);
                RightRocket.transform.localEulerAngles = new Vector3(-124.165f, 0.07899f, -0.55297f);
                RightRocket.transform.localScale = new Vector3(0.5319018f, 0.5319018f, 0.5319018f);
                RightParticles = RightRocket.transform.Find("Particles").GetComponent<ParticleSystem>();
            }
        }

        public void PlayParticles()
        {
            LeftParticles.Play();
            RightParticles.Play();
        }
    }
}
