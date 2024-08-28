using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostknights
{
    public class StatusEffectUntilTurnEnd : StatusEffectInstant
    {
        public override void Init()
        {
            base.OnCardMove += CheckPosition;
            Events.OnBattleTurnEnd += Remove;
        }

        public void OnDestroy()
        {
            Events.OnBattleTurnEnd -= Remove;
        }

        public IEnumerator CheckPosition(Entity entity)
        {
            if (entity == target && (target.containers.Contains(References.Player.drawContainer) || target.containers.Contains(References.Player.discardContainer)))
            {
                yield return Remove();
            }
            yield break;
        }

        public virtual void Remove(int _)
        {
            target.StartCoroutine(Remove());
        }

        public override IEnumerator Process()
        {
            yield break;
        }

    }

    public class StatusEffectApplyXUntilTurnEnd : StatusEffectApplyXInstant
    {
        public override bool HasStackRoutine => true;
        public override bool HasEndRoutine => true;
        public override void Init()
        {
            applyToFlags = ApplyToFlags.Self; //DO NOT CHANGE THE APPLYTOFLAGS

            base.OnCardMove += CheckPosition;
            Events.OnBattleTurnEnd += Remove;
        }

        public void OnDestroy()
        {
            Events.OnBattleTurnEnd -= Remove;
        }

        public override IEnumerator StackRoutine(int stacks)
        {
            yield return Run(GetTargets());
        }

        public IEnumerator CheckPosition(Entity entity)
        {
            if (entity == target && (target.containers.Contains(References.Player.drawContainer) || target.containers.Contains(References.Player.discardContainer)))
            {
                yield return Remove();
            }
            yield break;
        }

        public virtual void Remove(int _)
        {
            target.StartCoroutine(Remove());
        }

        public override IEnumerator EndRoutine()
        {
            if ((bool)target)
            {
                StatusEffectData effect = target.statusEffects.FirstOrDefault((e) => e.name == effectToApply.name);
                if (effect != default(StatusEffectData))
                {
                    yield return effect.RemoveStacks(count, true);
                    target.display.promptUpdateDescription = true;
                    target.PromptUpdate();
                }
            }
        }
    }

    public class StatusEffectTraitUntilTurnEnd : StatusEffectUntilTurnEnd
    {
        public TraitData trait;

        public Entity.TraitStacks added;

        public int addedAmount = 0;

        public override bool HasStackRoutine => true;

        public override bool HasEndRoutine => true;

        public override void Init()
        {
            base.Init();
        }

        public override IEnumerator BeginRoutine()
        {
            added = target.GainTrait(trait, count, temporary: true);
            yield return target.UpdateTraits();
            addedAmount += count;
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
        }

        public override IEnumerator StackRoutine(int stacks)
        {
            added = target.GainTrait(trait, stacks, temporary: true);
            yield return target.UpdateTraits();
            addedAmount += stacks;
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
        }

        public override IEnumerator EndRoutine()
        {
            if ((bool)target)
            {
                if (added != null)
                {
                    added.count -= addedAmount;
                    added.tempCount -= addedAmount;
                }

                addedAmount = 0;
                yield return target.UpdateTraits(added);
                target.display.promptUpdateDescription = true;
                target.PromptUpdate();
            }
        }
    }

    public class StatusEffectBoostUntilTurnEnd : StatusEffectUntilTurnEnd
    {
        public override void Init()
        {
            base.Init();
        }
        public override IEnumerator Process()
        {
            int amount = GetAmount();
            if ((bool)target.curveAnimator)
            {
                target.curveAnimator.Ping();
            }

            target.effectBonus += amount;
            target.PromptUpdate();


            return base.Process();
        }

        public override bool RunStackEvent(int stacks)
        {
            int amount = GetAmount();
            if ((bool)target.curveAnimator)
            {
                target.curveAnimator.Ping();
            }

            target.effectBonus += stacks;
            target.PromptUpdate();
            return base.RunStackEvent(stacks);
        }

        public override void Remove(int _)
        {
            target.effectBonus -= GetAmount();
            base.Remove(_);
        }
    }
}
