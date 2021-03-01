using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;
using HarmonyLib;
using ECCLibrary;
#if SN1
using Sprite = Atlas.Sprite;
#endif
namespace ECCLibrary
{
    /// <summary>
    /// Asset class for CreatureEggs. Must inherit from this class to add custom spawns.
    /// </summary>
    public class CreatureEggAsset : Spawnable
    {
        private GameObject model;
        /// <summary>
        /// The object to be edited in 'AddCustomBehaviours'.
        /// </summary>
        protected GameObject prefab;
        private TechType hatchingCreature;
        private Sprite sprite;
        Texture2D spriteTexture;
        static LiveMixinData eggLiveMixinData;
        float hatchingTime;

        /// <summary>
        /// Create a new egg asset.
        /// </summary>
        /// <param name="classId">TechType / ClassId of the egg.</param>
        /// <param name="friendlyName">The name displayed in-game.</param>
        /// <param name="description">The tooltip displayed in-game.</param>
        /// <param name="model">The default model of  this egg.</param>
        /// <param name="hatchingCreature">The creature that hatches out of this egg.</param>
        /// <param name="spriteTexture">The texture displayed in the inventory.</param>
        /// <param name="hatchingTime">How much time (in days) it takes for this egg to hatch.</param>
        public CreatureEggAsset(string classId, string friendlyName, string description, GameObject model, TechType hatchingCreature, Texture2D spriteTexture, float hatchingTime) : base(classId, friendlyName, description)
        {
            this.model = model;
            this.hatchingCreature = hatchingCreature;
            this.spriteTexture = spriteTexture;
            this.hatchingTime = hatchingTime;
            OnStartedPatching += () =>
            {
                sprite = ImageUtils.LoadSpriteFromTexture(spriteTexture);
                if (eggLiveMixinData == null)
                {
                    eggLiveMixinData = ECCHelpers.CreateNewLiveMixinData();
                    eggLiveMixinData.destroyOnDeath = true;
                    eggLiveMixinData.explodeOnDestroy = true;
                    eggLiveMixinData.maxHealth = GetMaxHealth;
                    eggLiveMixinData.knifeable = true;
                }
            };
            OnFinishedPatching += () =>
            {
                if (AcidImmune)
                {
                    ECCHelpers.MakeAcidImmune(TechType);
                }
                if (IsScannable)
                {
                    ScannableSettings.AttemptPatch(this, GetEncyTitle, GetEncyDesc);
                }
                ECCHelpers.PatchItemSounds(TechType, ItemSoundsType.Egg);
            };
        }
        /// <summary>
        /// Information related to spawning. Most of this is done for you; only override this if necessary.
        /// </summary>
        public override WorldEntityInfo EntityInfo => new WorldEntityInfo()
        {
            slotType = EntitySlot.Type.Small,
            cellLevel = LargeWorldEntity.CellLevel.Near,
            classId = ClassID,
            techType = TechType
        };
#if SN1
        public override GameObject GetGameObject()
        {
            if(prefab == null)
            {
                var obj = GameObject.Instantiate(model);
                prefab = obj;
                prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.EnsureComponent<TechTag>().type = TechType;
                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>();
                skyApplier.anchorSky = Skies.Auto;
                skyApplier.dynamic = false;
                skyApplier.emissiveFromPower = false;
                skyApplier.hideFlags = HideFlags.None;
                skyApplier.enabled = true;

                Pickupable pickupable = prefab.EnsureComponent<Pickupable>();

                LiveMixin lm = prefab.EnsureComponent<LiveMixin>();
                lm.data = eggLiveMixinData;
                lm.health = GetMaxHealth;

                VFXSurface surface = prefab.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.organic;

                WaterParkItem waterParkItem = prefab.EnsureComponent<WaterParkItem>();
                waterParkItem.pickupable = pickupable;

                Rigidbody rb = prefab.EnsureComponent<Rigidbody>();
                rb.mass = 10f;
                rb.isKinematic = true;

                WorldForces worldForces = prefab.EnsureComponent<WorldForces>();
                worldForces.useRigidbody = rb;

                CreatureEgg egg = prefab.EnsureComponent<CreatureEgg>();
                egg.animator = prefab.GetComponentInChildren<Animator>() ?? prefab.GetComponent<Animator>() ?? prefab.AddComponent<Animator>();
                egg.hatchingCreature = hatchingCreature;
                egg.overrideEggType = TechType;
                egg.daysBeforeHatching = hatchingTime;

                EntityTag entityTag = prefab.EnsureComponent<EntityTag>();
                entityTag.slotType = EntitySlot.Type.Small;
                ECCHelpers.ApplySNShaders(prefab, MaterialSettings);

                if (ManualEggExplosion)
                {
                    egg.explodeOnHatch = false;

                    if (egg.progress >= 1f)
                    {
                        lm.TakeDamage(GetMaxHealth);
                        GameObject.Destroy(prefab, 15f);
                    }
                }
                obj.SetActive(false);

                AddCustomBehaviours();
            }

            return prefab;
        }
#endif
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            if (prefab == null)
            {
                var obj = GameObject.Instantiate(model);
                prefab = obj;
                prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.EnsureComponent<TechTag>().type = TechType;
                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>();
                skyApplier.anchorSky = Skies.Auto;
                skyApplier.dynamic = false;
                skyApplier.emissiveFromPower = false;
                skyApplier.hideFlags = HideFlags.None;
                skyApplier.enabled = true;

                Pickupable pickupable = prefab.EnsureComponent<Pickupable>();

                LiveMixin lm = prefab.EnsureComponent<LiveMixin>();
                lm.data = eggLiveMixinData;
                lm.health = GetMaxHealth;

                VFXSurface surface = prefab.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.organic;

                WaterParkItem waterParkItem = prefab.EnsureComponent<WaterParkItem>();
                waterParkItem.pickupable = pickupable;

                Rigidbody rb = prefab.EnsureComponent<Rigidbody>();
                rb.mass = 10f;
                rb.isKinematic = true;

                WorldForces worldForces = prefab.EnsureComponent<WorldForces>();
                worldForces.useRigidbody = rb;

                CreatureEgg egg = prefab.EnsureComponent<CreatureEgg>();
                egg.animator = prefab.GetComponentInChildren<Animator>() ?? prefab.GetComponent<Animator>() ?? prefab.AddComponent<Animator>();
                egg.hatchingCreature = hatchingCreature;
                egg.overrideEggType = TechType;
                egg.daysBeforeHatching = hatchingTime;

                EntityTag entityTag = prefab.EnsureComponent<EntityTag>();
                entityTag.slotType = EntitySlot.Type.Small;
                ECCHelpers.ApplySNShaders(prefab, MaterialSettings);

                if (ManualEggExplosion)
                {
                    egg.explodeOnHatch = false;

                    if (egg.progress >= 1f)
                    {
                        lm.TakeDamage(GetMaxHealth);
                        GameObject.Destroy(prefab, 15f);
                    }
                }
                obj.SetActive(false);

                AddCustomBehaviours();
            }
            yield return null;
            gameObject.Set(prefab);
        }
        /// <summary>
        /// Override this to change the sprite. By default uses the sprite passed in from the constructor.
        /// </summary>
        /// <returns></returns>
        protected override Sprite GetItemSprite()
        {
            return sprite;
        }
        /// <summary>
        /// Settings related to how this egh is rendered.
        /// </summary>
        public virtual UBERMaterialProperties MaterialSettings
        {
            get
            {
                return new UBERMaterialProperties(8f, 1f);
            }
        }
        /// <summary>
        /// Is this egg immune to acid?
        /// </summary>
        public virtual bool AcidImmune
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// The max health of this egg. Note: one knife swing deals 20 damage.
        /// </summary>
        public virtual float GetMaxHealth
        {
            get
            {
                return 20f;
            }
        }
        /// <summary>
        /// Set to 'FriendlyName' by default.
        /// </summary>
        public virtual string GetEncyTitle
        {
            get
            {
                return FriendlyName;
            }
        }
        /// <summary>
        /// Override this to edit the ency text.
        /// </summary>
        public virtual string GetEncyDesc
        {
            get
            {
                return "No ency description.";
            }
        }
        /// <summary>
        /// Override and set to true if you want this to be scannable & have a databank entry.
        /// </summary>
        public virtual bool IsScannable
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Only override this if necessary. Scannable settings are already done for you. Override IsScannable, GetEncyTitle, and GetEncyDesc for most Ency related options.
        /// </summary>
        public virtual ScannableItemData ScannableSettings
        {
            get
            {
                return new ScannableItemData(true, 2f, "Lifeforms/Fauna/Eggs", new string[] { "Lifeforms", "Fauna", "Eggs" }, UnityEngine.Sprite.Create(sprite.texture, new Rect(Vector2.zero, new Vector2(sprite.texture.width, sprite.texture.height)), new Vector2(0.5f, 0.5f)), null);
            }
        }
        /// <summary>
        /// Override this method if you want to further edit the Egg prefab.
        /// </summary>
        public virtual void AddCustomBehaviours()
        {

        }
        /// <summary>
        /// <para>Only meshes with read and write enabled can be exploded.</para>
        /// <para>Override this to true if your mesh doesn't have read and write enabled.</para>
        /// </summary>
        public virtual bool ManualEggExplosion { get; } = false;
    }
}
