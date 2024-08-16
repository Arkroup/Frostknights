using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using static StatusEffectBonusDamageEqualToX;
using UnityEngine.AddressableAssets;
using UnityEngine.TextCore.Text;
using WildfrostHopeMod.VFX;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.Utils;

namespace Frostknights
{
    public class Frostknights : WildfrostMod
    {
        private void ArknightsPhoto(Scene scene)
        {
            if (scene.name == "Town")
            {
                References.instance.StartCoroutine(ArknightsPhoto2());
            }
        }

        private static IEnumerator ArknightsPhoto2()
        {
            string[] defenders = { "artemys.wildfrost.frostknights.nian", "artemys.wildfrost.frostknights.mudrock", "artemys.wildfrost.frostknights.blemishine", "artemys.wildfrost.frostknights.hoshiguma", "artemys.wildfrost.frostknights.horn", "artemys.wildfrost.frostknights.saria", "artemys.wildfrost.frostknights.penance", "artemys.wildfrost.frostknights.jessica"
            };
            string[] guards = { "artemys.wildfrost.frostknights.gavial","artemys.wildfrost.frostknights.blaze","artemys.wildfrost.frostknights.mountain","artemys.wildfrost.frostknights.chongyue","artemys.wildfrost.frostknights.irene","artemys.wildfrost.frostknights.ch'en","artemys.wildfrost.frostknights.nearl","artemys.wildfrost.frostknights.młynar","artemys.wildfrost.frostknights.silverAsh","artemys.wildfrost.frostknights.thorns","artemys.wildfrost.frostknights.qiubai","artemys.wildfrost.frostknights.pallas","artemys.wildfrost.frostknights.surtr","artemys.wildfrost.frostknights.degenbrecher"
            };
            string[] medics = { "artemys.wildfrost.frostknights.shining","artemys.wildfrost.frostknights.nightingale","artemys.wildfrost.frostknights.reed"
            };
            string[] sniper = { "artemys.wildfrost.frostknights.rosmontis","artemys.wildfrost.frostknights.exusiai","artemys.wildfrost.frostknights.archetto","artemys.wildfrost.frostknights.fiammetta","artemys.wildfrost.frostknights.w","artemys.wildfrost.frostknights.pozëmka","artemys.wildfrost.frostknights.schwarz","artemys.wildfrost.frostknights.typhon","artemys.wildfrost.frostknights.fartooth"
            };

            yield return SceneManager.WaitUntilUnloaded("CardFramesUnlocked");
            yield return SceneManager.Load("CardFramesUnlocked", SceneType.Temporary);
            CardFramesUnlockedSequence sequence = GameObject.FindObjectOfType<CardFramesUnlockedSequence>();
            TextMeshProUGUI titleObject = sequence.GetComponentInChildren<TextMeshProUGUI>(true);
            titleObject.text = $"New Companions: Snipers";
            yield return sequence.StartCoroutine("CreateCards", sniper);
        }

        public Frostknights(string modDirectory) : base(modDirectory)
        {
        }

        public override string GUID => "artemys.wildfrost.frostknights"; //[creator name].[game name].[mod name] is standard convention. LOWERCASE!

        public override string[] Depends => new string[0]; //The GUID of other mods that your mod requires. This tutorial has none of that.

        public override string Title => "Frostknights";

        public override string Description => "This mod intends to add operators from Arknights as cards, in the future I plan to add a new Rhodes Island clan, new enemies and bosses, and more.\r\n\r\nCurrently there are 36 new companions! I'll do updates of each class and progressively add more, as well as slowly edit and tweak already released cards for balance.\r\n\r\nPlease do tell me your thoughts on balance! I'm pretty new to the game so any help is welcome.\r\n\r\nThanks a lot for all the help to the modding channel on the discord! And also thanks a lot to the pokefrost team for letting me use their effects!";

        private T TryGet<T>(string name) where T : DataFile
        {
            T data;
            if (typeof(StatusEffectData).IsAssignableFrom(typeof(T)))
                data = base.Get<StatusEffectData>(name) as T;
            else
                data = base.Get<T>(name);

            if (data == null)
                throw new Exception($"TryGet Error: Could not find a [{typeof(T).Name}] with the name [{name}] or [{Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID(name, this)}]");

            return data;
        }

        private CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(TryGet<StatusEffectData>(name), amount);
        //See above

        private CardData.TraitStacks TStack(string name, int amount) => new CardData.TraitStacks(TryGet<TraitData>(name), amount);

        //Note: you need to add the reference DeadExtensions.dll in order to use InstantiateKeepName(). 
        private StatusEffectDataBuilder StatusCopy(string oldName, string newName)
        {
            StatusEffectData data = Get<StatusEffectData>(oldName).InstantiateKeepName();            //Copies the status effect
            data.name = Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID(newName, this);                                                                     //Changes its name
            StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>(); //Wraps the status effect in a builder
            builder.Mod = this;                                                                      //Gives the builder context.
            return builder;
        }

        private TraitDataBuilder TraitCopy(string oldName, string newName)
        {
            TraitData data = Get<TraitData>(oldName).InstantiateKeepName();            //Copies the status effect
            data.name = Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID(newName, this);                                                                     //Changes its name
            TraitDataBuilder builder = data.Edit<TraitData, TraitDataBuilder>(); //Wraps the status effect in a builder
            builder.Mod = this;                                                                      //Gives the builder context.
            return builder;
        }

