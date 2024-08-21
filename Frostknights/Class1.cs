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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI.Table;
using static Building;
using System.Threading;
using Rewired;
using static Steamworks.InventoryItem;

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
            titleObject.text = $"New Companions: Defenders";
            yield return sequence.StartCoroutine("CreateCards", defenders);
        }

        public Frostknights(string modDirectory) : base(modDirectory)
        {
        }

        public override string GUID => "artemys.wildfrost.frostknights"; //[creator name].[game name].[mod name] is standard convention. LOWERCASE!

        public override string[] Depends => new string[0]; //The GUID of other mods that your mod requires. This tutorial has none of that.

        public override string Title => "Frostknights";

        public override string Description => "This mod intends to add operators from Arknights as cards, in the future I plan to add a new Rhodes Island clan, new enemies and bosses, and more.\r\n\r\nCurrently there are 36 new companions! I'll do updates of each class and progressively add more, as well as slowly edit and tweak already released cards for balance.\r\n\r\nPlease do tell me your thoughts on balance! I'm pretty new to the game so any help is welcome.\r\n\r\nThanks a lot for all the help to the modding channel on the discord! And also thanks a lot to the Tokens mod people for tokens (really cool mod go check it out) and Pokefrost (also really cool mod go check it out) for the help and for letting me use their effects!\r\n\r\nAll the art is owned by Hypergryph";

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

        private CardDataBuilder CardCopy(string oldName, string newName)
        {
            CardData data = TryGet<CardData>(oldName).InstantiateKeepName();
            data.name = GUID + "." + newName;
            CardDataBuilder builder = data.Edit<CardData, CardDataBuilder>();
            builder.Mod = this;
            return builder;
        }

        private ClassDataBuilder TribeCopy(string oldName, string newName)
        {
            ClassData data = TryGet<ClassData>(oldName).InstantiateKeepName();
            data.name = GUID + "." + newName;
            ClassDataBuilder builder = data.Edit<ClassData, ClassDataBuilder>();
            builder.Mod = this;
            return builder;
        }

        private CardScript GiveUpgrade(string name = "Crown") //Give a crown
        {
            CardScriptGiveUpgrade script = ScriptableObject.CreateInstance<CardScriptGiveUpgrade>(); //This is the standard way of creating a ScriptableObject
            script.name = $"Give {name}";                               //Name only appears in the Unity Inspector. It has no other relevance beyond that.
            script.upgradeData = Get<CardUpgradeData>(name);
            return script;
        }

        private CardScript AddRandomHealth(int min, int max) //Boost health by a random amount
        {
            CardScriptAddRandomHealth health = ScriptableObject.CreateInstance<CardScriptAddRandomHealth>();
            health.name = "Random Health";
            health.healthRange = new Vector2Int(min, max);
            return health;
        }

        private CardScript AddRandomDamage(int min, int max) //Boost damage by a ranom amount
        {
            CardScriptAddRandomDamage damage = ScriptableObject.CreateInstance<CardScriptAddRandomDamage>();
            damage.name = "Give Damage";
            damage.damageRange = new Vector2Int(min, max);
            return damage;
        }

        private CardScript AddRandomCounter(int min, int max) //Increase counter by a random amount
        {
            CardScriptAddRandomCounter counter = ScriptableObject.CreateInstance<CardScriptAddRandomCounter>();
            counter.name = "Give Counter";
            counter.counterRange = new Vector2Int(min, max);
            return counter;
        }

        private RewardPool CreateRewardPool(string name, string type, DataFile[] list)
        {
            RewardPool pool = new RewardPool();
            pool.name = name;
            pool.type = type;            //The usual types are Units, Items, Charms, and Modifiers.
            pool.list = list.ToList();
            return pool;
        }

        private T[] DataList<T>(params string[] names) where T : DataFile => names.Select((s) => TryGet<T>(s)).ToArray();

        private List<CardDataBuilder> cards;                 //The list of custom CardData(Builder)
        private List<StatusEffectDataBuilder> statusEffects; //The list of custom StatusEffectData(Builder)
        private List<KeywordDataBuilder> keywords;
        private List<TraitDataBuilder> traits;
        //private List<CardUpgradeDataBuilder> cardUpgrades;
        //private List<GameModifierDataBuilder> bells;
        private List<ClassDataBuilder> tribes;
        private bool preLoaded = false;                      //Used to prevent redundantly reconstructing our data. Not truly necessary.
        public static Frostknights instance;

        public TMP_SpriteAsset assetSprites;

        public override TMP_SpriteAsset SpriteAsset => assetSprites;

        private void CreateModAssets()
        {
            //Icons
            assetSprites = HopeUtils
                .CreateSpriteAsset("assetSprites", directoryWithPNGs: this.ImagePath("Sprites"), textures: [], sprites: []);

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

            //Provoke Keyword
            keywords.Add(
                new KeywordDataBuilder(this)
                .Create("provoke")
                .WithTitle("Provoke")
                .WithShowName(true)
                .WithDescription("All enemies are <keyword=artemys.wildfrost.frostknights.provoked>")
                .WithCanStack(false)
                );

            //Provoked Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("provoked")
               .WithTitle("Provoked")
               .WithShowName(true)
               .WithDescription("Target only enemies with <keyword=artemys.wildfrost.frostknights.provoke>|Hits them all!")
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

            //Trial of Thorns Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("trialofthorns")
               .WithTitle("Trial of Thorns")
               .WithDescription("<End Turn>: Gain <keyword=artemys.wildfrost.frostknights.provoke> for a turn | Click to activate\nnCooldown: 5 turns")
               );

            //Blazing Sun's Obeisance Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("blazingsunsobeisance")
               .WithTitle("Blazing Sun's Obeisance")
               .WithDescription("<End Turn>: Summon <card=artemys.wildfrost.frostknights.blazingSun>| Click to activate\nnCooldown: 6 turns")
               );

            //Opprobrium Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("opprobrium")
               .WithTitle("Opprobrium")
               .WithDescription("<Free Action>: Add <card=artemys.wildfrost.frostknights.typewriter> to your hand| Click to activate\nnCooldown: 6 turns")
               );

            //Iron Defense Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("irondefense")
               .WithTitle("Iron Defense")
               .WithDescription("<End Turn>: Give <1><keyword=block> and <2><keyword=shell> to all allies| Click to activate\nnCooldown: 13 turns")
               );

            //Saw of Strength Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("sawofstrength")
               .WithTitle("Saw of Strength")
               .WithDescription("<End Turn>: Gain <keyword=barrage> for a turn| Click to activate\nnCooldown: 6 turns")
               );

            //Destreza Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("destreza")
               .WithTitle("Destreza")
               .WithDescription("<Free Action>: Reduce <keyword=counter> by 1| Click to activate\nnCooldown: 5 turns")
               );

            //Soul of the Jungle Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("soulofthejungle")
               .WithTitle("Soul of the Jungle")
               .WithDescription("<End Turn>: Gain 2<keyword=block> until turn end| Click to activate\nnCooldown: 7 turns")
               );

            //Order of the Icefield Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("orderoftheicefield")
               .WithTitle("Order of the Icefield")
               .WithDescription("<End Turn>: Gain 1<keyword=frenzy> until turn end| Click to activate\nnCooldown: 8 turns")
               );

            //"Paenitete" Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("paenitete")
               .WithTitle("\"Paenitete\"")
               .WithDescription("<End Turn>: Apply 1<keyword=artemys.wildfrost.frostknights.burning> to enemies in row| Click to activate\nnCooldown: 6 turns")
               );

            //Twilight Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("twilight")
               .WithTitle("Twilight")
               .WithDescription("<End Turn>: Gain <keyword=barrage> and deal damage to self on attack| Click to activate\nOne use")
               );

            //Bloodline of Desecrated Earth Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("bloodlineofdesecratedearth")
               .WithTitle("Bloodline of Desecrated Earth")
               .WithDescription("<End Turn>: Apply 5<keyword=snow> to self, and gain barrage, reduced counter and snow enemies once snow is removed on self| Click to activate\nOne use")
               );

            //Originite Prime Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("originiteprime")
               .WithTitle("Originite Prime")
               .WithDescription("<End Turn>: Earn 5<keyword=blings>| Click to activate\nCooldown: 2 turns")
               );

            //Command: Meltdown Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("command:meltdown")
               .WithTitle("Command: Meltdown")
               .WithDescription("<End Turn>: Increase <keyword=attack> by <5>| Click to activate\nCooldown: 7 turns")
               );

            //Cooldown Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("cooldown")
               .WithTitle("Cooldown")
               .WithShowName(true)
               .WithDescription("The amount of turns you have to wait before being able to activate a skill")
               );

            //Greenhouse's Boundary Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("greenhouse'sboundary")
               .WithTitle("Greenhouse's Boundary")
               .WithDescription("<Free Action>: Apply <keyword=snow> equal to damage dealt until turn end| Click to activate\nOne use")
               );

            //Sami's Benevolence Keyword
            keywords.Add(
               new KeywordDataBuilder(this)
               .Create("sami'sbenevolence")
               .WithTitle("Sami's Benevolence")
               .WithDescription("<Free Action>: Gain add <+1><keyword=attack> to front ally on kill until turn end| Click to activate\nOne use")
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

            //Provoke Trait
            traits.Add(
                TraitCopy("Hellbent", "Provoke")
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("provoke");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("While Active Provoked Until Turn End To Enemies") };
                    })
                    );

            //Provoked Trait
            traits.Add(
                TraitCopy("Hellbent", "Provoked")
                .WithOverrides(TryGet<TraitData>("Barrage"), TryGet<TraitData>("Aimless"), TryGet<TraitData>("Longshot"))
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("provoked");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Hit All Provoke") };
                    })
                    );

            statusEffects = new List<StatusEffectDataBuilder>();

            //Code for status effects
            //Status 0: Add Block to All Allies
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
                    ((StatusEffectData)data).textInsert = "<{a}>damage";
                })
                );

            //Status 32: On Turn Apply Shroom To Enemies
            statusEffects.Add(
                StatusCopy("On Turn Apply Demonize To Enemies", "On Turn Apply Shroom To Enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Shroom");
                    ((StatusEffectData)data).textInsert = "<{a}><keyword=shroom>";
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

            //Status 38: Provoke Until Turn End
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTraitUntilTurnEnd>("Provoke Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .FreeModify<StatusEffectTraitUntilTurnEnd>(
                    (data) =>
                    {
                        data.targetConstraints = new TargetConstraint[0];
                    })
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTraitUntilTurnEnd)data).trait = TryGet<TraitData>("Provoke");
                })
                );

            //Status 39: Trial of Thorns Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Trial of Thorns Button")
                .WithType("trialofthorns")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Provoke Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 5;
                    ((ButtonCooldown)data).cooldownCount = 3;
                })
                );

            //Status 40: Provoked Until Turn End
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTraitUntilTurnEnd>("Provoked Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .FreeModify<StatusEffectTraitUntilTurnEnd>(
                    (data) =>
                    {
                        data.targetConstraints = new TargetConstraint[0];
                    })
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTraitUntilTurnEnd)data).trait = TryGet<TraitData>("Provoked");
                })
                );

            //Status 41: Hit All Provoke
            statusEffects.Add(
                StatusCopy("Hit All Enemies", "Hit All Provoke")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeProvoke>();
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 42: While Active Provoked Until Turn End To Enemies
            statusEffects.Add(
                StatusCopy("While Active Aimless To Enemies", "While Active Provoked Until Turn End To Enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Provoked Until Turn End");
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 43: Blazing Sun's Obeisance Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Blazing Sun's Obeisance Button")
                .WithType("blazingsunsobeisance")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Blazing Sun");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 6;
                    ((ButtonCooldown)data).cooldownCount = 2;
                })
                );

            //Status 44: Opprobrium Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Opprobrium Button")
                .WithType("opprobrium")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Typewriter In Hand");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 6;
                    ((ButtonCooldown)data).cooldownCount = 5;
                })
                );

            //Status 45: Trigger When Typewriter In Row Attacks
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTriggerWhenCertainAllyAttacks>("Trigger When Typewriter In Row Attacks")
                .WithCanBeBoosted(false)
                .WithText("Trigger when {0} in row attacks")
                .WithTextInsert("<card=artemys.wildfrost.frostknights.typewriter>")
                .WithType("")
                .FreeModify(
                    delegate (StatusEffectData data)
                    {
                        data.isReaction = true;
                        data.stackable = false;
                    })
                .SubscribeToAfterAllBuildEvent(
                    delegate (StatusEffectData data)
                    {
                        ((StatusEffectTriggerWhenCertainAllyAttacks)data).ally = TryGet<CardData>("typewriter");
                    })
                );

            //Status 46: Iron Defense Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Iron Defense Button")
                .WithType("irondefense")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Shell");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                    ((ButtonCooldown)data).maxCooldown = 13;
                    ((ButtonCooldown)data).cooldownCount = 13;
                })
                );

            //Status 47: Iron Defense Button Listener_1
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Iron Defense Button Listener_1")
                .WithType("irondefense_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Block");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                })
                );

            //Status 48: Barrage Until Turn End
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTraitUntilTurnEnd>("Barrage Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .FreeModify<StatusEffectTraitUntilTurnEnd>(
                    (data) =>
                    {
                        data.targetConstraints = new TargetConstraint[0];
                    })
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTraitUntilTurnEnd)data).trait = TryGet<TraitData>("Barrage");
                })
                );

            //Status 49: Saw of Strength Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Saw of Strength Button")
                .WithType("sawofstrength")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Barrage Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 6;
                    ((ButtonCooldown)data).cooldownCount = 4;
                })
                );

            //Status 50: Destreza Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Destreza Button")
                .WithType("destreza")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 5;
                    ((ButtonCooldown)data).cooldownCount = 5;
                })
                );

            //Status 51: Gain Block Until Turn End
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Gain Block Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Block");
                })
                );

            //Status 52: Soul of the Jungle Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Soul of the Jungle Button")
                .WithType("soulofthejungle")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Block Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 6;
                    ((ButtonCooldown)data).cooldownCount = 4;
                })
                );

            //Status 53: On Card Played Apply Apply Burning To EnemiesInRow
            statusEffects.Add(
                StatusCopy("On Card Played Apply Snow To EnemiesInRow", "On Card Played Apply Apply Burning To EnemiesInRow")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Burning");
                })
                );

            //Status 54: Paenitete Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Paenitete Button")
                .WithType("paenitete")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Burning");
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 7;
                    ((ButtonCooldown)data).cooldownCount = 5;
                })
                );

            //Status 52: Gain Frenzy Until Turn End
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Gain Frenzy Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("MultiHit");
                })
                );

            //Status 53: Order of the Icefield Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Order of the Icefield Button")
                .WithType("orderoftheicefield")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Frenzy Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 8;
                    ((ButtonCooldown)data).cooldownCount = 6;
                })
                );

            //Status 54: Twilight Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Twilight Button")
                .WithType("twilight")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Barrage");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 10;
                    ((ButtonCooldown)data).cooldownCount = 10;
                })
                );

            //Status 55: Twilight Button Listener_1
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Twilight Button Listener_1")
                .WithType("twilight_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("On Card Played Damage To Self");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 56: Bloodline of Desecrated Earth Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Bloodline of Desecrated Earth Button")
                .WithType("bloodlineofdesecratedearth")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Barrage");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 9;
                    ((ButtonCooldown)data).cooldownCount = 9;
                })
                );

            //Status 57: Bloodline of Desecrated Earth Button Listener_1
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Bloodline of Desecrated Earth Button Listener_1")
                .WithType("bloodlineofdesecratedearth_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Snow");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 58: Bloodline of Desecrated Earth Button Listener_2
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Bloodline of Desecrated Earth Button Listener_2")
                .WithType("bloodlineofdesecratedearth_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 59: Apply Snow to All Enemies When Losing All Snow
            statusEffects.Add(
                StatusCopy("Trigger When Self Or Ally Loses Block", "Apply Snow to All Enemies When Losing All Snow")
                .WithText("When self loses all <keyword=snow> apply <{a}><keyword=snow> to all enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Snow");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                    ((StatusEffectApplyXWhenUnitLosesY)data).whenAllLost = true;
                    ((StatusEffectApplyXWhenUnitLosesY)data).statusType = ("snow");
                    ((StatusEffectApplyXWhenUnitLosesY)data).allies = false;
                    ((StatusEffectData)data).affectedBySnow = false;
                    ((StatusEffectData)data).isReaction = false;
                })
                );

            //Status 60: Bloodline of Desecrated Earth Button Listener_3
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Bloodline of Desecrated Earth Button Listener_3")
                .WithType("bloodlineofdesecratedearth_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Apply Snow to All Enemies When Losing All Snow");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 61: On Turn Heal Mon3tr And Kal'tsit
            statusEffects.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Mon3tr And Kal'tsit")
                .WithText("Restore <{a}><keyword=health> to self and <card=artemys.wildfrost.frostknights.mon3tr>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[1]
                    {
                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                                TryGet<CardData>("mon3tr"),
                                TryGet<CardData>("kal'tsit")
                            }
                        }
                    };
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self | StatusEffectApplyX.ApplyToFlags.Allies;
                })
                );

            //Status 62: Summon Mon3tr
            statusEffects.Add(
                StatusCopy("Summon Gunk", "Summon Mon3tr")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("mon3tr");
                })
                );

            //Status 63: Instant Summon Mon3tr In Hand
            statusEffects.Add(
                StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Mon3tr In Hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mon3tr") as StatusEffectSummon;
                })
                );

            //Status 64: When Deployed Add Mon3tr To Hand
            statusEffects.Add(
                StatusCopy("When Deployed Add Junk To Hand", "When Deployed Add Mon3tr To Hand")
                .WithText("Add <card=artemys.wildfrost.frostknights.mon3tr> to your hand when played")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mon3tr In Hand");
                })
                );

            //Status 65: Heal Self Based On Damage Dealt
            statusEffects.Add(
                StatusCopy("Heal Front Ally Based On Damage Dealt", "Heal Self Based On Damage Dealt")
                .WithText("Heal self based on damage dealt")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 66: Originite Prime Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Originite Prime Button")
                .WithType("originiteprime")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Gold");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 2;
                    ((ButtonCooldown)data).cooldownCount = 2;
                })
                );

            //Status 67: Command: Meltdown Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Command: Meltdown Button")
                .WithType("command:meltdown")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 7;
                    ((ButtonCooldown)data).cooldownCount = 2;
                })
                );

            //Status 68: On Turn Reduce Attack To Self
            statusEffects.Add(
                StatusCopy("On Turn Apply Attack To Self", "On Turn Reduce Attack To Self")
                .WithText("Lose <-{a}><keyword=attack>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Attack");
                })
                );

            //Status 69: Reduce Cooldown
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReduceCooldown>("Reduce Cooldown")
                .WithText("Count down <keyword=artemys.wildfrost.frostknights.cooldown> by <{a}>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectData)data).canBeBoosted = true;
                    ((StatusEffectData)data).stackable = true;
                    ((StatusEffectData)data).textInsert = "<keyword=artemys.wildfrost.frostknights.cooldown>";
                    ((StatusEffectData)data).type = "counter down";
                })
                );

            //Status 70: On Turn Decrease Cooldown To Allies
            statusEffects.Add(
                StatusCopy("On Turn Apply Shell To Allies", "On Turn Decrease Cooldown To Allies")
                .WithText("Reduce <{a}> <keyword=artemys.wildfrost.frostknights.cooldown> to all allies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Cooldown");
                })
                );

            //Status 71: Reduce Max Cooldown
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReduceMaxCooldown>("Reduce Max Cooldown")
                .WithText("Reduce max <keyword=artemys.wildfrost.frostknights.cooldown> by <{a}>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectData)data).canBeBoosted = true;
                    ((StatusEffectData)data).stackable = true;
                    ((StatusEffectData)data).textInsert = "<keyword=artemys.wildfrost.frostknights.cooldown>";
                    ((StatusEffectData)data).type = "counter down";
                })
                );

            //Status 72: Greenhouse's Boundary Button
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Greenhouse's Boundary Button")
                .WithType("greenhouse'sboundary")
                .WithVisible(true)
                .WithIconGroupName("health")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("On Hit Equal Snow To Target Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((ButtonCooldown)data).maxCooldown = 2;
                    ((ButtonCooldown)data).cooldownCount = 0;
                })
                );

            //Status 73: On Hit Equal Snow To Target Until Turn End
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("On Hit Equal Snow To Target Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("On Hit Equal Snow To Target");
                })
                );

            //Status 74: On Kill Apply Attack To FrontAlly
            statusEffects.Add(
                StatusCopy("On Kill Apply Attack To Self", "On Kill Apply Attack To FrontAlly")
                .WithText("Add <+{a}><keyword=attack> to front ally on kill")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontAlly;
                })
                );

            //Status 75: On Turn Heal AllyBehind
            statusEffects.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal AllyBehind")
                .WithText("Restore <{a}><keyword=health> to ally behind")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                })
                );

            //Status 76: When Deployed Apply Heal To Allies
            statusEffects.Add(
                StatusCopy("When Deployed Apply Ink To Allies", "When Deployed Apply Heal To Allies")
                .WithText("When deployed, restore <{a}><keyword=health> to all allies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
                })
                );

            //Status 77: While Active Double Gold
            statusEffects.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectWhileActiveXDoubleGold>("While Active Double Gold")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("While Active Double Gold Earned")
                );

            cards = new List<CardDataBuilder>();

            //Code for units
            //Nian Card 1
            cards.Add(
                new CardDataBuilder(this).CreateUnit("nian", "Nian")
                .SetSprites("Nian.png", "Nian BG.png")
                .SetStats(11, 3, 5)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Well, well, you got orders for me?", "A knife, a life; weapons are the basis for society." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Iron Defense Button", 2),
                        SStack("Iron Defense Button Listener_1", 1)
                    };
                })
                );

            //Mudrock Card 2
            cards.Add(
                new CardDataBuilder(this).CreateUnit("mudrock", "Mudrock")
                .SetSprites("Mudrock.png", "Mudrock BG.png")
                .SetStats(10, 3, 6)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "The soil answers my will.", "Speak. The rocks are listening." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[5]
                    {
                        SStack("On Block Lost Heal", 5),
                        SStack("Bloodline of Desecrated Earth Button", 1),
                        SStack("Bloodline of Desecrated Earth Button Listener_1", 5),
                        SStack("Bloodline of Desecrated Earth Button Listener_2", 4),
                        SStack("Bloodline of Desecrated Earth Button Listener_3", 2),
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Pigheaded", 1)
                    };
                })
                );

            //Saria Card 3
            cards.Add(
                new CardDataBuilder(this).CreateUnit("saria", "Saria")
                .SetSprites("Saria.png", "Saria BG.png")
                .SetStats(10, 2, 5)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "It's my turn.", "Suppress them." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Heal Self and Allies", 3)
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
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("On Turn Apply Shell To Self", 3),
                        SStack("When Hit Gain Teeth To Self", 2),
                        SStack("Trial of Thorns Button", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Pigheaded", 1)
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
                .SetStats(9, 5, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Yes, Sir.", "Got it." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Teeth", 3),
                        SStack("Saw of Strength Button", 1)
                    };
                })
                );

            //Thorns Card 9
            cards.Add(
                new CardDataBuilder(this).CreateUnit("thorns", "Thorns")
                .SetSprites("Thorns.png", "Thorns BG.png")
                .SetStats(5, 2, 4)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Understood.", "Give me your orders." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Destreza Button", 1)
                    };
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
                .SetStats(5, 0, 6)
                .WithCardType("Friendly")
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
                .SetStats(6, 6, 4)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Awaiting only your orders.", "Yes, I am fully ready." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Blazing Sun's Obeisance Button", 1)
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
                .SetStats(6, 2, 3)
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
                .SetStats(13, 5, 5)
                .WithCardType("Friendly")
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
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Soul of the Jungle Button", 2)
                    };
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
                .SetStats(5, 3, 5)
                .WithCardType("Friendly")
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
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Twilight Button", 1),
                        SStack("Twilight Button Listener_1", 3)
                    };
                })
                );

            //Degenbrecher Card 23
            cards.Add(
                new CardDataBuilder(this).CreateUnit("degenbrecher", "Degenbrecher")
                .SetSprites("Degenbrecher.png", "Degenbrecher BG.png")
                .SetStats(7, 3, 4)
                .WithCardType("Friendly")
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
                .SetStats(5, 4, 4)
                .WithCardType("Friendly")
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
                .SetStats(2, 1, 1)
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
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Paenitete Button", 1)
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
                .SetStats(5, 2, 0)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Your orders, please.", "With pleasure." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("Opprobrium Button", 1),
                        SStack("On Hit Damage Bomed Target", 3),
                        SStack("Trigger When Typewriter In Row Attacks", 1)
                    };
                })
                );

            //Typewriter Card 34
            cards.Add(
                new CardDataBuilder(this).CreateUnit("typewriter", "Typewriter")
                .SetSprites("Typewriter.png", "Typewriter BG.png")
                .SetStats(1, 2, 4)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Weakness", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Spark", 1)
                    };
                })
                );

            //Schwarz Card 35
            cards.Add(
                new CardDataBuilder(this).CreateUnit("schwarz", "Schwarz")
                .SetSprites("Schwarz.png", "Schwarz BG.png")
                .SetStats(4, 5, 4)
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
                        TStack("Aimless", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Order of the Icefield Button", 3)
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

            //Mon3tr Card
            cards.Add(
                new CardDataBuilder(this).CreateUnit("mon3tr", "Mon3tr")
                .SetSprites("Mon3tr.png", "Mon3tr BG.png")
                .SetStats(10, 2, 2)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Reduce Attack To Self", 1),
                        SStack("Command: Meltdown Button", 5)
                    };
                })
                );

            //Code for items
            //Vanilla Soda Item 1
            cards.Add(
                new CardDataBuilder(this).CreateItem("vanillaSoda", "Vanilla Soda", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Vanilla Soda.png", "Vanilla Soda BG.png")
                .SetStats(null, null)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Reduce Cooldown", 2)
                    };
                })
                );

            //Rusted Razor Item 2
            cards.Add(
                new CardDataBuilder(this).CreateItem("rustedRazor", "Rusted Razor", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Rusted Razor.png", "Rusted Razor BG.png")
                .SetStats(null, 2)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Greenhouse's Boundary Button", 1)
                    };
                })
                );

            //Vinecreep Mortar Gunner Item 3
            cards.Add(
                new CardDataBuilder(this).CreateItem("vinecreepMortarGunner", "Vinecreep Mortar Gunner", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Vinecreep Mortar Gunner.png", "Vinecreep Mortar Gunner BG.png")
                .SetStats(null, 2)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Splash", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Kill Apply Attack To FrontAlly", 1)
                    };
                })
                );

            //Lancet-2 Item 4
            cards.Add(
                new CardDataBuilder(this).CreateUnit("lancet-2", "Lancet-2")
                .SetSprites("Lancet-2.png", "Lancet-2 BG.png")
                .SetStats(null, null, 3)
                .WithCardType("Clunker")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("On Turn Heal AllyBehind", 2),
                        SStack("Scrap", 2),
                        SStack("When Deployed Apply Heal To Allies", 1)
                    };
                })
                );

            //Worn-out Group Photo Item 5
            cards.Add(
                new CardDataBuilder(this).CreateItem("worn-outGroupPhoto", "Worn-out Group Photo", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Worn-out Group Photo.png", "Worn-out Group Photo BG.png")
                .SetStats(null, 1)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Weakness", 1)
                    };
                })
                );

            //Code for leaders
            //Kal'tsit leader
            cards.Add(
                new CardDataBuilder(this).CreateUnit("kal'tsit", "Kal'tsit")
                .SetSprites("Kal'tsit.png", "Kal'tsit BG.png")
                .SetStats(6, null, 4)
                .WithCardType("Leader")
                .FreeModify(
                (data) =>
                {
                    data.createScripts = new CardScript[]
                    {
                        GiveUpgrade(),
                        AddRandomHealth(-1,3),
                        AddRandomCounter(-1,1)
                    };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Heal Mon3tr And Kal'tsit", 3),
                        SStack("When Deployed Add Mon3tr To Hand", 1)
                    };
                })
                );

            //Doctor leader
            cards.Add(
                new CardDataBuilder(this).CreateUnit("doctor", "Doctor")
                .SetSprites("Doctor.png", "Doctor BG.png")
                .SetStats(6, 3, 4)
                .WithCardType("Leader")
                .FreeModify(
                (data) =>
                {
                    data.createScripts = new CardScript[]
                    {
                        GiveUpgrade(),
                        AddRandomHealth(-1,3),
                        AddRandomDamage(0,2),
                        AddRandomCounter(-1,1)
                    };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Decrease Cooldown To Allies", 2)
                    };
                })
                );

            //Closure leader
            cards.Add(
                new CardDataBuilder(this).CreateUnit("closure", "Closure")
                .SetSprites("Closure.png", "Closure BG.png")
                .SetStats(7, 3, 5)
                .WithCardType("Leader")
                .FreeModify(
                (data) =>
                {
                    data.createScripts = new CardScript[]
                    {
                        GiveUpgrade(),
                        AddRandomHealth(-1,3),
                        AddRandomCounter(-1,1),
                        AddRandomDamage(-1,1),
                    };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("Heal Self Based On Damage Dealt", 1),
                        SStack("Originite Prime Button", 5),
                        SStack("While Active Double Gold", 1)
                    };
                })
                );

            tribes = new List<ClassDataBuilder>();

            //Code for Tribes
            tribes.Add(
                TribeCopy("Clunk", "Rhodes")
                .WithFlag("Images/Rhodes Island Tribe Flag.png")
                .WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/card/draw_multi"))
                .SubscribeToAfterAllBuildEvent(
                (data) =>
                {
                    GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                    gameObject.name = "Player (Rhodes)";
                    data.characterPrefab = gameObject.GetComponent<Character>();
                    data.leaders = DataList<CardData>("kal'tsit", "closure", "doctor");
                    Inventory inventory = new Inventory();
                    inventory.deck.list = DataList<CardData>("vanillaSoda", "worn-outGroupPhoto", "rustedRazor", "rustedRazor", "rustedRazor", "rustedRazor", "rustedRazor", "lancet-2", "vinecreepMortarGunner").ToList(); //Some odds and ends
                    data.startingInventory = inventory;
                    RewardPool unitPool = CreateRewardPool("RhodesUnitPool", "Units", DataList<CardData>("typhon", "pozëmka", "fiammetta", "rosmontis", "reed", "degenbrecher", "surtr", "blaze", "qiubai", "nearl", "młynar", "saria", "mudrock", "nian")
                        );
                    RewardPool itemPool = CreateRewardPool("RhodesItemPool", "Items", DataList<CardData>("vinecreepMortarGunner", "vanillaSoda", "worn-outGroupPhoto")
                        );
                    RewardPool charmPool = CreateRewardPool("RhodesCharmPool", "Charms", DataList<CardUpgradeData>("CardUpgradeOverload")
                        );
                    data.rewardPools = new RewardPool[]
                    {
                        unitPool,
                        itemPool,
                        charmPool,
                        Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralUnitPool"),
                        Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralItemPool"),
                        Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralCharmPool"),
                        Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("GeneralModifierPool"),
                        Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("SnowUnitPool"),
                        Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("SnowItemPool"),
                        Deadpan.Enums.Engine.Components.Modding.Extensions.GetRewardPool("SnowCharmPool")
                    };
                })
            );

            preLoaded = true;
        }

        private void FixImage(Entity entity)
        {
            if (entity.display is Card card && !card.hasScriptableImage)
            {
                card.mainImage.gameObject.SetActive(true);
            }
        }

        public override void Load()
        {
            if (!preLoaded) { CreateModAssets(); } //The if statement is a flourish really. It makes the 2nd load of Load-Unload-Load faster.
            base.Load();                          //Actual loading
            IconStuff();
            Events.OnCheckEntityDrag += ButtonExt.DisableDrag;
            FloatingText ftext = GameObject.FindObjectOfType<FloatingText>(true);
            ftext.textAsset.spriteAsset.fallbackSpriteAssets.Add(assetSprites);
            GameMode gameMode = Get<GameMode>("GameModeNormal");
            gameMode.classes = gameMode.classes.Append(Get<ClassData>("Rhodes")).ToArray();
            Events.OnEntityCreated += FixImage;
            //Events.OnSceneChanged += ArknightsPhoto;
        }

        private void IconStuff()
        {
            this.CreateIcon("burningicon", ImagePath("burningicon.png").ToSprite(), "burning", "spice", Color.black, new KeywordData[] { Get<KeywordData>("burning") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("penanceTrialofThorns", ImagePath("penancebutton.png").ToSprite(), "trialofthorns", "counter", Color.black, new KeywordData[] { Get<KeywordData>("trialofthorns") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("nearlBlazingSunsObeisance", ImagePath("nearlbutton.png").ToSprite(), "blazingsunsobeisance", "counter", Color.black, new KeywordData[] { Get<KeywordData>("blazingsunsobeisance") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("pozëmkaOpprobrium", ImagePath("pozëmkabutton.png").ToSprite(), "opprobrium", "counter", Color.black, new KeywordData[] { Get<KeywordData>("opprobrium") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("nianIronDefense", ImagePath("nianbutton.png").ToSprite(), "irondefense", "counter", Color.black, new KeywordData[] { Get<KeywordData>("irondefense") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("hoshigumaSawofStrength", ImagePath("hoshigumabutton.png").ToSprite(), "sawofstrength", "counter", Color.black, new KeywordData[] { Get<KeywordData>("sawofstrength") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("thornsDestreza", ImagePath("thornsbutton.png").ToSprite(), "destreza", "counter", Color.black, new KeywordData[] { Get<KeywordData>("destreza") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("gavialSouloftheJungle", ImagePath("gavialbutton.png").ToSprite(), "soulofthejungle", "counter", Color.black, new KeywordData[] { Get<KeywordData>("soulofthejungle") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("fiammetta\"Paenitete\"", ImagePath("fiammettabutton.png").ToSprite(), "paenitete", "counter", Color.black, new KeywordData[] { Get<KeywordData>("paenitete") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("typhonOrderoftheIcefield", ImagePath("typhonbutton.png").ToSprite(), "orderoftheicefield", "counter", Color.black, new KeywordData[] { Get<KeywordData>("orderoftheicefield") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("surtrTwilight", ImagePath("surtrbutton.png").ToSprite(), "twilight", "counter", Color.black, new KeywordData[] { Get<KeywordData>("twilight") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("mudrockBloodlineofDesecratedEarth", ImagePath("mudrockbutton.png").ToSprite(), "bloodlineofdesecratedearth", "counter", Color.black, new KeywordData[] { Get<KeywordData>("bloodlineofdesecratedearth") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("closureOriginitePrime", ImagePath("closurebutton.png").ToSprite(), "originiteprime", "counter", Color.black, new KeywordData[] { Get<KeywordData>("originiteprime") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("mon3trCommand:Meltdown", ImagePath("mon3trbutton.png").ToSprite(), "command:meltdown", "counter", Color.black, new KeywordData[] { Get<KeywordData>("command:meltdown") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("rustedRazorGrennhouse'sBoundary", ImagePath("rustedrazorbutton.png").ToSprite(), "greenhouse'sboundary", "counter", Color.black, new KeywordData[] { Get<KeywordData>("greenhouse'sboundary") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("rustedRazorSami'sBenevolence", ImagePath("rustedrazorbutton2.png").ToSprite(), "sami'sbenevolence", "counter", Color.black, new KeywordData[] { Get<KeywordData>("sami'sbenevolence") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            Events.OnCheckEntityDrag -= ButtonExt.DisableDrag;
            GameMode gameMode = Get<GameMode>("GameModeNormal");
            gameMode.classes = RemoveNulls(gameMode.classes);
            Events.OnEntityCreated -= FixImage;
            //Events.OnSceneChanged -= ArknightsPhoto;
        }

        internal T[] RemoveNulls<T>(T[] data) where T : DataFile
        {
            List<T> list = data.ToList();
            list.RemoveAll(x => x == null || x.ModAdded == this);
            return list.ToArray();
        }

        public override List<T> AddAssets<T, Y>()           //This method is called 6-7 times in base.Load() for each Builder. Can you name them all?
        {
            var typeName = typeof(Y).Name;
            Debug.Log(typeName);
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
                //case nameof(CardUpgradeData):
                //    return cardUpgrades.Cast<T>().ToList();  //Loads charms
                //case nameof(GameModifierData):                //Game Modifiers take the form of bells, both sun and gloom.
                //    return bells.Cast<T>().ToList();
                case nameof(ClassData):                       //To avoid confusion with C# classes, the word "tribe" will be used to talk about an instance of ClassData.
                    return tribes.Cast<T>().ToList();
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
            StatusIconExt icon = gameObject.AddComponent<StatusIconExt>();
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
        public static GameObject CreateButtonIcon(this WildfrostMod mod, string name, Sprite sprite, string type, string copyTextFrom, Color textColor, KeywordData[] keys)
        {
            GameObject gameObject = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
            StatusIconExt icon = gameObject.AddComponent<StatusIconExt>();
            Dictionary<string, GameObject> cardIcons = CardManager.cardIcons;
            icon.animator = gameObject.AddComponent<ButtonAnimator>();
            icon.button = gameObject.AddComponent<ButtonExt>();
            icon.animator.button = icon.button;
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
            rectTransform.sizeDelta *= 0.008f;
            gameObject.SetActive(true);
            icon.type = type;
            cardIcons[type] = gameObject;
            gameObject.AddComponent<UINavigationItem>();

            return gameObject;
        }
        public static T CreateStatusButton<T>(this WildfrostMod mod, string name, string type, string iconGroup = "counter") where T : StatusEffectData
        {
            T status = ScriptableObject.CreateInstance<T>();
            status.name = name;
            status.targetConstraints = new TargetConstraint[0];
            status.type = type;
            status.isStatus = true;
            status.iconGroupName = iconGroup;
            status.visible = true;
            status.stackable = false;
            return status;
        }
        public static T ApplyX<T>(this T t, StatusEffectData effectToApply, StatusEffectApplyX.ApplyToFlags flags) where T : StatusEffectApplyX
        {
            t.effectToApply = effectToApply;
            t.applyToFlags = flags;
            return t;
        }
        public static T Register<T>(this T status, WildfrostMod mod) where T : StatusEffectData
        {
            status.ModAdded = mod;
            AddressableLoader.AddToGroup<StatusEffectData>("StatusEffectData", status);
            return status;
        }
    }

    public class ButtonExt : Button
    {
        internal StatusIconExt Icon => GetComponent<StatusIconExt>();

        internal static ButtonExt dragBlocker = null;

        internal Entity Entity => Icon?.target;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            dragBlocker = this;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            DisableDragBlocking();
        }

        public void DisableDragBlocking()
        {
            if (dragBlocker == this)
            {
                dragBlocker = null;
            }
        }

        public static void DisableDrag(ref Entity arg0, ref bool arg1)
        {
            if (dragBlocker == null || arg0 != dragBlocker.Entity)
            {
                return;
            }
            arg1 = false;
        }
    }

    public interface IStatusToken
    {
        void ButtonCreate(StatusIconExt icon);

        void RunButtonClicked();

        IEnumerator ButtonClicked();


    }

    public class StatusIconExt : StatusIcon
    {
        public ButtonAnimator animator;
        public ButtonExt button;
        private IStatusToken effectToken;

        public override void Assign(Entity entity)
        {
            base.Assign(entity);
            SetText();
            onValueDown.AddListener(delegate { Ping(); });
            onValueUp.AddListener(delegate { Ping(); });
            afterUpdate.AddListener(SetText);
            onValueDown.AddListener(CheckDestroy);

            StatusEffectData effect = entity.FindStatus(type);
            if (effect is IStatusToken effect2)
            {
                effectToken = effect2;
                effect2.ButtonCreate(this);
                button.onClick.AddListener(effectToken.RunButtonClicked);
                onDestroy.AddListener(DisableDragBlocker);
            }
        }

        public void DisableDragBlocker()
        {
            button.DisableDragBlocking();
        }
    }

    public class StatusTokenApplyX : StatusEffectApplyX, IStatusToken
    {
        //Standard Code I wish I can put into IStatusToken
        [Flags]
        public enum PlayFromFlags
        {
            None = 0,
            Board = 1,
            Hand = 2,
            Draw = 4,
            Discard = 8
        }

        public PlayFromFlags playFrom = PlayFromFlags.Board | PlayFromFlags.Hand;
        public bool finiteUses = false;
        public bool oncePerTurn = false;
        protected bool unusedThisTurn = true;
        public bool endTurn = false;
        public float timing = 0.2f;

        public override void Init()
        {
            base.Init();
        }

        public override bool RunTurnStartEvent(Entity entity)
        {
            if (entity.data.cardType.name == "Leader")
            {
                unusedThisTurn = true;
            }
            return base.RunTurnStartEvent(entity);
        }

        public virtual void RunButtonClicked()
        {
            if ((bool)References.Battle && References.Battle.phase == Battle.Phase.Play
                && CorrectPlace()
                && !target.IsSnowed
                && target.owner == References.Player
                && !target.silenced
                && (!oncePerTurn || unusedThisTurn))

            {
                target.StartCoroutine(ButtonClicked());
                unusedThisTurn = false;
            }

            if ((bool)target.IsSnowed || target.silenced)
            {
                NoTargetTextSystem noText = GameSystem.FindObjectOfType<NoTargetTextSystem>();
                if (noText != null)
                {
                    TMP_Text textElement = noText.textElement;
                    if ((bool)target.IsSnowed)
                    {
                        textElement.text = "Snowed!";
                    }
                    if ((bool)target.silenced)
                    {
                        textElement.text = "Inked!";
                    }
                    noText.PopText(target.transform.position);
                }
            }

        }

        public bool CheckFlag(PlayFromFlags flag) => (playFrom & flag) != 0;

        public virtual bool CorrectPlace()
        {
            if (CheckFlag(PlayFromFlags.Board) && Battle.IsOnBoard(target))
            {
                return true;
            }
            if (CheckFlag(PlayFromFlags.Hand) && References.Player.handContainer.Contains(target))
            {
                return true;
            }
            if (CheckFlag(PlayFromFlags.Draw) && target.preContainers.Contains(References.Player.drawContainer))
            {
                return true;
            }
            if (CheckFlag(PlayFromFlags.Discard) && target.preContainers.Contains(References.Player.discardContainer))
            {
                return true;
            }
            return false;
        }

        //Main Code
        public int fixedAmount = 0;
        public int hitDamage = 0;

        public IEnumerator ButtonClicked()
        { 
            if (hitDamage != 0)
            {
                List<Entity> enemies = GetTargets();
                int trueAmount = (hitDamage == -1) ? count : hitDamage;
                foreach (Entity enemy in enemies)
                {
                    if (enemy.IsAliveAndExists())
                    {
                        Hit hit = new Hit(target, enemy, trueAmount);
                        hit.canRetaliate = false;
                        yield return hit.Process();
                    }

                }

            }
            yield return Run(GetTargets(), fixedAmount);
            List<StatusTokenApplyXListener> listeners = FindListeners();
            foreach (StatusTokenApplyXListener listener in listeners)
            {
                yield return listener.Run();
            }
            target.display.promptUpdateDescription = true;
            yield return PostClick();
        }

        public List<StatusTokenApplyXListener> FindListeners()
        {
            List<StatusTokenApplyXListener> listeners = new List<StatusTokenApplyXListener>();
            foreach (StatusEffectData status in target.statusEffects)
            {
                if (status is StatusTokenApplyXListener status2)
                {
                    if (status2.type == type + "_listener")
                    {
                        listeners.Add(status2);
                    }
                }
            }
            return listeners;
        }

        public virtual IEnumerator PostClick()
        {
            if (finiteUses)
            {
                count--;
                if (count == 0)
                {
                    yield return Remove();
                }
                target.promptUpdate = true;
            }
            if (endTurn)
            {
                yield return Sequences.Wait(timing);
                References.Player.endTurn = true;
            }
        }

        public virtual void ButtonCreate(StatusIconExt icon)
        {
            return;
        }
    }

    public class StatusTokenApplyXListener : StatusEffectApplyX
    {
        public IEnumerator Run()
        {
            yield return Run(GetTargets());
        }
    }

    public class ButtonCooldown : StatusTokenApplyX
    {
        public int cooldownCount;
        public int maxCooldown;

        public void SetCooldownText()
        {
            StatusIcon icon = target.display.FindStatusIcon(type);
            if (cooldownCount <= 0)
            {
                icon.textElement.text = "";
            }
            else
            {
                icon.textElement.text = string.Format(icon.textFormat, cooldownCount);
            }
            icon.StartCoroutine(Pinging());
        }

        public IEnumerator Pinging()
        {
            yield return new WaitForSeconds(0.01f);
            StatusIcon icon = target.display.FindStatusIcon(type);
            icon.Ping();
        }


        public override void Init()
        {
            OnTurnStart += CooldownCountDown;
        }

        public bool cooldown => this.cooldownCount > 0;

        public override void RunButtonClicked()
        {
            if (cooldown)
            {
                NoTargetTextSystem noText = GameSystem.FindObjectOfType<NoTargetTextSystem>();
                if (noText != null)
                {
                    TMP_Text textElement = noText.textElement;
                    if (cooldown)
                    {
                        textElement.text = $"On Cooldown! ({cooldownCount} turns)!";
                    }
                    noText.PopText(target.transform.position);
                }
                return;
            }
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
            base.RunButtonClicked();
        }

        public override IEnumerator PostClick()
        {
            cooldownCount = maxCooldown;
            SetCooldownText();
            yield return base.PostClick();
        }

        public override void ButtonCreate(StatusIconExt icon)
        {
            icon.afterUpdate.RemoveListener(icon.SetText);
            icon.afterUpdate.AddListener(SetCooldownText);
            SetCooldownText();
        }

        private IEnumerator CooldownCountDown(Entity entity)
        {
            if (entity != target)
            {
                yield break;
            }
            ButtonCooldown status = this;
            int amount = 1;
            global::Events.InvokeStatusEffectCountDown((StatusEffectData)status, ref amount);
            if (amount != 0 && Battle.IsOnBoard(target))
                cooldownCount -= amount;
            SetCooldownText();
        }
    }

    public class StatusEffectUntilTurnEnd : StatusEffectInstant
    {
        public override void Init()
        {
            base.OnCardMove += CheckPosition;
            Events.OnBattleTurnEnd += Remove;
        }

        public void OnDestroy()
        {
            Events.OnBattleTurnEnd -= Remove;
        }

        public IEnumerator CheckPosition(Entity entity)
        {
            if (entity == target && (target.containers.Contains(References.Player.drawContainer) || target.containers.Contains(References.Player.discardContainer)))
            {
                yield return Remove();
            }
            yield break;
        }

        public virtual void Remove(int _)
        {
            target.StartCoroutine(Remove());
        }

        public override IEnumerator Process()
        {
            yield break;
        }

    }

    public class StatusEffectApplyXUntilTurnEnd : StatusEffectApplyXInstant
    {
        public override bool HasStackRoutine => true;
        public override bool HasEndRoutine => true;
        public override void Init()
        {
            applyToFlags = ApplyToFlags.Self; //DO NOT CHANGE THE APPLYTOFLAGS

            base.OnCardMove += CheckPosition;
            Events.OnBattleTurnEnd += Remove;
        }

        public void OnDestroy()
        {
            Events.OnBattleTurnEnd -= Remove;
        }

        public override IEnumerator StackRoutine(int stacks)
        {
            yield return Run(GetTargets());
        }

        public IEnumerator CheckPosition(Entity entity)
        {
            if (entity == target && (target.containers.Contains(References.Player.drawContainer) || target.containers.Contains(References.Player.discardContainer)))
            {
                yield return Remove();
            }
            yield break;
        }

        public virtual void Remove(int _)
        {
            target.StartCoroutine(Remove());
        }

        public override IEnumerator EndRoutine()
        {
            if ((bool)target)
            {
                StatusEffectData effect = target.statusEffects.FirstOrDefault((e) => e.name == effectToApply.name);
                if (effect != default(StatusEffectData))
                {
                    yield return effect.RemoveStacks(count, true);
                    target.display.promptUpdateDescription = true;
                    target.PromptUpdate();
                }
            }
        }

    }

    public class StatusEffectTraitUntilTurnEnd : StatusEffectUntilTurnEnd
    {
        public TraitData trait;

        public Entity.TraitStacks added;

        public int addedAmount = 0;

        public override bool HasStackRoutine => true;

        public override bool HasEndRoutine => true;

        public override void Init()
        {
            base.Init();
        }

        public override IEnumerator BeginRoutine()
        {
            added = target.GainTrait(trait, count, temporary: true);
            yield return target.UpdateTraits();
            addedAmount += count;
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
        }

        public override IEnumerator StackRoutine(int stacks)
        {
            added = target.GainTrait(trait, stacks, temporary: true);
            yield return target.UpdateTraits();
            addedAmount += stacks;
            target.display.promptUpdateDescription = true;
            target.PromptUpdate();
        }

        public override IEnumerator EndRoutine()
        {
            if ((bool)target)
            {
                if (added != null)
                {
                    added.count -= addedAmount;
                    added.tempCount -= addedAmount;
                }

                addedAmount = 0;
                yield return target.UpdateTraits(added);
                target.display.promptUpdateDescription = true;
                target.PromptUpdate();
            }
        }
    }

    public class StatusEffectBoostUntilTurnEnd : StatusEffectUntilTurnEnd
    {
        public override void Init()
        {
            base.Init();
        }
        public override IEnumerator Process()
        {
            int amount = GetAmount();
            if ((bool)target.curveAnimator)
            {
                target.curveAnimator.Ping();
            }

            target.effectBonus += amount;
            target.PromptUpdate();


            return base.Process();
        }

        public override bool RunStackEvent(int stacks)
        {
            int amount = GetAmount();
            if ((bool)target.curveAnimator)
            {
                target.curveAnimator.Ping();
            }

            target.effectBonus += stacks;
            target.PromptUpdate();
            return base.RunStackEvent(stacks);
        }

        public override void Remove(int _)
        {
            target.effectBonus -= GetAmount();
            base.Remove(_);
        }
    }

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

    public class StatusEffectInstantReduceCooldown : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            foreach (StatusEffectData status in target.statusEffects)
            {
                if (status is ButtonCooldown button)
                {
                    button.cooldownCount -= GetAmount();
                }
            }
            yield return base.Process();
        }
    }

    public class StatusEffectInstantReduceMaxCooldown : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            foreach (StatusEffectData status in target.statusEffects)
            {
                if (status is ButtonCooldown button)
                {
                    button.maxCooldown -= GetAmount();
                }
            }
            yield return base.Process();
        }
    }

    public class StatusEffectWhileActiveXDoubleGold : StatusEffectWhileActiveX
    {
        public int storedGold = 0;

        public override bool RunBeginEvent()
        {
            Character player = References.Player;
            if ((bool)player && player.data != null && (bool)player.data.inventory)
            {
                storedGold = player.data.inventory.gold.Value;
                Debug.Log("guarde oro");
                Debug.Log($"\n {storedGold}");
            }
            return base.RunBeginEvent();
        }

        public override bool RunEndEvent()
        {
            Debug.Log("termine evento");
            DoubleGold();
            return base.RunEndEvent();
        }

        public void DoubleGold()
        { 
            Character player = References.Player;
            if ((bool)player && player.data != null && (bool)player.data.inventory)
            {
                int newGold = player.data.inventory.gold.Value;
                if (newGold > storedGold)
                {
                    int goldAdded = newGold - storedGold;
                    References.PlayerData.inventory.goldOwed += goldAdded;
                    storedGold = player.data.inventory.gold.Value;
                    Debug.Log("guarde oro jeje");
                    Debug.Log($"\n {storedGold}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffectApplyXWhenUnitLosesY), "CheckEntity")]
    public class whenAllLostPatch
    {
        static bool Prefix(StatusEffectApplyXWhenUnitLosesY __instance, ref bool ___whenAllLost, Entity entity)
        {
            __instance.currentAmounts.TryGetValue(entity.data.id, out var value);
            int currentAmount = __instance.GetCurrentAmount(entity);
            __instance.currentAmounts[entity.data.id] = currentAmount;
            int num = currentAmount - value;
            if (num < 0 && (!__instance.whenAllLost || currentAmount == 0) && !__instance.target.silenced && (!__instance.targetMustBeAlive || (__instance.target.alive && Battle.IsOnBoard(__instance.target))))
            {
                ActionQueue.Stack(new ActionSequence(__instance.Lost(-num))
                {
                    note = __instance.name,
                    priority = __instance.eventPriority
                }, fixedPosition: true);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(References), nameof(References.Classes), MethodType.Getter)]
    static class FixClassesGetter
    {
        static void Postfix(ref ClassData[] __result) => __result = AddressableLoader.GetGroup<ClassData>("ClassData").ToArray();
    }
}
