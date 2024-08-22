using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Frostknights
{
    public class StatusEffectTriggerWhenAnythingAttacks : StatusEffectReaction
    {
        [SerializeField]
        public bool againstTarget;

        public CardData cardThatAttacks;

        public readonly HashSet<Entity> prime = new HashSet<Entity>();

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.attacker?.name == cardThatAttacks.name && target.enabled && hit.countsAsHit && hit.Offensive && (bool)hit.target && hit.trigger != null && CheckEntity(hit.attacker))
            {
                prime.Add(hit.attacker);
            }

            return false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (prime.Count > 0 && prime.Contains(entity) && targets != null && targets.Length > 0)
            {
                prime.Remove(entity);
                if (CanTrigger())
                {
                    Run(entity, targets);
                }
            }

            return false;
        }

        public void Run(Entity attacker, Entity[] targets)
        {
            if (againstTarget)
            {
                foreach (Entity entity in targets)
                {
                    ActionQueue.Stack(new ActionTriggerAgainst(target, attacker, entity, null), fixedPosition: true);
                }
            }
            else
            {
                ActionQueue.Stack(new ActionTrigger(target, attacker), fixedPosition: true);
            }
        }

        public bool CheckEntity(Entity entity)
        {
            if ((bool)entity && entity.owner.team == target.owner.team && entity != target && CheckDuplicate(entity))
            {
                return CheckDuplicate(entity.triggeredBy);
            }

            return false;
        }

        public bool CheckDuplicate(Entity entity)
        {
            if (!entity.IsAliveAndExists())
            {
                return true;
            }

            foreach (StatusEffectData statusEffect in entity.statusEffects)
            {
                if (statusEffect.name == base.name)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