        private List<CardDataBuilder> cards;                 //The list of custom CardData(Builder)
        private List<StatusEffectDataBuilder> statusEffects; //The list of custom StatusEffectData(Builder)
        private List<KeywordDataBuilder> keywords;
        private List<TraitDataBuilder> traits;
        private bool preLoaded = false;                      //Used to prevent redundantly reconstructing our data. Not truly necessary.

        public TMP_SpriteAsset assetSprites;
        public override TMP_SpriteAsset SpriteAsset => assetSprites;
        private void patchstatuses(StatusIcon icon)
        {
            string[] newtypes = new string[] { "burning", "thing2" };
            if (newtypes.Contains(icon.type))
            {
                icon.SetText();
                icon.Ping();
                icon.onValueDown.AddListener(delegate { icon.Ping(); });
                icon.onValueUp.AddListener(delegate { icon.Ping(); });
                icon.afterUpdate.AddListener(icon.SetText);
                icon.onValueDown.AddListener(icon.CheckDestroy);
            }
        }

        private void CreateModAssets()
        {
            //Icons
            assetSprites = HopeUtils
                .CreateSpriteAsset("assetSprites", directoryWithPNGs: this.ImagePath("Sprites"),
                textures: new Texture2D[] {
                    this.ImagePath("Sprites/burningicon.png").ToSprite().ToTexture()
                }, sprites: new Sprite[] {
                    this.ImagePath("Sprites/burningicon.png").ToSprite()
                });

            foreach (var character in assetSprites.spriteCharacterTable)
            {
                character.scale = 1.3f;
            }

            keywords = new List<KeywordDataBuilder>();

            //Code for keywords
            //Splash Keyword
            keywords.Add(
                new KeywordDataBuilder(this)
                .Create("splash")
                .WithTitle("Splash")
                .WithShowName(true) //Shows name in Keyword box (as opposed to a nonexistant icon).
                .WithDescription("Deal damage to adjacent enemies") //Format is body|note.
                .WithCanStack(true)
                );

            //Taunt Keyword
            keywords.Add(
                new KeywordDataBuilder(this)
                .Create("taunt")
                .WithTitle("Taunt")
                .WithShowName(true)
                .WithDescription("All enemies are <keyword=artemys.wildfrost.frostknights.taunted>")
                .WithCanStack(false)
                );

            //Taunted Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("taunted")
               .WithTitle("Taunted")
               .WithShowName(true)
               .WithDescription("Target only enemies with <keyword=artemys.wildfrost.frostknights.taunt>|Hits them all!")
               .WithCanStack(false)
               );

            //Burning Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("burning")
               .WithTitle("Burning")
               .WithShowName(false)
               .WithDescription("Explodes when hit, damaging all targets in row then clearing|Applying more increases the explosion!")
               .WithIconName("burningicon")
               .WithTitleColour(new Color(1f, 0.2f, 0.2f))
               .WithNoteColour(new Color(1f, 0.2f, 0.2f))
               );

            traits = new List<TraitDataBuilder>();

