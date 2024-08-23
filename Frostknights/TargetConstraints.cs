using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Frostknights
{
    public class TargetConstraintHasStatusClassButtonCooldown : TargetConstraint
    {
        public Type statusClass;

        public override bool Check(CardData targetData)
        {
            foreach (CardData.StatusEffectStacks status in targetData.startWithEffects)
            {
                if (status.data is ButtonCooldown button)
                {
                    return !not;
                }
            }
            return not;
        }
        public override bool Check(Entity target)
        {
            return Check(target.data);
        }
    }

    public class TargetConstraintMaxCooldownMoreThan : TargetConstraint
    {
        [SerializeField]
        public int value;

        public override bool Check(CardData targetData)
        {
            foreach (CardData.StatusEffectStacks status in targetData.startWithEffects)
            {
                if (status.data is ButtonCooldown button)
                {
                    if(button.maxCooldown <= value)
                    {
                        return not;
                    }
                }
            }
            return !not;
        }
        public override bool Check(Entity target)
        {
            return Check(target.data);
        }
    }

    public class TargetConstraintInitialCooldownMoreThan : TargetConstraint
    {
        [SerializeField]
        public int value;

        public override bool Check(CardData targetData)
        {
            foreach (CardData.StatusEffectStacks status in targetData.startWithEffects)
            {
                if (status.data is ButtonCooldown button)
                {
                    if (button.cooldownCount <= value)
                    {
                        return not;
                    }
                }
            }
            return !not;
        }
        public override bool Check(Entity target)
        {
            return Check(target.data);
        }
    }
}
