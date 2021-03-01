using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;

namespace ECCLibrary
{
    /// <summary>
    /// Settings related to edible items.
    /// </summary>
    public struct EatableData
    {
        public bool CanBeEaten;
        public float FoodAmount;
        public float WaterAmount;
        public bool Decomposes;
        public float DecomposeSpeed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canBeEaten">Whether this Item is edible or not.</param>
        /// <param name="foodAmount">The max amount of Food this item will give when eaten.</param>
        /// <param name="waterAmount">The max amount of Water this item will give when eaten.</param>
        /// <param name="decomposes">Whether this item decomposes over time.</param>
        public EatableData(bool canBeEaten, float foodAmount, float waterAmount, bool decomposes)
        {
            CanBeEaten = canBeEaten;
            FoodAmount = foodAmount;
            WaterAmount = waterAmount;
            Decomposes = decomposes;
            DecomposeSpeed = 1f;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="canBeEaten">Whether this Item is edible or not.</param>
        /// <param name="foodAmount">The max amount of Food this item will give when eaten.</param>
        /// <param name="waterAmount">The max amount of Water this item will give when eaten.</param>
        /// <param name="decomposes">Whether this item decomposes over time.</param>
        /// <param name="decomposeSpeed">How fast this item decomposes. Default value is 1f.</param>
        public EatableData(bool canBeEaten, float foodAmount, float waterAmount, bool decomposes, float decomposeSpeed)
        {
            CanBeEaten = canBeEaten;
            FoodAmount = foodAmount;
            WaterAmount = waterAmount;
            Decomposes = decomposes;
            DecomposeSpeed = decomposeSpeed;
        }
        /// <summary>
        /// Apply this EatableData to an existing Item.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public Eatable MakeItemEatable(GameObject go)
        {
            if(go.GetComponent<Eatable>() != null)
            {
                ErrorMessage.AddMessage(string.Format("ECC: Object {0} already is edible.", go.name));
            }
            var eatable = go.AddComponent<Eatable>();
            eatable.allowOverfill = true;
            eatable.foodValue = FoodAmount;
            eatable.waterValue = WaterAmount;
            eatable.kDecayRate = 0.015f * DecomposeSpeed;
            eatable.SetDecomposes(Decomposes);
            return eatable;
        }
    }
    /// <summary>
    /// Settings related to scanning and Databank entries.
    /// </summary>
    public struct ScannableItemData
    {
        public bool scannable;
        public float scanTime;
        public string encyPath;
        public string[] encyNodes;
        public Sprite popup;
        public Texture2D encyImage;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scannable">Whether this can be scanned and has an encyclopedia entry.</param>
        /// <param name="scanTime">How long it takes to scan this creature.</param>
        /// <param name="encyPath">The path to the encyclopedia. Example: "Lifeforms/Fauna/Carnivores".</param>
        /// <param name="encyNodes">The path to the encyclopedia in array form. Example: { "Lifeforms", "Fauna", "Carnivores" }.</param>
        /// <param name="popup">The popup image. Must be exported as a Sprite. If null, the default popup is used.</param>
        /// <param name="encyImage">The image of the encyclopedia entry. If null, no image will be used.</param>
        public ScannableItemData(bool scannable, float scanTime, string encyPath, string[] encyNodes, Sprite popup, Texture2D encyImage)
        {
            this.scannable = scannable;
            this.scanTime = scanTime;
            this.encyPath = encyPath;
            this.encyNodes = encyNodes;
            this.popup = popup;
            this.encyImage = encyImage;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scannable">Whether this can be scanned and has an encyclopedia entry.</param>
        /// <param name="scanTime">How long it takes to scan this creature.</param>
        /// <param name="encyPath">The path to the encyclopedia. Example: "Lifeforms/Fauna/Carnivores".</param>
        /// <param name="popup">The popup image. Must be exported as a Sprite. If null, the default popup is used.</param>
        /// <param name="encyImage">The image of the encyclopedia entry. If null, no image will be used.</param>
        public ScannableItemData(bool scannable, float scanTime, string encyPath, Sprite popup, Texture2D encyImage)
        {
            this.scannable = scannable;
            this.scanTime = scanTime;
            this.encyPath = encyPath;
            this.encyNodes = encyPath.Split('/');
            this.popup = popup;
            this.encyImage = encyImage;
        }

        internal void AttemptPatch(ModPrefab prefab, string encyTitle, string encyDesc)
        {
            PDAEncyclopediaHandler.AddCustomEntry(new PDAEncyclopedia.EntryData()
            {
                key = prefab.ClassID,
                nodes = encyNodes,
                path = encyPath,
                image = encyImage,
                popup = popup
            });
            PDAHandler.AddCustomScannerEntry(new PDAScanner.EntryData()
            {
                key = prefab.TechType,
                encyclopedia = prefab.ClassID,
                scanTime = scanTime,
                isFragment = false
            });
            LanguageHandler.SetLanguageLine("Ency_" + prefab.ClassID, encyTitle);
            LanguageHandler.SetLanguageLine("EncyDesc_" + prefab.ClassID, encyDesc);
        }
    }
    /// <summary>
    /// Settings related to the standard Subnautica shader.
    /// </summary>
    public struct UBERMaterialProperties
    {
        public float Shininess;
        public float SpecularInt;
        public float EmissionScale;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shininess">How smooth the material appears. Recommended range: 1-8.</param>
        /// <param name="specularInt">How bright the reflection is. Recommended range: 1-10.</param>
        /// <param name="emissionScale">How bright the emission/illum is. Recommended range: 0-5.</param>
        public UBERMaterialProperties(float shininess, float specularInt = 1f, float emissionScale = 1f)
        {
            Shininess = shininess;
            SpecularInt = specularInt;
            EmissionScale = emissionScale;
        }
    }
}
