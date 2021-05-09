﻿using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using UnityEngine;
using ArchitectsLibrary.Handlers;
using System.Collections.Generic;
using UWE;

namespace ArchitectsLibrary.Items
{
    class DrillableEmerald : ReskinItem
    {
        protected override string ReferenceClassId => "4f441e53-7a9a-44dc-83a4-b1791dc88ffd";

        public DrillableEmerald() : base("DrillableEmerald", "Emerald", "Be₃Al₂SiO₆. Beryl variant with applications in advanced alien fabrication.")
        {
        }

        protected override void ApplyChangesToPrefab(GameObject prefab)
        {
            prefab.GetComponentInChildren<Light>().color = Color.green; 
            prefab.EnsureComponent<ResourceTracker>().overrideTechType = AUHandler.EmeraldTechType;
            var drillable = prefab.GetComponent<Drillable>();
            drillable.resources[0] = new Drillable.ResourceType() { chance = 1f, techType = AUHandler.EmeraldTechType };
            drillable.kChanceToSpawnResources = 0.6f;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach(var renderer in renderers)
            {
                //renderer.material.SetTexture("_MainTex", Main.assetBundle.LoadAsset<Texture2D>("Emerald_Diffuse"));
                //renderer.material.SetTexture("_Illum", Main.assetBundle.LoadAsset<Texture2D>("Emerald_Illum"));
                renderer.material.SetColor("_Color", new Color(1f, 1f, 1f, 0.3f));
                renderer.material.SetColor("_SpecColor", new Color(1f, 2f, 1.2f));
                renderer.material.SetFloat("_Fresnel", 0.6f);
                renderer.material.SetFloat("_SpecInt", 20f);
                renderer.material.SetFloat("_GlowStrength", 1f);
                renderer.material.SetFloat("_GlowStrengthNight", 1f);
                ApplyTranslucency(renderer);
            }
        }

        private void ApplyTranslucency(Renderer renderer)
        {
            renderer.material.EnableKeyword("_ZWRITE_ON");
            renderer.material.EnableKeyword("WBOIT");
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.SetInt("_Cutoff", 0);
            renderer.material.SetFloat("_SrcBlend", 1f);
            renderer.material.SetFloat("_DstBlend", 1f);
            renderer.material.SetFloat("_SrcBlend2", 0f);
            renderer.material.SetFloat("_DstBlend2", 10f);
            renderer.material.SetFloat("_AddSrcBlend", 1f);
            renderer.material.SetFloat("_AddDstBlend", 1f);
            renderer.material.SetFloat("_AddSrcBlend2", 0f);
            renderer.material.SetFloat("_AddDstBlend2", 10f);
            renderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack | MaterialGlobalIlluminationFlags.RealtimeEmissive;
            renderer.material.renderQueue = 3101;
            renderer.material.enableInstancing = true;
        }
    }
}
