﻿using UnityEngine;
using ProjectAncients.Prefabs.Modules;

namespace ProjectAncients.Mono.Modules
{
    public class ZapOnDamage : MonoBehaviour, IOnTakeDamage
    {
        public GameObject zapPrefab;
        public float cooldown = 7f;
        public float energyCost = 5f;
        private Vehicle vehicle;
        private LiveMixin myLiveMixin;
        private float timeCanZapAgain;

        void Start()
        {
            vehicle = GetComponent<Vehicle>();
            myLiveMixin = gameObject.GetComponent<LiveMixin>();
            myLiveMixin.damageReceivers = myLiveMixin.gameObject.GetComponents<IOnTakeDamage>();
        }

        public void Zap()
        {
            var obj = Object.Instantiate(zapPrefab);
            obj.name = "PrawnSuitZap";

            var fxElectSpheres = zapPrefab.GetComponent<ElectricalDefense>().fxElecSpheres;
            var defenseSound = zapPrefab.GetComponent<ElectricalDefense>().defenseSound;

            var ed = obj.GetComponent<ElectricalDefense>() ?? obj.GetComponentInParent<ElectricalDefense>();
            if (ed is not null)
            {
                Object.Destroy(ed);
            }

            var edMk2 = obj.EnsureComponent<ElectricalDefenseMK2>();
            if (edMk2 is not null)
            {
                edMk2.fxElectSpheres = fxElectSpheres;
                edMk2.defenseSound = defenseSound;
            }

            var electricalDefense = Utils.SpawnZeroedAt(obj, transform).GetComponent<ElectricalDefenseMK2>();

            if (electricalDefense is not null)
            {
                electricalDefense.charge = 1f;
                electricalDefense.chargeScalar = 1f;
            }
        }

        public void OnTakeDamage(DamageInfo damageInfo)
        {
            if (GetCanZap(damageInfo))
            {
                if (vehicle.ConsumeEnergy(energyCost))
                {
                    Zap();
                    timeCanZapAgain = Time.time + 5f;
                }
            }
        }

        public bool GetCanZap(DamageInfo damageInfo)
        {
            if(damageInfo == null)
            {
                //Why are we even checking??
                return false;
            }
            if (damageInfo.damage < 5f)
            {
                //Not enough damage, a waste of charge.
                return false;
            }
            if (Time.time < timeCanZapAgain)
            {
                //Still on cooldown.
                return false;
            }
            if (!vehicle.HasEnoughEnergy(energyCost + 5f))
            {
                //Not worth using energy at this point
                return false;
            }
            return true;
        }
    }
}
