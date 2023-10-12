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

namespace HereticUnchained.SurvivorTweaks
{
    public class HereticTweaks
    {
        public static GameObject secondaryProjectile = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/LunarSecondaryProjectile");
        public static float secondaryMaxCharge = 3; //2f
        public static float secondaryBladesDamage = 1f;
        public static float secondaryBladesFrequency = 6f;
        public static float secondaryBladesProc = 0.5f;
        public static float secondaryExplosionDamage = 9f;
        public static float secondaryExplosionProc = 1;

        public static GameObject bodyObject;

        public SkillLocator skillLocator;
        public SkillFamily primary;
        public SkillFamily secondary;
        public SkillFamily utility;
        public SkillFamily special;

        public static float shadowfadeBaseHealFraction = 0.25f;

        public string survivorName = "The Heretic";

        public string bodyName = "HereticBody";

        public static void Init()
        {
            HereticTweaks.bodyObject = LegacyResourcesAPI.Load<GameObject>($"prefabs/characterbodies/HereticBody");

            #region body
            CharacterBody vanillaHereticBody = bodyObject.GetComponent<CharacterBody>();
            vanillaHereticBody.baseMaxHealth = 260;
            vanillaHereticBody.baseRegen = -4;
            vanillaHereticBody.baseDamage = 16;
            vanillaHereticBody.baseArmor = 30;

            vanillaHereticBody.levelMaxHealth = vanillaHereticBody.baseMaxHealth * 0.3f;
            vanillaHereticBody.levelRegen = vanillaHereticBody.baseRegen * 0.2f;
            vanillaHereticBody.levelDamage = vanillaHereticBody.baseDamage * 0.2f;
            #endregion

            #region secondary
            ProjectileDotZone blades = secondaryProjectile.GetComponent<ProjectileDotZone>();
            blades.damageCoefficient = secondaryBladesDamage / secondaryExplosionDamage;
            blades.resetFrequency = secondaryBladesFrequency;
            blades.fireFrequency = secondaryBladesFrequency * 3;
            blades.overlapProcCoefficient = secondaryBladesProc / secondaryExplosionProc;

            ProjectileExplosion explosion = secondaryProjectile.GetComponent<ProjectileExplosion>();
            explosion.blastDamageCoefficient = 1;
            explosion.blastProcCoefficient = secondaryExplosionProc;
            explosion.falloffModel = BlastAttack.FalloffModel.Linear;
            explosion.blastRadius = 17f;

            On.EntityStates.Mage.Weapon.BaseThrowBombState.OnEnter += HooksDamageBuff;
            On.EntityStates.Mage.Weapon.BaseChargeBombState.OnEnter += HooksChargeTweak;

            LanguageAPI.Add("SKILL_LUNAR_SECONDARY_REPLACEMENT_DESCRIPTION",
                $"Charge up a ball of blades that " +
                $"deals <style=cIsDamage>{Tools.ConvertDecimal(secondaryBladesDamage * secondaryBladesFrequency)} damage per second</style>. " +
                $"After a delay, explode and " +
                $"<style=cIsDamage>root</style> all enemies " +
                $"for <style=cIsDamage>{Tools.ConvertDecimal(secondaryExplosionDamage)} damage</style>.");
            #endregion

            #region utility
            On.EntityStates.GhostUtilitySkillState.OnEnter += ShadowfadeEnter;
            On.EntityStates.GhostUtilitySkillState.OnEnter += ShadowfadeExit;

            LanguageAPI.Add("SKILL_LUNAR_UTILITY_REPLACEMENT_DESCRIPTION",
                $"Fade away, becoming <style=cIsUtility>intangible</style> " +
                $"and <style=cIsUtility>gaining movement speed</style>. " +
                $"<style=cIsHealing>Heal</style> for " +
                $"<style=cIsHealing>{Tools.ConvertDecimal(shadowfadeBaseHealFraction)} of your maximum health</style>.");
            #endregion
        }

        private static void HooksChargeTweak(On.EntityStates.Mage.Weapon.BaseChargeBombState.orig_OnEnter orig, EntityStates.Mage.Weapon.BaseChargeBombState self)
        {
            if (self is EntityStates.GlobalSkills.LunarNeedle.ChargeLunarSecondary)
            {
                self.baseDuration = secondaryMaxCharge;
            }
            orig(self);
        }

        private static void HooksDamageBuff(On.EntityStates.Mage.Weapon.BaseThrowBombState.orig_OnEnter orig, EntityStates.Mage.Weapon.BaseThrowBombState self)
        {
            if (self is EntityStates.GlobalSkills.LunarNeedle.ThrowLunarSecondary)
            {
                self.minDamageCoefficient = secondaryExplosionDamage;
                self.maxDamageCoefficient = secondaryExplosionDamage;
            }
            orig(self);
        }

        private static void ShadowfadeEnter(On.EntityStates.GhostUtilitySkillState.orig_OnEnter orig, GhostUtilitySkillState self)
        {
            orig(self);
            GhostUtilitySkillState.healFractionPerTick = shadowfadeBaseHealFraction / (GhostUtilitySkillState.baseDuration * GhostUtilitySkillState.healFrequency);
        }

        private static void ShadowfadeExit(On.EntityStates.GhostUtilitySkillState.orig_OnEnter orig, GhostUtilitySkillState self)
        {
            if (NetworkServer.active)
            {
                self.healthComponent.HealFraction(GhostUtilitySkillState.healFractionPerTick, default(ProcChainMask));
            }
            orig(self);
        }
    }
}
