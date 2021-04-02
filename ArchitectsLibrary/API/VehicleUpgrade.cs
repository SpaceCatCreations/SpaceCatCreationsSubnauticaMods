using System;
using System.Collections;
using ArchitectsLibrary.Handlers;
using ArchitectsLibrary.Interfaces;
using ArchitectsLibrary.Patches;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace ArchitectsLibrary.API
{
    /// <summary>
    /// an abstract class inheriting from <see cref="Craftable"/> that simplifies the process of making a Custom Seamoth Upgrade.
    /// </summary>
    public abstract class VehicleUpgrade : Craftable
    {
        internal Func<ModuleEquipmentType, EquipmentType> ParseAsEquipmentType =
            (x) => (EquipmentType)Enum.Parse(typeof(EquipmentType), x.ToString());
        
        /// <summary>
        /// Initializes a new <see cref="VehicleUpgrade"/>
        /// </summary>
        /// <param name="classId">The main internal identifier for this item. Your item's <see cref="TechType"/> will be created using this name.</param>
        /// <param name="friendlyName">The name displayed in-game for this item whether in the open world or in the inventory.</param>
        /// <param name="description">The description for this item, Typically seen in the PDA, Inventory, or crafting screens.</param>
        public VehicleUpgrade(string classId, string friendlyName, string description)
            : base(classId, friendlyName, description)
        {
            OnFinishedPatching += () =>
            {
                CraftDataHandler.SetEquipmentType(this.TechType, ParseAsEquipmentType(EquipmentType));
                CraftDataHandler.SetQuickSlotType(this.TechType, QuickSlotType);
                
                if (MaxCharge.HasValue)
                    VehicleHandler.MaxCharge(this.TechType, MaxCharge.Value);
                
                if (EnergyCost.HasValue)
                    VehicleHandler.EnergyCost(this.TechType, EnergyCost.Value);

                if (this is ISeaMothOnUse seaMothOnUse)
                {
                    SeaMothPatches.seaMothOnUses[this.TechType] = seaMothOnUse;
                }

                if (this is ISeaMothOnEquip seaMothOnEquip)
                {
                    SeaMothPatches.SeaMothOnEquips[this.TechType] = seaMothOnEquip;
                }

                if (this is ISeaMothOnToggle seaMothOnToggle)
                {
                    SeaMothPatches.SeaMothOnToggles[this.TechType] = seaMothOnToggle;
                }

                if (this is IExosuitOnEquip exosuitOnEquip)
                {
                    ExosuitPatches.ExosuitOnEquips[this.TechType] = exosuitOnEquip;
                }
            };
        }

        /// <summary>
        /// Gets the type of Vehicle this item can fit into.
        /// </summary>
        /// <value>
        /// The type of Vehicle that is compatible with this item.
        /// </value>
        public abstract ModuleEquipmentType EquipmentType { get; }

        /// <summary>
        /// Gets the type of equipment slot this item can fit into.
        /// </summary>
        /// <value>
        /// The type of the equipment slot compatible with this item.
        /// </value>
        public virtual QuickSlotType QuickSlotType => QuickSlotType.None;
        
        /// <summary>
        /// TechType that this module will instantiate its prefab and copy it as its own.
        /// </summary>
        public virtual TechType ModelTemplate => TechType.SeamothSolarCharge;
        
        /// <summary>
        /// Amount of Energy this module will cost on use
        /// </summary>
        public virtual float? EnergyCost { get; }
        
        /// <summary>
        /// <para>the total max charge that this module cant bypass when its charging up its shot.</para>
        /// 
        /// this Property is relavant only if your module's <see cref="QuickSlotType"/> is set as <see cref="QuickSlotType.Chargeable"/> or
        /// <see cref="QuickSlotType.SelectableChargeable"/>.
        /// </summary>
        public virtual float? MaxCharge { get; }

#if SN1
        public sealed override GameObject GetGameObject()
        {
            var prefab = CraftData.GetPrefabForTechType(ModelTemplate);
            var obj = GameObject.Instantiate(prefab);

            prefab.SetActive(false);
            obj.SetActive(true);

            return obj;
        }
#endif
        public sealed override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            if (ModelTemplate != TechType.None)
            {
                CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(ModelTemplate);
                yield return task;
                
                var prefab = task.GetResult();
                var obj = GameObject.Instantiate(prefab);
                
                prefab.SetActive(false);
                obj.SetActive(true);
                
                gameObject.Set(obj);
            }
        }

        public enum ModuleEquipmentType
        {
            SeamothModule,
            ExosuitModule,
            VehicleModule
        }
    }
}