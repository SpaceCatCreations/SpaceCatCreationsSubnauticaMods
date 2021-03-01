using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Handlers;
using System.Collections;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
using ECCLibrary.Internal;
using UWE;
#if SN1
using Sprite = Atlas.Sprite;
#endif
namespace ECCLibrary
{
    public abstract class CreatureAsset : Spawnable
    {
        private GameObject model;
        private Sprite sprite;

        /// <summary>
        /// The prefab for this Creature. Edit this from the AddCustomBehaviour override.
        /// </summary>
        protected GameObject prefab;

        static GameObject electricalDamagePrefab;

        /// <summary>
        /// Creates a new instance of a CreatureAsset.
        /// </summary>
        /// <param name="classId">The ClassID / TechType. Example: 'ReaperLeviathan'. Should not match an existing creature.</param>
        /// <param name="friendlyName">The name seen in-game. Example: 'Reaper Leviathan'.</param>
        /// <param name="description">The description/tooltip seen in the inventory.</param>
        /// <param name="model">The GameObject to be converted to a Creature.</param>
        /// <param name="spriteTexture">The image seen in the inventory/</param>
        public CreatureAsset(string classId, string friendlyName, string description, GameObject model, Texture2D spriteTexture) : base(classId, friendlyName, description)
        {
            this.model = model;
            if (spriteTexture != null)
            {
                sprite = ImageUtils.LoadSpriteFromTexture(spriteTexture);
            }
            OnFinishedPatching += () =>
            {
                WaterParkCreature.waterParkCreatureParameters.Add(TechType, WaterParkParameters);
                BioReactorHandler.SetBioReactorCharge(TechType, BioReactorCharge);
                ECCHelpers.PatchBehaviorType(TechType, BehaviourType);
                if (Pickupable)
                {
                    ECCHelpers.PatchEquipmentType(TechType, EquipmentType.Hand);
                }
                if (AcidImmune)
                {
                    DamageSystem.acidImmune.AddItem(TechType);
                }
                ScannableSettings.AttemptPatch(this, GetEncyTitle, GetEncyDesc);
                ECCHelpers.PatchItemSounds(TechType, ItemSounds);
                LanguageHandler.SetLanguageLine(string.Format("{0}_DiscoverMessage", ClassID), "NEW LIFEFORM DISCOVERED");
                PostPatch();
            };
        }

#if SN1
        private static void ValidateElectricalDamagePrefab()
        {
            if(electricalDamagePrefab)
            {
                return;
            }
            GameObject reaperLeviathan = Resources.Load<GameObject>("WorldEntities/Creatures/ReaperLeviathan");
            if (reaperLeviathan)
            {
                electricalDamagePrefab = reaperLeviathan.GetComponent<LiveMixin>().data.electricalDamageEffect;                
            }
        }
#endif

#if BZ
        private static void ValidateElectricalDamagePrefab()
        {
            if(electricalDamagePrefab)
            {
                return;
            }
            //logic for getting the electrical damage prefab goes here
        }
#endif

#if SN1
        /// <summary>
        /// Do not override this method unless necessary. Override 'AddCustomBehaviour' instead.
        /// </summary>
        /// <returns></returns>
        public override GameObject GetGameObject()
        {
            if(prefab == null)
            {
                SetupPrefab(out CreatureComponents components);
                AddCustomBehaviour(components);
                CompletePrefab(components);
            }
            return prefab;
        }
#endif
        /// <summary>
        /// Do not override this method unless necessary. Override 'AddCustomBehaviour' instead.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            if (prefab == null)
            {
                SetupPrefab(out CreatureComponents components);
                AddCustomBehaviour(components);
                CompletePrefab(components);
            }
            yield return null;
            gameObject.Set(prefab);
        }
        private void SetupPrefab(out CreatureComponents creatureComponents)
        {
            EnsurePrefabSetupCorrectly(model, ClassID);
            prefab = GameObject.Instantiate(model);
            prefab.tag = "Creature";
            prefab.SetActive(false);
            creatureComponents = SetupNecessaryComponents();
            ECCHelpers.ApplySNShaders(prefab, MaterialSettings);
        }
        static void EnsurePrefabSetupCorrectly(GameObject model, string name)
        {
            if (model == null)
            {
                ECCLog.AddMessage("ECC Warning: No model for creature {0} found.", name);
            }
            else
            {
                if (model.GetComponentInChildren<Animator>() == null)
                {
                    ECCLog.AddMessage("ECC Warning: Model for creature {0} needs an Animator somewhere in its hierarchy.", name);
                }
                if (model.GetComponentInChildren<Collider>() == null)
                {
                    ECCLog.AddMessage("ECC Warning: Model for creature {0} has no collider.", name);
                }
            }
        }
        private void CompletePrefab(CreatureComponents components)
        {

        }
        /// <summary>
        /// This is by default the 'sprite' defined in the constructor.
        /// </summary>
        /// <returns></returns>
        protected override Sprite GetItemSprite()
        {
            return sprite;
        }
        /// <summary>
        /// The settings based around spawning. Do not override this unless you have to.
        /// </summary>
        public override WorldEntityInfo EntityInfo => new WorldEntityInfo()
        {
            cellLevel = CellLevel,
            classId = ClassID,
            slotType = EntitySlot.Type.Creature,
            techType = TechType,
            localScale = Vector3.one
        };
        /// <summary>
        /// Any changes to be done after the AssetClass is patched.
        /// </summary>
        protected virtual void PostPatch()
        {

        }
        /// <summary>
        /// Adds a melee attack.
        /// </summary>
        /// <param name="mouth">The mouth object (should have a trigger collider).</param>
        /// <param name="biteInterval">Min time between attacks.</param>
        /// <param name="damage">Damage of the attack.</param>
        /// <param name="biteSoundPrefix">Creates a clip pool that can use ANY mod audio clips that start with this text.</param>
        /// <param name="consumeWholeHealthThreshold">If the creature has this much health or less, it is deleted from existence when chewed on.</param>
        /// <param name="regurgitateLater">If true, the creature will spit out anything swallowed whole.</param>
        /// <param name="components"></param>
        /// <returns></returns>
        public MeleeAttack_New AddMeleeAttack(GameObject mouth, float biteInterval, float damage, string biteSoundPrefix, float consumeWholeHealthThreshold, bool regurgitateLater, CreatureComponents components)
        {
            OnTouch onTouch = mouth.EnsureComponent<OnTouch>();
            onTouch.gameObject.EnsureComponent<Rigidbody>().isKinematic = true;
            MeleeAttack_New meleeAttack = prefab.AddComponent<MeleeAttack_New>();
            meleeAttack.mouth = mouth;
            meleeAttack.creature = components.creature;
            meleeAttack.liveMixin = components.liveMixin;
            meleeAttack.animator = components.creature.GetAnimator();
            meleeAttack.biteInterval = biteInterval;
            meleeAttack.lastTarget = components.lastTarget;
            meleeAttack.biteDamage = damage;
            meleeAttack.eatHappyIncrement = 0f;
            meleeAttack.eatHungerDecrement = 0.4f;
            meleeAttack.biteAggressionThreshold = 0f;
            meleeAttack.biteAggressionDecrement = 0.2f;
            meleeAttack.onTouch = onTouch;
            meleeAttack.biteSoundPrefix = biteSoundPrefix;
            meleeAttack.consumeWholeHealthThreshold = consumeWholeHealthThreshold;
            meleeAttack.regurgitate = regurgitateLater;
            return meleeAttack;
        }
        /// <summary>
        /// A messy method that does most of the work.
        /// </summary>
        /// <returns></returns>
        private CreatureComponents SetupNecessaryComponents()
        {
            CreatureComponents components = new CreatureComponents();
            components.prefabIdentifier = prefab.EnsureComponent<PrefabIdentifier>();
            components.prefabIdentifier.ClassId = ClassID;

            components.techTag = prefab.EnsureComponent<TechTag>();
            components.techTag.type = TechType;

            components.largeWorldEntity = prefab.EnsureComponent<LargeWorldEntity>();
            components.largeWorldEntity.cellLevel = CellLevel;

            components.entityTag = prefab.EnsureComponent<EntityTag>();
            components.entityTag.slotType = EntitySlot.Type.Creature;

            components.skyApplier = prefab.AddComponent<SkyApplier>();
            components.skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>(true);

            components.ecoTarget = prefab.AddComponent<EcoTarget>();
            components.ecoTarget.type = EcoTargetType;

            components.vfxSurface = prefab.EnsureComponent<VFXSurface>();
            components.vfxSurface.surfaceType = SurfaceType;

            components.behaviourLOD = prefab.EnsureComponent<BehaviourLOD>();
            components.behaviourLOD.veryCloseThreshold = BehaviourLODSettings.Close;
            components.behaviourLOD.closeThreshold = BehaviourLODSettings.Close;
            components.behaviourLOD.farThreshold = BehaviourLODSettings.Far;

            components.rigidbody = prefab.EnsureComponent<Rigidbody>();
            components.rigidbody.useGravity = false;
            components.rigidbody.mass = Mass;

            components.locomotion = prefab.AddComponent<Locomotion>();
            components.locomotion.useRigidbody = components.rigidbody;
            components.splineFollowing = prefab.AddComponent<SplineFollowing>();
            components.splineFollowing.respectLOD = false;
            components.splineFollowing.locomotion = components.locomotion;
            components.swimBehaviour = prefab.AddComponent<SwimBehaviour>();
            components.swimBehaviour.splineFollowing = components.splineFollowing;
            components.swimBehaviour.turnSpeed = TurnSpeed;

            components.lastScarePosition = prefab.AddComponent<LastScarePosition>();

            components.worldForces = prefab.EnsureComponent<WorldForces>();
            components.worldForces.useRigidbody = components.rigidbody;
            components.worldForces.handleGravity = true;
            components.worldForces.underwaterGravity = UnderwaterGravity;
            components.worldForces.aboveWaterGravity = AboveWaterGravity;

            ValidateElectricalDamagePrefab();
            components.liveMixin = prefab.EnsureComponent<LiveMixin>();
            components.liveMixin.data = ECCHelpers.CreateNewLiveMixinData();
            components.liveMixin.data.electricalDamageEffect = electricalDamagePrefab;
            SetLiveMixinData(ref components.liveMixin.data);
            if (components.liveMixin.data.maxHealth <= 0f)
            {
                ECCLog.AddMessage("Warning: Creatures should not have a max health of zero or below.");
            }
            components.liveMixin.health = components.liveMixin.maxHealth;

            components.creature = prefab.AddComponent<Creature>();

            components.creature.Aggression = new CreatureTrait(0f, TraitsSettings.AggressionDecreaseRate);
            components.creature.Hunger = new CreatureTrait(0f, -TraitsSettings.HungerIncreaseRate);
            components.creature.Scared = new CreatureTrait(0f, TraitsSettings.ScaredDecreaseRate);

            components.creature.liveMixin = components.liveMixin;
            ECCHelpers.SetPrivateField(typeof(Creature), components.creature, "traitsAnimator", components.creature.GetComponentInChildren<Animator>());
            components.creature.sizeDistribution = SizeDistribution;

            RoarAbility roar = null;
            if (!string.IsNullOrEmpty(RoarAbilitySettings.AudioClipPrefix))
            {
                roar = prefab.AddComponent<RoarAbility>();
                roar.minRoarDistance = RoarAbilitySettings.MinRoarDistance;
                roar.maxRoarDistance = RoarAbilitySettings.MaxRoarDistance;
                roar.animationName = RoarAbilitySettings.AnimationName;
                roar.clipPrefix = RoarAbilitySettings.AudioClipPrefix;
                roar.createCurrent = RoarAbilitySettings.CreateCurrentOnRoar;
                roar.currentStrength = RoarAbilitySettings.CurrentStrength;

                if (RoarAbilitySettings.RoarActionPriority > 0f)
                {
                    RoarRandomAction roarAction = prefab.AddComponent<RoarRandomAction>();
                    roarAction.roarIntervalMin = RoarAbilitySettings.MinTimeBetweenRoars;
                    roarAction.roarIntervalMax = RoarAbilitySettings.MaxTimeBetweenRoars;
                    roarAction.evaluatePriority = RoarAbilitySettings.RoarActionPriority;
                }
            }
            components.lastTarget = prefab.AddComponent<LastTarget_New>();
            components.lastTarget.roar = roar;
            if (EnableAggression)
            {
                if (AggressivenessToSmallVehicles.Aggression > 0f)
                {
                    AggressiveToPilotingVehicle atpv = prefab.AddComponent<AggressiveToPilotingVehicle>();
                    atpv.aggressionPerSecond = AggressivenessToSmallVehicles.Aggression;
                    atpv.range = AggressivenessToSmallVehicles.MaxRange;
                    atpv.creature = components.creature;
                    atpv.lastTarget = components.lastTarget;
                }
                if (AttackSettings.EvaluatePriority > 0f)
                {
                    AttackLastTarget actionAtkLastTarget = prefab.AddComponent<AttackLastTarget>();
                    actionAtkLastTarget.evaluatePriority = AttackSettings.EvaluatePriority;
                    actionAtkLastTarget.swimVelocity = AttackSettings.ChargeVelocity;
                    actionAtkLastTarget.aggressionThreshold = 0.02f;
                    actionAtkLastTarget.minAttackDuration = AttackSettings.MinAttackLength;
                    actionAtkLastTarget.maxAttackDuration = AttackSettings.MaxAttackLength;
                    actionAtkLastTarget.pauseInterval = AttackSettings.AttackInterval;
                    actionAtkLastTarget.rememberTargetTime = AttackSettings.RememberTargetTime;
                    actionAtkLastTarget.priorityMultiplier = ECCHelpers.Curve_Flat();
                    actionAtkLastTarget.lastTarget = components.lastTarget;
                }
            }
            components.swimRandom = prefab.AddComponent<SwimRandom>();
            components.swimRandom.swimRadius = SwimRandomSettings.SwimRadius;
            components.swimRandom.swimVelocity = SwimRandomSettings.SwimVelocity;
            components.swimRandom.swimInterval = SwimRandomSettings.SwimInterval;
            components.swimRandom.evaluatePriority = SwimRandomSettings.EvaluatePriority;
            components.swimRandom.priorityMultiplier = ECCHelpers.Curve_Flat();
            if (AvoidObstaclesSettings.evaluatePriority > 0f)
            {
                AvoidObstacles avoidObstacles = prefab.AddComponent<AvoidObstacles>();
                avoidObstacles.avoidTerrainOnly = AvoidObstaclesSettings.terrainOnly;
                avoidObstacles.avoidanceDistance = AvoidObstaclesSettings.avoidDistance;
                avoidObstacles.scanDistance = AvoidObstaclesSettings.avoidDistance;
                avoidObstacles.priorityMultiplier = ECCHelpers.Curve_Flat();
                avoidObstacles.evaluatePriority = AvoidObstaclesSettings.evaluatePriority;
                avoidObstacles.swimVelocity = SwimRandomSettings.SwimVelocity;
            }
            if (CanBeInfected)
            {
                components.infectedMixin = prefab.AddComponent<InfectedMixin>();
                components.infectedMixin.renderers = prefab.GetComponentsInChildren<Renderer>(true);
            }
            if (Pickupable)
            {
                components.pickupable = prefab.EnsureComponent<Pickupable>();
                if (ViewModelSettings.ReferenceHoldingAnimation != TechType.None)
                {
                    HeldFish heldFish = prefab.EnsureComponent<HeldFish>();
                    heldFish.animationName = ViewModelSettings.ReferenceHoldingAnimation.ToString().ToLower();
                    heldFish.mainCollider = prefab.GetComponent<Collider>();
                    heldFish.pickupable = components.pickupable;
                }
                else
                {
                    ECCLog.AddMessage("Creature {0} is Pickupable but has no applied ViewModelSettings. This is necessary to be held and placed in an aquarium.", TechType);
                }
                if (!string.IsNullOrEmpty(ViewModelSettings.ViewModelName))
                {
                    var fpsModel = prefab.EnsureComponent<FPModel>();
                    fpsModel.propModel = prefab.SearchChild(ViewModelSettings.WorldModelName);
                    if (fpsModel.propModel == null)
                    {
                        ECCLog.AddMessage("Error finding World model. No child of name {0} exists in the hierarchy of item {1}.", ViewModelSettings.WorldModelName, TechType);
                    }
                    else
                    {
                        prefab.EnsureComponent<AquariumFish>().model = fpsModel.propModel;
                        if(fpsModel.propModel.GetComponentInChildren<AnimateByVelocity>() == null)
                        {
                            fpsModel.propModel.AddComponent<AnimateByVelocity>();
                        }
                    }
                    fpsModel.viewModel = prefab.SearchChild(ViewModelSettings.ViewModelName);
                    if (fpsModel.viewModel == null)
                    {
                        ECCLog.AddMessage("Error finding View model. No child of name {0} exists in the hierarchy of item {1}.", ViewModelSettings.ViewModelName, TechType);
                    }
                }
            }
            Eatable eatable = null;
            if (EatableSettings.CanBeEaten)
            {
                eatable = EatableSettings.MakeItemEatable(prefab);
            }
            if(SwimInSchoolSettings.EvaluatePriority > 0f)
            {
                SwimInSchool swimInSchool = prefab.AddComponent<SwimInSchool>();
                swimInSchool.priorityMultiplier = ECCHelpers.Curve_Flat();
                swimInSchool.evaluatePriority = SwimInSchoolSettings.EvaluatePriority;
                swimInSchool.swimInterval = SwimInSchoolSettings.SwimInterval;
                swimInSchool.swimVelocity = SwimInSchoolSettings.SwimVelocity;
                swimInSchool.schoolSize = SwimInSchoolSettings.SchoolSize;
                ECCHelpers.SetPrivateField(typeof(SwimInSchool), swimInSchool, "percentFindLeaderRespond", SwimInSchoolSettings.FindLeaderChance);
                ECCHelpers.SetPrivateField(typeof(SwimInSchool), swimInSchool, "chanceLoseLeader", SwimInSchoolSettings.LoseLeaderChance);
                ECCHelpers.SetPrivateField(typeof(SwimInSchool), swimInSchool, "kBreakDistance", SwimInSchoolSettings.BreakDistance);
            }
            components.animateByVelocity = prefab.AddComponent<AnimateByVelocity>();
            components.animateByVelocity.animator = components.creature.GetAnimator();
            components.animateByVelocity.animationMoveMaxSpeed = MaxVelocityForSpeedParameter;
            components.animateByVelocity.levelOfDetail = components.behaviourLOD;
            components.animateByVelocity.rootGameObject = prefab;

            components.creatureDeath = prefab.AddComponent<CreatureDeath>();
            components.creatureDeath.useRigidbody = components.rigidbody;
            components.creatureDeath.liveMixin = components.liveMixin;
            components.creatureDeath.eatable = eatable;
            if (RespawnSettings.CanRespawn)
            {
                GameObject respawnerPrefab = new GameObject("Respawner");
                var respawnComponent = respawnerPrefab.AddComponent<Respawn>();
                respawnComponent.techType = TechType;
                respawnComponent.spawnTime = RespawnSettings.RespawnDelay;

                respawnerPrefab.SetActive(false);
                ECCHelpers.SetPrivateField(typeof(CreatureDeath), components.creatureDeath, "respawnerPrefab", respawnerPrefab);
            }
            var deadAnimationOnEnable = prefab.AddComponent<DeadAnimationOnEnable>();
            deadAnimationOnEnable.enabled = false;
            deadAnimationOnEnable.animator = components.creature.GetAnimator();
            deadAnimationOnEnable.liveMixin = components.liveMixin;
            deadAnimationOnEnable.enabled = true;
            if(StayAtLeashSettings.EvaluatePriority > 0f)
            {
                var stayAtLeash = prefab.AddComponent<StayAtLeashPosition>();
                stayAtLeash.evaluatePriority = StayAtLeashSettings.EvaluatePriority;
                stayAtLeash.priorityMultiplier = ECCHelpers.Curve_Flat(1f);
                stayAtLeash.swimVelocity = SwimRandomSettings.SwimVelocity;
                stayAtLeash.leashDistance = StayAtLeashSettings.MaxDistance;
            }
            if (ScannerRoomScannable)
            {
                ECCHelpers.MakeObjectScannerRoomScannable(prefab, true);
            }

            return components;
        }
        /// <summary>
        /// Makes the creature aggressive to creatures matching ecoTarget.
        /// </summary>
        /// <param name="maxRange">The absolute max range.</param>
        /// <param name="maxSearchRings">More rings means further search distance.</param>
        /// <param name="ecoTarget">What kind of creatures it will attack.</param>
        /// <param name="hungerThreshold">How hungry it has to be to attack something.</param>
        /// <param name="aggressionSpeed">How fast it becomes aggravated.</param>
        /// <returns></returns>
        protected AggressiveWhenSeeTarget MakeAggressiveTo(float maxRange, int maxSearchRings, EcoTargetType ecoTarget, float hungerThreshold, float aggressionSpeed)
        {
            AggressiveWhenSeeTarget aggressiveWhenSeeTarget = prefab.AddComponent<AggressiveWhenSeeTarget>();
            aggressiveWhenSeeTarget.maxRangeMultiplier = ECCHelpers.Curve_Flat();
            aggressiveWhenSeeTarget.distanceAggressionMultiplier = ECCHelpers.Curve_Flat();
            aggressiveWhenSeeTarget.maxRangeScalar = maxRange;
            aggressiveWhenSeeTarget.maxSearchRings = maxSearchRings;
            aggressiveWhenSeeTarget.lastScarePosition = prefab.GetComponent<LastScarePosition>();
            aggressiveWhenSeeTarget.lastTarget = prefab.GetComponent<LastTarget>();
            aggressiveWhenSeeTarget.targetType = ecoTarget;
            aggressiveWhenSeeTarget.hungerThreshold = hungerThreshold;
            aggressiveWhenSeeTarget.aggressionPerSecond = aggressionSpeed;
            return aggressiveWhenSeeTarget;
        }
        /// <summary>
        /// Creates a trail, which is used for procedural animation of tail-like objects.
        /// </summary>
        /// <param name="trailParent">The root segment, which does not move. The first child of this object and all children of the first child are used for the trail.</param>
        /// <param name="components">The reference to all the components.</param>
        /// <param name="segmentSnapSpeed">How fast each segment snaps back into the default position. A higher value gives a more rigid appearance.</param>
        /// <param name="maxSegmentOffset">How far each segment can be from the original position.</param>
        /// <param name="multiplier">The total strength of the movement. A value too low or too high will break the trail completely.</param>
        protected TrailManager CreateTrail(GameObject trailParent, CreatureComponents components, float segmentSnapSpeed, float maxSegmentOffset = -1f, float multiplier = 1f)
        {
            TrailManager trail = trailParent.AddComponent<TrailManager>();
            trail.trails = trailParent.transform.GetChild(0).GetComponentsInChildren<Transform>();
            trail.rootTransform = prefab.transform;
            trail.rootSegment = trail.transform;
            trail.levelOfDetail = components.behaviourLOD;
            trail.segmentSnapSpeed = segmentSnapSpeed;
            trail.maxSegmentOffset = maxSegmentOffset;
            trail.allowDisableOnScreen = false;
            AnimationCurve decreasing = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.25f * multiplier), new Keyframe(1f, 0.75f * multiplier) });
            trail.pitchMultiplier = decreasing;
            trail.rollMultiplier = decreasing;
            trail.yawMultiplier = decreasing;
            return trail;
        }
        /// <summary>
        /// Creates a trail manager, which is used for procedural animation of tail-like objects.
        /// </summary>
        /// <param name="trailRoot">The root segment, which does not move.</param>
        /// <param name="trails">Any objects that are simulated.</param>
        /// <param name="components">The reference to all the components.</param>
        /// <param name="segmentSnapSpeed">How fast each segment snaps back into the default position. A higher value gives a more rigid appearance.</param>
        /// <param name="maxSegmentOffset">How far each segment can be from the original position.</param>
        protected TrailManager CreateTrail(GameObject trailRoot, Transform[] trails, CreatureComponents components, float segmentSnapSpeed, float maxSegmentOffset = -1f)
        {
            trailRoot.gameObject.SetActive(false);
            TrailManager trail = trailRoot.AddComponent<TrailManager>();
            trail.trails = trails;
            trail.rootTransform = prefab.transform;
            trail.rootSegment = trail.transform;
            trail.levelOfDetail = components.behaviourLOD;
            trail.segmentSnapSpeed = segmentSnapSpeed;
            trail.maxSegmentOffset = maxSegmentOffset;
            trail.allowDisableOnScreen = false;
            AnimationCurve decreasing = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.25f), new Keyframe(1f, 0.75f) });
            trail.pitchMultiplier = decreasing;
            trail.rollMultiplier = decreasing;
            trail.yawMultiplier = decreasing;
            MethodInfo method = typeof(TrailManager).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(trail, new object[] { });
            trailRoot.gameObject.SetActive(true);
            return trail;
        }

