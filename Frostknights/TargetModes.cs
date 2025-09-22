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
    public class TargetModeSlash2 : TargetMode
    {
        public override bool TargetRow => false;

        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            TargetModeBasic targetModeBasic = new TargetModeBasic();
            Entity[] potentialTargets = targetModeBasic.GetPotentialTargets(entity, target, targetContainer);
            hashSet.AddRange(potentialTargets ?? Array.Empty<Entity>());
            if (hashSet.Count <= 0)
            {
                return null;
            }

            Entity adjacent = GetAdjacent(potentialTargets.First());
            if (adjacent != null)
            {
                potentialTargets = targetModeBasic.GetPotentialTargets(entity, adjacent, targetContainer);
                hashSet.AddRange(potentialTargets ?? Array.Empty<Entity>());
            }

            return (hashSet.Count <= 0) ? null : hashSet.ToArray();
        }

        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            return GetTargets(entity, target, targetContainer);
        }

        private static Entity GetAdjacent(Entity target)
        {
            CardContainer[] array = target.actualContainers.ToArray();
            foreach (CardContainer cardContainer in array)
            {
                if (!(cardContainer is CardSlot item) || !(cardContainer.Group is CardSlotLane cardSlotLane))
                {
                    continue;
                }

                int index = cardSlotLane.slots.IndexOf(item);
                foreach (CardContainer row in Battle.instance.GetRows(target.owner))
                {
                    Entity entity = row[index];
                    if (entity != null && entity != target)
                    {
                        return entity;
                    }
                }
            }

            return null;
        }
    }
    public class TargetModePierce : TargetModeBasic
    {
        public override Entity[] GetPotentialTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            Entity[] potentialTargets = base.GetPotentialTargets(entity, target, targetContainer);
            if (potentialTargets == null)
            {
                return null;
            }

            hashSet.AddRange(potentialTargets ?? Array.Empty<Entity>());
            IEnumerable<Entity> source = potentialTargets?.SelectMany((Entity target) => GetBehind(entity, target)) ?? Array.Empty<Entity>();
            if (source.Any())
            {
                foreach (Entity item in potentialTargets?.SelectMany((Entity target) => GetBehind(entity, target)))
                {
                    if (item != null)
                    {
                        hashSet.Add(item);
                    }
                }
            }

            return (hashSet.Count <= 0) ? null : hashSet.ToArray();
        }

        public override Entity[] GetSubsequentTargets(Entity entity, Entity target, CardContainer targetContainer)
        {
            return GetTargets(entity, target, targetContainer);
        }

        private static Entity[] GetBehind(Entity attacker, Entity target)
        {
            HashSet<Entity> hashSet = new HashSet<Entity>();
            CardContainer[] containers = attacker.containers;
            foreach (CardContainer cardContainer in containers)
            {
                if (Battle.IsOnBoard(attacker))
                {
                    CardSlotLane oppositeRow = Battle.instance.GetOppositeRow(cardContainer as CardSlotLane);
                    if (oppositeRow.Contains(target))
                    {
                        List<Entity> list = oppositeRow.ToArray().ToList();
                        if (!(list.First() != target))
                        {
                            int num = list.IndexOf(target) + 1;
                            if (num < list.Count)
                            {
                                hashSet.Add(list[num]);
                            }
                        }

                        continue;
                    }

                    CardSlotLane cardSlotLane = target.containers.OfType<CardSlotLane>().FirstOrDefault();
                    if (!(cardSlotLane == null))
                    {
                        List<Entity> list2 = cardSlotLane.ToArray().ToList();
                        int num2 = list2.IndexOf(target) + 1;
                        if (num2 < list2.Count)
                        {
                            hashSet.Add(list2[num2]);
                        }
                    }

                    continue;
                }

                CardSlotLane cardSlotLane2 = target.containers.OfType<CardSlotLane>().FirstOrDefault();
                if (!(cardSlotLane2 == null))
                {
                    List<Entity> list3 = cardSlotLane2.ToArray().ToList();
                    int num3 = list3.IndexOf(target) + 1;
                    if (num3 < list3.Count)
                    {
                        hashSet.Add(list3[num3]);
                    }
                }
            }

            return hashSet.ToArray();
        }
    }
}
