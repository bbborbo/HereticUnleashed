using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace HereticUnchained.Skills
{
    class LunarLesionsSkillDef : SkillDef
    {
		public static float cooldownPerStack = 5f;

		#region VERY IMPORTANT REQUIRED NECESSARY
		public override SkillDef.BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
		{
			return new InstanceData
			{
				skillSlot = skillSlot
			};
		}
		public override void OnUnassigned(GenericSkill skillSlot)
		{
			((InstanceData)skillSlot.skillInstanceData).skillSlot = null;
		}
		#endregion

		public override float GetRechargeInterval(GenericSkill skillSlot)
		{
			return this.baseRechargeInterval + 
				cooldownPerStack * (skillSlot.characterBody.inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement) - 1);
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
					this._skillSlot = value;
					if (this._skillSlot != null)
					{
						this._skillSlot.characterBody.onInventoryChanged += this.OnInventoryChanged;
					}
				}
			}

			public void OnInventoryChanged()
			{
				this.skillSlot.RecalculateValues();
			}

			private GenericSkill _skillSlot;
		}
	}
}