#region Abstracts

        /// <summary>
        /// Add CreatureActions and things of that like here.
        /// </summary>
        /// <param name="components"></param>
        public abstract void AddCustomBehaviour(CreatureComponents components);

        /// <summary>
        /// Should match the EcoTargetType whenever possible. Does not do much on its own.
        /// </summary>
        public abstract BehaviourType BehaviourType
        {
            get;
        }

        /// <summary>
        /// How far this creature can be loaded in.
        /// </summary>
        public abstract LargeWorldEntity.CellLevel CellLevel
        {
            get;
        }

        /// <summary>
        /// Settings on random swimming.
        /// </summary>
        public abstract SwimRandomData SwimRandomSettings { get; }

        /// <summary>
        /// What other creatures recognize this creature as. Should match with BehaviourType whenever possible.
        /// </summary>
        public abstract EcoTargetType EcoTargetType
        {
            get;
        }

        /// <summary>
        /// This is where you set the LiveMixin data.
        /// </summary>
        /// <param name="liveMixinData"></param>
        public abstract void SetLiveMixinData(ref LiveMixinData liveMixinData);

#endregion

#region Overrideable
        /// <summary>
        /// Settings that determine basic attributes of the creature.
        /// </summary>
        public virtual CreatureTraitsData TraitsSettings
        {
            get
            {
                if (EnableAggression)
                {
                    return new CreatureTraitsData(0.05f, 0.05f, 0.1f); //Aggressive fish are hungrier and less scared
                }
                else
                {
                    return new CreatureTraitsData(0.01f, 0.1f, 0.25f);
                }
            }
        }
        /// <summary>
        /// The type of sound effects this creature will use in the Inventory.
        /// </summary>
        public virtual ItemSoundsType ItemSounds
        {
            get
            {
                return ItemSoundsType.Fish;
            }
        }
        /// <summary>
        /// Settings related to how this creature is rendered.
        /// </summary>
        public virtual UBERMaterialProperties MaterialSettings
        {
            get
            {
                return new UBERMaterialProperties(8f, 1f);
            }
        }
        /// <summary>
        /// Settings related to how creatures respond to wandering out of their spawn zone.
        /// </summary>
        public virtual StayAtLeashData StayAtLeashSettings
        {
            get
            {
                return new StayAtLeashData();
            }
        }
        public virtual HeldFishData ViewModelSettings
        {
            get
            {
                return new HeldFishData();
            }
        }
        public virtual RespawnData RespawnSettings
        {
            get
            {
                return new RespawnData(true, 300f);
            }
        }
        /// <summary>
        /// Settings related to edibles.
        /// </summary>
        public virtual EatableData EatableSettings
        {
            get
            {
                return new EatableData();
            }
        }
        /// <summary>
        /// Used only for the 'speed' parameter on the Animator.
        /// </summary>
        public virtual float MaxVelocityForSpeedParameter
        {
            get
            {
                return SwimRandomSettings.SwimVelocity;
            }
        }
        /// <summary>
        /// Whether the creature can be picked up and held, or not. Pickupable fish can also be placed in the Small Aquarium if ViewModelSettings are set correctly.
        /// </summary>
        public virtual bool Pickupable
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Whether the creature is immune to brine or not.
        /// </summary>
        public virtual bool AcidImmune
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Possible sizes for this creature. Randomly picks a value in the range of 0 to 1. This value can not go above 1.
        /// </summary>
        public virtual AnimationCurve SizeDistribution
        {
            get
            {
                return ECCHelpers.Curve_Flat(1f);
            }
        }
        /// <summary>
        /// If set to true, the Scanner Room can scan for this creature.
        /// </summary>
        public virtual bool ScannerRoomScannable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Whether this creature can randomly spawn with Kharaa symptoms.
        /// </summary>
        public virtual bool CanBeInfected
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// By default, the creature does not roar.
        /// </summary>
        public virtual RoarAbilityData RoarAbilitySettings { get; }

        /// <summary>
        /// How aggressive the creature is to small vehicles. If set to zero (default value), it will not be aggressive to small vehicles.
        /// </summary>
        public virtual SmallVehicleAggressivenessSettings AggressivenessToSmallVehicles
        {
            get
            {
                return new SmallVehicleAggressivenessSettings(0f, 0f);
            }
        }
        /// <summary>
        /// Settings based around how this creature attacks its LastTarget.
        /// </summary>
        public virtual AttackLastTargetSettings AttackSettings
        {
            get
            {
                return new AttackLastTargetSettings(0f, 0f, 0f, 0f, 0f, 0f);
            }
        }

        /// <summary>
        /// Determines whether AttackSettings and AggressivenessToSmallVehicles actually apply.
        /// </summary>
        public virtual bool EnableAggression
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Settings for growth in Alien Containment.
        /// </summary>
        public virtual WaterParkCreatureParameters WaterParkParameters
        {
            get
            {
                return default;
            }
        }
        /// <summary>
        /// The mass of the Rigidbody.
        /// </summary>
        public virtual float Mass
        {
            get
            {
                return 1f;
            }
        }
        /// <summary>
        /// Gravity above water. A positive value portrays downward force while a negative value portrays upward force.
        /// </summary>
        public virtual float AboveWaterGravity
        {
            get
            {
                return 9.81f;
            }
        }
        /// <summary>
        /// Gravity below water. A positive value portrays downward force while a negative value portrays upward force.
        /// </summary>
        public virtual float UnderwaterGravity
        {
            get
            {
                return 0f;
            }
        }
        /// <summary>
        /// The SurfaceType to be applied to the main collider.
        /// </summary>
        public virtual VFXSurfaceTypes SurfaceType
        {
            get
            {
                return VFXSurfaceTypes.organic;
            }
        }

        /// <summary>
        /// For big creatures, you might want to increase the values.
        /// </summary>
        public virtual BehaviourLODLevelsStruct BehaviourLODSettings
        {
            get
            {
                return new BehaviourLODLevelsStruct(30f, 60f, 100f);
            }
        }

        /// <summary>
        /// Determines how fast the creature turns while swimming. One by default.
        /// </summary>
        public virtual float TurnSpeed
        {
            get
            {
                return 1f;
            }
        }

        /// <summary>
        /// The FOV used for detecting things, such as targets, on a scale from 0 to 1. Is 0.25 by default. A value of -1 means a given object is ALWAYS in view.
        /// </summary>
        public virtual float EyeFov
        {
            get
            {
                return 0.25f;
            }
        }

        /// <summary>
        /// Settings on how this creature will avoid terrain and/or obstacles.
        /// </summary>
        public virtual AvoidObstaclesData AvoidObstaclesSettings
        {
            get
            {
                return new AvoidObstaclesData(0f, false, 0f);
            }
        }

        /// <summary>
        /// Total power output of this creature.
        /// </summary>
        public virtual float BioReactorCharge
        {
            get
            {
                return 200f;
            }
        }

        /// <summary>
        /// Settings on shoaling behaviours.
        /// </summary>
        public virtual SwimInSchoolData SwimInSchoolSettings
        {
            get
            {
                return new SwimInSchoolData();
            }
        }

        #endregion

