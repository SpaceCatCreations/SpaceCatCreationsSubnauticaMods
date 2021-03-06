using System.Collections;
using ArchitectsLibrary.Handlers;
using ArchitectsLibrary.Patches;
using ArchitectsLibrary.Utility;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UWE;

namespace ArchitectsLibrary.API
{
    /// <summary>
    /// an abstract class inheriting from <see cref="Spawnable"/> that simplifies the process of making a Custom Egg.
    /// </summary>
    public abstract class EggPrefab : Spawnable
    {
        TechType _overridenTechType;
        GameObject _processedPrefab;
        
        /// <summary>
        /// Initializes a new <see cref="EggPrefab"/>
        /// </summary>
        /// <param name="classId">The main internal identifier for this item. Your item's <see cref="TechType"/> will be created using this name.</param>
        /// <param name="friendlyName">The name displayed in-game for this item whether in the open world or in the inventory.</param>
        /// <param name="description">The description for this item, Typically seen in the PDA, Inventory, or crafting screens.</param>
        public EggPrefab(string classId, string friendlyName, string description)
            : base(classId, friendlyName, description)
        {
            var mod = GetType().Assembly;
       
            OnStartedPatching += () =>
            {
                _overridenTechType = TechTypeHandler.AddTechType(mod, MakeATechTypeToOverride.classId,
                    MakeATechTypeToOverride.friendlyName,
                    MakeATechTypeToOverride.description);
            };
            OnFinishedPatching += () =>
            {
                if (AcidImmune)
                    AUHandler.MakeItemAcidImmune(this.TechType);

                if (ItemSprite != null)
                {
                    SpriteHandler.RegisterSprite(this.TechType, ItemSprite);
                    SpriteHandler.RegisterSprite(_overridenTechType, ItemSprite);
                }
                else if (ItemSpriteFromTexture != null)
                {
                    SpriteHandler.RegisterSprite(this.TechType, ImageUtils.LoadSpriteFromTexture(ItemSpriteFromTexture));
                    SpriteHandler.RegisterSprite(_overridenTechType, ImageUtils.LoadSpriteFromTexture(ItemSpriteFromTexture));
                }

                CraftDataHandler.SetItemSize(_overridenTechType, this.SizeInInventory);

                if (MakeCreatureLayEggs)
                    AUHandler.SetCreatureEgg(HatchingCreature, this.TechType);

                if (RequiredACUSize > 1)
                {
                    WaterParkPatches.requiredAcuSize[TechType] = RequiredACUSize;
                }
            };
        }
        public delegate void GameObjectEnhancements(GameObject gameObject);

        /// <summary>
        /// logic for your GameObject that will get executed before the core logic of your GameObject.
        /// </summary>
        public GameObjectEnhancements EarlyEnhancements;

        /// <summary>
        /// logic for your GameObject that will get executed after the core logic of your GameObject.
        /// </summary>
        public GameObjectEnhancements LateEnhancements;

        /// <summary>
        /// override this Property to define your egg's prefab.
        /// </summary>
        public abstract GameObject Model { get; }

        /// <summary>
        /// the creature that's gonna hatch from this egg.
        /// </summary>
        public abstract TechType HatchingCreature { get; }

        /// <summary>
        /// amount of in-game days this egg will take to hatch the <seealso cref="HatchingCreature"/>.
        /// </summary>
        public abstract float HatchingTime { get; }

        /// <summary>
        /// override this Property to define the <see cref="Sprite"/> of your egg.
        /// </summary>
        public virtual Sprite ItemSprite { get; } = null;

        /// <summary>
        /// override this Property to define the <see cref="Sprite"/> of your egg from a Texture2D.
        /// </summary>
        public virtual Texture2D ItemSpriteFromTexture { get; } = null;

        /// <summary>
        /// Mass of the egg by KG. defaulted to 100.
        /// </summary>
        public virtual float Mass => 100f;

        /// <summary>
        /// Health of the egg. defaulted to 60.
        /// </summary>
        public virtual float MaxHealth => 60f;

        /// <summary>
        /// The total amount of ACU floors required for the egg to be dropped in the ACU.<br/>
        /// defaulted to 1 and assigning this Property to "0" will be ignored.
        /// </summary>
        public virtual int RequiredACUSize => 1;

        /// <summary>
        /// makes the egg scannable via the Scanner Room.
        /// </summary>
        public virtual bool MakeObjectScannable => true;

        /// <summary>
        /// makes the egg immune to the Lost River's Acidic Brine.
        /// </summary>
        public virtual bool AcidImmune => true;

        /// <summary>
        /// <para>makes the Creature lay eggs in the ACU when it breeds, if this property is set to <see langword="false" />, it will breed creatures.</para>
        /// defaulted to <see langword="true" />.
        /// </summary>
        public virtual bool MakeCreatureLayEggs => true;

        /// <summary>
        /// Determines how close you have to be to the egg for it to appear.
        /// </summary>
        public virtual LargeWorldEntity.CellLevel CellLevel => LargeWorldEntity.CellLevel.Medium;

        /// <summary>
        /// determines the TechType of the undiscovered version of this egg. the friendlyName and the description of
        /// this egg will appear if this egg hasn't been Hatched at least once.
        /// </summary>
        public virtual OverrideTechType MakeATechTypeToOverride =>
            new OverrideTechType(ClassID + "Undiscovered", "Creature Egg", "An unidentified egg.");

