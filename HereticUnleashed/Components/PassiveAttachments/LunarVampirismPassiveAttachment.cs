using HereticUnchained.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HereticUnchained.Components
{
    class LunarVampirismPassiveAttachment : BasePassiveAttachment
    {
        public override void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
        {
            if (NetworkServer.active)
            {
                this.damageListener = attachedBody.gameObject.AddComponent<LunarVampirismPassiveAttachment.DamageListener>();
                this.damageListener.passiveController = this;
            }
        }

        public override void OnDestroy()
        {
            if (this.damageListener)
            {
                UnityEngine.Object.Destroy(this.damageListener);
            }
            this.damageListener = null;
        }

        static LunarVampirismPassiveAttachment()
		{
			NetworkBehaviour.RegisterCommandDelegate(typeof(LunarVampirismPassiveAttachment),
                LunarVampirismPassiveAttachment.kCmdCmdSetSkillAvailable,
				new NetworkBehaviour.CmdDelegate(LunarVampirismPassiveAttachment.InvokeCmdCmdSetSkillAvailable));
			NetworkCRC.RegisterBehaviour("LunarVampirismPassiveAttachment", 0);
		}

        private LunarVampirismPassiveAttachment.DamageListener damageListener;
        private class DamageListener : MonoBehaviour, IOnDamageDealtServerReceiver
        {
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                if (this.passiveController.skillAvailable && damageInfo.procCoefficient > 0)
                {
                    HealthComponent healthComponent = damageReport.attackerBody.healthComponent;

                    float healBase = damageInfo.damage * BloodUtilitySkill.vampirismHealFraction;
                    float healMax = healthComponent.fullHealth * BloodUtilitySkill.vampirismMaxHealPortion;
                    float healAmt = Mathf.Clamp(healBase, 1, healMax) * damageInfo.procCoefficient;

                    if(damageReport.attackerBodyIndex == HereticPlugin.hereticBodyIndex)
                    {
                        healAmt *= 2;
                    }

                    healthComponent.Heal(healAmt, damageInfo.procChainMask);
                }

            }

            public LunarVampirismPassiveAttachment passiveController;
        }
    }
}
