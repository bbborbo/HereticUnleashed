using EntityStates;
using EntityStates.GlobalSkills.LunarDetonator;
using EntityStates.Mage.Weapon;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HereticUnchained.EntityState
{
	public class BloodSpecial : BaseSkillState
	{
		[SerializeField]
		public GameObject flamethrowerEffectPrefab = new Flamethrower().flamethrowerEffectPrefab;
		public GameObject detonationEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/BleedOnHitAndExplode_Explosion");//Detonate.detonationEffectPrefab;
		public static GameObject impactEffectPrefab = Flamethrower.impactEffectPrefab;
		public static GameObject tracerEffectPrefab = Flamethrower.tracerEffectPrefab;


		public static float baseEntryDuration = 0.5f;
		public static float baseExitDuration = 0.5f;
		public static float minimumAttackDuration = 1f;

		public static float tickFrequency = 2;
		public static float damageCoefficientPerTick = 0.5f;
		public static float procCoefficientPerTick = 0.5f;

		public static float bleedPercentChance = 100f;
		public static float bleedDurationBase = 3;
		public static float bleedDurationStack = 2;
		public static float selfBleedDamageMultiplier = 0.2f;
		public static float enemyBleedDamageMultiplier = 1f;
		private bool isCrit;

		[SerializeField]
		public static float maxDistance = 125f;
		private static float flamethrowerEffectBaseDistance = maxDistance * 0.8f;
		public static float radius;
		public static float force = 20f;
		public static float recoilForce;

		public static string startAttackSoundString;
		public static string endAttackSoundString;

		private float stopwatch;
		private float flamethrowerStopwatch;
		private float entryDuration;
		private float exitDuration;
		private float tickRate;
		private float bleedDuration;

		private float fieldOfView = new EntityStates.Treebot.Weapon.FireSonicBoom().fieldOfView;
		private bool hasBegunFlamethrower;
		private bool isExitingFlamethrower;

		private ChildLocator childLocator;
		private Transform flamethrowerTransform;
		public static string muzzleName = "Head";
		private Transform muzzleTransform;


		public override void OnEnter()
		{
			base.OnEnter();
			this.stopwatch = 0f;
			entryDuration = baseEntryDuration / attackSpeedStat;
			exitDuration = baseExitDuration / attackSpeedStat;
			tickRate = 1 / (tickFrequency * attackSpeedStat);

			bleedDuration = bleedDurationBase;
			Inventory inv = characterBody.inventory;
            if (inv)
            {
				bleedDuration += bleedDurationStack * (characterBody.inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement) - 1);
			}

			Transform modelTransform = base.GetModelTransform();
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(entryDuration + minimumAttackDuration + exitDuration + bleedDuration);
			}
			if (modelTransform)
			{
				childLocator = modelTransform.GetComponent<ChildLocator>();
				muzzleTransform = childLocator.FindChild(muzzleName);
			}
			if (base.isAuthority && characterBody)
			{
				this.isCrit = Util.CheckRoll(this.critStat, base.characterBody.master);
			}
			//base.PlayAnimation("Gesture, Additive", "PrepFlamethrower", "Flamethrower.playbackRate", this.entryDuration);
		}

		public override void OnExit()
		{
			if (characterBody.HasBuff(RoR2Content.Buffs.LunarSecondaryRoot))
			{
				characterBody.RemoveBuff(RoR2Content.Buffs.LunarSecondaryRoot);
			}

			Util.PlaySound(Flamethrower.endAttackSoundString, base.gameObject);
			//base.PlayCrossfade("Gesture, Additive", "ExitFlamethrower", 0.1f);
			if (this.flamethrowerTransform)
			{
				Destroy(this.flamethrowerTransform.gameObject);
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatch += Time.fixedDeltaTime;


			if (this.stopwatch >= this.entryDuration && !this.hasBegunFlamethrower)
			{
				this.hasBegunFlamethrower = true;
				Util.PlaySound(Flamethrower.startAttackSoundString, base.gameObject);
				//base.PlayAnimation("Gesture, Additive", "Flamethrower", "Flamethrower.playbackRate", minimumAttackDuration);
				if (this.childLocator)
				{
					Transform transform = this.childLocator.FindChild(muzzleName);
					if (transform)
					{
						//this.flamethrowerTransform = UnityEngine.Object.Instantiate<GameObject>(this.flamethrowerEffectPrefab, transform).transform;
					}
					if (this.flamethrowerTransform)
					{
						this.flamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration = minimumAttackDuration;
					}
				}
				this.FireBleed(muzzleName);
			}
			if (this.hasBegunFlamethrower)
			{
                if (!characterBody.HasBuff(RoR2Content.Buffs.LunarSecondaryRoot))
				{
					characterBody.AddBuff(RoR2Content.Buffs.LunarSecondaryRoot);
				}
				this.flamethrowerStopwatch += Time.fixedDeltaTime;
				if (this.flamethrowerStopwatch > tickRate)
				{
					this.flamethrowerStopwatch -= tickRate;
					//this.FireGauntlet(muzzleName);
					this.FireBleed(muzzleName);
				}
				this.UpdateFlamethrowerEffect();
			}

			if (this.isExitingFlamethrower)
			{

				if (this.stopwatch >= this.exitDuration && base.isAuthority)
				{
					this.outer.SetNextStateToMain();
					return;
				}
			}
			if (!IsKeyDownAuthority() && this.hasBegunFlamethrower && !isExitingFlamethrower
				&& this.stopwatch >= (entryDuration + minimumAttackDuration))
			{
				this.isExitingFlamethrower = true;
				this.stopwatch = 0;

				if (this.flamethrowerTransform)
				{
					this.flamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration += exitDuration;
				}
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Pain;
		}
		private void FireBleed(string muzzleString)
		{
			if (characterBody)
			{
				characterBody.SetAimTimer(1f);
			}

			Ray aimRay = base.GetAimRay();

			if (NetworkServer.active)
			{
				BullseyeSearch bullseyeSearch = new BullseyeSearch();
				bullseyeSearch.teamMaskFilter = TeamMask.all;
				bullseyeSearch.maxAngleFilter = this.fieldOfView;
				bullseyeSearch.maxDistanceFilter = maxDistance;
				bullseyeSearch.searchOrigin = aimRay.origin;
				bullseyeSearch.searchDirection = aimRay.direction;
				bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
				bullseyeSearch.filterByLoS = true;
				bullseyeSearch.RefreshCandidates();
				bullseyeSearch.FilterOutGameObject(base.gameObject);
				IEnumerable<HurtBox> enumerable = bullseyeSearch.GetResults()
					.Where(new Func<HurtBox, bool>(Util.IsValid)).Distinct(default(HurtBox.EntityEqualityComparer));

				TeamIndex team = base.GetTeam();

				bool willBleed = Util.CheckRoll(bleedPercentChance, base.characterBody.master);
				foreach (HurtBox hurtBox in enumerable)
				{
					HealthComponent hc = hurtBox.healthComponent;
					if (FriendlyFireManager.ShouldSplashHitProceed(hc, team))
					{
						if (willBleed)
						{
							CharacterBody body = hc.body;

							this.AddDebuff(body);
							body.RecalculateStats();
						}

						DamageInfo damageInfo = new DamageInfo
						{
							attacker = base.gameObject,
							damage = damageStat * damageCoefficientPerTick,
							position = hc.transform.position,
							procCoefficient = procCoefficientPerTick
						};

						hc.TakeDamage(damageInfo);
						GlobalEventManager.instance.OnHitEnemy(damageInfo, hc.gameObject);
					}
				}
                if (willBleed)
				{
					AddDebuff(base.characterBody);
				}
			}
		}
		private void AddDebuff(CharacterBody body)
		{
			bool isSelf = body == base.characterBody;

			InflictDotInfo inflictDotInfo = default(InflictDotInfo);
			inflictDotInfo.dotIndex = DotController.DotIndex.Bleed;
			inflictDotInfo.attackerObject = gameObject;
			inflictDotInfo.damageMultiplier = (isSelf) ? selfBleedDamageMultiplier : 1;
			inflictDotInfo.victimObject = body.gameObject;
			inflictDotInfo.duration = bleedDuration;
			DotController.InflictDot(ref inflictDotInfo);

			EffectManager.SpawnEffect(detonationEffectPrefab, new EffectData
			{
				origin = body.corePosition,
				rotation = Quaternion.identity,
				scale = Mathf.Log(body.GetBuffCount(RoR2Content.Buffs.Bleeding), 5) * body.radius
			}, true);
		}

		private void UpdateFlamethrowerEffect()
		{
			Ray aimRay = base.GetAimRay();
			Vector3 direction = aimRay.direction;
			if (this.flamethrowerTransform)
			{
				this.flamethrowerTransform.forward = direction;
				if (!isExitingFlamethrower)
					this.flamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration += Time.fixedDeltaTime;
			}
		}
	}
}
