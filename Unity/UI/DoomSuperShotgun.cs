using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Unity.UI
{
    public class DoomSuperShotgun : MonoBehaviour
    {
        public AudioSource Audio;

        public void Shoot()
        {
            Audio.Play();
        }
    }
}
