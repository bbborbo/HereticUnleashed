using BepInEx.Configuration;
using HereticUnchained.CoreModules;
using HereticUnchained.EntityState;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.Skills
{
    class BloodPrimarySkill : SkillBase
    {
        public static GameObject tracerLaser;

        public override string SkillName => "Sanguine Verdict"; //crimson foresight, touch? of malice

        public override string SkillDescription => $"<style=cIsHealth>{Tools.ConvertDecimal(BloodPrimary.healthCostFraction)} HP</style>. <style=cIsDamage>Slayer</style>. " +
            $"Fire a laser <style=cIsDamage>{BloodPrimary.baseFireRate}x</style> per second that " +
            $"deals <style=cIsDamage>{Tools.ConvertDecimal(BloodPrimary.damageCoefficient)} damage</style>.";
        public override string SkillFullDescription => 
            $"<style=cIsHealth>{Tools.ConvertDecimal(BloodPrimary.healthCostFraction)} HP</style>. <style=cIsDamage>Slayer</style>. " +
            $"Fire an intense laser " +
            $"<style=cIsDamage>{BloodPrimary.baseFireRate}</style> <style=cStack>(+{BloodPrimary.stackFireRate} per stack)</style> times per second " +
            $"that deals <style=cIsDamage>{Tools.ConvertDecimal(BloodPrimary.damageCoefficient)} damage</style>.";

        public override string SkillLangTokenName => "BLOODPRIMARY";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "SanguineVerdict";

        public override Type ActivationState => typeof(BloodPrimary);

        public override string CharacterName => HereticPlugin.hereticBodyName;

        public override SkillFamilyName SkillSlot => SkillFamilyName.Primary;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            stockToConsume = 0,
            requiredStock = 0,
            cancelSprintingOnActivation = true,
            interruptPriority = EntityStates.InterruptPriority.Any
        };

        public override Type BaseSkillDef => typeof(SkillDef);

        public override void Hooks()
        {

        }

        public override void Init(ConfigFile config)
        {
            KeywordTokens = new string[2] { "KEYWORD_PERCENT_HP", "KEYWORD_SLAYER" };
            CreateLang();
            CreateSkill();
            Hooks();
            CreateTracer();
        }
        private void CreateTracer()
        {
            tracerLaser = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/TracerGolem").InstantiateClone("HereticBloodLaser", false);
            Tracer buckshotTracer = tracerLaser.GetComponent<Tracer>();
            buckshotTracer.speed = 300f;
            buckshotTracer.length = 15f;
            buckshotTracer.beamDensity = 10f;
            VFXAttributes buckshotAttributes = tracerLaser.AddComponent<VFXAttributes>();
            buckshotAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            buckshotAttributes.vfxIntensity = VFXAttributes.VFXIntensity.High;

            Tools.GetParticle(tracerLaser, "SmokeBeam", new Color(0.2f, 0.05f, 0.15f), 0.5f);
            ParticleSystem.MainModule main = tracerLaser.GetComponentInChildren<ParticleSystem>().main;
            main.startSizeXMultiplier *= 0.4f;
            main.startSizeYMultiplier *= 0.4f;
            main.startSizeZMultiplier *= 2f;

            Assets.CreateEffect(tracerLaser);
        }
    }
}
