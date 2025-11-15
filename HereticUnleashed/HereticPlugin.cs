using BepInEx;
using BepInEx.Configuration;
using HereticUnchained.CoreModules;
using HereticUnchained.Skills;
using HereticUnchained.Unlocks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using NegativeRegenFix;
using HereticUnchained.SurvivorTweaks;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
#pragma warning disable 

namespace HereticUnchained
{
    [BepInDependency(R2API.LoadoutAPI.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInPlugin(ModGUID, ModName, ModVer)]
    [BepInDependency("com.johnedwa.RTAutoSprintEx", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.HouseOfFruits.RiskierRain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(NegativeRegenFix.NegativeRegenFix.guid, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(LanguageAPI), 
        nameof(PrefabAPI), nameof(RecalculateStatsAPI), nameof(ContentAddition))]
    public class HereticPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.Borbo.HereticUnchained";
        public const string ModName = "HereticUnchained";
        public const string ModVer = "2.0.2";

        public static AssetBundle iconBundle = Tools.LoadAssetBundle(Properties.Resources.hereticunchained);
        public static string iconsPath = "Assets/HereticIcons/";
        public static string TokenName = "HERETICUNCHAINED_";

        public static bool isScepterLoaded = Tools.isLoaded("com.DestroyedClone.AncientScepter");
        public static bool isAutosprintLoaded = Tools.isLoaded("com.johnedwa.RTAutoSprintEx");
        public static bool is2R4RLoaded = Tools.isLoaded("com.HouseOfFruits.RiskierRain");

        internal static ConfigFile CustomConfigFile { get; set; }

        public static string hereticBodyName = "HeresyBody";
        internal static BodyIndex hereticBodyIndex;
        internal static SurvivorDef hereticSurvivorDef;
        internal static GameObject hereticDisplayPrefab;
        internal static GameObject hereticPrefab;
        internal static CharacterBody hereticBodyPrefab;

        public static SkillLocator skillLocator;
        public static SkillFamily passiveFamily; //idk if we will need this
        public static SkillFamily primaryFamily;
        public static SkillFamily secondaryFamily;
        public static SkillFamily utilityFamily;
        public static SkillFamily specialFamily;

        public static ItemDef lunarPrimary;
        public static string visionsShortDescriptionToken;
        public static string visionsLongDescriptionToken;
        public static ItemDef lunarSecondary;
        public static string hooksShortDescriptionToken;
        public static string hooksLongDescriptionToken;
        public static ItemDef lunarUtility;
        public static string stridesShortDescriptionToken;
        public static string stridesLongDescriptionToken;
        public static ItemDef lunarSpecial;
        public static string essenceShortDescriptionToken;
        public static string essenceLongDescriptionToken;

        public void Awake()
        {
            Debug.Log("Heretic Unchained is loading!");

            lunarPrimary = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/LunarSkillReplacements/LunarPrimaryReplacement.asset").WaitForCompletion();
            visionsShortDescriptionToken = lunarPrimary.pickupToken;
            visionsLongDescriptionToken = lunarPrimary.descriptionToken;
            lunarSecondary = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/LunarSkillReplacements/LunarSecondaryReplacement.asset").WaitForCompletion();
            hooksShortDescriptionToken = lunarSecondary.pickupToken;
            hooksLongDescriptionToken = lunarSecondary.descriptionToken;
            lunarUtility = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/LunarSkillReplacements/LunarUtilityReplacement.asset").WaitForCompletion();
            stridesShortDescriptionToken = lunarUtility.pickupToken;
            stridesLongDescriptionToken = lunarUtility.descriptionToken;
            lunarSpecial = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/LunarSkillReplacements/LunarSpecialReplacement.asset").WaitForCompletion();
            essenceShortDescriptionToken = lunarSpecial.pickupToken;
            essenceLongDescriptionToken = lunarSpecial.descriptionToken;

            InitializeConfig();
            InitializeUnlocks();

            InitializePrefab();
            InitializeSkins();
            CreateSkillFamilies(hereticPrefab);

            hereticSurvivorDef = Survivors.RegisterNewSurvivor(new Survivors.SurvivorDefInfo
            {
                bodyPrefab = hereticPrefab,
                displayPrefab = hereticDisplayPrefab,
                primaryColor = Color.grey,
                displayNameToken = "HERETIC_BODY_NAME",
                descriptionToken = "HERETIC_DESCRIPTION",
                outroFlavorToken = "HERETIC_OUTRO_FLAVOR",
                mainEndingEscapeFailureFlavorToken = "HERETIC_MAIN_ENDING_ESCAPE_FAILURE_FLAVOR",
                desiredSortPosition = 30f,
                unlockableDef = null//HereticPlugin.GetUnlockDef(typeof(HereticGlobalUnlock))
            });

            #region lang fixes
            LanguageAPI.Add("HERETIC_DESCRIPTION",
                "The Heretic is a powerful character who may be accessed through extraordinary means." +
                "" +
                "<style=cSub>\r\n\r\n< ! > Remember that The Heretic\u2019s skills can be used on other survivors at any time! " +
                "It might be worth considering skill combinations that work best on your favorite characters instead of The Heretic themself." +
                "" +
                "\r\n\r\n< ! > Health decay can kill The Heretic if you aren't careful! " +
                "Good Shadowfade timing is essential for survival, but it will also halt your damage output." +
                "" +
                "\r\n\r\n< ! > While their skills are powerful, The Heretic\u2019s strength is offset by long waits to recharge. " +
                "Cooldown reduction effects can be immensely valuable!" +
                "" +
                "\r\n\r\n< ! > Detonating Ruin at the right moment can devastate crowds and bosses. " +
                "Hungering Gaze can be your most effective tool for applying Ruin, " +
                "but may also easily be wasted on overkill against fodder enemies. Use it carefully!" +
                "\r\n</style>\r\n");

            LanguageAPI.Add("HERETIC_PASSIVE_NAME", "Dissident Will");
            LanguageAPI.Add("HERETIC_PASSIVE_DESC", "The Heretic can jump three times... " +
                "<style=cIsHealth>BUT their health decays rapidly over time.</style>");

            LanguageAPI.Add("SKILL_LUNAR_PRIMARY_REPLACEMENT_DESCRIPTION",
                $"Fire a flurry of <style=cIsUtility>tracking shards</style> " +
                $"that detonate after a delay, " +
                $"dealing <style=cIsDamage>120% damage</style>.");

            LanguageAPI.Add("SKILL_LUNAR_SPECIAL_REPLACEMENT_NAME", "Ruinous Wake");
            LanguageAPI.Add("SKILL_LUNAR_SPECIAL_REPLACEMENT_DESCRIPTION",
                $"<style=cIsUtility>Ruinous.</style> " +
                $"Activating this skill <style=cIsDamage>detonates</style> all Ruin at an unlimited range, " +
                $"dealing <style=cIsDamage>300% damage</style> " +
                $"plus <style=cIsDamage>120% per stack of Ruin</style>.");

            LanguageAPI.Add("KEYWORD_RUINOUS",
                $"<style=cKeywordName>Ruinous</style>" +
                $"<style=cSub>PASSIVE: While this skill is ready to use, " +
                $"<style=cIsDamage>your attacks</style> can apply " +
                $"a stack of <style=cIsDamage>Ruin</style> for 10 seconds. " +
                $"<i>Applying Ruin resets the duration of all stacks on the same target.</i></style>");
            #endregion

            CoreModules.HereticSkills.YoinkSkillDefs();
            InitializeCoreModules();
            InitializeSkills();
            InitializeScepterSkills();

            //new ContentPacks().Initialize();
        }

        public void Start()
        {
        }

        private void InitializeConfig()
        {
            CustomConfigFile = new ConfigFile(Paths.ConfigPath + "\\HereticUnchained.cfg", true);
        }

        void InitializeCoreModules()
        {
            var CoreModuleTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(CoreModule)));

            foreach (var coreModuleType in CoreModuleTypes)
            {
                CoreModule coreModule = (CoreModule)Activator.CreateInstance(coreModuleType);

                coreModule.Init();

                Debug.Log("Core Module: " + coreModule + " Initialized!");
            }
        }

