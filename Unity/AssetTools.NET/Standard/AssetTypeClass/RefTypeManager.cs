﻿using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class RefTypeManager
    {
        private Dictionary<AssetTypeReference, AssetTypeTemplateField> typeTreeLookup;
        private Dictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateLookup;
        private IMonoBehaviourTemplateGenerator monoTemplateGenerator;
        private UnityVersion unityVersion;
        private bool isSharedMonoLookup;

        public RefTypeManager()
        {
            typeTreeLookup = new Dictionary<AssetTypeReference, AssetTypeTemplateField>();
        }

        /// <summary>
        /// Clear the ref type lookup dictionaries.
        /// </summary>
        public void Clear()
        {
            typeTreeLookup.Clear();
            if (!isSharedMonoLookup)
            {
                monoTemplateLookup.Clear();
            }
        }

        /// <summary>
        /// Load the lookup from the type tree ref types of a serialized file.
        /// </summary>
        /// <param name="metadata">The metadata to load from.</param>
        public void FromTypeTree(AssetsFileMetadata metadata)
        {
            if (!metadata.TypeTreeEnabled || metadata.RefTypes == null)
            {
                return;
            }

            foreach (TypeTreeType type in metadata.RefTypes)
            {
                if (!type.IsRefType)
                    continue;

                AssetTypeTemplateField templateField = new AssetTypeTemplateField();
                templateField.FromTypeTree(type);
                // if ref type has fields with [SerializeReference] it can contain its own registry,
                // but it shouldn't be there, as the registry is only available at the root type
                if (templateField.Children.Count > 0 && templateField.Children[templateField.Children.Count - 1].ValueType == AssetValueType.ManagedReferencesRegistry)
                {
                    templateField.Children.RemoveAt(templateField.Children.Count - 1);
                }

                typeTreeLookup[type.TypeReference] = templateField;
            }
        }

        /// <summary>
        /// Initialize a lookup for MonoBehaviours.
        /// </summary>
        /// <param name="metadata">The metadata to load from.</param>
        /// <param name="monoTemplateGenerator">The mono template generator to use.</param>
        /// <param name="monoTemplateFieldCache">The cache to use.</param>
        public void WithMonoTemplateGenerator(AssetsFileMetadata metadata, IMonoBehaviourTemplateGenerator monoTemplateGenerator, Dictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateFieldCache = null)
        {
            this.monoTemplateGenerator = monoTemplateGenerator;
            unityVersion = new UnityVersion(metadata.UnityVersion);
            monoTemplateLookup = monoTemplateFieldCache ?? new Dictionary<AssetTypeReference, AssetTypeTemplateField>();
            isSharedMonoLookup = monoTemplateLookup != null;
        }

        /// <summary>
        /// Gets the template field from a reference.
        /// </summary>
        /// <param name="type">The type reference to use.</param>
        /// <returns>A template field for this reference.</returns>
        public AssetTypeTemplateField GetTemplateField(AssetTypeReference type)
        {
            if (type == null || (string.IsNullOrEmpty(type.ClassName) && string.IsNullOrEmpty(type.Namespace) && string.IsNullOrEmpty(type.AsmName)) || type.Equals(AssetTypeReference.TERMINUS))
            {
                return null;
            }

            if (typeTreeLookup.TryGetValue(type, out AssetTypeTemplateField templateField))
            {
                return templateField;
            }

            if (monoTemplateGenerator != null)
            {
                if (monoTemplateLookup.TryGetValue(type, out templateField))
                {
                    return templateField;
                }

                templateField = new AssetTypeTemplateField
                {
                    Name = "Base",
                    Type = type.ClassName,
                    ValueType = AssetValueType.None,
                    IsArray = false,
                    IsAligned = false,
                    HasValue = false,
                    Children = new List<AssetTypeTemplateField>(0)
                };
                templateField = monoTemplateGenerator.GetTemplateField(templateField, type.AsmName, type.Namespace, type.ClassName, unityVersion);
                if (templateField != null)
                {
                    monoTemplateLookup[type] = templateField;
                    return templateField;
                }
            }

            return null;
        }
    }
}
