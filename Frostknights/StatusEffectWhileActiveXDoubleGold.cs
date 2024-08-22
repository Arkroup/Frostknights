using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostknights
{
    public class StatusEffectWhileActiveXDoubleGold : StatusEffectWhileActiveX
    {
        public int storedGold = 0;

        public override void Init()
        {
            Events.OnCollectGold += DoubleGold;
            base.Init();
        }

        public override bool RunBeginEvent()
        {
            Character player = References.Player;
            if ((bool)player && player.data != null && (bool)player.data.inventory)
            {
                storedGold = player.data.inventory.gold.Value;
            }
            return base.RunBeginEvent();
        }

        public void DoubleGold(int Golda)
        {
            Character player = References.Player;
            if ((bool)player && player.data != null && (bool)player.data.inventory)
            {
                int newGold = player.data.inventory.gold.Value;
                if (newGold > storedGold)
                {
                    int goldAdded = newGold - storedGold;
                    player.data.inventory.gold.Value += goldAdded;
                    storedGold = player.data.inventory.gold.Value;
                }
            }
            return;
        }

        public override void OnDestroy()
        {
            Events.OnCollectGold -= DoubleGold;
            base.OnDestroy();
        }
    }
}