        public sealed override WorldEntityInfo EntityInfo => new WorldEntityInfo()
        {
            classId = this.ClassID,
            cellLevel = CellLevel,
            localScale = Vector3.one,
            prefabZUp = false,
            slotType = EntitySlot.Type.Medium,
            techType = this.TechType
        };
#if SN1
        public sealed override GameObject GetGameObject()
        {
            if (_processedPrefab != null)
            {
                return _processedPrefab;
            }
            
            GameObject prefab = Model;
            var obj = GameObject.Instantiate(prefab);

            EarlyEnhancements?.Invoke(obj);

            obj.EnsureComponent<TechTag>().type = this.TechType;
            obj.EnsureComponent<PrefabIdentifier>().ClassId = this.ClassID;
            obj.EnsureComponent<LargeWorldEntity>().cellLevel = CellLevel;

            var skyApplier = obj.EnsureComponent<SkyApplier>();
            skyApplier.anchorSky = Skies.Auto;
            skyApplier.emissiveFromPower = false;
            skyApplier.dynamic = false;
            skyApplier.renderers = obj.GetAllComponentsInChildren<Renderer>();
            skyApplier.enabled = true;

            obj.EnsureComponent<Pickupable>();

            var rb = obj.EnsureComponent<Rigidbody>();
            rb.mass = Mass;
            rb.isKinematic = true;
            rb.useGravity = false;

            var wf = obj.EnsureComponent<WorldForces>();
            wf.useRigidbody = rb;

            var liveMixin = obj.EnsureComponent<LiveMixin>();
            liveMixin.health = MaxHealth;

            var creatureEgg = obj.EnsureComponent<CreatureEgg>();
            creatureEgg.animator = obj.GetComponentInChildren<Animator>() ?? obj.EnsureComponent<Animator>();
            creatureEgg.hatchingCreature = HatchingCreature;
            creatureEgg.daysBeforeHatching = HatchingTime;
            if (_overridenTechType != TechType.None)
                creatureEgg.overrideEggType = _overridenTechType;

            if (MakeObjectScannable)
                AUHandler.SetObjectScannable(obj);

            MaterialUtils.ApplySNShaders(obj);

            LateEnhancements?.Invoke(obj);

            _processedPrefab = obj;
            return obj;
        }
#endif
        public sealed override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            if (_processedPrefab != null)
            {
                gameObject.Set(_processedPrefab);
                yield break;
            }
        
            GameObject prefab = Model;
            var obj = GameObject.Instantiate(prefab);


            EarlyEnhancements?.Invoke(obj);

            obj.EnsureComponent<TechTag>().type = this.TechType;
            obj.EnsureComponent<PrefabIdentifier>().ClassId = this.ClassID;
            obj.EnsureComponent<LargeWorldEntity>().cellLevel = CellLevel;

            var skyApplier = obj.EnsureComponent<SkyApplier>();
            skyApplier.anchorSky = Skies.Auto;
            skyApplier.emissiveFromPower = false;
            skyApplier.dynamic = false;
            skyApplier.renderers = obj.GetAllComponentsInChildren<Renderer>();
            skyApplier.enabled = true;

            obj.EnsureComponent<Pickupable>();

            var rb = obj.EnsureComponent<Rigidbody>();
            rb.mass = Mass;
            rb.isKinematic = true;

            var wf = obj.EnsureComponent<WorldForces>();
            wf.useRigidbody = rb;

            var liveMixin = obj.EnsureComponent<LiveMixin>();
            liveMixin.health = MaxHealth;

            var creatureEgg = obj.EnsureComponent<CreatureEgg>();
            Animator eggAnimator = obj.GetComponentInChildren<Animator>();
            if (eggAnimator == null)
            {
                eggAnimator = obj.EnsureComponent<Animator>();
            }
            creatureEgg.animator = eggAnimator;
            creatureEgg.hatchingCreature = HatchingCreature;
            creatureEgg.daysBeforeHatching = HatchingTime;
            if (_overridenTechType != TechType.None)
                creatureEgg.overrideEggType = _overridenTechType;

            if (MakeObjectScannable)
                AUHandler.SetObjectScannable(obj);

            MaterialUtils.ApplySNShaders(obj);

            LateEnhancements?.Invoke(obj);

            _processedPrefab = obj;
            gameObject.Set(obj);
        }

        /// <summary>
        /// determines the TechType of the undiscovered version of the egg.
        /// </summary>
        public struct OverrideTechType
        {
            public string classId;
            public string friendlyName;
            public string description;

            /// <summary>
            /// determines the TechType of the undiscovered version of the egg. the <see cref="friendlyName"/>,
            /// <see cref="description"/> of this <see cref="TechType"/> will replace the original ones of this egg
            ///  if the said egg hasn't Hatched at least once.
            /// </summary>
            /// <param name="classId">determines the <see cref="TechType"/>.</param>
            /// <param name="friendlyName">the friendlyName that's gonna replace the original egg's friendlyName
            /// if it hasn't Hatched at least once.</param>
            /// <param name="description">the description that's gonna replace the original egg's description
            /// if it hasn't Hatched at least once.</param>
            public OverrideTechType(string classId, string friendlyName, string description)
            {
                this.classId = classId;
                this.friendlyName = friendlyName;
                this.description = description;
            }
        }
    }
}
