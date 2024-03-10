using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany
{
    internal static partial class Extensions
    {
        public static void HideRenderers(this GameObject[] gameObjects, bool includeSkinned = true)
        {
            for (var i = 0; i < gameObjects.Length; i++)
                gameObjects[i].HideRenderers(includeSkinned);
        }

        public static void HideRenderers(this GameObject gameObject, bool includeSkinned = true)
        {
            gameObject.GetComponentsInChildren<MeshRenderer>().Hide();
            if (includeSkinned)
                gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Hide();
        }

        public static void ShowRenderers(this GameObject[] gameObjects, bool includeSkinned = true)
        {
            for (var i = 0; i < gameObjects.Length; i++)
                gameObjects[i].ShowRenderers(includeSkinned);
        }

        public static void ShowRenderers(this GameObject gameObject, bool includeSkinned = true)
        {
            gameObject.GetComponentsInChildren<MeshRenderer>().Show();
            if (includeSkinned)
                gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Show();
        }

        public static void Hide(this MeshRenderer[] renderers)
        {
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].enabled = false;
        }
        public static void Show(this MeshRenderer[] renderers)
        {
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].enabled = true;
        }
        public static void Hide(this SkinnedMeshRenderer[] renderers)
        {
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].enabled = false;
        }
        public static void Show(this SkinnedMeshRenderer[] renderers)
        {
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].enabled = true;
        }
    }
}
