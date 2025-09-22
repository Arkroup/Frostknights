using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.VFX;
using static WildfrostHopeMod.VFX.GIFLoader;

namespace Frostknights
{
    public class FXHelper
    {
        public GIFLoader giffy;
        public SFXLoader silly;
        public WildfrostMod mod;
        public FXHelper(WildfrostMod mod, string animLocation, string soundLocation)
        {
            giffy = new GIFLoader(null, mod.ImagePath(animLocation));
            giffy.RegisterAllAsApplyEffect();

            silly = new SFXLoader(mod.ImagePath(soundLocation), initialize: true);
            silly.LoadSoundsFromDir(silly.Directory);
            //silly.RegisterAllSoundsToGlobal();
        }

        public void TryPlaySound(string key, SFXLoader.PlayAs playAs = SFXLoader.PlayAs.SFX)
        {
            silly.TryPlaySound(key, playAs);
        }

        public GameObject TryPlayEffect(string key, Vector3 position = default(Vector3), Vector3 scale = default(Vector3), PlayType playAs = PlayType.applyEffect)
        {
            return giffy.TryPlayEffect(key, position, scale, playAs);
        }
    }
}
