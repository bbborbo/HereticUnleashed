using HereticUnchained.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.CoreModules
{
    class Assets : CoreModule
    {
        #region content pack
        public static void RegisterBodyPrefab(GameObject bodyPrefab)
        {
            ContentPacks.bodyPrefabs.Add(bodyPrefab);
        }
        public static void RegisterSkillFamily(SkillFamily skilLFamily)
        {
            ContentPacks.skillFamilies.Add(skilLFamily);
        }
        public static void RegisterSkillDef(SkillDef skillDef)
        {
            ContentPacks.skillDefs.Add(skillDef);
        }
        public static void RegisterEntityState(Type entityState)
        {
            ContentPacks.entityStates.Add(entityState);
        }
        public static void RegisterSurvivorDef(SurvivorDef survivorDefs)
        {
            ContentPacks.survivorDefs.Add(survivorDefs);
        }
        public static void RegisterNetworkedObjectPrefab(GameObject prefab)
        {
            ContentPacks.networkedObjectPrefabs.Add(prefab);
        }
        public static EffectDef CreateEffect(GameObject effect)
        {
            if (effect == null)
            {
                Debug.LogError("Effect prefab was null");
                return null;
            }

            var effectComp = effect.GetComponent<EffectComponent>();
            if (effectComp == null)
            {
                Debug.LogErrorFormat("Effect prefab: \"{0}\" does not have an EffectComponent.", effect.name);
                return null;
            }

            var vfxAttrib = effect.GetComponent<VFXAttributes>();
            if (vfxAttrib == null)
            {
                Debug.LogErrorFormat("Effect prefab: \"{0}\" does not have a VFXAttributes component.", effect.name);
                return null;
            }

            var def = new EffectDef
            {
                prefab = effect,
                prefabEffectComponent = effectComp,
                prefabVfxAttributes = vfxAttrib,
                prefabName = effect.name,
                spawnSoundEventName = effectComp.soundName
            };

            ContentPacks.effectDefs.Add(def);
            return def;
        }
        #endregion

        public override void Init()
        {
            GenerateVampirismAttachmentPrefab();

            IL.RoR2.HealthComponent.TakeDamage += AddExecutionThreshold;
            On.RoR2.HealthComponent.GetHealthBarValues += DisplayExecutionThreshold;
        }

        public static GameObject lunarVampirismAttachmentPrefab;
        private void GenerateVampirismAttachmentPrefab()
        {
            lunarVampirismAttachmentPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BodyAttachments/LunarDetonatorPassiveAttachment").InstantiateClone("LunarVampirismPassiveAttachment", true);
            LunarDetonatorPassiveAttachment detonator = lunarVampirismAttachmentPrefab.GetComponent<LunarDetonatorPassiveAttachment>();
            if (detonator)
            {
                UnityEngine.Object.Destroy(detonator);
            }

            lunarVampirismAttachmentPrefab.AddComponent<LunarVampirismPassiveAttachment>();

            RegisterNetworkedObjectPrefab(lunarVampirismAttachmentPrefab);
        }

        #region execution mechanics
        private void AddExecutionThreshold(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int thresholdPosition = 0;

            c.GotoNext(MoveType.After,
                x => x.MatchLdcR4(float.NegativeInfinity),
                x => x.MatchStloc(out thresholdPosition)
                );
            
            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<HealthComponent>("get_isInFrozenState")
                );

            c.Emit(OpCodes.Ldloc, thresholdPosition);
            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Func<float, HealthComponent, float>>((currentThreshold, hc) =>
            {
                float newThreshold = currentThreshold;

                newThreshold = GetExecutionThreshold(currentThreshold, hc);

                return newThreshold;
            });
            c.Emit(OpCodes.Stloc, thresholdPosition);
        }

        static float GetExecutionThreshold(float currentThreshold, HealthComponent healthComponent)
        {
            float newThreshold = currentThreshold;
            CharacterBody body = healthComponent.body;

            if (body != null)
            {
                if (!body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes))
                {
                    int executionBuffCount = 0;// body.GetBuffCount(executionDebuffIndex);
                    if (executionBuffCount > 0)
                    {
                        float threshold = 0;// newExecutionThresholdBase + newExecutionThresholdStack * executionBuffCount;
                        if(currentThreshold < threshold)
                        {
                            newThreshold = threshold;
                        }
                    }
                }
            }

            return newThreshold;
        }

        private HealthComponent.HealthBarValues DisplayExecutionThreshold(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
        {
            HealthComponent.HealthBarValues values = orig(self);

            values.cullFraction = Mathf.Clamp01(GetExecutionThreshold(values.cullFraction, self));

            return values;
        }
        #endregion
    }
}