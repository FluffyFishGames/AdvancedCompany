using AdvancedCompany.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace AdvancedCompany.Objects
{
    public class LightShoeRGB : MonoBehaviour
    {
        private Light[] Lights;
        private float[] Hues;
        private float Phase;
        internal AudioSource Audio;
        internal Material Material;
        private ParticleSystem ParticleSystem;
        private bool CurseIsLifted = false;

        void Awake()
        {
            Phase = UnityEngine.Random.value;
            Audio = GetComponentInChildren<AudioSource>();
            ParticleSystem = GetComponentInChildren<ParticleSystem>();
            Lights = GetComponentsInChildren<Light>();
            Hues = new float[Lights.Length];

            for (var i = 0; i < Lights.Length; i++)
            {
                Color.RGBToHSV(Lights[i].color, out var h, out var s, out var v);
                Hues[i] = h;
            }
        }

        public void CurseLifted()
        {
            if (CurseIsLifted)
                return;
            CurseIsLifted = true;
            ParticleSystem.Stop();
            this.StartCoroutine(LiftCurse());
        }

        IEnumerator LiftCurse()
        {
            for (var i = 0; i < Lights.Length; i++)
            {
                Lights[i].enabled = false;
            }
            var startVolume = Audio.volume;
            while (Audio.pitch > 0)
            {
                Audio.pitch -= Time.deltaTime / 3f;
                Audio.volume = Audio.pitch * startVolume;
                yield return new WaitForEndOfFrame();
            }
            GameObject.Destroy(this.gameObject);
        }

        public void Play()
        {
            if (Audio != null)
            {
                if (!ClientConfiguration.Instance.Compability.DisableMusic)
                    Audio.Play();
            }
            if (ParticleSystem != null)
                ParticleSystem.Play();
        }

        public void Stop()
        {
            if (Audio != null)
            {
                if (!ClientConfiguration.Instance.Compability.DisableMusic)
                    Audio.Stop();
            }
            if (ParticleSystem != null)
                ParticleSystem.Stop();
        }

        void Update()
        {
            Phase += Time.deltaTime;
            while (Phase > 1f)
                Phase -= 1f;

            Material.SetTextureOffset("_EmissiveColorMap", new Vector2(0f, Phase));
            for (var i = 0; i < Lights.Length; i++)
                Lights[i].color = Color.HSVToRGB((Hues[i] + Phase) % 1f, 1f, 1f);
        }
    }
}

