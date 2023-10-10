using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.Unlocks
{
    class HereticGlobalUnlock : UnlockBase
    {
        public static string HereticGlobalUnlockString()
        {
            return new HereticGlobalUnlock().UnlockLangTokenName;
        }

        public override string UnlockLangTokenName => "GLOBALHERETICUNLOCK";

        public override string UnlockName => "Complete Unorthodoxy";

        public override string AchievementName => "Complete Unorthodoxy";

        public override string AchievementDesc => "As any character, beat the game having committed the greatest act of Heresy.";

        public override string PrerequisiteUnlockableIdentifier => "KillBossQuantityInRun";

        public override Sprite Sprite => LegacyResourcesAPI.Load<Sprite>("textures/bodyicons/texHereticIcon");

        public override void Init(ConfigFile config)
        {
            LanguageAPI.Add(AchievementNameToken, AchievementName);
            LanguageAPI.Add(AchievementDescToken, AchievementDesc);
            LanguageAPI.Add(UnlockableNameToken, UnlockName);
        }
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
        }

        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
        }

        public void ClearCheck(Run run, RunReport runReport)
        {
            if (run is null) return;
            if (runReport is null) return;

            if (!runReport.gameEnding) return;

            Debug.Log("FUCK!!!!!!!!!!");
            if (runReport.gameEnding.isWin)
            {
                ItemIndex[] items = runReport.FindFirstPlayerInfo().itemAcquisitionOrder;
                List<ItemIndex> itemms = new List<ItemIndex>(items);

                if(itemms.Contains(RoR2Content.Items.LunarPrimaryReplacement.itemIndex)
                    && itemms.Contains(RoR2Content.Items.LunarSecondaryReplacement.itemIndex)
                    && itemms.Contains(RoR2Content.Items.LunarUtilityReplacement.itemIndex)
                    && itemms.Contains(RoR2Content.Items.LunarSpecialReplacement.itemIndex))
                {
                    base.Grant();
                }
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();

            Run.onClientGameOverGlobal += this.ClearCheck;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();

            Run.onClientGameOverGlobal -= this.ClearCheck;
        }
    }
}