            //Code for traits
            //Splash Trait
            TraitDataBuilder splash = new TraitDataBuilder(this)
                .Create("Splash")
                .WithOverrides(TryGet<TraitData>("Barrage"))
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("splash");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("On Card Played Apply Splash To Self") };
                    });
            traits.Add(splash);

            //Taunt Trait
            traits.Add(
                TraitCopy("Hellbent", "Taunt")
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("taunt");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("While Active Taunted To Enemies") };
                    })
                    );

            //Taunted Trait
            traits.Add(
                TraitCopy("Hellbent", "Taunted")
                .WithOverrides(TryGet<TraitData>("Barrage"), TryGet<TraitData>("Aimless"), TryGet<TraitData>("Longshot"))
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("taunted");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Hit All Taunt") };
                    })
                    );

            statusEffects = new List<StatusEffectDataBuilder>();

            //Code for status effects
            //Status 0: Add Block to Ally Behind
            statusEffects.Add(
                StatusCopy("On Turn Apply Spice To AllyBehind", "On Turn Apply Block To AllyBehind")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Block");
                })
                .WithText("Apply <{a}><keyword=block> to ally behind")
                );

            //Status 1: Heal Self
            statusEffects.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Self")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                .WithText("Restore <{a}><keyword=health> to self")
                );

            //Status 2: Heal Ally Behind
            statusEffects.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Ally Behind")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                })
                .WithText("Restore <{a}><keyword=health> to ally behind")
                );

            //Status 3: Heal on Block Removed
            statusEffects.Add(
                StatusCopy("Trigger When Self Or Ally Loses Block", "On Block Lost Heal")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
                    ((StatusEffectData)data).isReaction = false;
                })
                .WithText("When Block is broken, restore <{a}><keyword=health> to self")
                );

            //Status 4: Summon Horn
            statusEffects.Add(
                StatusCopy("Summon Dregg", "Summon Horn")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("hornSummon");
                })
                );

            //Status 5: Instant Summon Horn
            statusEffects.Add(
               StatusCopy("Instant Summon Dregg", "Instant Summon Horn")
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Horn") as StatusEffectSummon;
               })
               );

            //Status 6: When Destroyed Summon Horn
            statusEffects.Add(
               StatusCopy("When Destroyed Summon Dregg", "When Destroyed Summon Horn")
               .WithText("When destroyed, summon self with 2<keyword=health>")
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Horn");
               })
               );

            //Status 7: Summon Mobile Riot Shield
            statusEffects.Add(
                StatusCopy("Summon Fallow", "Summon Mobile Riot Shield")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("mobileRiotShield");
                })
                );

            //Status 8: Instant Summon Mobile Riot Shield
            statusEffects.Add(
                StatusCopy("Instant Summon Fallow", "Instant Summon Mobile Riot Shield")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mobile Riot Shield") as StatusEffectSummon;
                })
                );

            //Status 9: When Deployed Summon Mobile Riot Shield
            statusEffects.Add(
                StatusCopy("When Deployed Summon Wowee", "When Deployed Summon Mobile Riot Shield")
                .WithText("When deployed, summon {0}")
                .WithTextInsert("<card=artemys.wildfrost.frostknights.mobileRiotShield>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXWhenDeployed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mobile Riot Shield");
                })
                );

            //Status 10: When Active Give Barrage to Jessica
            statusEffects.Add(
                StatusCopy("While Active Barrage To AlliesInRow", "While Active Barrage To Jessica")
                .WithText("While Active Jessica has Barrage")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                    ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[1]
                    {
                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                                TryGet<CardData>("jessica")
                            }
                        }
                    };
                })
                );

            //Status 11: Damage Adjacent Enemies
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantSplashDamage>("Hit All Adjacent Enemies")
                .WithCanBeBoosted(false)
                .WithType("")
                );

            //Status 12: Apply Splash On Card Played
            statusEffects.Add(
                StatusCopy("On Hit Pull Target", "On Card Played Apply Splash To Self")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Hit All Adjacent Enemies");
                })
                );

            //Status 13: Summon Blazing Sun
            statusEffects.Add(
                StatusCopy("Summon Plep", "Summon Blazing Sun")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("blazingSun");
                })
                );

            //Status 14: Instant Summon Blazing Sun
            statusEffects.Add(
                StatusCopy("Instant Summon Fallow", "Instant Summon Blazing Sun")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Blazing Sun") as StatusEffectSummon;
                })
                );

            //Status 15: On Turn Summon Blazing Sun
            statusEffects.Add(
                StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Blazing Sun")
                .WithText("Summon {0}")
                .WithTextInsert("<card=artemys.wildfrost.frostknights.blazingSun>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXOnTurn)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Blazing Sun");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 16: When Deployed Deal Damage To Enemies
            statusEffects.Add(
                StatusCopy("When Deployed Apply Demonize To Enemies", "When Deployed Deal Damage To Enemies")
                .WithText("When deployed, deal <{a}> damage to all enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = null;
                    ((StatusEffectApplyX)data).countsAsHit = true;
                    ((StatusEffectApplyX)data).dealDamage = true;
                    ((StatusEffectApplyX)data).targetMustBeAlive = false;
                    ((StatusEffectApplyX)data).queue = false;
                    ((StatusEffectApplyX)data).doPing = false;
                })
                );

            //Status 17: Heal Self and Allies
            statusEffects.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Self and Allies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self|StatusEffectApplyX.ApplyToFlags.Allies;
                })
                .WithText("Restore <{a}><keyword=health> to self and allies")
                );

            //Status 18: On Turn Add Attack to Ally In Front
            statusEffects.Add(
                StatusCopy("On Turn Add Attack To Allies", "On Turn Add Attack to Ally In Front")
                .WithText("Add <+{a}><keyword=attack> to ally in front")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
                })
                );

            //Status 19: Increase Counter
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantCountUp>("Increase Counter")
                .WithCanBeBoosted(true)
                .WithType("")
                );

            //Status 20: On Hit Increase Counter
            statusEffects.Add(
                StatusCopy("On Hit Pull Target", "On Hit Increase Counter")
                .WithText("Increase <keyword=counter> by <{a}>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
                );

            //Status 21: On Turn Decrease Max Counter
            statusEffects.Add(
                StatusCopy("On Hit Pull Target", "On Hit Increase Counter")
                .WithText("Increase <keyword=counter> by <{a}>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
                );

            //Status 22: When Deployed Apply Increase Counter To Enemies
            statusEffects.Add(
                StatusCopy("When Deployed Apply Demonize To Enemies", "When Deployed Apply Increase Counter To Enemies")
                .WithText("When Deployed, increase <keyword=counter> of all enemies by <{a}>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
                );

            //Status 23: Summon Mirage
            statusEffects.Add(
                StatusCopy("Summon Gunk", "Summon Mirage")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("mirage");
                })
                );

            //Status 24: Instant Summon Mirage In Hand
            statusEffects.Add(
                StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Mirage In Hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mirage") as StatusEffectSummon;
                })
                );

            //Status 25: When Deployed Add Mirage To Hand
            statusEffects.Add(
                StatusCopy("When Deployed Add Junk To Hand", "When Deployed Add Mirage To Hand")
                .WithText("Add <{a}> <card=artemys.wildfrost.frostknights.mirage> to your hand when played")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mirage In Hand");
                })
                );

            //Status 26: Hit All Taunt
            statusEffects.Add(
                StatusCopy("Hit All Enemies", "Hit All Taunt")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeTaunt>();
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 27: While Active Taunted To Enemies
            statusEffects.Add(
                StatusCopy("While Active Aimless To Enemies", "While Active Taunted To Enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Taunted");
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 28: Temporary Taunted
            statusEffects.Add(
                StatusCopy("Temporary Aimless", "Temporary Taunted")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Taunted");
                })
                );

            //Status 29: On Turn Heal AllyInFrontOf
            statusEffects.Add(
               StatusCopy("On Turn Heal Allies", "On Turn Heal AllyInFrontOf")
               .WithText("Restore <{a}><keyword=health> to ally in front")
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
               })
               );

            //Status 30: On Turn Apply Shell AllyInFrontOf
            statusEffects.Add(
               StatusCopy("On Turn Apply Shell To Allies", "On Turn Apply Shell AllyInFrontOf")
               .WithText("Apply <{a}><keyword=shell> to ally in front")
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
               })
               );

            //Status 31: On Turn Deal Damage To Enemies
            statusEffects.Add(
                StatusCopy("On Turn Apply Demonize To Enemies", "On Turn Deal Damage To Enemies")
                .WithText("Deal <{a}> damage to all enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = null;
                    ((StatusEffectApplyX)data).countsAsHit = true;
                    ((StatusEffectApplyX)data).dealDamage = true;
                    ((StatusEffectApplyX)data).targetMustBeAlive = false;
                    ((StatusEffectApplyX)data).queue = false;
                    ((StatusEffectApplyX)data).doPing = false;
                })
                );

            //Status 32: On Turn Apply Shroom To Enemies
            statusEffects.Add(
                StatusCopy("On Turn Apply Demonize To Enemies", "On Turn Apply Shroom To Enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Shroom");
                })
                .WithText("Apply <{a}><keyword=shroom> to all enemies")
                );

            //Status 33: Summon Typewriter
            statusEffects.Add(
                StatusCopy("Summon Gunk", "Summon Typewriter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("typewriter");
                })
                );

            //Status 34: Instant Summon Typewriter In Hand
            statusEffects.Add(
                StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Typewriter In Hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Typewriter") as StatusEffectSummon;
                })
                );

            //Status 35: When Deployed Add Typewriter To Hand
            statusEffects.Add(
                StatusCopy("When Deployed Add Junk To Hand", "When Deployed Add Typewriter To Hand")
                .WithText("Add <{a}> <card=artemys.wildfrost.frostknights.typewriter> to your hand when deployed")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Typewriter In Hand");
                })
                );

            //Status 36: On Hit Damage Bomed Target
            statusEffects.Add(
                StatusCopy("On Hit Damage Snowed Target", "On Hit Damage Bomed Target")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    TargetConstraintHasStatus bomconstraint = new TargetConstraintHasStatus();
                    bomconstraint.status = Get<StatusEffectData>("Weakness");
                    ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { bomconstraint };
                })
                .WithText("Deal <{a}> additional damage to <keyword=weakness>'d targets")
                );

            //Status 37: Burning
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectBurning>("Burning")
                .WithVisible(true)
                .WithIconGroupName("health")
                .WithIsStatus(true)
                .WithOffensive(true)
                .WithStackable(true)
                .WithTextInsert("{a}")
                .WithKeyword("artemys.wildfrost.frostknights.burning")
                .WithType("burning")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectData)data).removeOnDiscard = true;
                    ((StatusEffectData)data).targetConstraints = new TargetConstraint[1] { ScriptableObject.CreateInstance<TargetConstraintIsUnit>() };
                    ((StatusEffectData)data).applyFormatKey = Get<StatusEffectData>("Shroom").applyFormatKey;
                })
                );

            cards = new List<CardDataBuilder>();

            //Code for cards
            //Nian Card 1
            cards.Add(
                new CardDataBuilder(this).CreateUnit("nian", "Nian")
                .SetSprites("Nian.png", "Nian BG.png")
                .SetStats(11, 3, 7)
                .WithCardType("Friendly")
                .AddPool("ClunkUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Well, well, you got orders for me?", "A knife, a life; weapons are the basis for society." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Apply Block To AllyBehind", 1)
                    };
                })
                );

            //Mudrock Card 2
            cards.Add(
                new CardDataBuilder(this).CreateUnit("mudrock", "Mudrock")
                .SetSprites("Mudrock.png", "Mudrock BG.png")
                .SetStats(8, 2, 5)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "The soil answers my will.", "Speak. The rocks are listening." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Block Lost Heal", 4),
                        SStack("On Turn Heal Self", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Splash", 2)
                    };
                })
                );

            //Saria Card 3
            cards.Add(
                new CardDataBuilder(this).CreateUnit("saria", "Saria")
                .SetSprites("Saria.png", "Saria BG.png")
                .SetStats(10, 0, 5)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "It's my turn.", "Suppress them." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Heal Self and Allies", 2)
                    };
                })
                );

            //Penance Card 4
            cards.Add(
                new CardDataBuilder(this).CreateUnit("penance", "Penance")
                .SetSprites("Penance.png", "Penance BG.png")
                .SetStats(9, 3, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "No matter what I might encounter, I will not back down.", "What the law does not specify, I will reveal through practice." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Apply Shell To Self", 3),
                        SStack("When Hit Gain Teeth To Self", 3)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Provoke", 1)
                    };
                })
                );

            //Blemishine Card 5
            cards.Add(
                new CardDataBuilder(this).CreateUnit("blemishine", "Blemishine")
                .SetSprites("Blemishine.png", "Blemishine BG.png")
                .SetStats(9, 3, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "All ready.", "Pray for the coming battle." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Heal Ally Behind", 4)
                    };
                })
                );

            //Horn Card 6
            cards.Add(
                new CardDataBuilder(this).CreateUnit("horn", "Horn")
                .SetSprites("Horn.png", "Horn BG.png")
                .SetStats(6, 5, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Allow me to study your command style in this battle, Doctor.", "At attention. Ready to fall in." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("When Destroyed Summon Horn", 1)
                    };
                    data.traits = new List <CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1),
                        TStack("Splash", 3)
                    };
                })
                );

            //Horn Summon Card 
            cards.Add(
                new CardDataBuilder(this).CreateUnit("hornSummon", "Horn")
                .SetSprites("Horn.png", "Horn BG.png")
                .SetStats(2, 5, 2)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1),
                        TStack("Splash", 3)
                    };
                })
                );

            //Jessica Card 7
            cards.Add(
                new CardDataBuilder(this).CreateUnit("jessica", "Jessica")
                .SetSprites("Jessica.png", "Jessica BG.png")
                .SetStats(7, 4, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "In position.", "Roger, wilco." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("When Deployed Summon Mobile Riot Shield", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1)
                    };
                })
                );

            //Mobile Riot Shield Card
            cards.Add(
                new CardDataBuilder(this).CreateUnit("mobileRiotShield", "Mobile Riot Shield")
                .SetSprites("Mobile Riot Shield.png", "Mobile Riot Shield BG.png")
                .SetStats(5, null, 2)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Apply Shell To AllyBehind", 2),
                        SStack("While Active Barrage To Jessica", 1)
                    };
                })
                );

            //Hoshiguma Card 8
            cards.Add(
                new CardDataBuilder(this).CreateUnit("hoshiguma", "Hoshiguma")
                .SetSprites("Hoshiguma.png", "Hoshiguma BG.png")
                .SetStats(9, 2, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Yes, Sir.", "Got it." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Teeth", 3)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Barrage", 1)
                    };
                })
                );

            //Thorns Card 9
            cards.Add(
                new CardDataBuilder(this).CreateUnit("thorns", "Thorns")
                .SetSprites("Thorns.png", "Thorns BG.png")
                .SetStats(5, 2, 2)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Understood.", "Give me your orders." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Shroom", 2)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1)
                    };
                })
                );

            //SilverAsh Card 10
            cards.Add(
                new CardDataBuilder(this).CreateUnit("silverAsh", "SilverAsh")
                .SetSprites("SilverAsh.png", "SilverAsh BG.png")
                .SetStats(5, 1, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "What are you planning?", "Made up your mind?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Hit All Enemies", 1)
                    };
                })
                );

            //Młynar Card 11
            cards.Add(
                new CardDataBuilder(this).CreateUnit("młynar", "Młynar")
                .SetSprites("Młynar.png", "Młynar BG.png")
                .SetStats(4, 7, 9)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "I'm fully prepared.", "Hm." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Hit All Enemies", 1)
                    };
                })
                );

            //Nearl Card 12
            cards.Add(
                new CardDataBuilder(this).CreateUnit("nearl", "Nearl")
                .SetSprites("Nearl.png", "Nearl BG.png")
                .SetStats(6, 6, 7)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Awaiting only your orders.", "Yes, I am fully ready." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Summon Blazing Sun", 1)
                    };
                })
                );

            //Blazing Sun Card
            cards.Add(
                new CardDataBuilder(this).CreateUnit("blazingSun", "Blazing Sun")
                .SetSprites("Blazing Sun.png", "Blazing Sun BG.png")
                .SetStats(3, null, 1)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("When Deployed Apply Increase Counter To Enemies", 1),
                        SStack("When Deployed Deal Damage To Enemies", 1)
                    };
                })
                );

            //Pallas Card 13
            cards.Add(
                new CardDataBuilder(this).CreateUnit("pallas", "Pallas")
                .SetSprites("Pallas.png", "Pallas BG.png")
                .SetStats(3, 2, 5)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Rejoice!", "Come––" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Add Attack to Ally In Front", 1),
                        SStack("MultiHit", 1)
                    };
                })
                );

            //Ch'en Card 14
            cards.Add(
                new CardDataBuilder(this).CreateUnit("ch'en", "Ch'en")
                .SetSprites("Ch'en.png", "Ch'en BG.png")
                .SetStats(6, 4, 6)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "This is Ch'en.", "Ready to move." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("MultiHit", 1)
                    };
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Snow", 1)
                    };
                })
                );

            //Qiubai Card 15
            cards.Add(
                new CardDataBuilder(this).CreateUnit("qiubai", "Qiubai")
                .SetSprites("Qiubai.png", "Qiubai BG.png")
                .SetStats(5, 4, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Within sheath, the sword awaits.", "Beyond substance, the spirit unchained." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Hit Increase Counter", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1)
                    };
                })
                );

            //Irene Card 16
            cards.Add(
                new CardDataBuilder(this).CreateUnit("irene", "Irene")
                .SetSprites("Irene.png", "Irene BG.png")
                .SetStats(5, 3, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "See the light of my lantern? I'm right here.", "Don't step into the shadows." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Aimless", 1),
                        TStack("Splash", 2)
                    };
                })
                );

            //Chongyue Card 17
            cards.Add(
                new CardDataBuilder(this).CreateUnit("chongyue", "Chongyue")
                .SetSprites("Chongyue.png", "Chongyue BG.png")
                .SetStats(4, 2, 2)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "One should spar from time to time.", "At your command." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Splash", 2)
                    };
                })
                );

            //Mountain Card 18
            cards.Add(
                new CardDataBuilder(this).CreateUnit("mountain", "Mountain")
                .SetSprites("Mountain.png", "Mountain BG.png")
                .SetStats(6, 2, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "I'm right here.", "Where would you like me?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Heal Self", 2)
                    };
                })
                );

            //Blaze Card 19
            cards.Add(
                new CardDataBuilder(this).CreateUnit("blaze", "Blaze")
                .SetSprites("Blaze.png", "Blaze BG.png")
                .SetStats(14, 7, 5)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "You know, it wasn't easy learning high-temperature vapor dynamics!", "Let me show you the chainsaw techniques the other elite operators taught me!" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Barrage", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Card Played Damage To Self", 3)
                    };
                })
                );

            //Gavial Card 20
            cards.Add(
                new CardDataBuilder(this).CreateUnit("gavial", "Gavial")
                .SetSprites("Gavial.png", "Gavial BG.png")
                .SetStats(9, 6, 0)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Let's begin.", "What would you like me to do?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Smackback", 1),
                        TStack("Pull", 1)
                    };
                })
                );

            //Hoederer Card 21
            cards.Add(
                new CardDataBuilder(this).CreateUnit("hoederer", "Hoederer")
                .SetSprites("Hoederer.png", "Hoederer BG.png")
                .SetStats(5, 3, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Don't doubt your own judgment.", "Seize the moment." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Hit Damage Snowed Target", 4)
                    };
                })
                );

            //Surtr Card 22
            cards.Add(
                new CardDataBuilder(this).CreateUnit("surtr", "Surtr")
                .SetSprites("Surtr.png", "Surtr BG.png")
                .SetStats(5, 3, 9)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Do not order me around.", "Enough, I understand." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Burning", 3)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Barrage", 1),
                    };
                })
                );

            //Degenbrecher Card 23
            cards.Add(
                new CardDataBuilder(this).CreateUnit("degenbrecher", "Degenbrecher")
                .SetSprites("Degenbrecher.png", "Degenbrecher BG.png")
                .SetStats(7, 3, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Still mulling things over, Dr. Perfectionist?", "You want my opinion?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Snow", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Hit Damage Snowed Target", 3)
                    };
                })
                );

            //Reed Card 24
            cards.Add(
                new CardDataBuilder(this).CreateUnit("reed", "Reed")
                .SetSprites("Reed.png", "Reed BG.png")
                .SetStats(5, 4, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Where do you wish to go?", "I'm prepared to fight." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Splash", 2)
                    };
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Overload", 3)
                    };
                })
                );

            //Nightingale Card 25
            cards.Add(
                new CardDataBuilder(this).CreateUnit("nightingale", "Nightingale")
                .SetSprites("Nightingale.png", "Nightingale BG.png")
                .SetStats(5, null, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Doctor...", "Who will you choose?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("On Turn Heal Allies", 2),
                        SStack("On Turn Apply Shell To Allies", 1),
                        SStack("When Deployed Add Mirage To Hand", 2)
                    };
                })
                );

            //Mirage Card 26
            cards.Add(
                new CardDataBuilder(this).CreateUnit("mirage", "Mirage")
                .SetSprites("Mirage.png", "Mirage BG.png")
                .SetStats(2, null, 0)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Taunt", 1)
                    };
                })
                );

            //Shining Card 27
            cards.Add(
                new CardDataBuilder(this).CreateUnit("shining", "Shining")
                .SetSprites("Shining.png", "Shining BG.png")
                .SetStats(5, null, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Let's begin.", "Yes, I'm listening." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Heal AllyInFrontOf", 4),
                        SStack("On Turn Apply Shell AllyInFrontOf", 3)
                    };
                })
                );

            //Rosmontis Card 28
            cards.Add(
                new CardDataBuilder(this).CreateUnit("rosmontis", "Rosmontis")
                .SetSprites("Rosmontis.png", "Rosmontis BG.png")
                .SetStats(5, null, 2)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "I won't remember you.", "Give me your order. I will see it through." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Deal Damage To Enemies", 2),
                        SStack("On Turn Apply Shroom To Enemies", 1)
                    };
                })
                );

            //Exusiai Card 29
            cards.Add(
                new CardDataBuilder(this).CreateUnit("exusiai", "Exusiai")
                .SetSprites("Exusiai.png", "Exusiai BG.png")
                .SetStats(3, 1, 1)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "All right!", "Is it my turn?" };
                })
                );

            //Archetto Card 30
            cards.Add(
                new CardDataBuilder(this).CreateUnit("archetto", "Archetto")
                .SetSprites("Archetto.png", "Archetto BG.png")
                .SetStats(3, 3, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Your orders.", "What arrow should I use?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Barrage", 1)
                    };
                })
                );

            //Fiametta Card 31
            cards.Add(
                new CardDataBuilder(this).CreateUnit("fiammetta", "Fiammetta")
                .SetSprites("Fiammetta.png", "Fiammetta BG.png")
                .SetStats(5, 4, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Sights locked.", "Boring." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1)
                    };
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Burning", 2)
                    };
                })
                );

            //W Card 32
            cards.Add(
                new CardDataBuilder(this).CreateUnit("w", "W")
                .SetSprites("W.png", "W BG.png")
                .SetStats(5, 3, 5)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Acknowledged.", "Understood." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Splash", 3)
                    };
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Overload", 2)
                    };
                })
                );

            //Pozëmka Card 33
            cards.Add(
                new CardDataBuilder(this).CreateUnit("pozëmka", "Pozëmka")
                .SetSprites("Pozëmka.png", "Pozëmka BG.png")
                .SetStats(5, 2, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Your orders, please.", "With pleasure." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("When Deployed Add Typewriter To Hand", 1),
                        SStack("On Hit Damage Bomed Target", 2)
                    };
                })
                );

            //Typewriter Card 34
            cards.Add(
                new CardDataBuilder(this).CreateUnit("typewriter", "Typewriter")
                .SetSprites("Typewriter.png", "Typewriter BG.png")
                .SetStats(3, 3, 3)
                .WithCardType("Summoned")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Your orders, please.", "With pleasure." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Weakness", 2)
                    };
                })
                );

            //Schwarz Card 35
            cards.Add(
                new CardDataBuilder(this).CreateUnit("schwarz", "Schwarz")
                .SetSprites("Schwarz.png", "Schwarz BG.png")
                .SetStats(4, 5, 5)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Where am I needed?", "Here." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Weakness", 2)
                    };
                })
                );

            //Typhon Card 36
            cards.Add(
                new CardDataBuilder(this).CreateUnit("typhon", "Typhon")
                .SetSprites("Typhon.png", "Typhon BG.png")
                .SetStats(5, 6, 5)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Stay calm and make your move. No need to worry, I've got your back.", "What do you need of me?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Snow", 2)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1)
                    };
                })
                );

            //Fartooth Card 37
            cards.Add(
                new CardDataBuilder(this).CreateUnit("fartooth", "Fartooth")
                .SetSprites("Fartooth.png", "Fartooth BG.png")
                .SetStats(4, 4, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Yeah, I'm ready whenever.", "One-on-one, or one-on-many?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1)
                    };
                })
                );

            preLoaded = true;
        }
        public override void Load()
        {
            if (!preLoaded) { CreateModAssets(); } //The if statement is a flourish really. It makes the 2nd load of Load-Unload-Load faster.
            Events.OnStatusIconCreated += patchstatuses;
            base.Load();                          //Actual loading
            IconStuff();
            FloatingText ftext = GameObject.FindObjectOfType<FloatingText>(true);
            ftext.textAsset.spriteAsset.fallbackSpriteAssets.Add(assetSprites);
        }

        private void IconStuff()
        {
            this.CreateIcon("burningicon", ImagePath("burningicon.png").ToSprite(), "burning", "spice", Color.black, new KeywordData[] { Get<KeywordData>("burning") })
                    .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            Events.OnStatusIconCreated -= patchstatuses;
        }

        public override List<T> AddAssets<T, Y>()           //This method is called 6-7 times in base.Load() for each Builder. Can you name them all?
        {
            var typeName = typeof(Y).Name;
            switch (typeName)                                //Checks what the current builder is
            {
                case nameof(CardData):
                    return cards.Cast<T>().ToList();         //Loads our cards
                case nameof(StatusEffectData):
                    return statusEffects.Cast<T>().ToList(); //Loads our status effects
                case nameof(KeywordData):
                    return keywords.Cast<T>().ToList();      //Loads keywords
                case nameof(TraitData):
                    return traits.Cast<T>().ToList();        //Loads traits
                default:
                    return null;
            }
        }
    }

    public class StatusEffectInstantSplashDamage : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            // set this up for later
            Routine.Clump clump = new();
            List<Entity> targets = new();

            if (TryGetVertical(this.target, out List<Entity> otherEntities))
                targets.AddRange(otherEntities);

            if (TryGetHorizontal(this.target, out otherEntities))
                targets.AddRange(otherEntities);

            foreach (Entity entity in targets)
            {
                // this.applier is needed to attribute the kill properly
                Hit hit = new Hit(this.applier, entity, count)
                {
                    canRetaliate = false,       // is an indirect hit
                    countsAsHit = true,         // is a damaging hit
                };
                clump.Add(hit.Process());       // process all the hits at once
            }

            yield return clump.WaitForEnd();

            yield return base.Process();        // calls this.Remove()
        }
        public List<int> GetColIndices(Entity entity)
        {
            List<int> indices = [-1];
            if (entity.owner)
            {
                foreach (CardContainer row in References.Battle.GetRows(entity.owner))
                {
                    if (row is not CardSlotLane lane)
                        continue;
                    // the row is a proper lane of cards

                    foreach (var slot in lane.slots)
                    {
                        // if the entity is alive, check where it is
                        if (entity.actualContainers.Contains(slot))
                            indices.Add(lane.slots.IndexOf(slot));

                        // if the entity died, check where it was
                        else if (entity.preActualContainers?.Contains(slot) ?? false)
                            indices.Add(lane.slots.IndexOf(slot));
                    }
                }
            }
            indices = indices.Distinct().Where(index => index >= 0).ToList();

            return indices;
        }

        public bool TryGetVertical(Entity entity, out List<Entity> otherEntities)
        {
            otherEntities = [];

            List<int> indices = GetColIndices(entity);
            if (entity.owner)
            {
                foreach (CardContainer row in References.Battle.GetRows(entity.owner))
                {
                    if (row is not CardSlotLane lane)
                        continue;
                    // the row is a proper lane of cards

                    foreach (int index in indices)
                    {
                        if (lane.ChildCount < index)
                            break;
                        // the lane has at least (index) many slots (variable due to mods)

                        otherEntities.Add(lane[index]);
                        // lane[index] = the entity in the indexed slot, null if empty
                    }
                }
            }
            otherEntities = otherEntities.Distinct().Where(e => e != null && e != entity).ToList();

            return otherEntities.Any();
        }
        public bool TryGetHorizontal(Entity entity, out List<Entity> otherEntities)
        {
            otherEntities = [];
            if (entity.owner)
            {
                foreach (CardContainer row in References.Battle.GetRows(entity.owner))
                {
                    if (!row.Contains(entity))
                        continue;
                    // the entity is in this row

                    if (row is not CardSlotLane lane)
                        continue;
                    // the row is a proper lane of cards

                    int index = lane.IndexOf(entity);
                    if (index - 1 >= 0)
                        otherEntities.Add(lane[index - 1]);

                    if (index + 1 < lane.ChildCount)
                        otherEntities.Add(lane[index + 1]);
                    // lane[index] = the entity in the indexed slot, null if empty
                }
            }
            otherEntities = otherEntities.Distinct().Where(e => e != null && e != entity).ToList();

            return otherEntities.Any();
        }
    }

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

    public class StatusEffectBurning : StatusEffectData
    {
        public CardAnimation buildupAnimation;

        public bool burning;

        public override void Init()
        {
            Events.OnEntityHit += EntityHit;
        }

        public void OnDestroy()
        {
            Events.OnEntityHit -= EntityHit;
        }

        public void EntityHit(Hit hit)
        {
            if (hit.target == target && hit.Offensive && hit.canRetaliate)
            {
                Check();
            }
        }

        public void Check()
        {
            if (count > 0 && !burning)
            {
                ActionQueue.Stack(new ActionSequence(DealDamage())
                {
                    fixedPosition = true,
                    priority = eventPriority,
                    note = "Burned"
                });
                ActionQueue.Stack(new ActionSequence(Clear())
                {
                    fixedPosition = true,
                    priority = eventPriority,
                    note = "Clear Burns"
                });
                burning = true;
            }
        }

        public IEnumerator DealDamage()
        {
            if (!this || !target || !target.alive)
            {
                yield break;
            }

            HashSet<Entity> targets = new HashSet<Entity>();
            CardContainer[] containers = target.containers;
            foreach (CardContainer collection in containers)
            {
                targets.AddRange(collection);
            }

            if ((bool)buildupAnimation)
            {
                yield return buildupAnimation.Routine(target);
            }

            Entity damager = GetDamager();
            Routine.Clump clump = new Routine.Clump();
            foreach (Entity item in targets)
            {
                Hit hit = new Hit(damager, item, count)
                {
                    damageType = "burning"
                };
                clump.Add(hit.Process());
            }
            SfxSystem.OneShot("event:/sfx/status/overburn_damage");
            clump.Add(Sequences.Wait(0.5f));
            yield return clump.WaitForEnd();
        }

        public IEnumerator Clear()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                yield return Remove();
                burning = false;
            }
        }
    }

    public static class Ext
    {
        public static GameObject CreateIcon(this WildfrostMod mod, string name, Sprite sprite, string type, string copyTextFrom, Color textColor, KeywordData[] keys)
        {
            GameObject gameObject = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
            StatusIcon icon = gameObject.AddComponent<StatusIcon>();
            Dictionary<string, GameObject> cardIcons = CardManager.cardIcons;
            if (!copyTextFrom.IsNullOrEmpty())
            {
                GameObject text = cardIcons[copyTextFrom].GetComponentInChildren<TextMeshProUGUI>().gameObject.InstantiateKeepName();
                text.transform.SetParent(gameObject.transform);
                icon.textElement = text.GetComponent<TextMeshProUGUI>();
                icon.textColour = textColor;
                icon.textColourAboveMax = textColor;
                icon.textColourBelowMax = textColor;
            }
            icon.onCreate = new UnityEngine.Events.UnityEvent();
            icon.onDestroy = new UnityEngine.Events.UnityEvent();
            icon.onValueDown = new UnityEventStatStat();
            icon.onValueUp = new UnityEventStatStat();
            icon.afterUpdate = new UnityEngine.Events.UnityEvent();
            UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            CardHover cardHover = gameObject.AddComponent<CardHover>();
            cardHover.enabled = false;
            cardHover.IsMaster = false;
            CardPopUpTarget cardPopUp = gameObject.AddComponent<CardPopUpTarget>();
            cardPopUp.keywords = keys;
            cardHover.pop = cardPopUp;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.sizeDelta *= 0.01f;
            gameObject.SetActive(true);
            icon.type = type;
            cardIcons[type] = gameObject;

            return gameObject;
        }
    }
}
