using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using SMLHelper.V2.Assets;
using System.IO;
using FMOD;
using ECCLibrary.Internal;
using System.Globalization;

namespace ECCLibrary
{
    public static class ECCHelpers
    {
        public static AssetBundle LoadAssetBundleFromAssetsFolder(Assembly modAssembly, string assetsFileName)
        {
            return AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(modAssembly.Location), "Assets", assetsFileName));
        }
        public static void ApplySNShaders(GameObject prefab, UBERMaterialProperties materialSettings)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            var newShader = Shader.Find("MarmosetUBER");
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    Material material = renderers[i].materials[j];
                    Texture specularTexture = material.GetTexture("_SpecGlossMap");
                    Texture emissionTexture = material.GetTexture("_EmissionMap");
                    material.shader = newShader;

                    material.DisableKeyword("_SPECGLOSSMAP");
                    material.DisableKeyword("_NORMALMAP");
                    if (specularTexture != null)
                    {
                        material.SetTexture("_SpecTex", specularTexture);
                        material.SetFloat("_SpecInt", materialSettings.SpecularInt);
                        material.SetFloat("_Shininess", materialSettings.Shininess);
                        material.EnableKeyword("MARMO_SPECMAP");
                        material.SetColor("_SpecColor", new Color(1f, 1f, 1f, 1f));
                        material.SetFloat("_Fresnel", 0.24f);
                        material.SetVector("_SpecTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }
                    if (material.IsKeywordEnabled("_EMISSION"))
                    {
                        material.EnableKeyword("MARMO_EMISSION");
                        material.SetFloat("_EnableGlow", 1f);
                        material.SetTexture("_Illum", emissionTexture);
                        material.SetFloat("_GlowStrength", materialSettings.EmissionScale);
                        material.SetFloat("_GlowStrengthNight", materialSettings.EmissionScale);
                    }

                    if (material.GetTexture("_BumpMap"))
                    {
                        material.EnableKeyword("MARMO_NORMALMAP");
                    }

                    if(CompareStrings(material.name, "Cutout", ECCStringComparison.Contains))
                    {
                        material.EnableKeyword("MARMO_ALPHA_CLIP");
                    }
                    if (CompareStrings(material.name, "Transparent", ECCStringComparison.Contains))
                    {
                        material.EnableKeyword("_ZWRITE_ON");
                        material.EnableKeyword("WBOIT");
                        material.SetInt("_ZWrite", 0);
                        material.SetInt("_Cutoff", 0);
                        material.SetFloat("_SrcBlend", 1f);
                        material.SetFloat("_DstBlend", 1f);
                        material.SetFloat("_SrcBlend2", 0f);
                        material.SetFloat("_DstBlend2", 10f);
                        material.SetFloat("_AddSrcBlend", 1f);
                        material.SetFloat("_AddDstBlend", 1f);
                        material.SetFloat("_AddSrcBlend2", 0f);
                        material.SetFloat("_AddDstBlend2", 10f);
                        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack | MaterialGlobalIlluminationFlags.RealtimeEmissive;
                        material.renderQueue = 3101;
                        material.enableInstancing = true;
                    }
                }
            }
        }
        internal static SwimBehaviour EssentialComponentSystem_Swimming(GameObject prefab, float turnSpeed, Rigidbody rb)
        {
            Locomotion locomotion = prefab.AddComponent<Locomotion>();
            locomotion.useRigidbody = rb;
            SplineFollowing splineFollow = prefab.AddComponent<SplineFollowing>();
            splineFollow.respectLOD = false;
            splineFollow.locomotion = locomotion;
            SwimBehaviour swim = prefab.AddComponent<SwimBehaviour>();
            swim.splineFollowing = splineFollow;
            swim.turnSpeed = turnSpeed;
            return swim;
        }
        internal static BehaviourLOD EssentialComponent_BehaviourLOD(GameObject prefab, float near, float medium, float far)
        {
            BehaviourLOD bLod = prefab.AddComponent<BehaviourLOD>();
            bLod.veryCloseThreshold = near;
            bLod.closeThreshold = medium;
            bLod.farThreshold = far;
            return bLod;
        }
        public static void MakeAcidImmune(TechType techType)
        {
            List<TechType> acidImmuneList = new List<TechType>(DamageSystem.acidImmune);
            acidImmuneList.Add(techType);
            DamageSystem.acidImmune = acidImmuneList.ToArray();
        }
        public static AnimationCurve Curve_Trail()
        {
            return new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.25f), new Keyframe(1f, 0.75f) });
        }
        public static AnimationCurve Curve_Flat(float value = 1f)
        {
            return new AnimationCurve(new Keyframe[] { new Keyframe(0f, value), new Keyframe(1f, value) });
        }

        public static LiveMixinData CreateNewLiveMixinData()
        {
            return ScriptableObject.CreateInstance<LiveMixinData>();
        }

        public static void SetPrivateField<T>(Type type, T instance, string name, object value)
        {
            var prop = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            prop.SetValue(instance, value);
        }
        public static OutputT GetPrivateField<OutputT>(Type type, object target, string name)
        {
            var prop = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return (OutputT)prop.GetValue(target);
        }
        public static OutputT GetPrivateStaticField<OutputT>(Type type, string name)
        {
            var prop = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
            return (OutputT)prop.GetValue(null);
        }
        public static void MakeObjectScannerRoomScannable(GameObject gameObject, bool updatePositionPeriodically)
        {
            ResourceTracker resourceTracker = gameObject.AddComponent<ResourceTracker>();
            resourceTracker.prefabIdentifier = gameObject.GetComponent<PrefabIdentifier>();
            resourceTracker.rb = gameObject.GetComponent<Rigidbody>();
            if (updatePositionPeriodically == true)
            {
                gameObject.AddComponent<ResourceTrackerUpdater>();
            }
        }
        /// <summary>
        /// Set the BehaviourType of a TechType. Used for certain creature interactions.
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="behaviourType"></param>
        public static void PatchBehaviorType(TechType techType, BehaviourType behaviourType)
        {
            GetPrivateStaticField<Dictionary<TechType, BehaviourType>>(typeof(BehaviourData), "behaviourTypeList").Add(techType, behaviourType);
        }
        /// <summary>
        /// Set the EquipmentType of an item.
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="equipmentType"></param>
        public static void PatchEquipmentType(TechType techType, EquipmentType equipmentType)
        {
            GetPrivateStaticField<Dictionary<TechType, EquipmentType>>(typeof(CraftData), "equipmentTypes").Add(techType, equipmentType);
        }
        /// <summary>
        /// Patch the inventory sounds of a TechType.
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="soundType"></param>
        public static void PatchItemSounds(TechType techType, ItemSoundsType soundType)
        {
            string pickupSound = GetPickupSoundEvent(soundType);
            string dropSound = GetDropSoundEvent(soundType);
            string eatSound = GetEatSoundEvent(soundType);
            GetPrivateStaticField<Dictionary<TechType, string>>(typeof(CraftData), "pickupSoundList").Add(techType, pickupSound);
            GetPrivateStaticField<Dictionary<TechType, string>>(typeof(CraftData), "dropSoundList").Add(techType, dropSound);
            GetPrivateStaticField<Dictionary<TechType, string>>(typeof(CraftData), "useEatSound").Add(techType, eatSound);
        }
        private static string GetPickupSoundEvent(ItemSoundsType soundType)
        {
            switch (soundType)
            {
                default:
                    return CraftData.defaultPickupSound;
                case ItemSoundsType.AirBladder:
                    return "event:/tools/airbladder/airbladder_pickup";
                case ItemSoundsType.Light:
                    return "event:/tools/lights/pick_up";
                case ItemSoundsType.Egg:
                    return "event:/loot/pickup_egg";
                case ItemSoundsType.Fins:
                    return "event:/loot/pickup_fins";
                case ItemSoundsType.Floater:
                    return "event:/loot/floater/floater_pickup";
                case ItemSoundsType.Suit:
                    return "event:/loot/pickup_suit";
                case ItemSoundsType.Tank:
                    return "event:/loot/pickup_tank";
                case ItemSoundsType.Organic:
                    return "event:/loot/pickup_organic";
                case ItemSoundsType.Fish:
                    return "event:/loot/pickup_fish";
        }
        }
        private static string GetDropSoundEvent(ItemSoundsType soundType)
        {
            switch (soundType)
            {
                default:
                    return CraftData.defaultDropSound;
                case ItemSoundsType.Floater:
                    return "event:/loot/floater/floater_place";
            }
        }
        private static string GetEatSoundEvent(ItemSoundsType soundType)
        {
            switch (soundType)
            {
                default:
                    return CraftData.defaultEatSound;
                case ItemSoundsType.Water:
                    return "event:/player/drink";
                case ItemSoundsType.FirstAidKit:
                    return "event:/player/use_first_aid";
                case ItemSoundsType.StillSuitWater:
                    return "event:/player/drink_stillsuit";
            }
        }

        /// <summary>
        /// Returns the master volume for ECC (ranges from 0-1)
        /// </summary>
        /// <returns></returns>
        public static float GetECCVolume()
        {
            return ECCPatch.config.VolumeNew / 100f;
        }
        public static bool CompareStrings(string original, string compareTo, ECCStringComparison comparisonMode)
        {
            switch (comparisonMode)
            {
                default:
                    return original == compareTo;
                case ECCStringComparison.Equals:
                    return original.ToLower() == compareTo.ToLower();
                case ECCStringComparison.EqualsCaseSensitive:
                    return original == compareTo;
                case ECCStringComparison.StartsWith:
                    return original.ToLower().StartsWith(compareTo.ToLower());
                case ECCStringComparison.StartsWithCaseSensitive:
                    return original.StartsWith(compareTo);
                case ECCStringComparison.Contains:
                    return original.ToLower().Contains(compareTo.ToLower());
                case ECCStringComparison.ContainsCaseSensitive:
                    return original.Contains(compareTo);
            }
        }
    }
    public static class GameObjectExtensions
    {
        public static GameObject SearchChild(this GameObject gameObject, string byName, ECCStringComparison stringComparison = ECCStringComparison.Equals)
        {
            GameObject obj = SearchChildRecursive(gameObject, byName, stringComparison);
            if(obj == null)
            {
                ECCLog.AddMessage("No child found in hierarchy by name {0}.", byName);
            }
            return obj;
        }

        static GameObject SearchChildRecursive(GameObject gameObject, string byName, ECCStringComparison stringComparison)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (ECCHelpers.CompareStrings(child.gameObject.name, byName, stringComparison))
                {
                    return child.gameObject;
                }
                GameObject recursive = SearchChildRecursive(child.gameObject, byName, stringComparison);
                if (recursive)
                {
                    return recursive;
                }
            }
            return null;
        }
    }
    public enum ECCStringComparison
    {
        Equals,
        EqualsCaseSensitive,
        StartsWith,
        StartsWithCaseSensitive,
        Contains,
        ContainsCaseSensitive
    }
    public enum ItemSoundsType
    {
        Default,
        Organic,
        Egg,
        Fins,
        Suit,
        Tank,
        Floater,
        Light,
        AirBladder,
        FirstAidKit,
        Water,
        StillSuitWater,
        Fish
    }
}
