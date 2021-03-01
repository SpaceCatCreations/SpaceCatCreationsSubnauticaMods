using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ECCLibrary.Internal
{
    public class BeingSuckedInWhole : MonoBehaviour
    {
        public Transform target;
        public float animationLength;
        float timeStarted;
        float timeFinished;
        Vector3 defaultScale;
        Vector3 defaultPosition;

        void Start()
        {
            timeStarted = Time.time;
            timeFinished = Time.time + animationLength;
            defaultScale = transform.localScale;
            defaultPosition = transform.position;
            foreach(Collider collider in GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
            }
            Creature creature = GetComponent<Creature>();
            if(creature != null)
            {
                ECCHelpers.SetPrivateField(typeof(Creature), creature, "flinch", 50f);
            }
        }

        void Update()
        {
            if(target != null)
            {
                float animationProgress = Mathf.InverseLerp(timeStarted, timeFinished, Time.time);
                transform.localScale = Vector3.Scale(defaultScale, Vector3.one - (Vector3.one * animationProgress));
                transform.position = Vector3.Lerp(defaultPosition, target.transform.position, animationProgress);
            }
        }
    }
}
