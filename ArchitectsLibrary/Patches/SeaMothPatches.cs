using System.Collections.Generic;
using ArchitectsLibrary.Interfaces;
using ArchitectsLibrary.MonoBehaviours;
using HarmonyLib;
using UnityEngine;

namespace ArchitectsLibrary.Patches
{
    [HarmonyPatch(typeof(SeaMoth))]
    public class SeaMothPatches
    {
        internal static Dictionary<TechType, ISeaMothOnUse> seaMothOnUses = new();
        internal static Dictionary<TechType, ISeaMothOnEquip> SeaMothOnEquips = new();
        internal static Dictionary<TechType, ISeaMothOnToggle> SeaMothOnToggles = new();
        
        [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleUse))]
        [HarmonyPostfix]
        static void OnUpgradeModuleUse_Postfix(SeaMoth __instance, TechType techType, int slotID)
        {
            if (seaMothOnUses.TryGetValue(techType, out ISeaMothOnUse seaMothOnUse))
            {
                seaMothOnUse.OnUpgradeUse(slotID, __instance);
                
                __instance.quickSlotTimeUsed[slotID] = Time.time;
                __instance.quickSlotCooldown[slotID] = seaMothOnUse.Cooldown;
            }
        }

        [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleChange))]
        [HarmonyPostfix]
        static void OnUpgradeModuleChange(SeaMoth __instance, TechType techType, int slotID)
        {
            if (SeaMothOnEquips.TryGetValue(techType, out ISeaMothOnEquip seaMothOnEquip))
            {
                seaMothOnEquip.OnEquip(slotID, __instance);
            }
        }

        [HarmonyPatch(nameof(SeaMoth.OnUpgradeModuleToggle))]
        [HarmonyPrefix]
        static bool OnUpgradeModuleToggle_Prefix(SeaMoth __instance, int slotID, bool active)
        {
            var techType = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[slotID]);
            
            if (SeaMothOnToggles.TryGetValue(techType, out ISeaMothOnToggle seaMothOnToggle))
            {
                var onToggles = __instance.gameObject.GetAllComponentsInChildren<SeaMothOnToggle>();
                foreach (var toggle in onToggles)
                {
                    // if the component already exists on the seamoth, then skip adding it again
                    if (toggle.techType == techType)
                    {
                        toggle.enabled = active;
                        return false;
                    }
                }
                var onToggle = __instance.gameObject.AddComponent<SeaMothOnToggle>();
                onToggle.seaMothOnToggle = seaMothOnToggle;
                onToggle.seaMoth = __instance;
                onToggle.slotID = slotID;
                onToggle.techType = techType;
                onToggle.enabled = active;
                
                return false;
            }

            return true;
        }
    }
}