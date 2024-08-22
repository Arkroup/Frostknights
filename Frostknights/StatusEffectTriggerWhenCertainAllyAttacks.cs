using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostknights
{
    public class StatusEffectTriggerWhenCertainAllyAttacks : StatusEffectTriggerWhenAllyAttacks
    {
        //Cannot change allyInRow or againstTarget without some publicizing. Shade Snake is sad :(
        //If you have done the assembly stripping part, feel free to change those variables so that ShadeSnake can rise to its true potential.

        public CardData ally;                    //Declared when we make the instance of the class.

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.attacker?.name == ally.name) //Checks if the ally attacker is Shade Serpent.
            {
                return base.RunHitEvent(hit);    //Most of the actual logic is done through the StatusEffectTriggerWhenAllyAttacks class, which is called here.
            }
            return false;                        //Otherwise, don't attack.
        }
    }
}
