using BepInEx.Configuration;
using HereticUnchained.EntityState;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace HereticUnchained.Skills
{
    class ScepterLesionsSkill : ScepterSkillBase
    {
        public override int TargetVariant => 1;

        public override string SkillName => "Lamenting Lesions"; //weeping lesions - lament, 

        public override string SkillDescription => "Channel a <style=cIsDamage>barrage of blades</style> " +
            $"that <style=cIsDamage>bleeds</style> EVERYTHING in sight " +
            $"for <style=cIsDamage>{Tools.ConvertDecimal(BloodSpecial.bleedDurationBase * 0.8f)} damage</style>... " +
            $"<style=cIsHealth>including yourself</style>." +
            $"\n<color=#d299ff>SCEPTER: No longer roots yourself. All enemies hurt by Lesions are also slowed by 80%.</color>";

        public override string SkillLangTokenName => "SCEPTERLESIONS";

        public override string IconName => "";


        public override Type ActivationState => typeof(ScepterLesions);

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            interruptPriority = EntityStates.InterruptPriority.Skill,
            baseRechargeInterval = 15,
            canceledFromSprinting = true,
            beginSkillCooldownOnSkillEnd = true
        };
        public override Type BaseSkillDef => typeof(LunarLesionsSkillDef);

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
