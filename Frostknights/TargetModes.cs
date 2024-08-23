using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostknights
{
    public class TargetModeTaunt : TargetMode
    {
        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            hashSet.AddRange(from e in entity.GetAllEnemies()
                             where (bool)e && e.enabled && e.alive && e.canBeHit && HasTaunt(e)
                             select e);
            if (hashSet.Count <= 0)
            {
                TargetModeBasic targetModeBasic = new TargetModeBasic();
                return targetModeBasic.GetPotentialTargets(entity, target, targetContainer);
            }

            return hashSet.ToArray();
        }

        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            return GetTargets(entity, target, targetContainer);
        }

        public bool HasTaunt(Entity entity)
        {
            foreach (CardData.TraitStacks t in entity.data.traits)
            {
                if (t.data.name == "artemys.wildfrost.frostknights.Taunt")
                {
                    return true;
                }
            }
            return false;
        }

    }

    public class TargetModeProvoke : TargetMode
    {
        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            hashSet.AddRange(from e in entity.GetAllEnemies()
                             where (bool)e && e.enabled && e.alive && e.canBeHit && HasProvoke(e)
                             select e);
            if (hashSet.Count <= 0)
            {
                TargetModeBasic targetModeBasic = new TargetModeBasic();
                return targetModeBasic.GetPotentialTargets(entity, target, targetContainer);
            }

            return hashSet.ToArray();
        }

        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            return GetTargets(entity, target, targetContainer);
        }

        public bool HasProvoke(Entity entity)
        {
            foreach (Entity.TraitStacks t in entity.traits)
            {
                if (t.data.name == "artemys.wildfrost.frostknights.Provoke")
                {
                    return true;
                }
            }
            return false;
        }

    }
    public class TargetModeTrulyRandom : TargetModeAll
    {
        public override bool Random => true;
        public override Entity[] GetTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            Entity random = base.GetTargets(entity, target, targetContainer)?.RandomItem();
            return random ? new Entity[] { random } : null;
        }
        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            Entity random = base.GetSubsequentTargets(entity, target, targetContainer)?.RandomItem();
            return random ? new Entity[] { random } : null;
        }
    }
}
