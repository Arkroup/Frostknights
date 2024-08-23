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

        public override bool Check(Entity target)
        {
            foreach (StatusEffectData status in target.statusEffects)
            {
                if (status is ButtonCooldown button)
                {
                    return !not;
                }
            }
            return not;
        }
    }

    public class TargetConstraintMaxCooldownMoreThan : TargetConstraint
    {
        [SerializeField]
        public int value;

        public override bool Check(Entity target)
        {
            foreach (StatusEffectData status in target.statusEffects)
            {
                if (status is ButtonCooldown button)
                {
                    if(button.maxCooldown <= value)
                    {
                        return not;
                    }
                }
            }
            return !not;
        }
    }

    public class TargetConstraintInitialCooldownMoreThan : TargetConstraint
    {
        [SerializeField]
        public int value;

        public override bool Check(Entity target)
        {
            foreach (StatusEffectData status in target.statusEffects)
            {
                if (status is ButtonCooldown button)
                {
                    if (button.cooldownCount <= value)
                    {
                        return not;
                    }
                }
            }
            return !not;
        }
    }
}
