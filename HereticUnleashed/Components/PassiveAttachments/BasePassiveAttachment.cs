using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HereticUnchained.Components
{
    [RequireComponent(typeof(NetworkedBodyAttachment))]
    internal abstract class BasePassiveAttachment : NetworkBehaviour, INetworkedBodyAttachmentListener
	{
		internal GenericSkill _monitoredSkill;
		[SyncVar(hook = "SetSkillSlotIndexPlusOne")]
		internal uint skillSlotIndexPlusOne;
		internal bool skillAvailable;

		internal NetworkedBodyAttachment networkedBodyAttachment;
		internal static int kCmdCmdSetSkillAvailable = -1453655134;

		#region abstract
		public abstract void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody);
		public abstract void OnDestroy();
        /*public void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
		{
			if (NetworkServer.active)
			{
				this.damageListener = attachedBody.gameObject.AddComponent<LunarDetonatorPassiveAttachment.DamageListener>();
				this.damageListener.passiveController = this;
			}
		}
		private void OnDestroy()
		{
			if (this.damageListener)
			{
				UnityEngine.Object.Destroy(this.damageListener);
			}
			this.damageListener = null;
		}*/
        #endregion

        #region passive ex
        /*static LunarDetonatorPassiveAttachment()
		{
			NetworkBehaviour.RegisterCommandDelegate(typeof(LunarDetonatorPassiveAttachment),
				LunarDetonatorPassiveAttachment.kCmdCmdSetSkillAvailable,
				new NetworkBehaviour.CmdDelegate(LunarDetonatorPassiveAttachment.InvokeCmdCmdSetSkillAvailable));
			NetworkCRC.RegisterBehaviour("LunarDetonatorPassiveAttachment", 0);
		}

        private LunarDetonatorPassiveAttachment.DamageListener damageListener;
        private class DamageListener : MonoBehaviour, IOnDamageDealtServerReceiver
		{
			public void OnDamageDealtServer(DamageReport damageReport)
			{
				if (this.passiveController.skillAvailable && damageReport.victim.alive && Util.CheckRoll(damageReport.damageInfo.procCoefficient * 100f, damageReport.attackerMaster))
				{
					damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.LunarDetonationCharge, 10f);
				}

			}

			public LunarDetonatorPassiveAttachment passiveController;
		}*/
        #endregion

        public GenericSkill monitoredSkill
		{
			get
			{
				return this._monitoredSkill;
			}
			set
			{
				if (this._monitoredSkill == value)
				{
					return;
				}
				this._monitoredSkill = value;
				int num = -1;
				if (this._monitoredSkill)
				{
					SkillLocator component = this._monitoredSkill.GetComponent<SkillLocator>();
					if (component)
					{
						num = component.GetSkillSlotIndex(this._monitoredSkill);
					}
				}
				this.SetSkillSlotIndexPlusOne((uint)(num + 1));
			}
		}

		private void Awake()
		{
			this.networkedBodyAttachment = base.GetComponent<NetworkedBodyAttachment>();
		}

		private void FixedUpdate()
		{
			if (this.networkedBodyAttachment.hasEffectiveAuthority)
			{
				this.FixedUpdateAuthority();
			}
		}

		public override void OnStartClient()
		{
			this.SetSkillSlotIndexPlusOne(this.skillSlotIndexPlusOne);
		}

		private void SetSkillSlotIndexPlusOne(uint newSkillSlotIndexPlusOne)
		{
			this.NetworkskillSlotIndexPlusOne = newSkillSlotIndexPlusOne;
			if (!NetworkServer.active)
			{
				this.ResolveMonitoredSkill();
			}
		}

		private void ResolveMonitoredSkill()
		{
			if (this.networkedBodyAttachment.attachedBody)
			{
				SkillLocator component = this.networkedBodyAttachment.attachedBody.GetComponent<SkillLocator>();
				if (component)
				{
					this.monitoredSkill = component.GetSkillAtIndex((int)(this.skillSlotIndexPlusOne - 1U));
				}
			}
		}

		private void FixedUpdateAuthority()
		{
			bool flag = false;
			if (this.monitoredSkill)
			{
				flag = (this.monitoredSkill.stock > 0);
			}
			if (this.skillAvailable != flag)
			{
				this.skillAvailable = flag;
				if (!NetworkServer.active)
				{
					this.CallCmdSetSkillAvailable(this.skillAvailable);
				}
			}
		}

		[Command]
		internal void CmdSetSkillAvailable(bool newSkillAvailable)
		{
			this.skillAvailable = newSkillAvailable;
		}

		private void UNetVersion()
		{
		}

		public uint NetworkskillSlotIndexPlusOne
		{
			get
			{
				return this.skillSlotIndexPlusOne;
			}
			[param: In]
			set
			{
				if (NetworkServer.localClientActive && !base.syncVarHookGuard)
				{
					base.syncVarHookGuard = true;
					this.SetSkillSlotIndexPlusOne(value);
					base.syncVarHookGuard = false;
				}
				base.SetSyncVar<uint>(value, ref this.skillSlotIndexPlusOne, 1U);
			}
		}

		protected static void InvokeCmdCmdSetSkillAvailable(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSetSkillAvailable called on client.");
				return;
			}
			((LunarDetonatorPassiveAttachment)obj).CmdSetSkillAvailable(reader.ReadBoolean());
		}

		public void CallCmdSetSkillAvailable(bool newSkillAvailable)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("Command function CmdSetSkillAvailable called on server.");
				return;
			}
			if (base.isServer)
			{
				this.CmdSetSkillAvailable(newSkillAvailable);
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write(0);
			networkWriter.Write((short)((ushort)5));
			networkWriter.WritePackedUInt32((uint)LunarDetonatorPassiveAttachment.kCmdCmdSetSkillAvailable);
			networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(newSkillAvailable);
			base.SendCommandInternal(networkWriter, 0, "CmdSetSkillAvailable");
		}


		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.WritePackedUInt32(this.skillSlotIndexPlusOne);
				return true;
			}
			bool flag = false;
			if ((base.syncVarDirtyBits & 1U) != 0U)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.WritePackedUInt32(this.skillSlotIndexPlusOne);
			}
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
			}
			return flag;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				this.skillSlotIndexPlusOne = reader.ReadPackedUInt32();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if ((num & 1) != 0)
			{
				this.SetSkillSlotIndexPlusOne(reader.ReadPackedUInt32());
			}
		}
	}
}