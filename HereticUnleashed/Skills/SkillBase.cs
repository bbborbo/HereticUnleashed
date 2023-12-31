﻿using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.Skills
{
    public abstract class SkillBase<T> : SkillBase where T : SkillBase<T>
    {
        public static T instance { get; private set; }

        public SkillBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ArtificerExtended SkillBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class SkillBase
    {
        public static Dictionary<string, SkillLocator> characterSkillLocators = new Dictionary<string, SkillLocator>();
        public static string Token = HereticPlugin.TokenName + "SKILL";
        public static string FullDescToken = "_FULLDESCRIPTION";
        public static string ShortDescToken = "_SHORTDESCRIPTION";
        public abstract string SkillName { get; }
        public abstract string SkillDescription { get; }
        public abstract string SkillFullDescription { get; }
        public abstract string SkillLangTokenName { get; }
        public abstract UnlockableDef UnlockDef { get; }
        public abstract string IconName { get; }
        public abstract Type ActivationState { get; }
        public abstract Type BaseSkillDef { get; }
        public abstract string CharacterName { get; }
        public abstract SkillFamilyName SkillSlot { get; }
        public abstract SimpleSkillData SkillData { get; }
        public string[] KeywordTokens;

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            LanguageAPI.Add(Token + SkillLangTokenName, SkillName);
            LanguageAPI.Add(Token + SkillLangTokenName + "_DESCRIPTION", SkillDescription);

            string shortDesc = $"<style=cIsUtility>Replace your {SkillSlot} Skill</style> with <style=cIsUtility>{SkillName}</style>. ";
            LanguageAPI.Add(Token + SkillLangTokenName + ShortDescToken, shortDesc);
            LanguageAPI.Add(Token + SkillLangTokenName + FullDescToken, shortDesc + "\n\n" + SkillFullDescription);
            Debug.Log(shortDesc);
            Debug.Log(shortDesc + SkillFullDescription);
        }

        protected void CreateSkill()
        {
            SkillLocator skillLocator;
            string name = CharacterName;
            if (characterSkillLocators.ContainsKey(name))
            {
                skillLocator = characterSkillLocators[name];
            }
            else
            {
                GameObject body = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/" + name);
                skillLocator = body?.GetComponent<SkillLocator>();

                if (skillLocator)
                {
                    characterSkillLocators.Add(name, skillLocator);
                }
            }

            if(skillLocator != null)
            {
                SkillFamily skillFamily = null;

                //get skill family from skill slot
                switch (SkillSlot)
                {
                    case SkillFamilyName.Primary:
                        skillFamily = skillLocator.primary.skillFamily;
                        break;
                    case SkillFamilyName.Secondary:
                        skillFamily = skillLocator.secondary.skillFamily;
                        break;
                    case SkillFamilyName.Utility:
                        skillFamily = skillLocator.utility.skillFamily;
                        break;
                    case SkillFamilyName.Special:
                        skillFamily = skillLocator.special.skillFamily;
                        break;
                    case SkillFamilyName.Misc:
                        Debug.Log("Special case!");
                        break;
                }

                if (skillFamily != null)
                {
                    string s = $"HereticUnchained: {SkillName} initializing!";// to unlock {UnlockDef.cachedName}!";
                    //Debug.Log(s);

                    var skillDef = (SkillDef)ScriptableObject.CreateInstance(BaseSkillDef);

                    RegisterEntityState(ActivationState);
                    skillDef.activationState = new SerializableEntityStateType(ActivationState);

                    skillDef.skillNameToken = Token + SkillLangTokenName;
                    skillDef.skillName = SkillName;
                    skillDef.skillDescriptionToken = Token + SkillLangTokenName + "_DESCRIPTION";
                    skillDef.activationStateMachineName = "Weapon";

                    skillDef.keywordTokens = KeywordTokens;
                    if(HereticPlugin.iconBundle != null)
                        skillDef.icon = HereticPlugin.iconBundle.LoadAsset<Sprite>(HereticPlugin.iconsPath + IconName + ".png");

                    #region SkillData
                    skillDef.baseMaxStock = SkillData.baseMaxStock;
                    skillDef.baseRechargeInterval = SkillData.baseRechargeInterval;
                    skillDef.beginSkillCooldownOnSkillEnd = SkillData.beginSkillCooldownOnSkillEnd;
                    skillDef.canceledFromSprinting = HereticPlugin.isAutosprintLoaded ? false : SkillData.canceledFromSprinting;
                    skillDef.cancelSprintingOnActivation = SkillData.cancelSprintingOnActivation;
                    skillDef.dontAllowPastMaxStocks = SkillData.dontAllowPastMaxStocks;
                    skillDef.fullRestockOnAssign = SkillData.fullRestockOnAssign;
                    skillDef.interruptPriority = SkillData.interruptPriority;
                    skillDef.isCombatSkill = SkillData.isCombatSkill;
                    skillDef.mustKeyPress = SkillData.mustKeyPress;
                    skillDef.rechargeStock = SkillData.rechargeStock;
                    skillDef.requiredStock = SkillData.requiredStock;
                    skillDef.resetCooldownTimerOnUse = SkillData.resetCooldownTimerOnUse;
                    skillDef.stockToConsume = SkillData.stockToConsume;
                    #endregion


                    CoreModules.Assets.RegisterSkillDef(skillDef);
                    Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
                    skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
                    {
                        skillDef = skillDef,
                        unlockableDef = UnlockDef,
                        viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
                    };
                }
                else
                {
                    Debug.Log($"No skill family {SkillSlot.ToString()} found from " + CharacterName);
                }
            }
            else
            {
                Debug.Log("No skill locator found from " + CharacterName);
            }
        }

        public abstract void Hooks();

        internal UnlockableDef GetUnlockDef(Type type)
        {
            UnlockableDef u = null;

            /*foreach (KeyValuePair<UnlockBase, UnlockableDef> keyValuePair in Main.UnlockBaseDictionary)
            {
                string key = keyValuePair.Key.ToString();
                UnlockableDef value = keyValuePair.Value;
                if (key == type.ToString())
                {
                    u = value;
                    //Debug.Log($"Found an Unlock ID Match {value} for {type.Name}! ");
                    break;
                }
            }*/

            return u;
        }
        public static bool RegisterEntityState(Type entityState)
        {
            //Check if the entity state has already been registered, is abstract, or is not a subclass of the base EntityState
            if (/*CoreModules.ContentPacks.entityStates.Contains(entityState) ||*/ !entityState.IsSubclassOf(typeof(EntityStates.EntityState)) || entityState.IsAbstract)
            {
                //LogCore.LogE(entityState.AssemblyQualifiedName + " is either abstract, not a subclass of an entity state, or has already been registered.");
                //LogCore.LogI("Is Abstract: " + entityState.IsAbstract + " Is not Subclass: " + !entityState.IsSubclassOf(typeof(EntityState)) + " Is already added: " + EntityStateDefinitions.Contains(entityState));
                return false;
            }
            //If not, add it to our EntityStateDefinitions
            CoreModules.Assets.RegisterEntityState(entityState);
            return true;
        }
    }
}
