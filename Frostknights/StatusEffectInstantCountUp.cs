using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostknights
{
    public class StatusEffectInstantCountUp : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            Hit hit = new Hit(this.applier, this.target, 0)
            {
                countsAsHit = false,
                counterReduction = -this.GetAmount()
            };
            yield return hit.Process();
            yield return base.Process();
            yield break;
        }
    }
}
