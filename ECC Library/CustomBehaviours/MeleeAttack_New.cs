using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace ECCLibrary.Internal
{
    public class MeleeAttack_New : MeleeAttack
    {
        public OnTouch onTouch;
        public string biteSoundPrefix;
        ECCAudio.AudioClipPool clipPool;
        AudioSource source;
        public float consumeWholeHealthThreshold;
        public bool regurgitate;

        void Start()
        {
            onTouch.onTouch = new OnTouch.OnTouchEvent();
            onTouch.onTouch.AddListener(OnTouch);
            clipPool = ECCAudio.CreateClipPool(biteSoundPrefix);
            source = gameObject.AddComponent<AudioSource>();
            source.maxDistance = 10f;
            source.spatialBlend = 1f;
            source.volume = ECCHelpers.GetECCVolume();
        }
        public override void OnTouch(Collider collider)
        {
            if (clipPool.clips != null && clipPool.clips.Length > 0 && CanBite(collider.gameObject))
            {
                source.PlayOneShot(clipPool.GetRandomClip());
                LiveMixin lm = collider.gameObject.GetComponent<LiveMixin>();
                if (lm != null && lm.gameObject != null)
                {
                    if (lm.maxHealth < consumeWholeHealthThreshold && !lm.destroyOnDeath)
                    {
                        if(collider.gameObject.GetComponent<Player>() == null)
                        {
                            var suckIn = collider.gameObject.AddComponent<BeingSuckedInWhole>();
                            suckIn.target = mouth.transform;
                            suckIn.animationLength = 0.5f;
                            Destroy(lm.gameObject, 0.5f);
                            if (regurgitate)
                            {
                                StartCoroutine(Regurgitate(CraftData.GetTechType(collider.gameObject), Random.Range(5f, 8f)));
                            }
                        }
                    }
                }
            }
            base.OnTouch(collider);
        }

        public override bool CanBite(GameObject target)
        {
            if (target == gameObject || Time.time < timeLastBite + biteInterval)
            {
                return false;
            }
            LiveMixin lm = target.GetComponent<LiveMixin>();
            if (lm == null)
            {
                return false;
            }
            if (!lm.IsAlive())
            {
                return false;
            }
            return true;
        }

        IEnumerator Regurgitate(TechType techType, float delay)
        {
            yield return new WaitForSeconds(delay);
            GameObject regurgitation = CraftData.InstantiateFromPrefab(techType);
            regurgitation.transform.position = mouth.transform.position;
            LiveMixin lm = regurgitation.GetComponent<LiveMixin>();
            if (lm)
            {
                lm.Kill(DamageType.Acid);
            }
            Rigidbody rb = regurgitation.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddForce(transform.forward * 15f, ForceMode.VelocityChange);
            }
        }
    }
}
