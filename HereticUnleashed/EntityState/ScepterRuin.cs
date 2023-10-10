using EntityStates;
using EntityStates.GlobalSkills.LunarDetonator;
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
    class ScepterRuin : BaseSkillState
    {
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = Detonate.baseDuration / this.attackSpeedStat;
			if (NetworkServer.active)
			{
				BullseyeSearch bullseyeSearch = new BullseyeSearch();
				bullseyeSearch.filterByDistinctEntity = true;
				bullseyeSearch.filterByLoS = false;
				bullseyeSearch.maxDistanceFilter = float.PositiveInfinity;
				bullseyeSearch.minDistanceFilter = 0f;
				bullseyeSearch.minAngleFilter = 0f;
				bullseyeSearch.maxAngleFilter = 180f;
				bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
				bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(base.GetTeam());
				bullseyeSearch.searchOrigin = base.characterBody.corePosition;
				bullseyeSearch.viewer = null;
				bullseyeSearch.RefreshCandidates();
				bullseyeSearch.FilterOutGameObject(base.gameObject);
				IEnumerable<HurtBox> results = bullseyeSearch.GetResults();
				this.detonationTargets = results.ToArray<HurtBox>();
				Detonate.DetonationController detonationController = new Detonate.DetonationController();
				detonationController.characterBody = base.characterBody;
				detonationController.interval = Detonate.detonationInterval;
				detonationController.detonationTargets = this.detonationTargets;
				detonationController.damageStat = this.damageStat;
				detonationController.isCrit = base.RollCrit();
				detonationController.active = true;
			}
			base.PlayAnimation(this.animationLayerName, this.animationStateName, this.playbackRateParam, this.duration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.fixedAge >= this.duration)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public static float baseDuration;

		public static float baseDamageCoefficient;

		public static float damageCoefficientPerStack;

		public static float procCoefficient;

		public static float detonationInterval;

		public static GameObject detonationEffectPrefab;

		public static GameObject orbEffectPrefab;

		[SerializeField]
		public string animationLayerName;

		[SerializeField]
		public string animationStateName;

		[SerializeField]
		public string playbackRateParam;

		private float duration;

		private HurtBox[] detonationTargets;

		private class DetonationController
		{
			public bool active
			{
				get
				{
					return this._active;
				}
				set
				{
					if (this._active == value)
					{
						return;
					}
					this._active = value;
					if (this._active)
					{
						RoR2Application.onFixedUpdate += this.FixedUpdate;
						return;
					}
					RoR2Application.onFixedUpdate -= this.FixedUpdate;
				}
			}

			private void FixedUpdate()
			{
				if (!this.characterBody || !this.characterBody.healthComponent || !this.characterBody.healthComponent.alive)
				{
					this.active = false;
					return;
				}
				this.timer -= Time.deltaTime;
				if (this.timer <= 0f)
				{
					this.timer = this.interval;
					while (this.i < this.detonationTargets.Length)
					{
						try
						{
							HurtBox targetHurtBox = null;
							Util.Swap<HurtBox>(ref targetHurtBox, ref this.detonationTargets[this.i]);
							if (this.DoDetonation(targetHurtBox))
							{
								break;
							}
						}
						catch (Exception message)
						{
							Debug.LogError(message);
						}
						this.i++;
					}
					if (this.i >= this.detonationTargets.Length)
					{
						this.active = false;
					}
				}
			}

			private bool DoDetonation(HurtBox targetHurtBox)
			{
				if (!targetHurtBox)
				{
					return false;
				}
				HealthComponent healthComponent = targetHurtBox.healthComponent;
				if (!healthComponent)
				{
					return false;
				}
				CharacterBody body = healthComponent.body;
				if (!body)
				{
					return false;
				}
				if (body.GetBuffCount(RoR2Content.Buffs.LunarDetonationCharge) <= 0)
				{
					return false;
				}
				LunarDetonatorOrb lunarDetonatorOrb = new LunarDetonatorOrb();
				lunarDetonatorOrb.origin = this.characterBody.corePosition;
				lunarDetonatorOrb.target = targetHurtBox;
				lunarDetonatorOrb.attacker = this.characterBody.gameObject;
				lunarDetonatorOrb.baseDamage = this.damageStat * Detonate.baseDamageCoefficient;
				lunarDetonatorOrb.damagePerStack = this.damageStat * Detonate.damageCoefficientPerStack;
				lunarDetonatorOrb.damageColorIndex = DamageColorIndex.Default;
				lunarDetonatorOrb.isCrit = this.isCrit;
				lunarDetonatorOrb.procChainMask = default(ProcChainMask);
				lunarDetonatorOrb.procCoefficient = 1f;
				lunarDetonatorOrb.detonationEffectPrefab = Detonate.detonationEffectPrefab;
				lunarDetonatorOrb.travelSpeed = 120f;
				lunarDetonatorOrb.orbEffectPrefab = Detonate.orbEffectPrefab;
				OrbManager.instance.AddOrb(lunarDetonatorOrb);
				return true;
			}

			public HurtBox[] detonationTargets;

			public CharacterBody characterBody;

			public float damageStat;

			public bool isCrit;

			public float interval;

			private int i;

			private float timer;

			private bool _active;
		}
	}
}
