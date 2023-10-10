using EntityStates;
using EntityStates.Huntress;
using EntityStates.Merc;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.EntityState
{
    class MassPrimary : BlinkState
    {
        public static float speedCoeff = 20f;
        public static float baseDuration = 0.05f;

        public static float damagePerSecond = 3f;
        public static float procCoefficient = 0.5f;
        float damageCoefficient;
        public static float radius = 3f;
        static float force = 250;
        static Vector3 bonusForce = Vector3.zero;


        public override void OnEnter()
        {
            duration = baseDuration / this.attackSpeedStat;
            speedCoefficient = speedCoeff * this.attackSpeedStat;

            /*baseDuration = 0.3f;
            damageCoefficient = damagePerSecond * baseDuration;
            procCoefficient = 0.8f;

            hitEffectPrefab = hitEffect;
            swingEffectPrefab = swingEffect;

            hitPauseDuration = 0.1f;
            pushAwayForce = force;*/

            base.OnEnter();
            damageCoefficient = damagePerSecond;
        }

        public override Vector3 GetBlinkVector()
        {
            return ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
        }

        public override void OnExit()
        {
            base.OnExit();

            float blastRadius = radius * this.characterBody.bestFitRadius;

            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
            {
                origin = this.characterBody.corePosition,
                scale = blastRadius
            }, true);
            EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
            {
                origin = this.characterBody.corePosition,
                scale = blastRadius
            }, true);
            new BlastAttack
            {
                attacker = base.gameObject,
                baseDamage = this.damageStat * this.damageCoefficient,
                baseForce = force,
                bonusForce = bonusForce,
                crit = Util.CheckRoll(this.critStat, this.characterBody?.master),
                damageType = this.GetBlastDamageType(),
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = procCoefficient,
                radius = blastRadius,
                position = this.characterBody.corePosition,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                //impactEffect = EffectCatalog.FindEffectIndexFromPrefab(this.blastImpactEffectPrefab),
                teamIndex = base.teamComponent.teamIndex
            }.Fire();
        }
        protected virtual DamageType GetBlastDamageType()
        {
            return DamageType.Generic;
        }
    }
}
