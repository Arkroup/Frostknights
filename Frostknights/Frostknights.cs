using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using NaughtyAttributes;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.Utils;
using WildfrostHopeMod.VFX;
using static Building;
using static CardData;
using static DynamicTutorialSystem;
using static SfxSystem;
using static Steamworks.InventoryItem;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Frostknights
{
    public class VFXHelper
    {
        public static GIFLoader VFX;
        public static SFXLoader SFX;
    }

    public class Frostknights : WildfrostMod
    {
        internal static FXHelper fx;

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
            string[] snipers = { "artemys.wildfrost.frostknights.rosmontis","artemys.wildfrost.frostknights.exusiai","artemys.wildfrost.frostknights.archetto","artemys.wildfrost.frostknights.fiammetta","artemys.wildfrost.frostknights.w","artemys.wildfrost.frostknights.pozëmka","artemys.wildfrost.frostknights.schwarz","artemys.wildfrost.frostknights.typhon","artemys.wildfrost.frostknights.fartooth"
            };
            string[] vanguards = { "artemys.wildfrost.frostknights.saileach","artemys.wildfrost.frostknights.elysium","artemys.wildfrost.frostknights.myrtle"
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

        public override string[] Depends => new string[] { "hope.wildfrost.vfx" }; //The GUID of other mods that your mod requires. This tutorial has none of that.

        public override string Title => "Frostknights";

        public override string Description => "This mod adds a new tribe to the game based on the game Arknights, as well as many operators as companions, several items, charms, and more.\r\n\r\nCurrently there around 40 new companions! I'll do updates of each class and progressively add more, as well as slowly edit and tweak already released companions for balance.\r\n\r\nPlease do tell me your thoughts on balance! I'm pretty new to the game so any help is welcome. \r\n\r\nThanks a lot for all the help to the modding channel on the discord! And also thanks a lot to the Tokens mod people for tokens (really cool mod go check it out) and Pokefrost (also really cool mod go check it out) for the help and for letting me use their effects! Thanks also to @artemis_w for the art for the tribe flag and the chain of the charms! And @sunnysoap for letting me use their effects!\r\n\r\nAll the card art is owned by Hypergryph";

        public T TryGet<T>(string name) where T : DataFile
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

        public static List<object> assets = new List<object>();
        public override List<T> AddAssets<T, Y>()
        {
            if (assets.OfType<T>().Any())
                Debug.LogWarning($"[{Title}] adding {typeof(Y).Name}s: {assets.OfType<T>().Select(a => a._data.name).Join()}");
            return assets.OfType<T>().ToList();
        }
        private bool preLoaded = false;                      //Used to prevent redundantly reconstructing our data. Not truly necessary.
        public static Frostknights instance;

        public TMP_SpriteAsset assetSprites;

        public override TMP_SpriteAsset SpriteAsset => assetSprites;

        private void CreateModAssets()
        {
            //Animations
            VFXHelper.VFX = new GIFLoader(this, this.ImagePath("Anim"));
            VFXHelper.VFX.RegisterAllAsApplyEffect();

            VFXHelper.SFX = new SFXLoader(ImagePath("Sounds"));
            VFXHelper.SFX.RegisterAllSoundsToGlobal();

            if (fx == null)
            {
                fx = new FXHelper(this, "Anim", "Sounds");
            }

            //Icons
            assetSprites = HopeUtils
                .CreateSpriteAsset("assetSprites", directoryWithPNGs: this.ImagePath("Sprites"), textures: [], sprites: []);

            foreach (var character in assetSprites.spriteCharacterTable)
            {
                character.scale = 1.3f;
            }

            //Code for keywords
            //Splash Keyword
            assets.Add(
                new KeywordDataBuilder(this)
                .Create("splash")
                .WithTitle("Splash")
                .WithShowName(true) //Shows name in Keyword box (as opposed to a nonexistant icon).
                .WithDescription("Deal damage to adjacent enemies", SystemLanguage.English) //Format is body|note.
                .WithCanStack(true)
                );

            //Taunt Keyword
            assets.Add(
                new KeywordDataBuilder(this)
                .Create("taunt")
                .WithTitle("Taunt")
                .WithShowName(true)
                .WithDescription("All enemies are <keyword=artemys.wildfrost.frostknights.taunted>", SystemLanguage.English)
                .WithCanStack(false)
                );

            //Taunted Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("taunted")
               .WithTitle("Taunted")
               .WithShowName(true)
               .WithDescription("Target only enemies with <keyword=artemys.wildfrost.frostknights.taunt>|Hits them all!", SystemLanguage.English)
               .WithCanStack(false)
               );

            //Provoke Keyword
            assets.Add(
                new KeywordDataBuilder(this)
                .Create("provoke")
                .WithTitle("Provoke")
                .WithShowName(true)
                .WithDescription("All enemies are <keyword=artemys.wildfrost.frostknights.provoked>", SystemLanguage.English)
                .WithCanStack(false)
                );

            //Provoked Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("provoked")
               .WithTitle("Provoked")
               .WithShowName(true)
               .WithDescription("Target only enemies with <keyword=artemys.wildfrost.frostknights.provoke>|Hits them all!", SystemLanguage.English)
               .WithCanStack(false)
               );

            //Burnage Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("burnage")
               .WithTitle("Burnage")
               .WithShowName(false)
               .WithDescription("When hit, deal <Inferno> damage to allies in the row. Take 1 <Fire> damage every turn. | Clears when hit, counts down by 2 every turn.", SystemLanguage.English)
               .WithIconName("burnageicon")
               );

            //Trial of Thorns Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("trialofthorns")
               .WithTitle("Trial of Thorns")
               .WithDescription("<End Turn>: Gain <keyword=artemys.wildfrost.frostknights.provoke>, 5<keyword=artemys.wildfrost.frostknights.bones>,and 5<keyword=shell> for a turn | Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Blazing Sun's Obeisance Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("blazingsunsobeisance")
               .WithTitle("Blazing Sun's Obeisance")
               .WithDescription("<Free Action>: Summon <card=artemys.wildfrost.frostknights.blazingSun>| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Opprobrium Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("opprobrium")
               .WithTitle("Opprobrium")
               .WithDescription("<Free Action>: Add <card=artemys.wildfrost.frostknights.typewriter> to your hand| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Iron Defense Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("irondefense")
               .WithTitle("Iron Defense")
               .WithDescription("<End Turn>: Give 1<keyword=block> and 2<keyword=shell> to all allies| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Saw of Strength Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("sawofstrength")
               .WithTitle("Saw of Strength")
               .WithDescription("<Free Action>: Gain <keyword=barrage> for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Destreza Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("destreza")
               .WithTitle("Destreza")
               .WithDescription("<Free Action>: Reduce <keyword=counter> by 1| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Soul of the Jungle Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("soulofthejungle")
               .WithTitle("Soul of the Jungle")
               .WithDescription("<End Turn>: Gain 2<keyword=block> until turn end| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Order of the Icefield Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("orderoftheicefield")
               .WithTitle("Order of the Icefield")
               .WithDescription("<Free Action>: Gain <keyword=artemys.wildfrost.frostknights.wideshot>| Click to activate\nOne use", SystemLanguage.English)
               );

            //"Paenitete" Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("paenitete")
               .WithTitle("\"Paenitete\"")
               .WithDescription("<End Turn>: Apply 4<keyword=artemys.wildfrost.frostknights.burnage> to enemies in row| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Twilight Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("twilight")
               .WithTitle("Twilight")
               .WithDescription("<Free Action>: Gain <keyword=barrage> and deal 3 damage to self on attack| Click to activate\nOne use", SystemLanguage.English)
               );

            //Bloodline of Desecrated Earth Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("bloodlineofdesecratedearth")
               .WithTitle("Bloodline of Desecrated Earth")
               .WithDescription("<Free Action>: Apply 5<keyword=snow> to self, and gain barrage, reduced counter and snow enemies once snow is removed on self| Click to activate\nOne use", SystemLanguage.English)
               );

            //Originite Prime Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("originiteprime")
               .WithTitle("Originite Prime")
               .WithDescription("<Free Action>: Until the end of this turn, earn double<keyword=blings>| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Command: Meltdown Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("command:meltdown")
               .WithTitle("Command: Meltdown")
               .WithDescription("<End Turn>: Increase <keyword=attack> by 5| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Cooldown Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("cooldown")
               .WithTitle("Cooldown")
               .WithShowName(true)
               .WithIconName("cooldownicon")
               .WithDescription("The amount of turns you have to wait before being able to activate a skill", SystemLanguage.English)
               );

            //Greenhouse's Boundary Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("greenhouse'sboundary")
               .WithTitle("Greenhouse's Boundary")
               .WithDescription("<Free Action>: Apply <keyword=snow> equal to damage dealt until turn end| Click to activate\nOne use", SystemLanguage.English)
               );

            //Anatta Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("anatta")
               .WithTitle("Anatta")
               .WithDescription("<Free Action>: Gain 1<keyword=frenzy>| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Calcification Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("calcification")
               .WithTitle("Calcification")
               .WithDescription("<End Turn>: Change to Calcification stance: increases enemy counters and applies bom| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Medicine Dispensing Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("medicinedispensing")
               .WithTitle("Medicine Dispensing")
               .WithDescription("<End Turn>: Change to Medicine Dispensing stance: heals allies and self| Click to activate\nCooldown: {0} turns" +
               "| Click to activate\nCooldown: 3 turns", SystemLanguage.English)
               );

            //Feathershine Arrows Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("feathershinearrows")
               .WithTitle("Feathershine Arrows")
               .WithDescription("<Free Action>: Add <card=artemys.wildfrost.frostknights.fartoothTargeting> to your hand| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Spirit Burst Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("spiritburst")
               .WithTitle("Spirit Burst")
               .WithDescription("<End Turn>: Gain 3<keyword=frenzy> and <keyword=artemys.wildfrost.frostknights.blindshot> for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Monitor Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("monitor")
               .WithTitle("Monitor")
               .WithDescription("<Free Action>: Apply 3<keyword=weakness> to all enemies for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Inheritance of Faith Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("inheritanceoffaith")
               .WithTitle("Inheritance of Faith")
               .WithDescription("<Free Action>: Heal active companion with the lowest <keyword=health> by <4>| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Divine Avatar Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("divineavatar")
               .WithTitle("Divine Avatar")
               .WithDescription("<Free Action>: Heal active companion with the lowest <keyword=health> equal to damage dealt and gain attack equal to health for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Boiling Burst Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("boilingburst")
               .WithTitle("Boiling Burst")
               .WithDescription("<Free Action>: Gain <keyword=barrage>, deal 3 damage to self and 1<keyword=frenzy> for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Charging Mode Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("chargingmode")
               .WithTitle("Charging Mode")
               .WithDescription("<End Turn>: Trigger self 3 times| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Judgement Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("judgement")
               .WithTitle("Judgement")
               .WithDescription("<Free Action>: Gain 2 extra <keyword=frenzy>, <keyword=artemys.wildfrost.frostknights.splash> 2 and <keyword=artemys.wildfrost.frostknights.blindshot> for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Chi Xiao - Unsheath Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("chixiao-unsheath")
               .WithTitle("Chi Xiao - Unsheath")
               .WithDescription("<Free Action>: Deal 4 damage to enemies in the row| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Blessing of Heroism Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("blessingofheroism")
               .WithTitle("Blessing of Heroism")
               .WithDescription("<Free Action>: Apply 2<keyword=spice> to self and ally in front when triggering for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Ember of Life Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("emberoflife")
               .WithTitle("Ember of Life")
               .WithDescription("<Free Action>: On turn, apply 1<keyword=overload> to all enemies| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Blindshot Keyword
            assets.Add(
                new KeywordDataBuilder(this)
                .Create("blindshot")
                .WithTitle("Blindshot")
                .WithTitleColour(new Color(0.50f, 0.50f, 0.85f))
                .WithShowName(true) //Shows name in Keyword box (as opposed to a nonexistant icon).
                .WithDescription("Hits a random enemy in any row", SystemLanguage.English) //Format is body|note.
                .WithCanStack(false)
                );

            //Bones Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("bones")
               .WithTitle("Bones")
               .WithShowName(false)
               .WithDescription("Deals damage to attackers, lose equal to damage taken from hits|Does teeth damage", SystemLanguage.English)
               .WithIconName("bonesicon")
               );

            //Wideshot Keyword
            assets.Add(
                new KeywordDataBuilder(this)
                .Create("wideshot")
                .WithTitle("Wideshot")
                .WithShowName(show: true)
                .WithDescription("Also hits the unit on the opposite row of the target.")
                .WithCanStack(show: false)
                );

            //Pierce Keyword
            assets.Add(
                new KeywordDataBuilder(this)
                .Create("pierce")
                .WithTitle("Pierce")
                .WithShowName(show: true)
                .WithDescription("Also hits the unit behind the target.")
                .WithCanStack(show: false)
                );

            //Fracture Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("fracture")
               .WithTitle("Fracture")
               .WithShowName(false)
               .WithDescription("Take additional damage from all sources| Counts down every turn.", SystemLanguage.English)
               .WithIconName("fractureicon")
               );

            //Final Tactics Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("finaltactics")
               .WithTitle("Final Tactics")
               .WithDescription("<Free Action>: Gain <keyword=artemys.wildfrost.frostknights.pierce> for a turn| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Bones Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("defense")
               .WithTitle("Defense")
               .WithShowName(false)
               .WithDescription("Blocks damage|Does not count down!", SystemLanguage.English)
               .WithIconName("defenseicon")
               );

            //Burden of Cinder and Ash Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("burdenofcinderandash")
               .WithTitle("Burden of Cinder and Ash")
               .WithDescription("<Free Action>: Change to Burden of Cinder and Ash stance: Deal <6> extra damage to <keyword=snow>'d targets, but increase max counter by 2.| Click to activate\nCooldown: {0} turns", SystemLanguage.English)
               );

            //Unquenchable Front Keyword
            assets.Add(
               new KeywordDataBuilder(this)
               .Create("unquenchablefront")
               .WithTitle("Unquenchable Front")
               .WithDescription("<Free Action>: Change to Unquenchable Front stance: On turn, restore <2><keyword=health> to self, and decrease max counter by 2.| Click to activate\nCooldown: {0} turns" +
               "| Click to activate\nCooldown: 3 turns", SystemLanguage.English)
               );

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
            assets.Add(splash);

            //Taunt Trait
            assets.Add(
                TraitCopy("Hellbent", "Taunt")
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("taunt");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("While Active Taunted To Enemies") };
                    })
                    );

            //Taunted Trait
            assets.Add(
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
            assets.Add(
                TraitCopy("Hellbent", "Provoke")
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("provoke");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("While Active Provoked Until Turn End To Enemies") };
                    })
                    );

            //Provoked Trait
            assets.Add(
                TraitCopy("Hellbent", "Provoked")
                .WithOverrides(TryGet<TraitData>("Barrage"), TryGet<TraitData>("Aimless"), TryGet<TraitData>("Longshot"))
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("provoked");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Hit All Provoke") };
                    })
                    );

            //Blindshot Trait
            TraitDataBuilder blindshot = new TraitDataBuilder(this)
                .Create("Blindshot")
                .WithOverrides(TryGet<TraitData>("Aimless"), TryGet<TraitData>("Barrage"), TryGet<TraitData>("Longshot"))
                .SubscribeToAfterAllBuildEvent(
                    (trait) =>
                    {
                        trait.keyword = Get<KeywordData>("blindshot");
                        trait.effects = new StatusEffectData[] { TryGet<StatusEffectData>("Hit Truly Random Target") };
                    });
            assets.Add(blindshot);

            //Wideshot Trait
            assets.Add(new TraitDataBuilder(this)
                .Create("Wideshot")
                .WithOverrides(TryGet<TraitData>("Barrage"), TryGet<TraitData>("Longshot"), TryGet<TraitData>("Aimless"))
                .SubscribeToAfterAllBuildEvent(delegate (TraitData trait)
                {
                    trait.keyword = Get<KeywordData>("wideshot");
                    trait.effects = new StatusEffectData[1] { Get<StatusEffectData>("Wideshot") };
                    trait.overrides = new TraitData[1] { TryGet<TraitData>("Pierce") };
                })
                );

            //Pierce Trait
            assets.Add(new TraitDataBuilder(this)
                .Create("Pierce")
                .WithOverrides(TryGet<TraitData>("Barrage"), TryGet<TraitData>("Longshot"), TryGet<TraitData>("Aimless"))
                .SubscribeToAfterAllBuildEvent(delegate (TraitData trait)
                {
                    trait.keyword = Get<KeywordData>("pierce");
                    trait.effects = new StatusEffectData[1] { Get<StatusEffectData>("Pierce") };
                    trait.overrides = new TraitData[1] { TryGet<TraitData>("Wideshot") };
                })
                );

            //Code for charms
            //Ancient Gaulish Silver Coin
            assets.Add(
                new CardUpgradeDataBuilder(this)
                .CreateCharm("CardUpgradeAncientGaulishSilverCoin")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Ancient Gaulish Silver Coin.png")
                .WithTitle("Ancient Gaulish Silver Coin")
                .WithText("Reduce skills initial <keyword=artemys.wildfrost.frostknights.cooldown> by 4")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(delegate (CardUpgradeData data)
                {
                    ((CardUpgradeData)data).targetConstraints = new TargetConstraint[]
                    {
                        new TargetConstraintHasStatusClassButtonCooldown(),
                        new TargetConstraintInitialCooldownMoreThan()
                        {
                            value = 4
                        }
                    };
                    CardScriptReduceInitialCooldownBy4 cardScriptReduceInitialCooldownBy4 = ScriptableObject.CreateInstance<CardScriptReduceInitialCooldownBy4>();
                    data.scripts = new CardScript[1] { cardScriptReduceInitialCooldownBy4 };
                })
                );

            //Royal Liqueur
            assets.Add(
                new CardUpgradeDataBuilder(this)
                .CreateCharm("CardUpgradeRoyalLiqueur")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Royal Liqueur.png")
                .WithTitle("Royal Liqueur")
                .WithText("Reduce skills max and initial <keyword=artemys.wildfrost.frostknights.cooldown> by 2")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(delegate (CardUpgradeData data)
                {
                    CardScriptReduceInitialCooldownBy2 cardScriptReduceInitialCooldownBy2 = ScriptableObject.CreateInstance<CardScriptReduceInitialCooldownBy2>();
                    CardScriptReduceMaxCooldown cardScriptReduceMaxCooldown = ScriptableObject.CreateInstance<CardScriptReduceMaxCooldown>();
                    data.scripts = new CardScript[2]
                    {
                        cardScriptReduceInitialCooldownBy2,
                        cardScriptReduceMaxCooldown
                    };
                    ((CardUpgradeData)data).targetConstraints = new TargetConstraint[]
                    {
                        new TargetConstraintHasStatusClassButtonCooldown(),
                        new TargetConstraintMaxCooldownMoreThan()
                        {
                            value = 2
                        },
                        new TargetConstraintInitialCooldownMoreThan()
                        {
                            value = 2
                        }
                    };
                })
                );

            //Captain Morgan's Wine
            assets.Add(
                new CardUpgradeDataBuilder(this)
                .CreateCharm("CardUpgradeCaptainMorgan'sWine")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Captain Morgan's Wine.png")
                .WithTitle("Captain Morgan's Wine")
                .WithText("Gain Reduce <keyword=artemys.wildfrost.frostknights.cooldown> By 1 On Attack")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(delegate (CardUpgradeData data)
                {
                    data.effects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Apply Reduce Cooldown To Self", 1)
                    };
                    ((CardUpgradeData)data).targetConstraints = new TargetConstraint[]
                    {
                        new TargetConstraintHasStatusClassButtonCooldown(),
                        new TargetConstraintDoesAttack()
                    };
                })
                );

            //Water of Life
            assets.Add(
                new CardUpgradeDataBuilder(this)
                .CreateCharm("CardUpgradeWaterOfLife")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Water of Life.png")
                .WithTitle("Water of Life")
                .WithText("Gain Reduce <keyword=artemys.wildfrost.frostknights.cooldown> By 1 When Hit")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(delegate (CardUpgradeData data)
                {
                    data.effects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("When Hit Add Reduce Cooldown To Self", 1)
                    };
                    ((CardUpgradeData)data).targetConstraints = new TargetConstraint[]
                    {
                        new TargetConstraintCanBeHit(),
                        new TargetConstraintHasStatusClassButtonCooldown()
                    };
                })
                );

            //Unleashings
            assets.Add(
                new CardUpgradeDataBuilder(this)
                .CreateCharm("CardUpgradeUnleashings")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Unleashings.png")
                .WithTitle("Unleashings")
                .WithTier(2)
                .WithText($"Gain <keyword={Deadpan.Enums.Engine.Components.Modding.Extensions.PrefixGUID("splash", this)}> <1>")
                .SubscribeToAfterAllBuildEvent(delegate (CardUpgradeData data)
                {
                    data.giveTraits = new CardData.TraitStacks[]
                    {
                        TStack("Splash", 1)
                    };
                    ((CardUpgradeData)data).targetConstraints = new TargetConstraint[]
                    {
                        new TargetConstraintDoesDamage(),
                        new TargetConstraintAttackMoreThan()
                        {
                            value = 0
                        },
                        new TargetConstraintHasTrait()
                        {
                            trait = TryGet<TraitData>("Barrage"),
                            not = true
                        }
                    };
                })
                );

            //Code for status effects
            //Status 0: On Turn Apply Block To Allies
            assets.Add(
                StatusCopy("On Turn Apply Spice To AllyBehind", "On Turn Apply Block To Allies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Block");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                })
                .WithText("Apply <{a}><keyword=block> to ally behind", SystemLanguage.English)
                );

            //Status 1: Heal Self
            assets.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Self")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                .WithText("Restore <{a}><keyword=health> to self", SystemLanguage.English)
                );

            //Status 2: Heal Ally Behind
            assets.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Ally Behind")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                })
                .WithText("Restore <{a}><keyword=health> to ally behind", SystemLanguage.English)
                );

            //Status 3: Heal on Block Removed
            assets.Add(
                StatusCopy("Trigger When Self Or Ally Loses Block", "On Block Lost Heal")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
                    ((StatusEffectData)data).isReaction = false;
                })
                .WithText("When Block is broken, restore <{a}><keyword=health> to self", SystemLanguage.English)
                );

            //Status 4: Summon Horn
            assets.Add(
                StatusCopy("Summon Dregg", "Summon Horn")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("hornSummon");
                })
                );

            //Status 5: Instant Summon Horn
            assets.Add(
               StatusCopy("Instant Summon Dregg", "Instant Summon Horn")
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Horn") as StatusEffectSummon;
               })
               );

            //Status 6: When Destroyed Summon Horn
            assets.Add(
               StatusCopy("When Destroyed Summon Dregg", "When Destroyed Summon Horn")
               .WithText("When destroyed, summon self with 2<keyword=health>", SystemLanguage.English)
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectApplyXWhenDestroyed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Horn");
               })
               );

            //Status 7: Summon Mobile Riot Shield
            assets.Add(
                StatusCopy("Summon Fallow", "Summon Mobile Riot Shield")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("mobileRiotShield");
                })
                );

            //Status 8: Instant Summon Mobile Riot Shield
            assets.Add(
                StatusCopy("Instant Summon Fallow", "Instant Summon Mobile Riot Shield")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mobile Riot Shield") as StatusEffectSummon;
                })
                );

            //Status 9: When Deployed Summon Mobile Riot Shield
            assets.Add(
                StatusCopy("When Deployed Summon Wowee", "When Deployed Summon Mobile Riot Shield")
                .WithText("When deployed, summon {0}", SystemLanguage.English)
                .WithTextInsert("<card=artemys.wildfrost.frostknights.mobileRiotShield>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXWhenDeployed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mobile Riot Shield");
                })
                );

            //Status 10: When Active Give Barrage to Jessica
            assets.Add(
                StatusCopy("While Active Barrage To AlliesInRow", "While Active Barrage To Jessica")
                .WithText("While Active Jessica has Barrage", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                    ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[]
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

            //Status 11: Hit All Adjacent Enemies
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantSplashDamage>("Hit All Adjacent Enemies")
                .WithCanBeBoosted(true)
                .WithType("")
                );

            //Status 12: Apply Splash On Card Played
            assets.Add(
                StatusCopy("On Hit Pull Target", "On Card Played Apply Splash To Self")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Hit All Adjacent Enemies");
                    ((StatusEffectApplyXOnHit)data).postHit = true;
                })
                );

            //Status 13: Summon Blazing Sun
            assets.Add(
                StatusCopy("Summon Plep", "Summon Blazing Sun")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("blazingSun");
                })
                );

            //Status 14: Instant Summon Blazing Sun
            assets.Add(
                StatusCopy("Instant Summon Fallow", "Instant Summon Blazing Sun")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Blazing Sun") as StatusEffectSummon;
                })
                );

            //Status 15: On Turn Summon Blazing Sun
            assets.Add(
                StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Blazing Sun")
                .WithText("Summon {0}", SystemLanguage.English)
                .WithTextInsert("<card=artemys.wildfrost.frostknights.blazingSun>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXOnTurn)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Blazing Sun");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 16: When Deployed Deal Damage To Enemies
            assets.Add(
                StatusCopy("When Deployed Apply Demonize To Enemies", "When Deployed Deal Damage To Enemies")
                .WithText("When deployed, deal <{a}> damage to all enemies", SystemLanguage.English)
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

            //Status 17: On Turn Heal Self and Allies
            assets.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Self and Allies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self | StatusEffectApplyX.ApplyToFlags.Allies;
                })
                .WithText("Restore <{a}><keyword=health> to self and allies", SystemLanguage.English)
                );

            //Status 18: On Turn Add Attack to Ally In Front
            assets.Add(
                StatusCopy("On Turn Add Attack To Allies", "On Turn Add Attack to Ally In Front")
                .WithText("Add <+{a}><keyword=attack> to ally in front", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
                })
                );

            //Status 19: Increase Counter
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantCountUp>("Increase Counter")
                .WithCanBeBoosted(true)
                .WithType("")
                );

            //Status 20: On Hit Increase Counter
            assets.Add(
                StatusCopy("On Hit Pull Target", "On Hit Increase Counter")
                .WithText("Count up <keyword=counter> by <{a}>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
                );

            //Status 21: Trial of Thorns Button Listener_1
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Trial of Thorns Button Listener_1")
                .WithType("trialofthorns_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Shell Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 22: When Deployed Apply Increase Counter To Enemies
            assets.Add(
                StatusCopy("When Deployed Apply Demonize To Enemies", "When Deployed Apply Increase Counter To Enemies")
                .WithText("When Deployed, increase <keyword=counter> of all enemies by <{a}>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
                );

            //Status 23: Summon Mirage
            assets.Add(
                StatusCopy("Summon Gunk", "Summon Mirage")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("mirage");
                })
                );

            //Status 24: Instant Summon Mirage In Hand
            assets.Add(
                StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Mirage In Hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mirage") as StatusEffectSummon;
                })
                );

            //Status 25: When Deployed Add Mirage To Hand
            assets.Add(
                StatusCopy("When Deployed Add Junk To Hand", "When Deployed Add Mirage To Hand")
                .WithText("Add <{a}> <card=artemys.wildfrost.frostknights.mirage> to your hand when played", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mirage In Hand");
                })
                );

            //Status 26: Hit All Taunt
            assets.Add(
                StatusCopy("Hit All Enemies", "Hit All Taunt")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeTaunt>();
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 27: While Active Taunted To Enemies
            assets.Add(
                StatusCopy("While Active Aimless To Enemies", "While Active Taunted To Enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Taunted");
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 28: Temporary Taunted
            assets.Add(
                StatusCopy("Temporary Aimless", "Temporary Taunted")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Taunted");
                })
                );

            //Status 29: On Turn Heal AllyInFrontOf
            assets.Add(
               StatusCopy("On Turn Heal Allies", "On Turn Heal AllyInFrontOf")
               .WithText("Restore <{a}><keyword=health> to ally in front", SystemLanguage.English)
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
               })
               );

            //Status 30: On Turn Apply Shell AllyInFrontOf
            assets.Add(
               StatusCopy("On Turn Apply Shell To Allies", "On Turn Apply Shell AllyInFrontOf")
               .WithText("Apply <{a}><keyword=shell> to ally in front", SystemLanguage.English)
               .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
               {
                   ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyInFrontOf;
               })
               );

            //Status 31: On Turn Deal Damage To Enemies
            assets.Add(
                StatusCopy("On Turn Apply Demonize To Enemies", "On Turn Deal Damage To Enemies")
                .WithText("Deal <{a}> damage to all enemies", SystemLanguage.English)
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
            assets.Add(
                StatusCopy("On Turn Apply Demonize To Enemies", "On Turn Apply Shroom To Enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Shroom");
                    ((StatusEffectData)data).textInsert = "<{a}><keyword=shroom>";
                })
                .WithText("Apply <{a}><keyword=shroom> to all enemies", SystemLanguage.English)
                );

            //Status 33: Summon Typewriter
            assets.Add(
                StatusCopy("Summon Gunk", "Summon Typewriter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("typewriter");
                })
                );

            //Status 34: Instant Summon Typewriter In Hand
            assets.Add(
                StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Typewriter In Hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Typewriter") as StatusEffectSummon;
                })
                );

            //Status 35: When Deployed Add Typewriter To Hand
            assets.Add(
                StatusCopy("When Deployed Add Junk To Hand", "When Deployed Add Typewriter To Hand")
                .WithText("Add <{a}> <card=artemys.wildfrost.frostknights.typewriter> to your hand when deployed", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Typewriter In Hand");
                })
                );

            //Status 36: On Hit Damage Bomed Target
            assets.Add(
                StatusCopy("On Hit Damage Snowed Target", "On Hit Damage Bomed Target")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    TargetConstraintHasStatus bomconstraint = new TargetConstraintHasStatus();
                    bomconstraint.status = Get<StatusEffectData>("Weakness");
                    ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[] { bomconstraint };
                })
                .WithText("Deal <{a}> additional damage to <keyword=weakness>'d targets", SystemLanguage.English)
                );

            //Status 37: Burnage
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectBurnage>("Burnage")
                .WithVisible(true)
                .WithIconGroupName("health")
                .WithIsStatus(true)
                .WithOffensive(true)
                .WithStackable(true)
                .WithTextInsert("{a}")
                .WithKeyword("artemys.wildfrost.frostknights.burnage")
                .WithType("burnage")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectData)data).removeOnDiscard = true;
                    ((StatusEffectData)data).targetConstraints = new TargetConstraint[1]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsUnit>()
                    };
                    ((StatusEffectData)data).applyFormatKey = Get<StatusEffectData>("Shroom").applyFormatKey;
                })
                );

            //Status 38: Provoke Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTraitUntilTurnEnd>("Provoke Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(false)
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
            assets.Add(
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
                    ((ButtonCooldown)data).maxCooldown = 9;
                    ((ButtonCooldown)data).cooldownCount = 3;
                })
                );

            //Status 40: Provoked Until Turn End
            assets.Add(
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
            assets.Add(
                StatusCopy("Hit All Enemies", "Hit All Provoke")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeProvoke>();
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 42: While Active Provoked Until Turn End To Enemies
            assets.Add(
                StatusCopy("While Active Aimless To Enemies", "While Active Provoked Until Turn End To Enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Provoked Until Turn End");
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 43: Blazing Sun's Obeisance Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Blazing Sun's Obeisance Button")
                .WithType("blazingsunsobeisance")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Blazing Sun");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 8;
                    ((ButtonCooldown)data).cooldownCount = 3;
                })
                );

            //Status 44: Opprobrium Button
            assets.Add(
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
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTriggerWhenCertainAllyAttacks>("Trigger When Typewriter In Row Attacks")
                .WithCanBeBoosted(false)
                .WithText("Trigger when {0} in row attacks", SystemLanguage.English)
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
            assets.Add(
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
            assets.Add(
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
            assets.Add(
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
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Saw of Strength Button")
                .WithType("sawofstrength")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Barrage Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 6;
                    ((ButtonCooldown)data).cooldownCount = 4;
                })
                );

            //Status 50: Destreza Button
            assets.Add(
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
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Gain Block Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("Apply <{a}><keyword=block> until turn end")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Block");
                })
                );

            //Status 52: Soul of the Jungle Button
            assets.Add(
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

            //Status 53: On Card Played Apply Apply Burnage To EnemiesInRow
            assets.Add(
                StatusCopy("On Card Played Apply Snow To EnemiesInRow", "On Card Played Apply Apply Burnage To EnemiesInRow")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Burnage");
                })
                );

            //Status 54: Paenitete Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Paenitete Button")
                .WithType("paenitete")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Burnage");
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 7;
                    ((ButtonCooldown)data).cooldownCount = 5;
                })
                );

            //Status 52: Gain Frenzy Until Turn End
            assets.Add(
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
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Order of the Icefield Button")
                .WithType("orderoftheicefield")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Wideshot");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 6;
                    ((ButtonCooldown)data).cooldownCount = 6;
                })
                );

            //Status 54: Twilight Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Twilight Button")
                .WithType("twilight")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Barrage");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 14;
                    ((ButtonCooldown)data).cooldownCount = 14;
                })
                );

            //Status 55: Twilight Button Listener_1
            assets.Add(
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
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Bloodline of Desecrated Earth Button")
                .WithType("bloodlineofdesecratedearth")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Temporary Barrage");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 9;
                    ((ButtonCooldown)data).cooldownCount = 9;
                })
                );

            //Status 57: Bloodline of Desecrated Earth Button Listener_1
            assets.Add(
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
            assets.Add(
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
            assets.Add(
                StatusCopy("Trigger When Self Or Ally Loses Block", "Apply Snow to All Enemies When Losing All Snow")
                .WithText("When losing all <keyword=snow> apply <{a}><keyword=snow> to all enemies", SystemLanguage.English)
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
            assets.Add(
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
            assets.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Mon3tr And Kal'tsit")
                .WithText("Restore <{a}><keyword=health> to self and <card=artemys.wildfrost.frostknights.mon3tr>", SystemLanguage.English)
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

            //Status 61b: On Turn Heal Mon3tr2 And Kal'tsit
            assets.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Mon3tr2 And Kal'tsit")
                .WithText("Restore <{a}><keyword=health> to self and <card=artemys.wildfrost.frostknights.mon3tr2>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyConstraints = new TargetConstraint[1]
                    {
                        new TargetConstraintIsSpecificCard()
                        {
                            allowedCards = new CardData[]
                            {
                                TryGet<CardData>("mon3tr2"),
                                TryGet<CardData>("kal'tsit")
                            }
                        }
                    };
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self | StatusEffectApplyX.ApplyToFlags.Allies;
                })
                );

            //Status 62: Summon Mon3tr
            assets.Add(
                StatusCopy("Summon Gunk", "Summon Mon3tr")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("mon3tr");
                })
                );

            //Status 63: Instant Summon Mon3tr In Hand
            assets.Add(
                StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Mon3tr In Hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mon3tr") as StatusEffectSummon;
                })
                );

            //Status 64: When Deployed Add Mon3tr To Hand
            assets.Add(
                StatusCopy("When Deployed Add Junk To Hand", "When Deployed Add Mon3tr To Hand")
                .WithText("Add <card=artemys.wildfrost.frostknights.mon3tr> to your hand when played", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mon3tr In Hand");
                })
                );

            //Status 65b: On Hit Equal Heal To Self
            assets.Add(
                StatusCopy("On Hit Equal Snow To Target", "On Hit Equal Heal To Self")
                .WithText("Heal self equal to damage dealt", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = Get<StatusEffectData>("Heal");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 66: Originite Prime Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Originite Prime Button")
                .WithType("originiteprime")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("While Active Double Gold Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 8;
                    ((ButtonCooldown)data).cooldownCount = 8;
                })
                );

            //Status 67: Command: Meltdown Button
            assets.Add(
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
            assets.Add(
                StatusCopy("On Turn Apply Attack To Self", "On Turn Reduce Attack To Self")
                .WithText("Lose <{a}><keyword=attack>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Attack");
                })
                );

            //Status 69: Reduce Cooldown
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReduceCooldown>("Reduce Cooldown")
                .WithText("Count down <keyword=artemys.wildfrost.frostknights.cooldown> by <{a}>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectData)data).canBeBoosted = true;
                    ((StatusEffectData)data).stackable = true;
                    ((StatusEffectData)data).textInsert = "<keyword=artemys.wildfrost.frostknights.cooldown>";
                    ((StatusEffectData)data).type = "cooldown";
                })
                );

            //Status 70: On Turn Decrease Cooldown To Allies
            assets.Add(
                StatusCopy("On Turn Apply Shell To Allies", "On Turn Decrease Cooldown To Allies")
                .WithText("Reduce <{a}> <keyword=artemys.wildfrost.frostknights.cooldown> to all allies", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Cooldown");
                    ((StatusEffectApplyX)data).noTargetTypeArgs = new string[] { "<sprite name=cooldownicon>" };
                })
                );

            //Status 71: Reduce Max Cooldown
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReduceMaxCooldown>("Reduce Max Cooldown")
                .WithText("Reduce max <keyword=artemys.wildfrost.frostknights.cooldown> by <{a}>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectData)data).canBeBoosted = true;
                    ((StatusEffectData)data).stackable = true;
                    ((StatusEffectData)data).textInsert = "<keyword=artemys.wildfrost.frostknights.cooldown>";
                    ((StatusEffectData)data).type = "counter down";
                })
                );

            //Status 72: Greenhouse's Boundary Button
            assets.Add(
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
            assets.Add(
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
            assets.Add(
                StatusCopy("On Kill Apply Attack To Self", "On Kill Apply Attack To FrontAlly")
                .WithText("Add <+{a}><keyword=attack> to front ally on kill", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontAlly;
                })
                );

            //Status 75: On Turn Heal AllyBehind
            assets.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal AllyBehind")
                .WithText("Restore <{a}><keyword=health> to ally behind", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                })
                );

            //Status 76: When Deployed Apply Heal To Allies
            assets.Add(
                StatusCopy("When Deployed Apply Ink To Allies", "When Deployed Apply Heal To Allies")
                .WithText("When deployed, restore <{a}><keyword=health> to all allies", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
                })
                );

            //Status 77: While Active Double Gold
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectWhileActiveXDoubleGold>("While Active Double Gold")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("While Active Double Gold Earned")
                );

            //Status 78: While Active Double Gold Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("While Active Double Gold Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("While Active Double Gold Earned Until Turn End")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("While Active Double Gold");
                })
                );

            //Status 79: Anatta Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Anatta Button")
                .WithType("anatta")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("MultiHit");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 11;
                    ((ButtonCooldown)data).cooldownCount = 9;
                })
                );

            //Status 80: When Deployed Apply Shell Until Turn End To Allies
            assets.Add(
                StatusCopy("When Deployed Apply Ink To Allies", "When Deployed Apply Shell Until Turn End To Allies")
                .WithText("When deployed, apply <{a}><keyword=shell> to all allies until turn end", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Shell Until Turn End");
                })
                );

            //Status 81: Gain Shell Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Gain Shell Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Shell");
                })
                );

            //Status 82: Shell Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Shell Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("Apply <{a}><keyword=shell> until turn end")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Shell");
                })
                );

            //Status 83: On Turn Apply Reduce Cooldown To Self
            assets.Add(
                StatusCopy("On Turn Apply Attack To Self", "On Turn Apply Reduce Cooldown To Self")
                .WithText("Count down <keyword=artemys.wildfrost.frostknights.cooldown> by <{a}>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Cooldown");
                })
                );

            //Status 84: Calcification Effect
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReplaceEffects>("Calcification Effect")
                .WithCanBeBoosted(false)
                .WithType("")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantReplaceEffects)data).replaceEffectNames = new List<CardData.StatusEffectStacks>
                    {
                        SStack("On Turn Apply Increase Counter To Enemies", 1),
                        SStack("On Turn Apply Bom Until Turn End To Enemies", 1),
                        SStack("Medicine Dispensing Button", 1)
                    };
                })
                );

            //Status 85: On Turn Apply Increase Counter To Enemies
            assets.Add(
                StatusCopy("On Turn Apply Demonize To Enemies", "On Turn Apply Increase Counter To Enemies")
                .WithText("Count up enemies' <keyword=counter> by <{a}>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
                );

            //Status 86: Bom Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Bom Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Weakness");
                })
                );

            //Status 86: On Turn Apply Bom Until Turn End To Enemies
            assets.Add(
                StatusCopy("On Turn Apply Demonize To Enemies", "On Turn Apply Bom Until Turn End To Enemies")
                .WithText("Apply 1<keyword=weakness> to enemies for 1 turn", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Bom Until Turn End");
                })
                );

            //Status 87: Calcification Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Calcification Button")
                .WithType("calcification")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Calcification Effect");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((ButtonCooldown)data).maxCooldown = 3;
                    ((ButtonCooldown)data).cooldownCount = 3;
                })
                );

            //Status 88: Medicine Dispensing Effect
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReplaceEffects>("Medicine Dispensing Effect")
                .WithCanBeBoosted(false)
                .WithType("")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantReplaceEffects)data).replaceEffectNames = new List<CardData.StatusEffectStacks>
                    {
                        SStack("On Turn Heal Self and Allies", 3),
                        SStack("Calcification Button", 1)
                    };
                })
                );

            //Status 89: Medicine Dispensing Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Medicine Dispensing Button")
                .WithType("medicinedispensing")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Medicine Dispensing Effect");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((ButtonCooldown)data).maxCooldown = 3;
                    ((ButtonCooldown)data).cooldownCount = 3;
                })
                );

            //Status 90: On Card Played Frost To Enemies
            assets.Add(
                StatusCopy("On Card Played Void To Enemies", "On Card Played Frost To Enemies")
                .WithText("Apply <{a}><keyword=frost> to all enemies", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Frost");
                })
                );

            //Status 91: On Card Played Increase Attack To Allies
            assets.Add(
                StatusCopy("On Card Played Apply Spice To Allies", "On Card Played Increase Attack To Allies")
                .WithText("Increase <keyword=attack> by <{a}> to all allies", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Attack");
                })
                );

            //Status 92: When Hit Equal Damage To Attacker Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("When Hit Equal Damage To Attacker Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("When Hit Equal Damage To Attacker Until Turn End")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("When Hit Equal Damage To Attacker");
                })
                );

            //Status 93: On Card Played Increase Max Health To Allies
            assets.Add(
                StatusCopy("On Card Played Apply Spice To Allies", "On Card Played Increase Max Health To Allies")
                .WithText("Increase max <keyword=health> by <{a}> to all allies", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Max Health (Not Current)");
                    ((StatusEffectData)data).type = "max health up";
                })
                );

            //Status 94: Trigger Against Anything That Attacks
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTriggerWhenAnythingAttacks>("Trigger Against Anything That Attacks")
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("Trigger Against Anything That  <card=artemys.wildfrost.frostknights.fartoothTargeting> Attacks")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTriggerWhenAnythingAttacks)data).againstTarget = true;
                    ((StatusEffectData)data).affectedBySnow = true;
                    ((StatusEffectData)data).isReaction = true;
                    ((StatusEffectTriggerWhenAnythingAttacks)data).cardThatAttacks = TryGet<CardData>("fartoothTargeting");
                })
                );

            //Status 95: Summon Fartooth Targeting
            assets.Add(
                StatusCopy("Summon Gunk", "Summon Fartooth Targeting")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("fartoothTargeting");
                })
                );

            //Status 96: Instant Summon Fartooth Targeting In Hand
            assets.Add(
                StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Fartooth Targeting In Hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Fartooth Targeting") as StatusEffectSummon;
                })
                );

            //Status 97: Feathershine Arrows Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Feathershine Arrows Button")
                .WithType("feathershinearrows")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Fartooth Targeting In Hand");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 4;
                    ((ButtonCooldown)data).cooldownCount = 1;
                })
                );

            //Status 98: On Kill Apply Reduce Cooldown To Self
            assets.Add(
                StatusCopy("On Kill Apply Attack To Self", "On Kill Apply Reduce Cooldown To Self")
                .WithText("Count down <keyword=artemys.wildfrost.frostknights.cooldown> by <{a}> on kill", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Cooldown");
                })
                );

            //Status 99: Hit Truly Random Target
            assets.Add(
                StatusCopy("Hit Random Target", "Hit Truly Random Target")
                .WithText("Hits a random target")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectChangeTargetMode)data).targetMode = ScriptableObject.CreateInstance<TargetModeTrulyRandom>();
                    ((StatusEffectData)data).textKey = new UnityEngine.Localization.LocalizedString();
                })
                );

            //Status 100: Spirit Burst Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Spirit Burst Button")
                .WithType("spiritburst")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Frenzy Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 12;
                    ((ButtonCooldown)data).cooldownCount = 12;
                })
                );

            //Status 101: Spirit Burst Button Listener_1
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Spirit Burst Button Listener_1")
                .WithType("spiritburst_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Blindshot Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 102: On Card Played Reduce Counter To Allies Equal To Gold Factor 0.015
            assets.Add(
                StatusCopy("On Card Played Reduce Counter To Allies", "On Card Played Reduce Counter To Allies Equal To Gold Factor 0.015")
                .WithText("Count down all allies' <keyword=counter> by <1> for each <75><keyword=blings> you have")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).scriptableAmount = new Func<ScriptableGold>(() =>
                    {
                        var script = ScriptableObject.CreateInstance<ScriptableGold>();
                        script.factor = 0.015f;
                        script.name = "75 Gold";
                        return script;
                    })();
                })
                );

            //Status 103: On Card Played Reduce Counter To Ally Equal To Gold Factor 0.015
            assets.Add(
                StatusCopy("On Card Played Add Fury To Target", "On Card Played Reduce Counter To Ally Equal To Gold Factor 0.015")
                .WithText("Count down <keyword=counter> by <1> for each <75><keyword=blings> you have")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                    ((StatusEffectApplyX)data).scriptableAmount = new Func<ScriptableGold>(() =>
                    {
                        var script = ScriptableObject.CreateInstance<ScriptableGold>();
                        script.factor = 0.015f;
                        script.name = "75 Gold";
                        return script;
                    })();
                })
                );

            //Status 104: Increase Cooldown Countdown
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectDoubleButtonCooldown>("Increase Cooldown Countdown")
                );

            //Status 105: While Active Increase Cooldown Countdown
            assets.Add(
                StatusCopy("While Active Halt Spice To Allies", "While Active Increase Cooldown Countdown")
                .WithText("While active, all allies' <keyword=artemys.wildfrost.frostknights.cooldown> count down by <{a}> more each turn")
                .WithCanBeBoosted(true)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Increase Cooldown Countdown");
                })
                );

            //Status 106: On Turn Apply Noomlin To Card In Hand
            assets.Add(
                StatusCopy("On Turn Apply Attack To Self", "On Turn Apply Zoomlin To Card In Hand")
                .WithText("Add <keyword=zoomlin> to a random card in your hand")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Free Action (Zoomlin)");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomCardInHand;
                })
                );

            //Status 107: Inheritance of Faith Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Inheritance of Faith Button")
                .WithType("inheritanceoffaith")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                    ((ButtonCooldown)data).maxCooldown = 8;
                    ((ButtonCooldown)data).cooldownCount = 4;
                    ((StatusEffectApplyX)data).selectScript = ScriptableObject.CreateInstance<SelectScriptEntityLowestHealth>();
                })
                );

            //Status 108: Monitor Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Monitor Button")
                .WithType("monitor")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Bom Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                    ((ButtonCooldown)data).maxCooldown = 12;
                    ((ButtonCooldown)data).cooldownCount = 6;
                })
                );

            //Status 109: When Hit Add Reduce Cooldown To Self
            assets.Add(
                StatusCopy("When Hit Add Frenzy To Self", "When Hit Add Reduce Cooldown To Self")
                .WithText("When hit, reduce <keyword=artemys.wildfrost.frostknights.cooldown> by <{a}>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Cooldown");
                })
                );

            //Status 110: Divine Avatar Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Divine Avatar Button")
                .WithType("divineavatar")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("On Hit Equal Heal To Ally With Lowest Health Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 12;
                    ((ButtonCooldown)data).cooldownCount = 6;
                })
                );

            //Status 111: On Hit Equal Heal To Ally With Lowest Health
            assets.Add(
                StatusCopy("On Hit Equal Snow To Target", "On Hit Equal Heal To Ally With Lowest Health")
                .WithText("Heal self equal to damage dealt", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = Get<StatusEffectData>("Heal (No Ping)");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                    ((StatusEffectApplyX)data).selectScript = ScriptableObject.CreateInstance<SelectScriptEntityLowestHealth>();
                })
                );

            //Staus 112: On Hit Equal Heal To Ally With Lowest Health Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("On Hit Equal Heal To Ally With Lowest Health Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("Heal ally with the lowest health equal to damage dealt")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("On Hit Equal Heal To Ally With Lowest Health");
                })
                );

            //Staus 113: Damage Equal To Health Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Damage Equal To Health Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Damage Equal To Health");
                })
                );

            //Status 114: Divine Avatar Button Listener_1
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Divine Avatar Button Listener_1")
                .WithType("divineavatar_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Damage Equal To Health Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 115: Boiling Burst Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Boiling Burst Button")
                .WithType("boilingburst")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Barrage Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 11;
                    ((ButtonCooldown)data).cooldownCount = 11;
                })
                );

            //Status 116: Boiling Burst Button Listener_1
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Boiling Burst Button Listener_1")
                .WithType("boilingburst_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("On Card Played Damage To Self Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Staus 116b: On Card Played Damage To Self Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("On Card Played Damage To Self Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("On Card Played Damage To Self");
                })
                );

            //Status 117: Boiling Burst Button Listener_2
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Boiling Burst Button Listener_2")
                .WithType("boilingburst_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Frenzy Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 118: Charging Mode Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Charging Mode Button")
                .WithType("chargingmode")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    ((StatusTokenApplyX)data).endTurn = true;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 8;
                    ((ButtonCooldown)data).cooldownCount = 5;
                })
                );

            //Status 119: Charging Mode Button Listener_1
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Charging Mode Button Listener_1")
                .WithType("chargingmode_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 120: Charging Mode Button Listener_2
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Charging Mode Button Listener_2")
                .WithType("chargingmode_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 121: On Turn Summon Mirage
            assets.Add(
                StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Mirage")
                .WithText("Summon {0}", SystemLanguage.English)
                .WithTextInsert("<card=artemys.wildfrost.frostknights.mirage>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXOnTurn)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mirage");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 122: Instant Summon Mirage
            assets.Add(
                StatusCopy("Instant Summon Fallow", "Instant Summon Mirage")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mirage") as StatusEffectSummon;
                })
                );

            //Status 123: On Turn Heal Ally With The Lowest Health
            assets.Add(
                StatusCopy("On Turn Heal Allies", "On Turn Heal Ally With The Lowest Health")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).selectScript = ScriptableObject.CreateInstance<SelectScriptEntityLowestHealth>();
                })
                .WithText("Restore <{a}><keyword=health> to ally with the lowest health", SystemLanguage.English)
                );

            //Status 124: Hit Truly Random Target Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Hit Truly Random Target Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("When Hit Equal Damage To Attacker Until Turn End")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Hit Truly Random Target");
                })
                );

            //Status 125: Temporary Smackback
            assets.Add(
                StatusCopy("Temporary Barrage", "Temporary Smackback")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Smackback");
                })
                );

            //Status 126: On Turn Apply Reduce Max Counter To Self
            assets.Add(
                StatusCopy("On Turn Apply Attack To Self", "On Turn Apply Reduce Max Counter To Self")
                .WithText("Reduce self's <keyword=counter> by <{a}>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
                })
                );

            //Status 127: On Turn Summon Typewriter
            assets.Add(
                StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Typewriter")
                .WithText("Summon {0}", SystemLanguage.English)
                .WithTextInsert("<card=artemys.wildfrost.frostknights.typewriter>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXOnTurn)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Typewriter");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 128: Instant Summon Typewriter
            assets.Add(
                StatusCopy("Instant Summon Fallow", "Instant Summon Typewriter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Typewriter") as StatusEffectSummon;
                })
                );


            //Status 129: On Turn Reduce Counter To Allies
            assets.Add(
                StatusCopy("On Turn Add Attack To Allies", "On Turn Reduce Counter To Allies")
                .WithText("On turn count down allies <keyword=counter>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                })
                );

            //Status 130: Temporary Splash
            assets.Add(
                StatusCopy("Temporary Barrage", "Temporary Splash")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Splash");
                })
                );

            //Status 131: Summon Mon3tr2
            assets.Add(
                StatusCopy("Summon Fallow", "Summon Mon3tr2")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectSummon)data).summonCard = TryGet<CardData>("mon3tr2");
                })
                );

            //Status 132: Instant Summon Mon3tr2
            assets.Add(
                StatusCopy("Instant Summon Fallow", "Instant Summon Mon3tr2")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantSummon)data).targetSummon = TryGet<StatusEffectData>("Summon Mon3tr2") as StatusEffectSummon;
                })
                );

            //Status 133: When Deployed Summon Mon3tr2
            assets.Add(
                StatusCopy("When Deployed Summon Wowee", "When Deployed Summon Mon3tr2")
                .WithText("When deployed, summon {0}", SystemLanguage.English)
                .WithTextInsert("<card=artemys.wildfrost.frostknights.mon3tr2>")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXWhenDeployed)data).effectToApply = TryGet<StatusEffectData>("Instant Summon Mon3tr2");
                })
                );

            //Status 134: Judgement Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Judgement Button")
                .WithType("judgement")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Blindshot Until Turn End");
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((ButtonCooldown)data).maxCooldown = 10;
                    ((ButtonCooldown)data).cooldownCount = 6;
                })
                );

            //Status 135: Judgement Button Listener_1
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Judgement Button Listener_1")
                .WithType("judgement_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Splash Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 136: Judgement Button Listener_2
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Judgement Button Listener_2")
                .WithType("judgement_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Frenzy Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 137: Splash Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTraitUntilTurnEnd>("Splash Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(false)
                .WithType("")
                .WithVisible(false)
                .FreeModify<StatusEffectTraitUntilTurnEnd>(
                    (data) =>
                    {
                        data.targetConstraints = new TargetConstraint[0];
                    })
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTraitUntilTurnEnd)data).trait = TryGet<TraitData>("Splash");
                })
                );

            //Status 138: Chi Xiao - Unsheath Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Chi Xiao - Unsheath Button")
                .WithType("chixiao-unsheath")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Deal Damage");
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 6;
                    ((ButtonCooldown)data).cooldownCount = 0;
                })
                );

            //Status 139: Deal Damage
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXInstant>("Deal Damage")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).countsAsHit = true;
                    ((StatusEffectApplyX)data).dealDamage = true;
                    ((StatusEffectApplyX)data).targetMustBeAlive = false;
                    ((StatusEffectApplyX)data).doPing = false;
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
                })
                );

            //Status 140: Blessing of Heroism Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Blessing of Heroism Button")
                .WithType("blessingofheroism")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("On Turn Apply Spice To AllyInFrontOf Until Turn End");
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 7;
                    ((ButtonCooldown)data).cooldownCount = 2;
                })
                );

            //Staus 141: On Turn Apply Spice To AllyInFrontOf Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("On Turn Apply Spice To AllyInFrontOf Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("On Turn Apply Spice To AllyInFrontOf");
                })
                );

            //Status 142: On Turn Apply Spice To Self
            assets.Add(
                StatusCopy("On Turn Add Attack To Allies", "On Turn Reduce Counter To Allies")
                .WithText("Apply <{a}><keyword=spice> to self")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Staus 143: On Turn Apply Spice To Self Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("On Turn Apply Spice To Self Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("On Turn Apply Spice To Self");
                })
                );

            //Status 144: Blessing of Heroism Button Listener_1
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Blessing of Heroism Button Listener_1")
                .WithType("blessingofheroism_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("On Turn Apply Spice To Self Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 145: Ember of Life Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Ember of Life Button")
                .WithType("emberoflife")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("On Turn Apply Overload To Enemies Until Turn End");
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 5;
                    ((ButtonCooldown)data).cooldownCount = 2;
                })
                );

            //Staus 143: On Turn Apply Overload To Enemies Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("On Turn Apply Overload To Enemies Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .WithText("Aplly <a><keyword=overload> to all enemies")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("On Turn Apply Overload To Enemies");
                })
                );

            //Status 144: Blindshot Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTraitUntilTurnEnd>("Blindshot Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(false)
                .WithType("")
                .WithVisible(false)
                .FreeModify<StatusEffectTraitUntilTurnEnd>(
                    (data) =>
                    {
                        data.targetConstraints = new TargetConstraint[0];
                    })
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTraitUntilTurnEnd)data).trait = TryGet<TraitData>("Blindshot");
                })
                );

            //Status 145: Bones
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectBones>("Bones")
                .WithIconGroupName("health")
                .WithVisible(value: true)
                .WithIsStatus(value: true)
                .WithStackable(value: true)
                .WithOffensive(value: false)
                .WithTextInsert("{a}")
                .WithKeyword("bones")
                .WithType("bones")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    StatusEffectBones statusEffectBones = data as StatusEffectBones;
                    data.applyFormatKey = TryGet<StatusEffectData>("Shroom").applyFormatKey;
                    statusEffectBones.targetConstraints = new TargetConstraint[1]
                    {
                        new TargetConstraintCanBeHit()
                    };
                })
                );

            //Status 146: On Turn Apply Bones To Self
            assets.Add(
                StatusCopy("On Turn Apply Attack To Self", "On Turn Apply Bones To Self")
                .WithText("Gain <{a}><keyword=artemys.wildfrost.frostknights.bones>", SystemLanguage.English)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Bones");
                })
                );

            //Status 147: Gain Bones Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXUntilTurnEnd>("Gain Bones Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(true)
                .WithType("")
                .WithVisible(false)
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyXUntilTurnEnd)data).effectToApply = TryGet<StatusEffectData>("Bones");
                })
                );

            //Status 148: Trial of Thorns Button Listener_2
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusTokenApplyXListener>("Trial of Thorns Button Listener_2")
                .WithType("trialofthorns_listener")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectApplyX)data).effectToApply = TryGet<StatusEffectData>("Gain Bones Until Turn End");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
                );

            //Status 149: Wideshot
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectChangeTargetMode>("Wideshot")
                .WithCanBeBoosted(value: false)
                .WithType("wideshot")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    StatusEffectChangeTargetMode statusEffectChangeTargetMode4 = data as StatusEffectChangeTargetMode;
                    statusEffectChangeTargetMode4.targetMode = ScriptableObject.CreateInstance<TargetModeSlash2>();
                })
                );

            //Status 150: Pierce
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectChangeTargetMode>("Pierce")
                .WithCanBeBoosted(value: false)
                .WithType("pierce")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    StatusEffectChangeTargetMode statusEffectChangeTargetMode5 = data as StatusEffectChangeTargetMode;
                    statusEffectChangeTargetMode5.targetMode = ScriptableObject.CreateInstance<TargetModePierce>();
                })
                );

            //Status 151: Temporary Wideshot
            assets.Add(
                StatusCopy("Temporary Aimless", "Temporary Wideshot")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Wideshot");
                })
                );

            //Status 151: Temporary Blindshot
            assets.Add(
                StatusCopy("Temporary Aimless", "Temporary Blindshot")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTemporaryTrait)data).trait = TryGet<TraitData>("Blindshot");
                })
                );

            //Status 152: Fracture
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectFracture>("Fracture")
                .WithIconGroupName("health")
                .WithVisible(value: true)
                .WithIsStatus(value: true)
                .WithStackable(value: true)
                .WithOffensive(value: false)
                .WithTextInsert("{a}")
                .WithKeyword("artemys.wildfrost.frostknights.fracture")
                .WithType("fracture")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    StatusEffectFracture statusEffectFracture = data as StatusEffectFracture;
                    data.applyFormatKey = TryGet<StatusEffectData>("Shroom").applyFormatKey;
                    statusEffectFracture.targetConstraints = new TargetConstraint[1]
                    {
                        new TargetConstraintIsUnit()
                    };
                })
                );

            //Status 153: Pierce Until Turn End
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectTraitUntilTurnEnd>("Pierce Until Turn End")
                .WithCanBeBoosted(false)
                .WithIsStatus(false)
                .WithStackable(false)
                .WithType("")
                .WithVisible(false)
                .FreeModify<StatusEffectTraitUntilTurnEnd>(
                    (data) =>
                    {
                        data.targetConstraints = new TargetConstraint[0];
                    })
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectTraitUntilTurnEnd)data).trait = TryGet<TraitData>("Pierce");
                })
                );

            //Status 154: Final Tactics Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Final Tactics Button")
                .WithType("finaltactics")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Pierce Until Turn End");
                    ((StatusTokenApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = false;
                    ((ButtonCooldown)data).maxCooldown = 7;
                    ((ButtonCooldown)data).cooldownCount = 5;
                })
                );

            //Status 155: Defense
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectDefense>("Defense")
                .WithIconGroupName("health")
                .WithVisible(value: true)
                .WithIsStatus(value: true)
                .WithStackable(value: true)
                .WithOffensive(value: false)
                .WithTextInsert("{a}")
                .WithKeyword("defense")
                .WithType("defense")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    StatusEffectDefense statusEffectDefense = data as StatusEffectDefense;
                    data.applyFormatKey = TryGet<StatusEffectData>("Shroom").applyFormatKey;
                    statusEffectDefense.targetConstraints = new TargetConstraint[1]
                    {
                        new TargetConstraintCanBeHit()
                    };
                })
                );

            //Status 156: Burden of Cinder and Ash Effect
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReplaceEffects>("Burden of Cinder and Ash Effect")
                .WithCanBeBoosted(false)
                .WithType("")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantReplaceEffects)data).replaceEffectNames = new List<CardData.StatusEffectStacks>
                    {
                        SStack("Increase Max Counter", 2),
                        SStack("On Hit Damage Snowed Target", 6),
                        SStack("Unquenchable Front Button", 1)
                    };
                })
                );

            //Status 157: Burden of Cinder and Ash Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Burden of Cinder and Ash Button")
                .WithType("burdenofcinderandash")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Burden of Cinder and Ash Effect");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((ButtonCooldown)data).maxCooldown = 3;
                    ((ButtonCooldown)data).cooldownCount = 3;
                })
                );

            //Status 88: Unquenchable Front Effect
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantReplaceEffects>("Unquenchable Front Effect")
                .WithCanBeBoosted(false)
                .WithType("")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusEffectInstantReplaceEffects)data).replaceEffectNames = new List<CardData.StatusEffectStacks>
                    {
                        SStack("Reduce Max Counter", 2),
                        SStack("On Turn Heal Self", 2),
                        SStack("Burden of Cinder and Ash Button", 1)
                    };
                })
                );

            //Status 89: Unquenchable Front Button
            assets.Add(
                new StatusEffectDataBuilder(this)
                .Create<ButtonCooldown>("Unquenchable Front Button")
                .WithType("unquenchablefront")
                .WithVisible(true)
                .WithIconGroupName("counter")
                .SubscribeToAfterAllBuildEvent(delegate (StatusEffectData data)
                {
                    ((StatusTokenApplyX)data).effectToApply = TryGet<StatusEffectData>("Unquenchable Front Effect");
                    ((StatusEffectApplyX)data).applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    ((StatusTokenApplyX)data).endTurn = false;
                    ((StatusTokenApplyX)data).finiteUses = true;
                    ((ButtonCooldown)data).maxCooldown = 3;
                    ((ButtonCooldown)data).cooldownCount = 3;
                })
                );

            //Code for units
            //Nian Card 1
            assets.Add(
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
            assets.Add(
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
            assets.Add(
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
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Calcification Button", 1),
                        SStack("On Turn Heal Self and Allies", 3),
                    };
                })
                );

            //Penance Card 4
            assets.Add(
                new CardDataBuilder(this).CreateUnit("penance", "Penance")
                .SetSprites("Penance.png", "Penance BG.png")
                .SetStats(7, 3, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "No matter what I might encounter, I will not back down.", "What the law does not specify, I will reveal through practice." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[5]
                    {
                        SStack("On Turn Apply Bones To Self", 2),
                        SStack("On Turn Apply Shell To Self", 2),
                        SStack("Trial of Thorns Button Listener_1", 5),
                        SStack("Trial of Thorns Button Listener_2", 5),
                        SStack("Trial of Thorns Button", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Pigheaded", 1)
                    };
                })
                );

            //Blemishine Card 5
            assets.Add(
                new CardDataBuilder(this).CreateUnit("blemishine", "Blemishine")
                .SetSprites("Blemishine.png", "Blemishine BG.png")
                .SetStats(5, 3, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "All ready.", "Pray for the coming battle." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Divine Avatar Button", 1),
                        SStack("Divine Avatar Button Listener_1", 1)
                    };
                })
                );

            //Horn Card 6
            assets.Add(
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
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Longshot", 1),
                        TStack("Splash", 3)
                    };
                })
                );

            //Horn Summon Card 
            assets.Add(
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
            assets.Add(
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
            assets.Add(
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
            assets.Add(
                new CardDataBuilder(this).CreateUnit("hoshiguma", "Hoshiguma")
                .SetSprites("Hoshiguma.png", "Hoshiguma BG.png")
                .SetStats(5, 5, 4)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Yes, Sir.", "Got it." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("Teeth", 3),
                        SStack("Defense", 1),
                        SStack("Saw of Strength Button", 1)
                    };
                })
                );

            //Thorns Card 9
            assets.Add(
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
            //REWORK
            assets.Add(
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
            //REWORK
            assets.Add(
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
            assets.Add(
                new CardDataBuilder(this).CreateUnit("nearl", "Nearl")
                .SetSprites("Nearl.png", "Nearl BG.png")
                .SetStats(6, 5, 4)
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
            assets.Add(
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
            assets.Add(
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
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("Blessing of Heroism Button", 2),
                        SStack("Blessing of Heroism Button Listener_1", 2),
                        SStack("MultiHit", 1)
                    };
                })
                );

            //Ch'en Card 14
            assets.Add(
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
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("MultiHit", 1),
                        SStack("Chi Xiao - Unsheath Button", 4)
                    };
                })
                );

            //Qiubai Card 15
            //REWORK
            assets.Add(
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
            assets.Add(
                new CardDataBuilder(this).CreateUnit("irene", "Irene")
                .SetSprites("Irene.png", "Irene BG.png")
                .SetStats(5, 1, 4)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "See the light of my lantern? I'm right here.", "Don't step into the shadows." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[4]
                    {
                        SStack("Judgement Button", 1),
                        SStack("Judgement Button Listener_1", 2),
                        SStack("Judgement Button Listener_2", 2),
                        SStack("MultiHit", 1)
                    };
                })
                );

            //Chongyue Card 17
            assets.Add(
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
                        TStack("Splash", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Apply Reduce Cooldown To Self", 1),
                        SStack("Anatta Button", 1)
                    };
                })
                );

            //Mountain Card 18
            assets.Add(
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
            assets.Add(
                new CardDataBuilder(this).CreateUnit("blaze", "Blaze")
                .SetSprites("Blaze.png", "Blaze BG.png")
                .SetStats(13, 6, 4)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "You know, it wasn't easy learning high-temperature vapor dynamics!", "Let me show you the chainsaw techniques the other elite operators taught me!" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[4]
                    {
                        SStack("ImmuneToSnow", 1),
                        SStack("Boiling Burst Button", 1),
                        SStack("Boiling Burst Button Listener_1", 3),
                        SStack("Boiling Burst Button Listener_2", 1)
                    };
                })
                );

            //Gavial Card 20
            //REWORK
            assets.Add(
                new CardDataBuilder(this).CreateUnit("gavial", "Gavial")
                .SetSprites("Gavial.png", "Gavial BG.png")
                .SetStats(6, 4, 0)
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
            //REWORK
            assets.Add(
                new CardDataBuilder(this).CreateUnit("hoederer", "Hoederer")
                .SetSprites("Hoederer.png", "Hoederer BG.png")
                .SetStats(5, 5, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Don't doubt your own judgment.", "Seize the moment." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Heal Self", 2),
                        SStack("Burden of Cinder and Ash Button", 1)
                    };
                })
                );

            //Surtr Card 22
            assets.Add(
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
                        SStack("Burnage", 6)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Twilight Button", 1),
                        SStack("Twilight Button Listener_1", 3)
                    };
                })
                );

            //Degenbrecher Card 23
            //REWORK
            assets.Add(
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
            assets.Add(
                new CardDataBuilder(this).CreateUnit("reed", "Reed")
                .SetSprites("Reed.png", "Reed BG.png")
                .SetStats(5, 3, 2)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Where do you wish to go?", "I'm prepared to fight." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Ember of Life Button", 1)
                    };
                })
                );

            //Nightingale Card 25
            assets.Add(
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
            assets.Add(
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
            assets.Add(
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
            assets.Add(
                new CardDataBuilder(this).CreateUnit("rosmontis", "Rosmontis")
                .SetSprites("Rosmontis.png", "Rosmontis BG.png")
                .SetStats(5, 2, 5)
                .WithCardType("Friendly")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "I won't remember you.", "Give me your order. I will see it through." };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Deal Damage To Enemies", 1),
                        SStack("On Turn Apply Shroom To Enemies", 1)
                    };
                })
                );

            //Exusiai Card 29
            assets.Add(
                new CardDataBuilder(this).CreateUnit("exusiai", "Exusiai")
                .SetSprites("Exusiai.png", "Exusiai BG.png")
                .SetStats(2, 1, 3)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "All right!", "Is it my turn?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[3]
                    {
                        SStack("Charging Mode Button", 1),
                        SStack("Charging Mode Button Listener_1", 1),
                        SStack("Charging Mode Button Listener_2", 1)
                    };
                })
                );

            //Archetto Card 30
            assets.Add(
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
            assets.Add(
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
                        SStack("Burnage", 4)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Paenitete Button", 4)
                    };
                })
                );

            //W Card 32
            //REWORK
            assets.Add(
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
            assets.Add(
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
            assets.Add(
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
            //REWORK
            assets.Add(
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
                        SStack("Fracture", 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Final Tactics Button", 1)
                    };
                })
                );

            //Typhon Card 36
            assets.Add(
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
                        SStack("Order of the Icefield Button", 2)
                    };
                })
                );

            //Fartooth Card 37
            assets.Add(
                new CardDataBuilder(this).CreateUnit("fartooth", "Fartooth")
                .SetSprites("Fartooth.png", "Fartooth BG.png")
                .SetStats(4, 4, 0)
                .WithCardType("Friendly")
                .AddPool("GeneralUnitPool")
                .FreeModify(delegate (CardData data)
                {
                    ((CardData)data).greetMessages = new string[] { "Yeah, I'm ready whenever.", "One-on-one, or one-on-many?" };
                })
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Trigger Against Anything That Attacks", 1),
                        SStack("Feathershine Arrows Button", 1)
                    };
                })
                );

            //Mon3tr Card
            assets.Add(
                new CardDataBuilder(this).CreateUnit("mon3tr", "Mon3tr")
                .SetSprites("Mon3tr.png", "Mon3tr BG.png")
                .SetStats(8, 2, 2)
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

            //Mon3tr2 Card
            assets.Add(
                new CardDataBuilder(this).CreateUnit("mon3tr2", "Mon3tr")
                .SetSprites("Mon3tr.png", "Mon3tr BG.png")
                .SetStats(26, 4, 2)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Turn Apply Attack To Self", 2)
                    };
                })
                );

            //Code for items
            //Vanilla Soda Item 1
            assets.Add(
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
                        SStack("Reduce Cooldown", 3)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Noomlin", 1)
                    };
                })
                );

            //Rusted Razor Item 2
            assets.Add(
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
            assets.Add(
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

            //Lancet-2 Clunker Item 4
            assets.Add(
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
            assets.Add(
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

            //Castle-3 Clunker Item 6
            assets.Add(
                new CardDataBuilder(this).CreateUnit("castle-3", "Castle-3")
                .SetSprites("Castle-3.png", "Castle-3 BG.png")
                .SetStats(null, 2, 3)
                .WithCardType("Clunker")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Scrap", 2),
                        SStack("While Active Increase Attack To Allies", 1)
                    };
                })
                );

            //Justice Knight Clunker Item 7
            assets.Add(
                new CardDataBuilder(this).CreateUnit("justiceKnight", "Justice Knight")
                .SetSprites("Justice Knight.png", "Justice Knight BG.png")
                .SetStats(null, 1, 2)
                .WithCardType("Clunker")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Scrap", 2)
                    };
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Weakness", 1)
                    };
                })
                );

            //THRM-EX "Clunker" Item 8
            assets.Add(
                new CardDataBuilder(this).CreateUnit("thrm-ex", "THRM-EX")
                .SetSprites("THRM-EX.png", "THRM-EX BG.png")
                .SetStats(null, null, 1)
                .WithCardType("Clunker")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Scrap", 1),
                        SStack("Destroy Self After Turn", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Explode", 8),
                    };
                })
                );

            //Friston-3 Clunker Item 9
            assets.Add(
                new CardDataBuilder(this).CreateUnit("friston-3", "Friston-3")
                .SetSprites("Friston-3.png", "Friston-3 BG.png")
                .SetStats(null, null, 3)
                .WithCardType("Clunker")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Scrap", 1),
                        SStack("When Deployed Apply Shell Until Turn End To Allies", 2)
                    };
                })
                );

            //Fissured Restraints Item 10
            assets.Add(
                new CardDataBuilder(this).CreateItem("fissuredRestraints", "Fissured Restraints", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Fissured Restraints.png", "Fissured Restraints BG.png")
                .SetStats(null, 0)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanPlayOnHand(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Hit All Enemies", 1)
                    };
                    data.attackEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Frost", 2)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Consume", 1),
                    };
                })
                );

            //Oriron Round Shield Item 11
            assets.Add(
                new CardDataBuilder(this).CreateItem("orironRoundShield", "Oriron Round Shield", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Oriron Round Shield.png", "Oriron Round Shield BG.png")
                .SetStats(null, null)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .WithText("Add <keyword=artemys.wildfrost.frostknights.provoke> until turn end")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Shell Until Turn End", 6),
                        SStack("Provoke Until Turn End", 1)
                    };
                })
                );

            //Military Mirror Armor Item 12
            assets.Add(
                new CardDataBuilder(this).CreateItem("militaryMirrorArmor", "Military Mirror Armor", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Military Mirror Armor.png", "Military Mirror Armor BG.png")
                .SetStats(null, null)
                .WithValue(60)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Gain Block Until Turn End", 1),
                        SStack("When Hit Equal Damage To Attacker Until Turn End", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Consume", 1)
                    };
                })
                );

            //Old Steam Armor Item 13
            assets.Add(
                new CardDataBuilder(this).CreateItem("oldSteamArmor", "Old Steam Armor", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Old Steam Armor.png", "Old Steam Armor BG.png")
                .SetStats(null, null)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(false)
                .CanPlayOnFriendly(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .WithText("Can only target allies")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.attackEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Haze", 1),
                        SStack("Increase Max Health", 8)
                    };
                })
                );

            //Royal Rapier Item 14
            assets.Add(
                new CardDataBuilder(this).CreateItem("royalRapier", "Royal Rapier", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Royal Rapier.png", "Royal Rapier BG.png")
                .SetStats(null, null)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanPlayOnHand(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(false)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Card Played Increase Attack To Allies", 1)
                    };
                })
                );

            //Unknown Instrument Item 15
            assets.Add(
                new CardDataBuilder(this).CreateItem("unknownInstrument", "Unknown Instrument", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Unknown Instrument.png", "Unknown Instrument BG.png")
                .SetStats(null, null)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanPlayOnHand(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(false)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Card Played Increase Max Health To Allies", 4)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Consume", 1)
                    };
                })
                );

            //Fartooth Targeting Item
            assets.Add(
                new CardDataBuilder(this).CreateItem("fartoothTargeting", "Fartooth Targeting", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Fartooth Targeting.png", "Fartooth Targeting BG.png")
                .SetStats(null, 0)
                .WithValue(50)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanPlayOnHand(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(true)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Zoomlin", 1),
                        TStack("Consume", 1)
                    };
                })
                );

            //Golden Chalice Item 16
            assets.Add(
                new CardDataBuilder(this).CreateItem("goldenChalice", "Golden Chalice", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Golden Chalice.png", "Golden Chalice BG.png")
                .SetStats(null, null)
                .WithValue(60)
                .CanPlayOnBoard(true)
                .CanPlayOnEnemy(true)
                .CanPlayOnFriendly(true)
                .CanPlayOnHand(true)
                .CanShoveToOtherRow(true)
                .NeedsTarget(false)
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("On Card Played Reduce Counter To Allies Equal To Gold Factor 0.015", 1)
                    };
                    data.traits = new List<CardData.TraitStacks>()
                    {
                        TStack("Consume", 1)
                    };
                })
                );

            //Coin Operated Toy Item 17
            assets.Add(
                new CardDataBuilder(this).CreateItem("coinOperatedToy", "Coin Operated Toy", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Coin Operated Toy.png", "Coin Operated Toy BG.png")
                .SetStats(null, 0)
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
                        SStack("On Card Played Reduce Counter To Ally Equal To Gold Factor 0.015", 1)
                    };
                })
                );

            //Dreaming Essence Clunker Item 18
            assets.Add(
                new CardDataBuilder(this).CreateUnit("dreamingEssence", "Dreaming Essence")
                .SetSprites("Dreaming Essence.png", "Dreaming Essence BG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("Scrap", 1),
                        SStack("When Deployed Apply Shell Until Turn End To Allies", 2)
                    };
                })
                );

            //Myrtle "Card" item? 19
            assets.Add(
                new CardDataBuilder(this).CreateUnit("myrtle", "Myrtle")
                .SetSprites("Myrtle.png", "Myrtle BG.png")
                .SetStats(4, 2, 4)
                .WithCardType("Friendly")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("While Active Increase Cooldown Countdown", 1)
                    };
                })
                );

            //Elysium "Card" item? 20
            assets.Add(
                new CardDataBuilder(this).CreateUnit("elysium", "Elysium")
                .SetSprites("Elysium.png", "Elysium BG.png")
                .SetStats(4, 2, 4)
                .WithCardType("Friendly")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[1]
                    {
                        SStack("Monitor Button", 3)
                    };
                })
                );

            //Saileach "Card" item? 21
            assets.Add(
                new CardDataBuilder(this).CreateUnit("saileach", "Saileach")
                .SetSprites("Saileach.png", "Saileach BG.png")
                .SetStats(4, 2, 6)
                .WithCardType("Friendly")
                .SubscribeToAfterAllBuildEvent(delegate (CardData data)
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Turn Apply Zoomlin To Card In Hand", 1),
                        SStack("Inheritance of Faith Button", 4)
                    };
                })
                );

            //Vieux Vanguard's Blade Item 22
            assets.Add(
                new CardDataBuilder(this).CreateItem("vieuxVanguard'sBlade", "Vieux Vanguard's Blade", "TargetModeBasic", "ShakeAnimationProfile")
                .SetSprites("Vieux Vanguard's Blade.png", "Vieux Vanguard's Blade BG.png")
                .SetStats(null, 2)
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
                        SStack("Burnage", 4)
                    };
                })
                );

            //Code for leaders
            //Kal'tsit leader
            assets.Add(
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
            assets.Add(
                new CardDataBuilder(this).CreateUnit("doctor", "Doctor")
                .SetSprites("Doctor.png", "Doctor BG.png")
                .SetStats(6, 2, 4)
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
            assets.Add(
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
                    data.startWithEffects = new CardData.StatusEffectStacks[2]
                    {
                        SStack("On Hit Equal Heal To Self", 1),
                        SStack("Originite Prime Button", 1)
                    };
                })
                );

            //Amiya leader
            assets.Add(
                new CardDataBuilder(this).CreateUnit("amiya", "Amiya")
                .SetSprites("Amiya.png", "Amiya BG.png")
                .SetStats(5, 5, 4)
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
                        SStack("On Kill Apply Reduce Cooldown To Self", 3),
                        SStack("Spirit Burst Button", 2),
                        SStack("Spirit Burst Button Listener_1", 1)
                    };
                })
                );

            //Code for Tribes
            assets.Add(
                TribeCopy("Clunk", "Rhodes")
                .WithFlag("Images/Rhodes Island Tribe Flag.png")
                .WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/town/stormbell_overcrank"))
                .SubscribeToAfterAllBuildEvent(
                (data) =>
                {
                    GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                    gameObject.name = "Player (Rhodes)";
                    data.characterPrefab = gameObject.GetComponent<Character>();
                    data.leaders = DataList<CardData>(
                        "kal'tsit",
                        "closure",
                        "doctor",
                        "amiya"
                        );
                    Inventory inventory = new Inventory();
                    inventory.deck.list = DataList<CardData>(
                        "vanillaSoda",
                        "worn-outGroupPhoto",
                        "rustedRazor",
                        "rustedRazor",
                        "rustedRazor",
                        "rustedRazor",
                        "rustedRazor",
                        "lancet-2",
                        "vinecreepMortarGunner"
                        )
                    .ToList(); //Some odds and ends
                    data.startingInventory = inventory;
                    RewardPool unitPool = CreateRewardPool("RhodesUnitPool", "Units", DataList<CardData>(
                        "typhon",
                        "pozëmka",
                        "fiammetta",
                        "rosmontis",
                        "reed",
                        "degenbrecher",
                        "surtr",
                        "blaze",
                        "qiubai",
                        "nearl",
                        "młynar",
                        "saria",
                        "mudrock",
                        "nian")
                        );
                    RewardPool itemPool = CreateRewardPool("RhodesItemPool", "Items", DataList<CardData>("castle-3", "friston-3", "justiceKnight", "thrm-ex", "fissuredRestraints", "orironRoundShield", "militaryMirrorArmor", "oldSteamArmor", "royalRapier", "goldenChalice", "coinOperatedToy", "myrtle", "saileach", "elysium", "vieuxVanguard'sBlade")
                        );
                    RewardPool charmPool = CreateRewardPool("RhodesCharmPool", "Charms", DataList<CardUpgradeData>("CardUpgradeUnleashings", "CardUpgradeWaterOfLife", "CardUpgradeCaptainMorgan'sWine", "CardUpgradeRoyalLiqueur", "CardUpgradeAncientGaulishSilverCoin")
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
            instance = this;
            if (!preLoaded) { CreateModAssets(); } //The if statement is a flourish really. It makes the 2nd load of Load-Unload-Load faster.
            base.Load();                          //Actual loading
            IconStuff();
            Events.OnCheckEntityDrag += ButtonExt.DisableDrag;
            FloatingText ftext = GameObject.FindObjectOfType<FloatingText>(true);
            ftext.textAsset.spriteAsset.fallbackSpriteAssets.Add(assetSprites);
            GameMode gameMode = Get<GameMode>("GameModeNormal");
            gameMode.classes = gameMode.classes.Append(Get<ClassData>("Rhodes")).ToArray();
            Events.OnEntityCreated += FixImage;
            //Events.OnUpgradeAssign += ButtonCooldown.SetUpgradeCooldownText;
            //Events.OnSceneChanged += ArknightsPhoto;
        }

        private void IconStuff()
        {
            this.CreateIcon("burnageicon", ImagePath("burnageicon.png").ToSprite(), "burnage", "frost", Color.white, shadowColor: new Color(0.4f, 0f, 0f), new KeywordData[] { Get<KeywordData>("burnage") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateIcon("cooldownicon", ImagePath("cooldownicon.png").ToSprite(), "cooldown", "counter", Color.white, shadowColor: new Color(0f, 0f, 0f), new KeywordData[] { Get<KeywordData>("cooldown") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateIcon("bonesicon", ImagePath("bonesicon.png").ToSprite(), "bones", "frost", Color.black, new Color(0.5f, 0.4f, 0f), new KeywordData[1] { Get<KeywordData>("bones") })
                .GetComponentInChildren<TextMeshProUGUI>(includeInactive: true).enabled = true;

            this.CreateIcon("fractureicon", ImagePath("fractureicon.png").ToSprite(), "fracture", "frost", Color.white, new Color(0.4f, 0f, 0f), new KeywordData[1] { Get<KeywordData>("fracture") })
                .GetComponentInChildren<TextMeshProUGUI>(includeInactive: true).enabled = true;

            this.CreateIcon("defenseicon", ImagePath("defenseicon.png").ToSprite(), "defense", "health", new Color(0f, 0f, 0f), new Color(1f, 1f, 1f), new KeywordData[1] { Get<KeywordData>("defense") })
                .GetComponentInChildren<TextMeshProUGUI>(includeInactive: true).enabled = true;

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

            this.CreateButtonIcon("chongyueAnatta", ImagePath("chongyuebutton.png").ToSprite(), "anatta", "counter", Color.black, new KeywordData[] { Get<KeywordData>("anatta") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("sariaCalcification", ImagePath("sariabutton.png").ToSprite(), "calcification", "counter", Color.black, new KeywordData[] { Get<KeywordData>("calcification") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("sariaMedicineDispensing", ImagePath("sariabutton2.png").ToSprite(), "medicinedispensing", "counter", Color.black, new KeywordData[] { Get<KeywordData>("medicinedispensing") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("fartoothFeathershineArrows", ImagePath("fartoothbutton.png").ToSprite(), "feathershinearrows", "counter", Color.black, new KeywordData[] { Get<KeywordData>("feathershinearrows") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("amiyaSpiritBurst", ImagePath("amiyabutton.png").ToSprite(), "spiritburst", "counter", Color.black, new KeywordData[] { Get<KeywordData>("spiritburst") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("saileachInheritanceofFaith", ImagePath("saileachbutton.png").ToSprite(), "inheritanceoffaith", "counter", Color.black, new KeywordData[] { Get<KeywordData>("inheritanceoffaith") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("elysiumMonitor", ImagePath("elysiumbutton.png").ToSprite(), "monitor", "counter", Color.black, new KeywordData[] { Get<KeywordData>("monitor") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("blemishineDivineAvatar", ImagePath("blemishinebutton.png").ToSprite(), "divineavatar", "counter", Color.black, new KeywordData[] { Get<KeywordData>("divineavatar") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("blazeBoilingBurst", ImagePath("blazebutton.png").ToSprite(), "boilingburst", "counter", Color.black, new KeywordData[] { Get<KeywordData>("boilingburst") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("exusiaiChargingMode", ImagePath("exusiaibutton.png").ToSprite(), "chargingmode", "counter", Color.black, new KeywordData[] { Get<KeywordData>("chargingmode") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("ireneJudgement", ImagePath("irenebutton.png").ToSprite(), "judgement", "counter", Color.black, new KeywordData[] { Get<KeywordData>("judgement") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("ch'enChiXiao-Unsheath", ImagePath("ch'enbutton.png").ToSprite(), "chixiao-unsheath", "counter", Color.black, new KeywordData[] { Get<KeywordData>("chixiao-unsheath") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("pallasBlessingofHeroism", ImagePath("pallasbutton.png").ToSprite(), "blessingofheroism", "counter", Color.black, new KeywordData[] { Get<KeywordData>("blessingofheroism") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("reedEmberofLife", ImagePath("reedbutton.png").ToSprite(), "emberoflife", "counter", Color.black, new KeywordData[] { Get<KeywordData>("emberoflife") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("schwarzFinalTactics", ImagePath("schwarzbutton.png").ToSprite(), "finaltactics", "counter", Color.black, new KeywordData[] { Get<KeywordData>("finaltactics") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("hoedererBurdenofCinderandAsh", ImagePath("hoedererbutton.png").ToSprite(), "burdenofcinderandash", "counter", Color.black, new KeywordData[] { Get<KeywordData>("burdenofcinderandash") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;

            this.CreateButtonIcon("hoedererUnquenchableFront", ImagePath("hoedererbutton2.png").ToSprite(), "unquenchablefront", "counter", Color.black, new KeywordData[] { Get<KeywordData>("unquenchablefront") })
                .GetComponentInChildren<TextMeshProUGUI>(true).enabled = true;
        }

        public override void Unload()
        {
            base.Unload();
            Events.OnCheckEntityDrag -= ButtonExt.DisableDrag;
            GameMode gameMode = Get<GameMode>("GameModeNormal");
            gameMode.classes = RemoveNulls(gameMode.classes);
            Events.OnEntityCreated -= FixImage;
            //Events.OnUpgradeAssign -= ButtonCooldown.SetUpgradeCooldownText;
            //Events.OnSceneChanged -= ArknightsPhoto;
        }

        internal T[] RemoveNulls<T>(T[] data) where T : DataFile
        {
            List<T> list = data.ToList();
            list.RemoveAll(x => x == null || x.ModAdded == this);
            return list.ToArray();
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

        [HarmonyPatch(typeof(TribeHutSequence), "SetupFlags")]
        class PatchTribeHut
        {

            static string TribeName = "Rhodes";
            static void Postfix(TribeHutSequence __instance)                                            //After it unlocks the base mods, it'll move on to ours.
            {
                GameObject gameObject = GameObject.Instantiate(__instance.flags[0].gameObject);         //Clone the Snowdweller flag
                gameObject.transform.SetParent(__instance.flags[0].gameObject.transform.parent, false); //Place it in the same groupas the others
                TribeFlagDisplay flagDisplay = gameObject.GetComponent<TribeFlagDisplay>();
                ClassData tribe = Frostknights.instance.TryGet<ClassData>(TribeName);
                flagDisplay.flagSprite = tribe.flag;                                                    //Replace the flag with our tribe flag
                __instance.flags = __instance.flags.Append(flagDisplay).ToArray();                      //Add it the flag to the list to check
                flagDisplay.SetAvailable();                                                             //Set it available
                flagDisplay.SetUnlocked();                                                              //And unlocked

                TribeDisplaySequence sequence2 = GameObject.FindObjectOfType<TribeDisplaySequence>(true);   //TribeDisplaySequence sequence should be unique, so Find should find the right one.
                GameObject gameObject2 = GameObject.Instantiate(sequence2.displays[1].gameObject);          //Copy one of them (Shademancers)
                gameObject2.transform.SetParent(sequence2.displays[2].gameObject.transform.parent, false);  //Place the copy in the right place in the hieracrhy
                sequence2.tribeNames = sequence2.tribeNames.Append(TribeName).ToArray();                    //Add the name to the list
                sequence2.displays = sequence2.displays.Append(gameObject2).ToArray();                      //Add the display itself to the list

                Button button = flagDisplay.GetComponentInChildren<Button>();                               //Find the button component on our flagDisplay
                button.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);   //Deactivate the cloned listener (which opens the Snowdweller display)
                button.onClick.AddListener(() => { sequence2.Run(TribeName); });                            //Add our own listener that opens our display

                //(SfxOneShot)
                gameObject2.GetComponent<SfxOneshot>().eventRef = FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/town/stormbell_overcrank"); //Shuffling noises

                //0: Flag (ImageSprite)
                gameObject2.transform.GetChild(0).GetComponent<ImageSprite>().SetSprite(tribe.flag);        //Set the sprite of the ImageSprite component to our tribe flag

                //1: Left (ImageSprite)
                Sprite doctor = Frostknights.instance.TryGet<CardData>("doctor").mainSprite;             //Find needle's sprite
                gameObject2.transform.GetChild(1).GetComponent<ImageSprite>().SetSprite(doctor);            //and set it as the left image

                //2: Right (ImageSprite)
                Sprite kaltsit = Frostknights.instance.TryGet<CardData>("kal'tsit").mainSprite;           //Find Frost Muncher's sprite
                gameObject2.transform.GetChild(2).GetComponent<ImageSprite>().SetSprite(kaltsit);           //and set it as the right image
                gameObject2.transform.GetChild(2).localScale *= 1.2f;                                       //and make it 20% bigger

                //3: Textbox (Image)
                gameObject2.transform.GetChild(3).GetComponent<Image>().color = new Color(0.12f, 0.47f, 0.57f); //Change the color of the textbox background

                //3-0: Text (LocalizeStringEvent)
                StringTable collection = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);   //Find a string table (in the desired language)
                collection.SetString("RhodesTribeDesc", "Rhodes Island Pharmaceuticals Inc. legally operates as a pharmaceutical company that provides care and treatment to the Infected, but they are more of a paramilitary organization." +
                    "\n\n" +
                    "Deciding when and where to use skills and manage their cooldowns to defeat your enemies is the main strategy for Rhodes Island.");                                         //Create the description.
                gameObject2.transform.GetChild(3).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString("RhodesTribeDesc");
                //Set the string in the LocaliseStringEvent

                //4:Title Ribbon (Image)
                //4-0: Text (LocalizeStringEvent)
                collection.SetString("RhodesTribeTitle", "Rhodes Island");                                       //Create the description
                gameObject2.transform.GetChild(4).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString("RhodesTribeTitle");
                //Set the string in the LocaliseStringEvent

            }
        }

        [HarmonyPatch(typeof(CardPopUpTarget), "Pop")]
        internal static class PatchDynamicKeyword
        {
            public static List<string> dynamicKeywords = new List<string> { 
                "artemys.wildfrost.frostknights.trialofthorns",
                "artemys.wildfrost.frostknights.blazingsunsobeisance",
                "artemys.wildfrost.frostknights.opprobrium",
                "artemys.wildfrost.frostknights.irondefense",
                "artemys.wildfrost.frostknights.sawofstrength",
                "artemys.wildfrost.frostknights.destreza",
                "artemys.wildfrost.frostknights.soulofthejungle",
                "artemys.wildfrost.frostknights.orderoftheicefield", 
                "artemys.wildfrost.frostknights.paenitete",
                "artemys.wildfrost.frostknights.originiteprime",
                "artemys.wildfrost.frostknights.command:meltdown",
                "artemys.wildfrost.frostknights.calcification",
                "artemys.wildfrost.frostknights.medicinedispensing",
                "artemys.wildfrost.frostknights.feathershinearrows",
                "artemys.wildfrost.frostknights.spiritburst",
                "artemys.wildfrost.frostknights.monitor",
                "artemys.wildfrost.frostknights.inheritanceoffaith",
                "artemys.wildfrost.frostknights.divineavatar",
                "artemys.wildfrost.frostknights.boilingburst",
                "artemys.wildfrost.frostknights.chargingmode",
                "artemys.wildfrost.frostknights.judgement",
                "artemys.wildfrost.frostknights.chixiao-unsheath",
                "artemys.wildfrost.frostknights.blessingofheroism",
                "artemys.wildfrost.frostknights.emberoflife",
                "artemys.wildfrost.frostknights.finaltactics",
                "artemys.wildfrost.frostknights.burdenofcinderandash",
                "artemys.wildfrost.frostknights.unquenchablefront"
            };
            public static string dynamicTypes = typeof(ButtonCooldown).Name;
            static void Postfix(CardPopUpTarget __instance)
            {
                foreach (string s in __instance.current)
                {
                    if (dynamicKeywords.Contains(s))
                    {
                        if (MonoBehaviourRectSingleton<CardPopUp>.instance.activePanels.TryGetValue(s, out var value))
                        {
                            int index = dynamicKeywords.IndexOf(s);
                            string count = "???";
                            if (__instance.GetComponent<StatusIconExt>() != null)
                            {
                                foreach (var effect in __instance.GetComponent<StatusIconExt>().target.statusEffects)
                                {
                                    if (effect.GetType().Name == dynamicTypes)
                                    {
                                        if (effect is ButtonCooldown button)
                                        {
                                            count = button.maxCooldown.ToString();
                                        }

                                    }
                                }
                            }
                            KeywordData keyword = AddressableLoader.Get<KeywordData>("KeywordData", s);
                            ((CardPopUpPanel)value).SetNote(keyword.note.Replace("{0}", count), keyword.noteColour);
                            ((CardPopUpPanel)value).BuildTextElement();
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FinalBossGenerationSettings), "ProcessEffects", new Type[]
            {
            typeof(IList<CardData>)
            })]
        internal static class AppendEffectSwapper
        {
            internal static void Prefix(FinalBossGenerationSettings __instance)
            {
                List<FinalBossEffectSwapper> swappers = new List<FinalBossEffectSwapper>();
                swappers.Add(CreateSwapper("When Deployed Add Mirage To Hand", "On Turn Summon Mirage", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Charging Mode Button", "MultiHit", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Charging Mode Button Listener_1", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Charging Mode Button Listener_2", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Boiling Burst Button", "Temporary Barrage", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Boiling Burst Button Listener_1", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Boiling Burst Button Listener_2", "MultiHit", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Monitor Button", "On Turn Apply Bom Until Turn End To Enemies", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Divine Avatar Button", "On Hit Equal Heal To Ally With Lowest Health", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Divine Avatar Button Listener_1", "Damage Equal To Health", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Inheritance of Faith Button", "On Turn Heal Ally With The Lowest Health", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Spirit Burst Button", "MultiHit", minBoost: 0, maxBoost: 1));
                swappers.Add(CreateSwapper("Spirit Burst Button Listener_1", "Hit Truly Random Target", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Feathershine Arrows Button", "Temporary Smackback", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Trigger Against Anything That Attacks", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Calcification Button", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Anatta Button", "MultiHit", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Originite Prime Button", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Bloodline of Desecrated Earth Button", "Temporary Splash", minBoost: 1, maxBoost: 1));
                swappers.Add(CreateSwapper("Bloodline of Desecrated Earth Button Listener_1", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Bloodline of Desecrated Earth Button Listener_2", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Bloodline of Desecrated Earth Button Listener_3", "Apply Snow to All Enemies When Losing All Snow", minBoost: 1, maxBoost: 3));
                swappers.Add(CreateSwapper("Twilight Button", "Temporary Barrage", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Twilight Button Listener_1", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Order of the Icefield Button", "MultiHit", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Paenitete Button", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Soul of the Jungle Button", "Block", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Destreza Button", "On Turn Apply Reduce Max Counter To Self", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Saw of Strength Button", "Temporary Barrage", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Iron Defense Button", "On Turn Apply Block To Allies", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Iron Defense Button Listener_1", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Opprobrium Button", "On Turn Summon Typewriter", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Trial of Thorns Button",  minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Trial of Thorns Button Listener_1", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("Trial of Thorns Button Listener_2", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("On Turn Decrease Cooldown To Allies", "On Turn Reduce Counter To Allies", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("On Turn Heal Mon3tr And Kal'tsit", "On Turn Heal Mon3tr2 And Kal'tsit", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("When Deployed Add Mon3tr To Hand", "When Deployed Summon Mon3tr2", minBoost: 0, maxBoost: 0));
                swappers.Add(CreateSwapper("While Active Increase Cooldown Countdown", "On Card Played Reduce Counter To Allies", minBoost: 0, maxBoost: 0));
                __instance.effectSwappers = __instance.effectSwappers.AddRangeToArray(swappers.ToArray()).ToArray();
            }

            internal static FinalBossEffectSwapper CreateSwapper(string effect, string replaceOption = null, string attackOption = null, int minBoost = 0, int maxBoost = 0)
            {
                FinalBossEffectSwapper swapper = ScriptableObject.CreateInstance<FinalBossEffectSwapper>();
                swapper.effect = Frostknights.instance.Get<StatusEffectData>(effect);
                swapper.replaceWithOptions = new StatusEffectData[0];
                String s = "";
                if (!replaceOption.IsNullOrEmpty())
                {
                    swapper.replaceWithOptions = swapper.replaceWithOptions.Append(Frostknights.instance.Get<StatusEffectData>(replaceOption)).ToArray();
                    s += swapper.replaceWithOptions[0].name;
                }
                if (!attackOption.IsNullOrEmpty())
                {
                    swapper.replaceWithAttackEffect = Frostknights.instance.Get<StatusEffectData>(attackOption);
                    s += swapper.replaceWithAttackEffect.name;
                }
                if (s.IsNullOrEmpty())
                {
                    s = "Nothing";
                }
                swapper.boostRange = new Vector2Int(minBoost, maxBoost);
                Debug.Log($"[Frostknights] {swapper.effect.name} => {s} + {swapper.boostRange}");
                return swapper;
            }
        }

    }
}
