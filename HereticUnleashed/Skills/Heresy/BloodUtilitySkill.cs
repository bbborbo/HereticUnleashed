using BepInEx.Configuration;
using HereticUnchained.Components;
using HereticUnchained.EntityState;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.Skills
{
    class BloodUtilitySkill : SkillBase
    {
        public static float vampirismHealFraction = 0.08f;
        public static float vampirismMaxHealPortion = 0.1f;

        public static GameObject stormPrefab;
        public static float stormDuration = 3f;
        public static string vampirismKeywordToken = HereticPlugin.TokenName + "KEYWORD_VAMPIRISM";
        public override string SkillName => "Duskwarp"; //duskwarp

        public override string SkillDescription => $"<style=cIsHealing>Vampiric</style>. " +
            $"Dash backwards, leaving behind a <style=cIsDamage>slicing vortex</style> that deals " +
            $"<style=cIsDamage>{Tools.ConvertDecimal(BloodUtility.damageCoefficient)} damage per second</style>.";
        public override string SkillFullDescription => 
            $"Your attacks <style=cIsHealing>heal</style> for " +
            $"<style=cIsHealing>{Tools.ConvertDecimal(vampirismHealFraction)} of damage dealt</style>." +
            $"Dash backwards, leaving behind a <style=cIsDamage>slicing vortex</style> for {stormDuration} seconds that deals " +
            $"<style=cIsDamage>{Tools.ConvertDecimal(BloodUtility.damageCoefficient * stormDuration)}</style> " +
            $"<style=cIsStack>(+{BloodUtility.damageCoefficient * stormDuration} per stack)</style> damage. " +
            $"Recharges after {SkillData.baseRechargeInterval} " +
            $"<style=cIsStack>(+{LunarVampirismSkillDef.cooldownPerStack} per stack)</style> seconds.";

        public override string SkillLangTokenName => "VAMPIREUTILITY";

        public override UnlockableDef UnlockDef => null;

        public override string IconName => "Duskwarp";

        public override Type ActivationState => typeof(BloodUtility);

        public override string CharacterName => HereticPlugin.hereticBodyName;

        public override SkillFamilyName SkillSlot => SkillFamilyName.Utility;

        public override SimpleSkillData SkillData => new SimpleSkillData()
        {
            baseMaxStock = 1,
            baseRechargeInterval = 9,
            interruptPriority = EntityStates.InterruptPriority.PrioritySkill
        };
        public override Type BaseSkillDef => typeof(LunarVampirismSkillDef);

        public override void Hooks()
        {

        }

        public override void Init(ConfigFile config)
        {
            KeywordTokens = new string[1] { vampirismKeywordToken };
            LanguageAPI.Add(vampirismKeywordToken, $"<style=cKeywordName>Vampiric</style>" +
                $"<style=cSub>PASSIVE: While this skill is ready to use, " +
                $"<style=cIsDamage>your attacks</style> can <style=cIsHealing>heal</style> for " +
                $"<style=cIsHealing>{Tools.ConvertDecimal(vampirismHealFraction)}</style> " +
                $"of <style=cIsDamage>total damage</style>.");
            CreateLang();
            CreateSkill();
            Hooks();
            CreateStorm();
        }

        private void CreateStorm()
        {
            stormPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/FireTornado").InstantiateClone("HeresyVampireStorm", true);
            Tools.DebugParticleSystem(stormPrefab);
            ProjectileOverlapAttack overlap = stormPrefab.GetComponent<ProjectileOverlapAttack>();
            overlap.overlapProcCoefficient = 0.75f;
            overlap.damageCoefficient = overlap.resetInterval;
            ProjectileSimple simple = stormPrefab.GetComponent<ProjectileSimple>();
            simple.lifetime = 3;
        }
    }
}
