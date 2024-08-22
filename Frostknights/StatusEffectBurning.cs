using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostknights
{
    public class StatusEffectBurning : StatusEffectData
    {
        public CardAnimation buildupAnimation;

        public bool burning;

        public override void Init()
        {
            Events.OnEntityHit += EntityHit;
        }

        public void OnDestroy()
        {
            Events.OnEntityHit -= EntityHit;
        }

        public void EntityHit(Hit hit)
        {
            if (hit.target == target && hit.Offensive && hit.canRetaliate)
            {
                Check();
            }
        }

        public void Check()
        {
            if (count > 0 && !burning)
            {
                ActionQueue.Stack(new ActionSequence(DealDamage())
                {
                    fixedPosition = true,
                    priority = eventPriority,
                    note = "Burned"
                });
                ActionQueue.Stack(new ActionSequence(Clear())
                {
                    fixedPosition = true,
                    priority = eventPriority,
                    note = "Clear Burns"
                });
                burning = true;
            }
        }

        public IEnumerator DealDamage()
        {
            if (!this || !target || !target.alive)
            {
                yield break;
            }

            HashSet<Entity> targets = new HashSet<Entity>();
            CardContainer[] containers = target.containers;
            foreach (CardContainer collection in containers)
            {
                targets.AddRange(collection);
            }

            if ((bool)buildupAnimation)
            {
                yield return buildupAnimation.Routine(target);
            }

            Entity damager = GetDamager();
            Routine.Clump clump = new Routine.Clump();
            foreach (Entity item in targets)
            {
                Hit hit = new Hit(damager, item, count)
                {
                    damageType = "burning"
                };
                clump.Add(hit.Process());
            }
            SfxSystem.OneShot("event:/sfx/status/overburn_damage");
            clump.Add(Sequences.Wait(0.5f));
            yield return clump.WaitForEnd();
        }

        public IEnumerator Clear()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                yield return Remove();
                burning = false;
            }
        }
    }
}
