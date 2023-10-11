using BepInEx.Configuration;
using HereticUnchained.EntityState;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;

namespace HereticUnchained.Skills
{ 
    class MassPrimarySkill : SkillBase
    {
        public static int baseMaxStock = 2;
        public static float baseRechargeInterval = 2;

        public override string SkillName => "Blind Allegiance"; //splice

        public override string SkillDescription => "Warp forward a short distance, splicing nearby enemies.";

        public override string SkillFullDescription => $"Warp forward {0} meters, " +
            $"splicing enemies in your immediate vicinity for {Tools.ConvertDecimal(0)} damage. " +
            $"Hold up to {baseMaxStock} charges (+{baseMaxStock} per stack) that " +
            $"reload after {baseRechargeInterval} seconds (+{baseRechargeInterval} per stack).";

        public override string SkillLangTokenName => "MASSPRIMARY";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "";

        public override Type ActivationState => typeof(MassPrimary);

        public override Type BaseSkillDef => typeof(LunarPrimaryReplacementSkill);

        public override string CharacterName => HereticPlugin.hereticBodyName;

        public override SkillFamilyName SkillSlot => SkillFamilyName.Primary;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 1,
            requiredStock = 1,
            cancelSprintingOnActivation = true,
            interruptPriority = EntityStates.InterruptPriority.Any,
            baseMaxStock = baseMaxStock,
            baseRechargeInterval = baseRechargeInterval,
            resetCooldownTimerOnUse = false,
            mustKeyPress = true
        };

        public override void Hooks()
        {

        }

        public override void Init(ConfigFile config)
        {
            return;
            CreateLang();
            CreateSkill();
            Hooks();
        }
    }
}
