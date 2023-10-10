using BepInEx.Configuration;
using HereticUnchained.EntityState;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2.Skills;

namespace HereticUnchained.Skills
{
    class ScepterRuinSkill : ScepterSkillBase
    {
        public override int TargetVariant => 0;

        public override string SkillName => "Calamitous Wake"; //ruinous wake

        public override string SkillDescription => $"<style=cIsUtility>Ruinous.</style> " +
            $"Activating this skill <style=cIsDamage>detonates</style> all Ruin at an unlimited range, " +
            $"dealing <style=cIsDamage>300% damage</style> " +
            $"plus <style=cIsDamage>120% per stack of Ruin</style>. " +
            $"\n<color=#d299ff>SCEPTER: All enemies killed with Ruin damage explode for X% of their max health plus Y% base damage.</color>";

        public override string SkillLangTokenName => "SCEPTERRUIN";

        public override string IconName => "";


        public override Type ActivationState => typeof(ScepterRuin);

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            interruptPriority = EntityStates.InterruptPriority.Skill,
            baseRechargeInterval = 15,
            canceledFromSprinting = true,
            beginSkillCooldownOnSkillEnd = true
        };

        public override Type BaseSkillDef => typeof(LunarDetonatorSkill);

        public override void Hooks()
        {

        }

        public override void Init(ConfigFile config)
        {
            Hooks();
            CreateLang();
            CreateSkill();
        }
    }
}
