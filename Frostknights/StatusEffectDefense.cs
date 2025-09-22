using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.VFX;
using static SfxSystem;


namespace Frostknights
{
    public class StatusEffectDefense : StatusEffectData
    {
        public override void Init()
        {
            base.OnHit += Check;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.target == target)
            {
                return hit.damage > 0;
            }

            return false;
        }

        public IEnumerator Check(Hit hit)
        {
            hit.damage = hit.damage - count;
            target.PromptUpdate();
            yield break;
        }
    }
}