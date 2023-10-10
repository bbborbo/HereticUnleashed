using EntityStates;
using EntityStates.GlobalSkills.LunarNeedle;
using HereticUnchained.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HereticUnchained.EntityState
{
    class BloodPrimary : BaseSkillState
    {
        public static int baseFireRate = 3;
        public static int stackFireRate = 2;
        float fireRate;
        float duration;
        float stopwatch;

        public static float damageCoefficient = 1.8f;
        public static float force = 0;
        public static float healthCostFraction = 0.05f;
        public static float spreadBloomValue = 0.2f;
        public static float maxRange = 100f;

        public static GameObject hitEffectPrefab;
        public override void OnEnter()
        {
            fireRate = baseFireRate;

            Inventory inv = characterBody.inventory;
            if (inv)
            {
                fireRate += (inv.GetItemCount(RoR2.RoR2Content.Items.LunarPrimaryReplacement) - 1) * stackFireRate;
            }

            duration = 1 / (fireRate * attackSpeedStat);

            base.OnEnter();

            FireBullet();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (base.isAuthority && !IsKeyDownAuthority())
            {
                if(stopwatch > duration)
                    this.outer.SetNextStateToMain();
            }
            else
            {
                while (stopwatch >= duration)
                {
                    stopwatch -= duration;
                    FireBullet();
                }
            }
        }

        void FireBullet()
        {
            if (NetworkServer.active && base.healthComponent && healthCostFraction >= Mathf.Epsilon)
            {
                DamageType dt = DamageType.NonLethal;
                dt |= DamageType.BypassArmor;
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = base.healthComponent.fullCombinedHealth * healthCostFraction;
                damageInfo.position = base.characterBody.corePosition;
                damageInfo.force = Vector3.zero;
                damageInfo.damageColorIndex = DamageColorIndex.Default;
                damageInfo.crit = false;
                damageInfo.attacker = null;
                damageInfo.inflictor = null;
                damageInfo.damageType = dt;
                damageInfo.procCoefficient = 0f;
                damageInfo.procChainMask = default(ProcChainMask);
                base.healthComponent.TakeDamage(damageInfo);
            }

            Ray aimRay = base.GetAimRay();
            if (base.isAuthority)
            {
                new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    damage = damageCoefficient * this.damageStat,
                    damageType = DamageType.BonusToLowHealth,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    force = force,
                    tracerEffectPrefab = BloodPrimarySkill.tracerLaser,
                    muzzleName = "Head",
                    hitEffectPrefab = hitEffectPrefab,
                    isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
                    radius = 0.1f,
                    maxDistance = maxRange,
                    smartCollision = true
                }.Fire();
            }
            EffectManager.SimpleMuzzleFlash(FireLunarNeedle.muzzleFlashEffectPrefab, base.gameObject, "Head", false);

            base.StartAimMode(2f, false);
            base.PlayAnimation("Gesture, Override", "FireLunarNeedle", "FireLunarNeedle.playbackRate", this.duration);
            base.AddRecoil(-0.4f * FireLunarNeedle.recoilAmplitude, -0.8f * FireLunarNeedle.recoilAmplitude, -0.3f * FireLunarNeedle.recoilAmplitude, 0.3f * FireLunarNeedle.recoilAmplitude);
            base.characterBody.AddSpreadBloom(spreadBloomValue / attackSpeedStat);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
