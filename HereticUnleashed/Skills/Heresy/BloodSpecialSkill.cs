using BepInEx.Configuration;
using HereticUnchained.EntityState;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace HereticUnchained.Skills
{
    class BloodSpecialSkill : SkillBase
    {
        public override string SkillName => "Weeping Lesions"; //Weeping Lesions

        public override string SkillDescription => $"Hold to <style=cIsDamage>root yourself</style> in place, " +
            $"channeling a <style=cIsDamage>barrage of blades</style> " +
            $"that <style=cIsDamage>bleeds</style> everything in sight " +
            $"for <style=cIsDamage>{Tools.ConvertDecimal(BloodSpecial.bleedDurationBase * 0.8f)} damage</style>... " +
            $"<style=cIsHealth>including yourself</style>.";
        public override string SkillFullDescription => $"Hold to <style=cIsDamage>root yourself</style> in place, " +
            $"channeling a <style=cIsDamage>barrage of blades</style> " +
            $"that <style=cIsDamage>bleeds</style> EVERYTHING in sight " +
            $"for <style=cIsDamage>{Tools.ConvertDecimal(BloodSpecial.bleedDurationBase * 0.8f)} damage</style> " +
            $"<style=cIsStack>(+{Tools.ConvertDecimal(BloodSpecial.bleedDurationBase * 0.8f)} per stack)</style>... " +
            $"<style=cIsHealth>including yourself</style>. " +
            $"Recharges after {SkillData.baseRechargeInterval} " +
            $"<style=cIsStack>(+{LunarLesionsSkillDef.cooldownPerStack} per stack)</style> seconds.";

        public override string SkillLangTokenName => "BLOODSPECIAL";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "";

        public override Type ActivationState => typeof(BloodSpecial);

        public override string CharacterName => HereticPlugin.hereticBodyName;

        public override SkillFamilyName SkillSlot => SkillFamilyName.Special;

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
            On.RoR2.CharacterBody.RecalculateStats += RootChange;
        }

        private void RootChange(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(RoR2Content.Buffs.LunarSecondaryRoot) || self.HasBuff(RoR2Content.Buffs.Nullified))
            {
                self.jumpPower = 0;
            }
        }

        public override void Init(ConfigFile config)
        {
            CreateLang();
            CreateSkill();
            Hooks();
        }
    }
}
