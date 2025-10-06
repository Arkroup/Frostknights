using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Frostknights
{
    internal class StatusEffectInstantCombineCard : StatusEffectInstant
    {

        [Serializable]
        public struct Combo
        {
            public string[] cardNames;

            public string resultingCardName;

            public bool AllCardsInDeck(List<Entity> deck)
            {
                bool result = true;
                string[] array = cardNames;
                foreach (string cardName in array)
                {
                    if (!HasCard(cardName, deck))
                    {
                        result = false;
                        break;
                    }
                }
                return result;
            }

            public List<Entity> FindCards(List<Entity> deck)
            {
                List<Entity> tooFuse = new List<Entity>();
                string[] array = cardNames;
                foreach (string cardName in array)
                {
                    foreach (Entity item in deck)
                    {
                        if (item.data.name == cardName)
                        {
                            tooFuse.Add(item);
                            break;
                        }
                    }
                }

                return tooFuse;
            }

            public bool HasCard(string cardName, List<Entity> deck)
            {
                foreach (Entity item in deck)
                {
                    if (item.data.name == cardName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [SerializeField]
        public string combineSceneName = "CardCombine";

        public string[] cardNames;

        public string resultingCardName;

        public bool checkHand = true;
        public bool checkDeck = true;
        public bool checkBoard = true;

        public bool keepUpgrades = true;
        public List<CardUpgradeData> extraUpgrades;

        public bool keepHealth = false;

        public bool spawnOnBoard = false;

        public bool changeDeck = false;

        public override IEnumerator Process()
        {
            Combo combo = new Combo()
            {
                cardNames = cardNames,
                resultingCardName = resultingCardName
            };

            List<Entity> fulldeck = new List<Entity>();
            if (checkHand)
            {
                fulldeck.AddRange(References.Player.handContainer.ToList());
            }
            if (checkDeck)
            {
                fulldeck.AddRange(References.Player.drawContainer.ToList());
                fulldeck.AddRange(References.Player.discardContainer.ToList());
            }
            if (checkBoard)
            {
                fulldeck.AddRange(Battle.GetCardsOnBoard(References.Player).ToList());
            }


            if (combo.AllCardsInDeck(fulldeck))
            {
                CombineAction action = new CombineAction(keepUpgrades, extraUpgrades, keepHealth, spawnOnBoard, target.containers[0]);
                action.combineSceneName = combineSceneName;
                action.tooFuse = combo.FindCards(fulldeck);
                action.combo = combo;

                if (changeDeck)
                {
                    EditDeck(combo.cardNames, combo.resultingCardName);
                }

                bool queueAction = true;
                foreach (PlayAction playAction in ActionQueue.instance.queue)
                {
                    if (playAction.GetType() == action.GetType())
                    {
                        queueAction = false;
                        break;
                    }
                }

                if (queueAction)
                {
                    ActionQueue.Add(action);
                }

            }

            yield return base.Process();
        }

        public void EditDeck(string[] cardsToRemove, string cardToAdd)
        {
            List<CardData> oldCards = new List<CardData>();

            foreach (string name in cardsToRemove)
            {
                foreach (CardData card in References.Player.data.inventory.deck)
                {
                    if (card.name == name && !oldCards.Contains(card))
                    {
                        oldCards.Add(card);
                        break;
                    }
                }
            }

            if (oldCards.Count == cardsToRemove.Length)
            {
                List<CardUpgradeData> upgrades = new List<CardUpgradeData> { };

                foreach (CardData card in oldCards)
                {
                    if (keepUpgrades)
                    {
                        upgrades.AddRange(card.upgrades.Select(u => u.Clone()));
                    }

                    References.Player.data.inventory.deck.Remove(card);
                }

                CardData cardDataClone = AddressableLoader.GetCardDataClone(cardToAdd);

                upgrades.AddRange(extraUpgrades.Select(u => u.Clone()));

                foreach (CardUpgradeData upgrade in upgrades)
                {
                    upgrade.Assign(cardDataClone);
                }


                if (cardDataClone.cardType.miniboss)
                {
                    References.Player.data.inventory.deck.Insert(0, cardDataClone);
                }

                else
                {
                    References.Player.data.inventory.deck.Add(cardDataClone);
                }


            }


        }

        public class CombineAction : PlayAction
        {

            [SerializeField]
            public string combineSceneName;

            public Combo combo;

            public List<Entity> tooFuse;

            public bool keepUpgrades;

            public List<CardUpgradeData> extraUpgrades;

            public bool keepHealth;

            public bool spawnOnBoard;

            public CardContainer row;

            public CombineAction(bool keepUpgrades, List<CardUpgradeData> extraUpgrades, bool keepHealth, bool spawnOnBoard, CardContainer row)
            {
                this.keepUpgrades = keepUpgrades;
                this.extraUpgrades = extraUpgrades;
                this.keepHealth = keepHealth;
                this.spawnOnBoard = spawnOnBoard;
                this.row = row;


            }

            public override IEnumerator Run()
            {
                return CombineSequence(combo, tooFuse);
            }

            public IEnumerator CombineSequence(Combo combo, List<Entity> tooFuse)
            {
                CombineCardSequence combineSequence = null;
                yield return SceneManager.Load(combineSceneName, SceneType.Temporary, delegate (Scene scene)
                {
                    combineSequence = scene.FindObjectOfType<CombineCardSequence>();
                });
                if ((bool)combineSequence)
                {
                    yield return combineSequence.Run2(tooFuse, combo.resultingCardName, keepUpgrades, extraUpgrades, keepHealth, spawnOnBoard, row);
                }

                yield return SceneManager.Unload(combineSceneName);
            }

        }

    }

    public static class CombineCardSequenceExtension
    {
        public static IEnumerator Run2(this CombineCardSequence seq, List<Entity> cardsToCombine, string resultingCard, bool keepUpgrades, List<CardUpgradeData> extraUpgrades, bool keepHealth, bool spawnOnBoard, CardContainer row)
        {
            CardData cardDataClone = AddressableLoader.GetCardDataClone(resultingCard);

            List<CardUpgradeData> upgrades = new List<CardUpgradeData> { };
            if (keepUpgrades)
            {
                foreach (Entity ent in cardsToCombine)
                {
                    upgrades.AddRange(ent.data.upgrades.Select(u => u.Clone()));
                }
            }
            upgrades.AddRange(extraUpgrades.Select(u => u.Clone()));

            foreach (CardUpgradeData upgrade in upgrades)
            {
                upgrade.Assign(cardDataClone);
            }


            yield return Run2(seq, cardsToCombine.ToArray(), cardDataClone, spawnOnBoard, row, keepHealth);
        }

        public static IEnumerator Run2(this CombineCardSequence seq, Entity[] entities, CardData finalCard, bool spawnOnBoard, CardContainer row, bool keepHealth)
        {

            PauseMenu.Block();
            Card card = CardManager.Get(finalCard, Battle.instance.playerCardController, References.Player, inPlay: false, isPlayerCard: true);
            card.transform.localScale = Vector3.one * 1f;
            card.transform.SetParent(seq.finalEntityParent);
            Entity finalEntity = card.entity;
            Routine.Clump clump = new Routine.Clump();


            Entity[] array = entities;
            foreach (Entity entity in array)
            {
                //clump.Add(entity.display.UpdateData());
                foreach (StatusEffectData effect in entity.statusEffects)
                {
                    if (effect is StatusEffectWhileActiveX activeEffect)
                    {
                        if (activeEffect.active)
                        {
                            yield return activeEffect.Deactivate();
                        }
                    }
                }


            }

            clump.Add(finalEntity.display.UpdateData());

            clump.Add(Sequences.Wait(0.5f));
            yield return clump.WaitForEnd();

            if (keepHealth)
            {
                foreach (Entity ent in entities)
                {
                    if (ent.hp.current > 0)
                    {
                        finalEntity.hp = ent.hp;
                        break;
                    }
                }

                clump.Add(finalEntity.display.UpdateDisplay());

            }

            array = entities;
            foreach (Entity entity2 in array)
            {
                entity2.RemoveFromContainers();
            }

            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].transform.localScale = Vector3.one * 0.8f;
            }

            seq.fader.In();
            Vector3 zero = Vector3.zero;
            array = entities;
            foreach (Entity entity3 in array)
            {
                zero += entity3.transform.position;
            }

            zero /= (float)entities.Length;

            seq.group.position = zero;
            array = entities;
            foreach (Entity entity4 in array)
            {
                Transform transform = UnityEngine.Object.Instantiate(seq.pointPrefab, entity4.transform.position, Quaternion.identity, seq.group);
                transform.gameObject.SetActive(value: true);
                entity4.transform.SetParent(transform);
                entity4.flipper.FlipUp();
                seq.points.Add(transform);
                LeanTween.alphaCanvas(((Card)entity4.display).canvasGroup, 1f, 0.4f).setEaseInQuad();
            }

            foreach (Transform point in seq.points)
            {
                LeanTween.moveLocal(to: point.localPosition.normalized, gameObject: point.gameObject, time: 0.4f).setEaseInQuart();
            }

            yield return new WaitForSeconds(0.4f);

            Events.InvokeScreenShake(1f, 0f);
            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].wobbler.WobbleRandom();
            }

            foreach (Transform point2 in seq.points)
            {
                LeanTween.moveLocal(to: point2.localPosition.normalized * 3f, gameObject: point2.gameObject, time: 1f).setEase(seq.bounceCurve);
            }

            LeanTween.moveLocal(seq.group.gameObject, new Vector3(0f, 0f, -2f), 1f).setEaseInOutQuad();
            LeanTween.rotateZ(seq.group.gameObject, Dead.PettyRandom.Range(160f, 180f), 1f).setOnUpdateVector3(delegate
            {
                foreach (Transform point3 in seq.points)
                {
                    point3.transform.eulerAngles = Vector3.zero;
                }
            }).setEaseInOutQuad();
            yield return new WaitForSeconds(1f);

            Events.InvokeScreenShake(1f, 0f);
            if ((bool)seq.ps)
            {
                seq.ps.Play();
            }

            seq.combinedFx.SetActive(value: true);

            finalEntity.transform.position = Vector3.zero;
            array = entities;
            for (int i = 0; i < array.Length; i++)
            {
                CardManager.ReturnToPool(array[i]);
            }

            seq.group.transform.localRotation = Quaternion.identity;
            finalEntity.curveAnimator.Ping();
            finalEntity.wobbler.WobbleRandom();

            yield return new WaitForSeconds(1f);

            seq.fader.gameObject.Destroy();
            PauseMenu.Unblock();

            //
            bool flag = true;
            if (spawnOnBoard)
            {

                if (row.owner == References.Player && row.Count != row.max)
                {
                    yield return Sequences.CardMove(finalEntity, new CardContainer[1] { row });
                    finalEntity.inPlay = true;
                    flag = false;

                    foreach (StatusEffectData effect in finalEntity.statusEffects)
                    {

                        if (effect is StatusEffectWhileActiveX activeEffect)
                        {
                            if (!activeEffect.active)
                            {
                                yield return activeEffect.Activate();
                            }
                        }
                    }
                }

                if (flag)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        row = Battle.instance.GetRow(References.Player, i);
                        if (row.Count != row.max)
                        {

                            yield return Sequences.CardMove(finalEntity, new CardContainer[1] { row });
                            finalEntity.inPlay = true;
                            flag = false;

                            foreach (StatusEffectData effect in finalEntity.statusEffects)
                            {

                                if (effect is StatusEffectWhileActiveX activeEffect)
                                {
                                    if (!activeEffect.active)
                                    {
                                        yield return activeEffect.Activate();
                                    }
                                }
                            }

                            break;
                        }
                    }
                }



            }

            //
            if (flag)
            {
                yield return Sequences.CardMove(finalEntity, new CardContainer[1] { References.Player.handContainer });
                finalEntity.inPlay = true;
            }

            References.Player.handContainer.TweenChildPositions();
            ActionQueue.Stack(new ActionReveal(finalEntity));
        }
    }
}