        #region unlocks
        public static Dictionary<UnlockBase, UnlockableDef> UnlockBaseDictionary = new Dictionary<UnlockBase, UnlockableDef>();
        private void InitializeUnlocks()
        {
            var UnlockTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(UnlockBase)));

            foreach (Type unlockType in UnlockTypes)
            {
                UnlockBase unlock = (UnlockBase)System.Activator.CreateInstance(unlockType);

                if (!unlock.HideUnlock)
                {
                    unlock.Init(CustomConfigFile);

                    var baseMethod = typeof(Unlocks.UnlockableAPI).GetMethod("AddUnlockable");
                    UnlockableDef unlockableDef = (UnlockableDef)baseMethod.MakeGenericMethod(new Type[] { unlockType }).Invoke(null, new object[] { true });

                    bool forceUnlock = unlock.ForceDisable;

                    if (!forceUnlock)
                    {
                        forceUnlock = CustomConfigFile.Bind<bool>("Config: UNLOCKS", "Force Unlock Achievement: " + unlock.UnlockName,
                        false, $"Force this achievement to unlock: {unlock.UnlockName}?").Value;
                    }

                    if (!forceUnlock)
                        UnlockBaseDictionary.Add(unlock, unlockableDef);
                    else
                        UnlockBaseDictionary.Add(unlock, ScriptableObject.CreateInstance<UnlockableDef>());
                }
            }
        }
        public static UnlockableDef GetUnlockDef(Type type)
        {
            UnlockableDef u = null;

            foreach (KeyValuePair<UnlockBase, UnlockableDef> keyValuePair in HereticPlugin.UnlockBaseDictionary)
            {
                string key = keyValuePair.Key.ToString();
                UnlockableDef value = keyValuePair.Value;
                if (key == type.ToString())
                {
                    u = value;
                    //Debug.Log($"Found an Unlock ID Match {value} for {type.Name}! ");
                    break;
                }
            }

            return u;
        }
        #endregion

        #region skills
        public static List<SkillBase> Skills = new List<SkillBase>();
        public static Dictionary<uint, SkillDef> ScepterSkills = new Dictionary<uint, SkillDef>();
        public static Dictionary<SkillBase, bool> SkillStatusDictionary = new Dictionary<SkillBase, bool>();

        private void InitializeSkills()
        {
            var SkillTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SkillBase)));

            foreach (var skillType in SkillTypes)
            {
                SkillBase skill = (SkillBase)System.Activator.CreateInstance(skillType);

                if (ValidateSkill(skill))
                {
                    skill.Init(CustomConfigFile);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void InitializeScepterSkills()
        {
            var SkillTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ScepterSkillBase)));

            foreach (var skillType in SkillTypes)
            {
                ScepterSkillBase skill = (ScepterSkillBase)System.Activator.CreateInstance(skillType);

                if (ValidateScepterSkill(skill))
                {
                    skill.Init(CustomConfigFile);
                }
            }
        }

        bool ValidateSkill(SkillBase item)
        {
            var forceUnlock = true;

            if (forceUnlock)
            {
                Skills.Add(item);
            }
            SkillStatusDictionary.Add(item, forceUnlock);

            return forceUnlock;
        }

        bool ValidateScepterSkill(ScepterSkillBase item)
        {
            var forceUnlock = isScepterLoaded;

            return forceUnlock;
        }
        #endregion

        #region The heretic, unleashed!

        void FixSetStateOnHurt()
        {
            SetStateOnHurt ssoh = hereticPrefab.GetComponent<SetStateOnHurt>(); //Might be GetComponent in this case
            if (!ssoh)
            {
                ssoh = hereticPrefab.AddComponent<SetStateOnHurt>(); //Might be GetComponent in this case
            }

            ssoh.canBeStunned = false;
            ssoh.canBeHitStunned = false;
            ssoh.canBeFrozen = true;
            ssoh.hitThreshold = 5;

            //Ice Fix Credits: SushiDev
            foreach (EntityStateMachine esm in hereticPrefab.GetComponentsInChildren<EntityStateMachine>())
            {
                if (esm.customName == "Body")
                {
                    ssoh.targetStateMachine = esm;
                    break;
                }
            }
        }

        private static void AddHeresyItems(On.RoR2.CharacterMaster.orig_Start orig, CharacterMaster self)
        {
            if (true && self.teamIndex == TeamIndex.Player)
            {
                bool flagA = !NetworkServer.active || !Run.instance || (Run.instance.stageClearCount != 0 &&
                !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.randomSurvivorOnRespawnArtifactDef));
                bool flagB = self.bodyPrefab == hereticPrefab;

                if (flagB)
                {
                    if (!flagA)
                    {
                        Inventory inventory = self.inventory;
                        if (inventory)
                        {
                            Debug.Log("Adding Heretic items!");

                            ItemDef[] requiredItems = new ItemDef[]
                            {
                            RoR2Content.Items.LunarPrimaryReplacement,
                            RoR2Content.Items.LunarSecondaryReplacement,
                            RoR2Content.Items.LunarUtilityReplacement,
                            RoR2Content.Items.LunarSpecialReplacement
                            };

                            foreach (ItemDef item in requiredItems)
                            {
                                if (inventory.GetItemCount(item) < 1)
                                {
                                    inventory.GiveItem(item);
                                }
                            }
                        }
                    }
                    self.bodyPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/HereticBody");
                }
            }

            orig(self);
        }

        private static void SkillReplacementIntercept(On.RoR2.CharacterBody.orig_ReplaceSkillIfItemPresent orig, 
            CharacterBody self, GenericSkill skill, ItemIndex itemIndex, SkillDef skillDef)
        {
            if (self.teamComponent != null)
            {
                if (self.teamComponent.teamIndex == TeamIndex.Player)
                {
                    Loadout currentPlayerLoadout = self.master.loadout;

                    if (itemIndex == RoR2Content.Items.LunarPrimaryReplacement.itemIndex)
                    {
                        skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Primary, skillDef);
                    }
                    if (itemIndex == RoR2Content.Items.LunarSecondaryReplacement.itemIndex)
                    {
                        skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Secondary, skillDef);
                    }
                    if (itemIndex == RoR2Content.Items.LunarUtilityReplacement.itemIndex)
                    {
                        skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Utility, skillDef);
                    }
                    if (itemIndex == RoR2Content.Items.LunarSpecialReplacement.itemIndex)
                    {
                        bool hasScepter = false;
                        bool foundScepterSkill = false;
                        if (isScepterLoaded)
                        {
                            hasScepter = HasAncientScepter(self);
                        }

                        if (hasScepter)
                        {
                            uint preference = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Special);
                            SkillDef scepterSkill = ScepterSkills[preference];
                            if(scepterSkill != null)
                            {
                                skillDef = scepterSkill;
                                foundScepterSkill = true;
                            }
                        }
                        if (foundScepterSkill == false)
                        {
                            skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Special, skillDef);
                        }
                    }
                }
            }

            orig(self, skill, itemIndex, skillDef);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool HasAncientScepter(CharacterBody body)
        {
            if(body.inventory?.GetItemCount(AncientScepter.AncientScepterItem.instance.ItemDef) > 0)
            {
                return true;
            }
            return false;
        }

        private static void PickupTokenIntercept(On.RoR2.UI.ItemIcon.orig_SetItemIndex orig, RoR2.UI.ItemIcon self, ItemIndex newItemIndex, int newItemCount)
        {
            CharacterMaster master = CharacterMaster.instancesList[0];
            Loadout currentPlayerLoadout = master.loadout;

            if (newItemIndex == RoR2Content.Items.LunarPrimaryReplacement.itemIndex)
            {
                SkillDef skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Primary, null);
            }
            /*if (newItemIndex == RoR2Content.Items.LunarSecondaryReplacement.itemIndex)
            {
                skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Secondary, skillDef);
            }
            if (newItemIndex == RoR2Content.Items.LunarUtilityReplacement.itemIndex)
            {
                skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Utility, skillDef);
            }
            if (newItemIndex == RoR2Content.Items.LunarSpecialReplacement.itemIndex)
            {
                skillDef = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Special, skillDef);
            }*/
            orig(self, newItemIndex, newItemCount);
        }

        private static void InterceptPickupToken(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            if (!(!NetworkServer.active || (ulong)itemIndex >= (ulong)((long)self.itemStacks.Length) || count <= 0))
            {
                CharacterMaster master = CharacterMaster.instancesList[0];
                Loadout currentPlayerLoadout = master.loadout;
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);

                if (itemDef == RoR2Content.Items.LunarPrimaryReplacement)
                {
                    uint preference = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Primary);
                    switch (preference)
                    {
                        case 0:
                            RoR2Content.Items.LunarPrimaryReplacement.pickupToken = visionsShortDescriptionToken;
                            RoR2Content.Items.LunarPrimaryReplacement.descriptionToken = visionsLongDescriptionToken;
                            break;
                        case 1:
                            string bloodName = new BloodPrimarySkill().SkillLangTokenName;
                            RoR2Content.Items.LunarPrimaryReplacement.pickupToken = SkillBase.Token + bloodName + SkillBase.ShortDescToken;
                            RoR2Content.Items.LunarPrimaryReplacement.descriptionToken = SkillBase.Token + bloodName + SkillBase.FullDescToken;
                            break;
                        case 2:
                            string massName = new MassPrimarySkill().SkillLangTokenName;
                            RoR2Content.Items.LunarPrimaryReplacement.pickupToken = SkillBase.Token + massName + SkillBase.ShortDescToken;
                            RoR2Content.Items.LunarPrimaryReplacement.descriptionToken = SkillBase.Token + massName + SkillBase.FullDescToken;
                            break;
                    }
                }
                if (itemDef == RoR2Content.Items.LunarSecondaryReplacement)
                {
                    uint preference = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Primary);
                    switch (preference)
                    {
                        case 0:
                            RoR2Content.Items.LunarSecondaryReplacement.pickupToken = visionsShortDescriptionToken;
                            RoR2Content.Items.LunarSecondaryReplacement.descriptionToken = visionsLongDescriptionToken;
                            break;
                        case 1:
                            string s = new BloodSecondarySkill().SkillLangTokenName;
                            RoR2Content.Items.LunarSecondaryReplacement.pickupToken = SkillBase.Token + s + SkillBase.ShortDescToken;
                            RoR2Content.Items.LunarSecondaryReplacement.descriptionToken = SkillBase.Token + s + SkillBase.FullDescToken;
                            break;
                    }
                }
                if (itemDef == RoR2Content.Items.LunarUtilityReplacement)
                {
                    uint preference = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Primary);
                    switch (preference)
                    {
                        case 0:
                            RoR2Content.Items.LunarUtilityReplacement.pickupToken = stridesShortDescriptionToken;
                            RoR2Content.Items.LunarUtilityReplacement.descriptionToken = stridesLongDescriptionToken;
                            break;
                        case 1:
                            string s = new BloodUtilitySkill().SkillLangTokenName;
                            RoR2Content.Items.LunarUtilityReplacement.pickupToken = SkillBase.Token + s + SkillBase.ShortDescToken;
                            RoR2Content.Items.LunarUtilityReplacement.descriptionToken = SkillBase.Token + s + SkillBase.FullDescToken;
                            break;
                    }
                }
                if (itemDef == RoR2Content.Items.LunarSpecialReplacement)
                {
                    uint preference = GetSkillPreferenceFromLoadout(currentPlayerLoadout, (int)SkillSlot.Primary);
                    switch (preference)
                    {
                        case 0:
                            RoR2Content.Items.LunarSpecialReplacement.pickupToken = essenceShortDescriptionToken;
                            RoR2Content.Items.LunarSpecialReplacement.descriptionToken = essenceLongDescriptionToken;
                            break;
                        case 1:
                            string s = new BloodSpecialSkill().SkillLangTokenName;
                            RoR2Content.Items.LunarSpecialReplacement.pickupToken = SkillBase.Token + s + SkillBase.ShortDescToken;
                            RoR2Content.Items.LunarSpecialReplacement.descriptionToken = SkillBase.Token + s + SkillBase.FullDescToken;
                            break;
                    }
                }
            }

            orig(self, itemIndex, count);
        }

        private static SkillDef GetSkillPreferenceFromLoadout(Loadout loadout, int skillSlot, SkillDef defaultSkill)
        {
            SkillDef skillDef = defaultSkill;

            uint preference = GetSkillPreferenceFromLoadout(loadout, skillSlot);
            if (preference > 0)
            {
                Loadout.BodyLoadoutManager.BodyLoadout bodyLoadout = loadout.bodyLoadoutManager.GetReadOnlyBodyLoadout(hereticBodyIndex);
                skillDef = bodyLoadout.GetSkillFamily(skillSlot).variants[preference].skillDef;
            }

            return skillDef;
        }

        private static uint GetSkillPreferenceFromLoadout(Loadout loadout, int skillSlot)
        {
            if (loadout == null || loadout.bodyLoadoutManager == null)
            {
                Debug.Log("WTF!");
                return 0;
            }

            if (hereticBodyIndex != BodyIndex.None)
            {
                hereticBodyIndex = BodyCatalog.FindBodyIndex(hereticBodyPrefab);
            }

            uint preference = loadout.bodyLoadoutManager.GetSkillVariant(hereticBodyIndex, skillSlot);
            return preference;
        }
        #endregion

        private static void InitializePrefab()
        {
            GameObject vanillaHereticPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/HereticBody");

            hereticPrefab = PrefabAPI.InstantiateClone(vanillaHereticPrefab, hereticBodyName);
            hereticBodyPrefab = hereticPrefab.GetComponent<CharacterBody>();
            Assets.RegisterBodyPrefab(hereticPrefab);

            hereticDisplayPrefab = PrefabAPI.InstantiateClone(hereticPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "HereticUnchainedDisplay");
            hereticDisplayPrefab.transform.localScale = Vector3.one * 0.7f;
            hereticDisplayPrefab.AddComponent<NetworkIdentity>();
            hereticDisplayPrefab.AddComponent<Components.HereticMenuAnimation>();

            Animator hereticAnimator = hereticPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<Animator>();
            if (hereticAnimator)
            {
                //Animator displayAnimator = hereticDisplayPrefab.AddComponent<Animator>(hereticAnimator);
                //EntityStateConfiguration spawnstate = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/EntityStates.Heretic.SpawnState");
            }
            else
            {
                Debug.Log("Heretic has no animator?");
            }

            //On.RoR2.CharacterMaster.OnBodyStart += AddHereticItems;
            On.RoR2.CharacterMaster.Start += AddHeresyItems;

            On.RoR2.CharacterBody.ReplaceSkillIfItemPresent += SkillReplacementIntercept;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += InterceptPickupToken;
            //On.RoR2.UI.ItemIcon.SetItemIndex += PickupTokenIntercept;
        }

        private static void InitializeSkins()
        {
            GameObject model = hereticPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            UnityEngine.Object.DestroyImmediate(model.GetComponent<ModelSkinController>());
            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            SkinDef defaultSkin = HereticUnchained.CoreModules.Skins.CreateSkinDef("Default",
                LoadoutAPI.CreateSkinIcon(new Color(0.7f, 0.05f, 0.1f), new Color(0.6f, 0.5f, 0.6f), new Color(0.1f, 0.1f, 0.15f), new Color(0.15f, 0.1f, 0.2f)),
                defaultRenderers,
                mainRenderer,
                model);

            skins.Add(defaultSkin);
            #endregion

            #region MasterySkin
            /*SkinDef masterySkin = Skins.CreateSkinDef("Mastery",
                LoadoutAPI.CreateSkinIcon(new Color(0.15f, 0.15f, 0.15f), new Color(0.15f, 0.15f, 0.15f), new Color(0.15f, 0.15f, 0.15f), new Color(0.15f, 0.15f, 0.15f)),
                defaultRenderers,
                mainRenderer,
                model);

            skins.Add(masterySkin);*/
            #endregion

            skinController.skins = skins.ToArray();
        }
        private static void CreateSkillFamilies(GameObject targetPrefab)
        {
            foreach (GenericSkill obj in targetPrefab.GetComponentsInChildren<GenericSkill>())
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }

            skillLocator = targetPrefab.GetComponent<SkillLocator>();
            SkillBase.characterSkillLocators.Add(hereticBodyName, skillLocator);

            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = "HERETIC_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = "HERETIC_PASSIVE_DESC";
            skillLocator.passiveSkill.icon = iconBundle.LoadAsset<Sprite>(HereticPlugin.iconsPath + "DissidentWill.png");

            skillLocator.primary = targetPrefab.AddComponent<GenericSkill>();
            primaryFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (primaryFamily as ScriptableObject).name = targetPrefab.name + "PrimaryFamily";
            primaryFamily.variants = new SkillFamily.Variant[0];
            skillLocator.primary._skillFamily = primaryFamily;

            skillLocator.secondary = targetPrefab.AddComponent<GenericSkill>();
            secondaryFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (secondaryFamily as ScriptableObject).name = targetPrefab.name + "SecondaryFamily";
            secondaryFamily.variants = new SkillFamily.Variant[0];
            skillLocator.secondary._skillFamily = secondaryFamily;

            skillLocator.utility = targetPrefab.AddComponent<GenericSkill>();
            utilityFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (utilityFamily as ScriptableObject).name = targetPrefab.name + "UtilityFamily";
            utilityFamily.variants = new SkillFamily.Variant[0];
            skillLocator.utility._skillFamily = utilityFamily;

            skillLocator.special = targetPrefab.AddComponent<GenericSkill>();
            specialFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (specialFamily as ScriptableObject).name = targetPrefab.name + "SpecialFamily";
            specialFamily.variants = new SkillFamily.Variant[0];
            skillLocator.special._skillFamily = specialFamily;

            //Assets.AddSkillFamily(passiveFamily);
            Assets.RegisterSkillFamily(primaryFamily);
            Assets.RegisterSkillFamily(secondaryFamily);
            Assets.RegisterSkillFamily(utilityFamily);
            Assets.RegisterSkillFamily(specialFamily);
        }
    }
}
