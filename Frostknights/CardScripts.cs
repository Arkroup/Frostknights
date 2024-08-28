using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostknights
{
    public class CardScriptReduceInitialCooldownBy4 : CardScript
    {
        public override void Run(CardData target)
        {
            foreach (CardData.StatusEffectStacks status in target.startWithEffects)
            {
                if (status.data is ButtonCooldown button)
                {
                    button.cooldownCount -= 4;
                }
            }
        }
    }

    public class CardScriptReduceInitialCooldownBy2 : CardScript
    {
        public override void Run(CardData target)
        {
            foreach (CardData.StatusEffectStacks status in target.startWithEffects)
            {
                if (status.data is ButtonCooldown button)
                {
                    button.cooldownCount -= 2;
                }
            }
        }
    }

    public class CardScriptReduceMaxCooldown : CardScript
    {
        public override void Run(CardData target)
        {
            foreach (CardData.StatusEffectStacks status in target.startWithEffects)
            {
                if (status.data is ButtonCooldown button)
                {
                    button.maxCooldown -= 2;
                }
            }
        }
    }
}
