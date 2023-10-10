using HereticUnchained.Components;
using HereticUnchained.CoreModules;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HereticUnchained.Skills
{
    class LunarVampirismSkillDef : SkillDef
	{
		public static float cooldownPerStack = 3f;

		#region VERY IMPORTANT REQUIRED NECESSARY
		public override SkillDef.BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
		{
			return new LunarVampirismSkillDef.InstanceData
			{
				skillSlot = skillSlot
			};
		}
		public override void OnUnassigned(GenericSkill skillSlot)
		{
			((LunarVampirismSkillDef.InstanceData)skillSlot.skillInstanceData).skillSlot = null;
		}
        #endregion

        public override float GetRechargeInterval(GenericSkill skillSlot)
		{
			return this.baseRechargeInterval +
				cooldownPerStack * (skillSlot.characterBody.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement) - 1);
		}

		class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public GenericSkill skillSlot
			{
				get
				{
					return this._skillSlot;
				}
				set
				{
					if (this._skillSlot == value)
					{
						return;
					}
					if (this._skillSlot != null)
					{
						this._skillSlot.characterBody.onInventoryChanged -= this.OnInventoryChanged;
					}
					if (this.passiveAttachment)
					{
						UnityEngine.Object.Destroy(this.passiveAttachment.gameObject);
					}
					this.passiveAttachment = null;
					this._skillSlot = value;
					if (this._skillSlot != null)
					{
						this._skillSlot.characterBody.onInventoryChanged += this.OnInventoryChanged;
						if (NetworkServer.active && this._skillSlot.characterBody)
						{
							GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Assets.lunarVampirismAttachmentPrefab);
							this.passiveAttachment = gameObject.GetComponent<LunarVampirismPassiveAttachment>();
							this.passiveAttachment.monitoredSkill = this.skillSlot;
							gameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(this._skillSlot.characterBody.gameObject);
						}
					}
				}
			}

			public void OnInventoryChanged()
			{
				this.skillSlot.RecalculateValues();
			}

			private GenericSkill _skillSlot;

			private LunarVampirismPassiveAttachment passiveAttachment;
		}
	}
}
