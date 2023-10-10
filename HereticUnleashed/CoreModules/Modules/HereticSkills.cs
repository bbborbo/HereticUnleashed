using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HereticUnchained.CoreModules
{
    internal class HereticSkills : CoreModule
    {
        internal static SkillDef nevermoreSkill;
        internal static SkillDef lunarPrimarySkill;
        internal static SkillDef lunarSecondarySkill;
        internal static SkillDef lunarUtilitySkill;
        internal static SkillDef lunarSpecialSkill;

        public override void Init()
        {
            //failsafes
            //On.RoR2.Inventory.GetItemCount_ItemIndex += ItemCountSafetyNet;
            //On.RoR2.GenericSkill.RecalculateMaxStock += MaxStockSafetyNet;
            //On.RoR2.GenericSkill.CalculateFinalRechargeInterval += RechargeIntervalSafetyNet;
        }

        #region FAILSAFES
        private float RechargeIntervalSafetyNet(On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, GenericSkill self)
        {
            try
            {
                return Mathf.Min(self.baseRechargeInterval, Mathf.Max(0.5f, self.baseRechargeInterval * self.cooldownScale - self.flatCooldownReduction));
            }
            catch
            {
                //Debug.Log("Failed to get recharge interval for a skill!");
                return 1;
            }
        }

        private void MaxStockSafetyNet(On.RoR2.GenericSkill.orig_RecalculateMaxStock orig, GenericSkill self)
        {
            try
            {
                if (self != null)
                {
                    orig(self);
                }
            }
            catch
            {
                //Debug.Log("Failed to get max stock for a skill!");
                self.maxStock = self.skillDef.baseMaxStock + self.bonusStockFromBody;
            }
        }

        private int ItemCountSafetyNet(On.RoR2.Inventory.orig_GetItemCount_ItemIndex orig, Inventory self, ItemIndex itemIndex)
        {
            try
            {
                return orig(self, itemIndex);
            }
            catch
            {
                //Debug.Log("Failed to find item count for index: " + itemIndex);
                return 0;
            }
        }
        #endregion

        internal static void YoinkSkillDefs()
        {
            nevermoreSkill = LegacyResourcesAPI.Load<SkillDef>("skilldefs/lunarreplacements/LunarDetonatorSpecialReplacement");// SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("HereticDefaultSkill"));
            lunarPrimarySkill = LegacyResourcesAPI.Load<SkillDef>("skilldefs/lunarreplacements/LunarPrimaryReplacement");//SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarPrimaryReplacement"));
            lunarSecondarySkill = LegacyResourcesAPI.Load<SkillDef>("skilldefs/lunarreplacements/LunarSecondaryReplacement");//SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarSecondaryReplacement"));
            lunarUtilitySkill = LegacyResourcesAPI.Load<SkillDef>("skilldefs/lunarreplacements/LunarUtilityReplacement");//SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarUtilityReplacement"));
            lunarSpecialSkill = LegacyResourcesAPI.Load<SkillDef>("skilldefs/lunarreplacements/LunarDetonatorSpecialReplacement");//SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("LunarDetonatorSpecialReplacement"));

            lunarSpecialSkill.keywordTokens = new string[1] { "KEYWORD_RUINOUS" };

            AddSkillToSkillFamily(HereticPlugin.hereticPrefab, lunarPrimarySkill, HereticPlugin.primaryFamily);
            AddSkillToSkillFamily(HereticPlugin.hereticPrefab, lunarSecondarySkill, HereticPlugin.secondaryFamily);
            AddSkillToSkillFamily(HereticPlugin.hereticPrefab, lunarUtilitySkill, HereticPlugin.utilityFamily);
            AddSkillToSkillFamily(HereticPlugin.hereticPrefab, lunarSpecialSkill, HereticPlugin.specialFamily);
        }

        internal static void AddSkillToSkillFamily(GameObject targetPrefab, SkillDef skillDef, SkillFamily skillFamily, UnlockableDef unlockDef = null, bool registerSkillDef = true)
        {
            if (unlockDef = null)
            {
                unlockDef = ScriptableObject.CreateInstance<UnlockableDef>();
            }

            int index = skillFamily.variants.Length;
            Array.Resize(ref skillFamily.variants, index + 1);
            skillFamily.variants[index] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = unlockDef,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
            if (registerSkillDef)
            {
                // need to add skilldefs through ETAPI's content pack because HG fucked up skilldef loading and ETAPI has the fix
                Assets.RegisterSkillDef(skillDef);
            }
        }

        internal static void AddPrimarySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            foreach (SkillDef i in skillDefs)
            {
                AddSkillToSkillFamily(targetPrefab, i, HereticPlugin.primaryFamily);
            }
        }
        internal static void AddSecondarySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            foreach (SkillDef i in skillDefs)
            {
                AddSkillToSkillFamily(targetPrefab, i, HereticPlugin.secondaryFamily);
            }
        }
        internal static void AddUtilitySkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            foreach (SkillDef i in skillDefs)
            {
                AddSkillToSkillFamily(targetPrefab, i, HereticPlugin.utilityFamily);
            }
        }
        internal static void AddSpecialSkills(GameObject targetPrefab, params SkillDef[] skillDefs)
        {
            foreach (SkillDef i in skillDefs)
            {
                AddSkillToSkillFamily(targetPrefab, i, HereticPlugin.specialFamily);
            }
        }

        internal static SkillDef CreateSkillDef(SkillDefInfo skillDefInfo)
        {
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();

            skillDef.skillName = skillDefInfo.skillName;
            skillDef.skillNameToken = skillDefInfo.skillNameToken;
            skillDef.skillDescriptionToken = skillDefInfo.skillDescriptionToken;
            skillDef.icon = skillDefInfo.skillIcon;

            skillDef.activationState = skillDefInfo.activationState;
            skillDef.activationStateMachineName = skillDefInfo.activationStateMachineName;
            skillDef.baseMaxStock = skillDefInfo.baseMaxStock;
            skillDef.baseRechargeInterval = skillDefInfo.baseRechargeInterval;
            skillDef.beginSkillCooldownOnSkillEnd = skillDefInfo.beginSkillCooldownOnSkillEnd;
            skillDef.canceledFromSprinting = skillDefInfo.canceledFromSprinting;
            skillDef.forceSprintDuringState = skillDefInfo.forceSprintDuringState;
            skillDef.fullRestockOnAssign = skillDefInfo.fullRestockOnAssign;
            skillDef.interruptPriority = skillDefInfo.interruptPriority;
            skillDef.resetCooldownTimerOnUse = skillDefInfo.resetCooldownTimerOnUse;
            skillDef.isCombatSkill = skillDefInfo.isCombatSkill;
            skillDef.mustKeyPress = skillDefInfo.mustKeyPress;
            skillDef.cancelSprintingOnActivation = skillDefInfo.cancelSprintingOnActivation;
            skillDef.rechargeStock = skillDefInfo.rechargeStock;
            skillDef.requiredStock = skillDefInfo.requiredStock;
            skillDef.stockToConsume = skillDefInfo.stockToConsume;

            skillDef.keywordTokens = skillDefInfo.keywordTokens;

            Assets.RegisterSkillDef(skillDef);

            return skillDef;
        }
    }
}

internal class SkillDefInfo
{
    public string skillName;
    public string skillNameToken;
    public string skillDescriptionToken;
    public Sprite skillIcon;

    public SerializableEntityStateType activationState;
    public string activationStateMachineName;
    public int baseMaxStock;
    public float baseRechargeInterval;
    public bool beginSkillCooldownOnSkillEnd;
    public bool canceledFromSprinting;
    public bool forceSprintDuringState;
    public bool fullRestockOnAssign;
    public InterruptPriority interruptPriority;
    public bool resetCooldownTimerOnUse;
    public bool isCombatSkill;
    public bool mustKeyPress;
    public bool cancelSprintingOnActivation;
    public int rechargeStock;
    public int requiredStock;
    public int stockToConsume;

    public string[] keywordTokens;
}