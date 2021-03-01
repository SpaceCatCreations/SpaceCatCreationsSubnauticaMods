using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ECCLibrary.Internal
{
    public class RoarRandomAction : CreatureAction
    {
        public float roarIntervalMin = 4f;
        public float roarIntervalMax = 8f;
        float timeNextRoar;
        RoarAbility roarAction;

        void Start()
        {
            timeNextRoar = Time.time + Random.Range(roarIntervalMin, roarIntervalMax);
            roarAction = GetComponent<RoarAbility>();
        }
        public override float Evaluate(Creature creature)
        {
            if(Time.time > timeNextRoar)
            {
                return evaluatePriority;
            }
            return 0f;
        }
        public override void StartPerform(Creature creature)
        {
            if(roarAction == null) roarAction = GetComponent<RoarAbility>();
            timeNextRoar = Time.time + Random.Range(roarIntervalMin, roarIntervalMax);
            roarAction.PlayRoar();
        }
    }
}
