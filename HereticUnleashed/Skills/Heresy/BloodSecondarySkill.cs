using BepInEx.Configuration;
using HereticUnchained.EntityState;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;

namespace HereticUnchained.Skills
{
    class BloodSecondarySkill : SkillBase
    {
        public override string SkillName => "Touch of Malice"; //vicious, catabolic; slash, rend

        public override string SkillDescription => "Slash through enemies in front of you, crippling them and yourself.";

        public override string SkillFullDescription => "Hold to slash through enemies in front of you, dealing X% damage and crippling every target hit including yourself for Y seconds.";

        public override string SkillLangTokenName => "BLOODSECONDARY";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "";

        public override Type ActivationState => typeof(BloodSecondary);

        public override Type BaseSkillDef => typeof(SkillDef);

        public override string CharacterName => HereticPlugin.hereticBodyName;

        public override SkillFamilyName SkillSlot => SkillFamilyName.Secondary;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 1,
            requiredStock = 1,
            cancelSprintingOnActivation = true,
            interruptPriority = EntityStates.InterruptPriority.Any
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
