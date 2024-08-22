using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

namespace Frostknights
{
    public class StatusEffectInstantReplaceEffects : StatusEffectInstant
    {
        [SerializeField]
        public bool replaceAllEffects = true;

        [SerializeField]
        [HideIf("replaceAllEffects")]
        public int replaceEffectNumber;
        public List<CardData.StatusEffectStacks> replaceEffectNames;
        public List<CardData.TraitStacks> replaceTraitNames;
        public List<CardData.StatusEffectStacks> replaceAttackEffectNames;

        public override IEnumerator Process()
        {
            yield return replaceAllEffects ? RemoveAllEffects() : RemoveEffect(replaceEffectNumber);
            yield return ReplaceEffects();
            if (target.display is Card card)
            {
                card.promptUpdateDescription = true;
            }
            target.PromptUpdate();
            yield return base.Process();
        }

        public IEnumerator RemoveAllEffects()
        {
            foreach (Entity.TraitStacks trait in target.traits)
            {
                trait.count = 0;
            }
            yield return target.UpdateTraits();
            Routine.Clump clump = new Routine.Clump();
            foreach (StatusEffectData item in target.statusEffects)
            {
                clump.Add(item.Remove());
            }
            yield return clump.WaitForEnd();
        }

        public IEnumerator RemoveEffect(int effectNumber)
        {
            StatusEffectData statusEffectData = target.statusEffects[effectNumber];
            yield return statusEffectData.Remove();
        }

        public IEnumerator ReplaceEffects()
        {
            foreach (CardData.TraitStacks trait in replaceTraitNames)
            {
                int num = trait.count;
                if (num > 0)
                {
                    applier.GainTrait(trait.data, num);
                }
            }
            foreach (CardData.StatusEffectStacks item in replaceEffectNames)
            {
                yield return StatusEffectSystem.Apply(target, target, item.data, item.count);
            }
            target.attackEffects = (from a in CardData.StatusEffectStacks.Stack(target.attackEffects, replaceAttackEffectNames)
                                    select a.Clone()).ToList();
            yield return target.UpdateTraits();
        }
    }
}