#region Unused

        /// <summary>
        /// The creature prefab used for reference. Easier than declaring every stat manually.
        /// </summary>
        [System.Obsolete("Doesn't do anything.")]
        public virtual TechType CreatureTraitsReference { get; }

#endregion

#region Ency Related Overridables
        /// <summary>
        /// The Title of the encyclopedia entry.
        /// </summary>
        public virtual string GetEncyTitle
        {
            get
            {
                return FriendlyName;
            }
        }

        /// <summary>
        /// The description of the encyclopedia entry.
        /// </summary>
        public virtual string GetEncyDesc
        {
            get
            {
                return "no description";
            }
        }

        /// <summary>
        /// Settings related to the encyclopedia entry.
        /// </summary>
        public virtual ScannableItemData ScannableSettings
        {
            get
            {
                return new ScannableItemData();
            }
        }
#endregion

#region Structs
        /// <summary>
        /// First person view model settings.
        /// </summary>
        public struct HeldFishData
        {
            public TechType ReferenceHoldingAnimation;
            public string WorldModelName;
            public string ViewModelName;
            /// <summary>
            /// First person view model settings.
            /// </summary>
            /// <param name="referenceHoldingAnimation">The TechType that is used to find the holding animation.</param>
            /// <param name="worldModelName">The name of the model used for the World View, which must be a child of the object.</param>
            /// <param name="viewModelName">The name of the model used for the First Person View, which must be a child of the object.</param>
            public HeldFishData(TechType referenceHoldingAnimation, string worldModelName, string viewModelName)
            {
                ReferenceHoldingAnimation = referenceHoldingAnimation;
                WorldModelName = worldModelName;
                ViewModelName = viewModelName;
            }
            /// <summary>
            /// First person view model settings.
            /// </summary>
            /// <param name="referenceHoldingAnimation">The TechType that is used to find the holding animation.</param>
            public HeldFishData(TechType referenceHoldingAnimation)
            {
                ReferenceHoldingAnimation = referenceHoldingAnimation;
                WorldModelName = null;
                ViewModelName = null;
            }
        }
        /// <summary>
        /// Settings related to respawning.
        /// </summary>
        public struct RespawnData
        {
            public bool CanRespawn;
            public float RespawnDelay;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="respawns">Whether this creature can respawn when killed or not.</param>
            /// <param name="respawnDelay">How long it takes for this creature to respawn, after death.</param>
            public RespawnData(bool respawns, float respawnDelay = 300f)
            {
                CanRespawn = respawns;
                RespawnDelay = respawnDelay;
            }
        }
        public struct BehaviourLODLevelsStruct
        {
            public float VeryClose;
            public float Close;
            public float Far;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="veryClose">Beyond this distance some animations may be removed.</param>
            /// <param name="close">Beyond this distance some functionalities may be less precise.</param>
            /// <param name="far">Beyond this distance trail animations will no longer exist.</param>
            public BehaviourLODLevelsStruct(float veryClose, float close, float far)
            {
                VeryClose = veryClose;
                Close = close;
                Far = far;
            }
        }

        public struct AttackLastTargetSettings
        {
            public float EvaluatePriority;
            public float ChargeVelocity;
            public float MinAttackLength;
            public float MaxAttackLength;
            public float AttackInterval;
            public float RememberTargetTime;

            public AttackLastTargetSettings(float actionPriority, float chargeVelocity, float minAttackLength, float maxAttackLength, float attackInterval, float rememberTargetTime)
            {
                EvaluatePriority = actionPriority;
                ChargeVelocity = chargeVelocity;
                MinAttackLength = minAttackLength;
                MaxAttackLength = maxAttackLength;
                AttackInterval = attackInterval;
                RememberTargetTime = rememberTargetTime;
            }
        }
        
        public struct AvoidObstaclesData
        {
            public bool terrainOnly;
            public float avoidDistance;
            public float evaluatePriority;

            public AvoidObstaclesData(float actionPriority, bool terrainOnly, float avoidDistance)
            {
                this.evaluatePriority = actionPriority;
                this.terrainOnly = terrainOnly;
                this.avoidDistance = avoidDistance;
            }
        }

        public struct SmallVehicleAggressivenessSettings
        {
            public float Aggression;
            public float MaxRange;

            public SmallVehicleAggressivenessSettings(float aggression, float maxRange)
            {
                Aggression = aggression;
                MaxRange = maxRange;
            }
        }
        public struct RoarAbilityData
        {
            public bool CanRoar;
            public float MinRoarDistance;
            public float MaxRoarDistance;
            /// <summary>
            /// All sounds starting with AudioClipPrefix in the asset bundle have a possibility to be played.
            /// </summary>
            public string AudioClipPrefix;
            public string AnimationName;
            /// <summary>
            /// The name of the Animator trigger parameter.
            /// </summary>
            public float MinTimeBetweenRoars;
            public float MaxTimeBetweenRoars;
            public float RoarActionPriority;
            public bool CreateCurrentOnRoar;
            public float CurrentStrength;

            public RoarAbilityData(bool canRoar, float roarSoundFalloffStart, float roarSoundMaxDistance, string audioClipPrefix, string animationName, float roarActionPriority = 0.5f, float minTimeBetweenRoars = 4f, float maxTimeBetweenRoars = 8f)
            {
                CanRoar = canRoar;
                MinRoarDistance = roarSoundFalloffStart;
                MaxRoarDistance = roarSoundMaxDistance;
                AudioClipPrefix = audioClipPrefix;
                AnimationName = animationName;
                MinTimeBetweenRoars = minTimeBetweenRoars;
                MaxTimeBetweenRoars = maxTimeBetweenRoars;
                RoarActionPriority = roarActionPriority;
                CreateCurrentOnRoar = false;
                CurrentStrength = 0f;
            }

            public RoarAbilityData(bool canRoar, float roarSoundFalloffStart, float roarSoundMaxDistance, string audioClipPrefix, string animationName, bool createCurrent, float currentStrength, float roarActionPriority = 0.5f, float minTimeBetweenRoars = 4f, float maxTimeBetweenRoars = 8f)
            {
                CanRoar = canRoar;
                MinRoarDistance = roarSoundFalloffStart;
                MaxRoarDistance = roarSoundMaxDistance;
                AudioClipPrefix = audioClipPrefix;
                AnimationName = animationName;
                MinTimeBetweenRoars = minTimeBetweenRoars;
                MaxTimeBetweenRoars = maxTimeBetweenRoars;
                RoarActionPriority = roarActionPriority;
                CreateCurrentOnRoar = createCurrent;
                CurrentStrength = currentStrength;
            }
        }

        public struct SwimRandomData
        {
            public bool SwimRandomly;
            public Vector3 SwimRadius;
            public float SwimVelocity;
            public float SwimInterval;
            public float EvaluatePriority;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="swimRandomly">Whether this action is added to the creature or not.</param>
            /// <param name="swimRadius">The distance this creature can wander in each direction.</param>
            /// <param name="swimVelocity">The speed at which this creature swims idly.</param>
            /// <param name="swimInterval">The time in seconds between each change in direction.</param>
            /// <param name="priority">The priority for this inidividual CreatureAction.</param>
            public SwimRandomData(bool swimRandomly, Vector3 swimRadius, float swimVelocity, float swimInterval, float priority)
            {
                SwimRandomly = swimRandomly;
                SwimRadius = swimRadius;
                SwimVelocity = swimVelocity;
                SwimInterval = swimInterval;
                EvaluatePriority = priority;
            }
        }
        /// <summary>
        /// Stores references to basic components of the creature. Do not rely on all of these to exist.
        /// </summary>
        public struct CreatureComponents
        {
            public PrefabIdentifier prefabIdentifier;
            public TechTag techTag;
            public LargeWorldEntity largeWorldEntity;
            public EntityTag entityTag;
            public SkyApplier skyApplier;
            public Renderer renderer;
            public EcoTarget ecoTarget;
            public VFXSurface vfxSurface;
            public BehaviourLOD behaviourLOD;
            public Rigidbody rigidbody;
            public LastScarePosition lastScarePosition;
            public WorldForces worldForces;
            public Creature creature;
            public LiveMixin liveMixin;
            public LastTarget_New lastTarget;
            public SwimBehaviour swimBehaviour;
            public Locomotion locomotion;
            public SplineFollowing splineFollowing;
            public SwimRandom swimRandom;
            public InfectedMixin infectedMixin;
            public Pickupable pickupable;
            public AnimateByVelocity animateByVelocity;
            public CreatureDeath creatureDeath;
        }
        public struct SwimInSchoolData
        {
            public float EvaluatePriority;
            public float SwimVelocity;
            public float SwimInterval;
            public float SchoolSize;
            public float BreakDistance;
            public float FindLeaderChance;
            public float LoseLeaderChance;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="evaluatePriority">The priority of this CreatureAction.</param>
            /// <param name="swimVelocity">The speed at which the creature moves while schooling.</param>
            /// <param name="swimInterval">The time between each "swim".</param>
            /// <param name="schoolSize">The max distance each fish can move from its leader.</param>
            /// <param name="breakDistance">The distance at which an individual fish breaks off from the school.</param>
            /// <param name="findLeaderChance">The chance, per attempt, for a school to be formed.</param>
            /// <param name="loseLeaderChance">The chance, each second, for a school to be broken.</param>
            public SwimInSchoolData(float evaluatePriority, float swimVelocity, float swimInterval, float schoolSize, float breakDistance, float findLeaderChance, float loseLeaderChance)
            {
                EvaluatePriority = evaluatePriority;
                SwimVelocity = swimVelocity;
                SwimInterval = swimInterval;
                SchoolSize = schoolSize;
                BreakDistance = breakDistance;
                FindLeaderChance = findLeaderChance;
                LoseLeaderChance = loseLeaderChance;
            }
        }
        /// <summary>
        /// Settings based around StayAtLeash.
        /// </summary>
        public struct StayAtLeashData
        {
            public float EvaluatePriority;
            public float MaxDistance;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="evaluatePriority">The priority of the action when applicable.</param>
            /// <param name="maxDistance">The distance at which the creature will attempt to perform this action.</param>
            public StayAtLeashData(float evaluatePriority, float maxDistance)
            {
                EvaluatePriority = evaluatePriority;
                MaxDistance = maxDistance;
            }
        }

        /// <summary>
        /// Basic settings related to creature "traits".
        /// </summary>
        public struct CreatureTraitsData
        {
            /// <summary>
            /// The rate at which the creature gets hungrier. Predators often require higher levels of hunger to attack.
            /// </summary>
            public float HungerIncreaseRate;
            /// <summary>
            /// The rate at which this creature becomes passive while actively hunting.
            /// </summary>
            public float AggressionDecreaseRate;
            /// <summary>
            /// The rate at which this creature becomes less scared. Used in very specific circumstances, most notably when taking damage.
            /// </summary>
            public float ScaredDecreaseRate;

            /// <summary>
            /// The rate of change for each 'trait' is measured per second.
            /// </summary>
            /// <param name="hungerIncreaseRate">The rate at which the creature gets hungrier. Predators often require higher levels of hunger to attack.</param>
            /// <param name="aggressionDecreaseRate">The rate at which this creature becomes passive while actively hunting.</param>
            /// <param name="scaredDecreaseRate">The rate at which this creature becomes less scared. Used in very specific circumstances, most notably when taking damage.</param>
            public CreatureTraitsData(float hungerIncreaseRate = 0f, float aggressionDecreaseRate = 0.05f, float scaredDecreaseRate = 0.25f)
            {
                HungerIncreaseRate = hungerIncreaseRate;
                AggressionDecreaseRate = aggressionDecreaseRate;
                ScaredDecreaseRate = scaredDecreaseRate;
            }
        }
#endregion
    }
}
