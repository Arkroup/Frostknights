using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Frostknights
{
    public class StatusEffectDoubleButtonCooldown : StatusEffectData
    {
        [SerializeField]
        public bool ignoreSilence = true;

        public override void Init()
        {
            Events.OnStatusEffectCountDown += CooldownCountDouble;
        }

        public void OnDestroy()
        {
            Events.OnStatusEffectCountDown -= CooldownCountDouble;
        }

        public void CooldownCountDouble(StatusEffectData status, ref int amount)
        {
            if (status is ButtonCooldown && !Silenced() && Battle.IsOnBoard(target) && status.target == target)
            {
                amount += GetAmount();
            }
        }

        public bool Silenced()
        {
            if (target.silenced)
            {
                return !ignoreSilence;
            }
            return false;
        }
    }
}
