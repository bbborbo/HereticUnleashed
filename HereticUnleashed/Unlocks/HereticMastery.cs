using BepInEx.Configuration;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.Unlocks
{
    class HereticMastery : UnlockBase
    {
        public override bool HideUnlock => true;

        public override string UnlockLangTokenName => "HERETICMASTERYUNLOCK";

        public override string UnlockName => "Borderline Excessive Unorthodoxy";

        public override string AchievementName => "Borderline Excessive Unorthodoxy";

        public override string AchievementDesc => "As any character on Monsoon difficulty, beat the game having committed the greatest act of Heresy.";

        public override string PrerequisiteUnlockableIdentifier => "KillBossQuantityInRun";

        public override Sprite Sprite => LoadoutAPI.CreateSkinIcon(new Color(0.15f, 0.15f, 0.15f), new Color(0.15f, 0.15f, 0.15f), new Color(0.15f, 0.15f, 0.15f), new Color(0.15f, 0.15f, 0.15f));

        public override void Init(ConfigFile config)
        {
            LanguageAPI.Add(AchievementNameToken, AchievementName);
            LanguageAPI.Add(AchievementDescToken, AchievementDesc);
            LanguageAPI.Add(UnlockableNameToken, UnlockName);
        }
    }
}
