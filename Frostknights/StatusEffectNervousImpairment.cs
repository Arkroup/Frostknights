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
    public class StatusEffectNervousImpairment : StatusEffectData
    {
        [SerializeField]
        public CardAnimation buildupAnimation;

        public bool braining;

        public override void Init()
        {
            base.OnStack += Stack;
            Events.OnEntityDisplayUpdated += EntityDisplayUpdated;
        }

        public void OnDestroy()
        {
            Events.OnEntityDisplayUpdated -= EntityDisplayUpdated;
        }

        public void EntityDisplayUpdated(Entity entity)
        {
            if (entity == target && target.enabled)
            {
                Check();
            }
        }

        public IEnumerator Stack(int stacks)
        {
            Check();
            yield return null;
        }

        public void Check()
        {
            if (count >= 3 && !braining)
            {
                ActionQueue.Stack(new ActionSequence(DealDamage())
                {
                    fixedPosition = true,
                    priority = eventPriority,
                    note = "NervousImpairment"
                });
                ActionQueue.Stack(new ActionSequence(Clear())
                {
                    fixedPosition = true,
                    priority = eventPriority,
                    note = "Clear NervousImpairment"
                });
                braining = true;
            }
        }

        public IEnumerator DealDamage()
        {
            if (!this || !target || !target.alive)
            {
                yield break;
            }

            if ((bool)buildupAnimation)
            {
                yield return buildupAnimation.Routine(target);
            }

            Entity damager = GetDamager();

            Hit hit = new Hit(damager, target, 5)
            {
                canRetaliate = false
            };
            yield return hit.Process();

            if (target && target.alive)
            {
                StatusEffectData snowEffect = AddressableLoader.Get<StatusEffectData>("StatusEffectData", "Snow");
                yield return StatusEffectSystem.Apply(target, damager, snowEffect, 1);
            }

            VFXHelper.VFX.TryPlayEffect("nervousimpairmentdmg", target.transform.position, 1f * target.transform.lossyScale);
            VFXHelper.SFX.TryPlaySound("nervousimpairmentdmg");

            Routine.Clump clump = new Routine.Clump();
            clump.Add(Sequences.Wait(0.5f));
            yield return clump.WaitForEnd();
        }

        public IEnumerator Clear()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                yield return Remove();
                braining = false;
            }
        }
    }
}



