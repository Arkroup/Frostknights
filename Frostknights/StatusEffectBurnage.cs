using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WildfrostHopeMod.VFX;
using WildfrostHopeMod.SFX;
using static SfxSystem;


namespace Frostknights
{
    public class StatusEffectBurnage : StatusEffectData
    {
        public CardAnimation buildupAnimation;

        public bool burning;

        public bool subbed;

        public bool primed;

        public override void Init()
        {
            Events.OnEntityHit += EntityHit;
            base.OnTurnEnd += DealDamage2;
            Events.OnPostProcessUnits += Prime;
            subbed = true;
        }

        public void Unsub()
        {
            if (subbed)
            {
                Events.OnPostProcessUnits -= Prime;
                subbed = false;
            }
        }

        public void Prime(Character character)
        {
            primed = true;
            Unsub();
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (primed && target.enabled)
            {
                return entity == target;
            }

            return false;
        }

        public void OnDestroy()
        {
            Events.OnEntityHit -= EntityHit;
            Unsub();
        }

        public void EntityHit(Hit hit)
        {
            if (hit.target == target && hit.BasicHit && hit.Offensive && hit.canRetaliate)
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
            CardContainer[] array = containers;
            foreach (CardContainer collection in array)
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
                    screenShake = 0.5f,
                    damageType = "inferno"
                };
                clump.Add(hit.Process());
            }
            VFXHelper.VFX.TryPlayEffect("burninga", target.transform.position, 1f * target.transform.lossyScale);
            VFXHelper.SFX.TryPlaySound("burnagedmg");
            target.curveAnimator.Ping();
            yield return new WaitForSeconds(0f);
            yield return Sequences.Wait(0.2f);
            clump.Add(Sequences.Wait(0.5f));
            yield return clump.WaitForEnd();
        }

        public IEnumerator DealDamage2(Entity entity)
        {
            Hit hit = new Hit(GetDamager(), target, 1)
            {
                screenShake = 0.25f,
                damageType = "fire",
                countsAsHit = true,
                canRetaliate = false
            };
            VFXHelper.VFX.TryPlayEffect("bdmg", target.transform.position, 1f * target.transform.lossyScale);
            VFXHelper.SFX.TryPlaySound("burningdmg");
            yield return hit.Process();
            yield return Sequences.Wait(0.2f);
            int amount = 2;
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                yield return CountDown(entity, amount);
            }
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