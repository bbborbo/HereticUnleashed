using EntityStates;
using HereticUnchained.Skills;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HereticUnchained.EntityState
{
    class BloodUtility : BaseSkillState
    {
        public static float damageCoefficient = 4f;

        public static float initialSpeedCoefficient = 80;
        public static float finalSpeedCoefficient = 10;
        public static float endSpeedCoefficient = 0.5f;
        private float currentSpeed;
        private Vector3 angle;

        public float baseDuration = 0.2f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = base.GetAimRay();
            this.duration = this.baseDuration;

            //Util.PlaySound(DiggerPlugin.Sounds.Backblast, base.gameObject);
            base.StartAimMode(0.6f, true);

            /*float angle = Vector3.Angle(new Vector3(0, -1, 0), aimRay.direction);
            if (angle < 60)
            {
                base.PlayAnimation("FullBody, Override", "BackblastUp");
            }
            else if (angle > 120)
            {
                base.PlayAnimation("FullBody, Override", "BackblastDown");
            }
            else
            {
                base.PlayAnimation("FullBody, Override", "Backblast");
            }*/

            //if (NetworkServer.active) base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            if (GhostUtilitySkillState.exitEffectPrefab && !this.outer.destroying)
            {
                EffectManager.SimpleEffect(GhostUtilitySkillState.exitEffectPrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), false);
            }
            if (base.isAuthority)
            {
                Vector3 dir = -aimRay.direction;
                dir.y = 0;

                angle = Vector3.up + 2 * dir;//-base.characterMotor.moveDirection;
                Vector3 origin = aimRay.origin - 2 * angle;


                float damage = damageCoefficient * characterBody.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement);
                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    damage = damage * damageStat,
                    crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
                    damageColorIndex = DamageColorIndex.Item,
                    position = origin,
                    force = 0f,
                    owner = base.gameObject,
                    projectilePrefab = BloodUtilitySkill.stormPrefab,
                    rotation = Quaternion.identity,
                    speedOverride = -1,
                    target = null
                });

                this.currentSpeed = initialSpeedCoefficient;
                SetSpeed();

                /*BlastAttack blastAttack = new BlastAttack();
                blastAttack.radius = 14f;
                blastAttack.procCoefficient = 1f;
                blastAttack.position = theSpot;
                blastAttack.attacker = base.gameObject;
                blastAttack.crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
                blastAttack.baseDamage = base.characterBody.damage * BloodUtility.damageCoefficient;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.baseForce = 500f;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                blastAttack.damageType = DamageType.Stun1s;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHit;
                BlastAttack.Result result = blastAttack.Fire();

                EffectData effectData = new EffectData();
                effectData.origin = theSpot;
                effectData.scale = 15;*/

                //EffectManager.SpawnEffect(DiggerPlugin.DiggerPlugin.backblastEffect, effectData, false);
            }
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                //base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                //base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f);
            }
            if (GhostUtilitySkillState.entryEffectPrefab)
            {
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleEffect(GhostUtilitySkillState.entryEffectPrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), false);
            }

            base.characterMotor.velocity *= endSpeedCoefficient;

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.currentSpeed = (this.moveSpeedStat / this.characterBody.baseMoveSpeed) 
                * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, base.fixedAge / this.duration);
            SetSpeed();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void SetSpeed()
        {
            base.characterMotor.velocity = angle * currentSpeed;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
