using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ECCLibrary.Internal
{
    public class RoarAbility : MonoBehaviour
    {
        AudioSource source;
        Creature creature;
        public float minRoarDistance = 1f;
        public float maxRoarDistance = 50f;
        public string animationName;
        public string clipPrefix;
        public bool createCurrent;
        public float currentStrength;

        ECCAudio.AudioClipPool clipPool;

        void Start()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.minDistance = minRoarDistance;
            source.maxDistance = maxRoarDistance;
            source.spatialBlend = 1f;
            source.volume = ECCHelpers.GetECCVolume();

            clipPool = ECCAudio.CreateClipPool(clipPrefix);

            creature = GetComponent<Creature>();
        }

        public void PlayRoar()
        {
            if (!creature.liveMixin.IsAlive())
            {
                return;
            }
            creature.GetAnimator().SetTrigger(animationName);
            AudioClip clip = clipPool.GetRandomClip();
            source.PlayOneShot(clip);
            if (createCurrent)
            {
                WorldForces.AddCurrent(source.transform.position, DayNightCycle.main.timePassed, 40f, transform.forward, currentStrength, 5f);
            }
        }
    }
}
