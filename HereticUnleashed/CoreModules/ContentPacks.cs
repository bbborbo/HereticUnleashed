using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.CoreModules
{
    internal class ContentPacks : IContentPackProvider
    {
        public static List<BuffDef> buffDefs = new List<BuffDef>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<GameObject> projectilePrefabs = new List<GameObject>();
        public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();

        public static List<SurvivorDef> survivorDefs = new List<SurvivorDef>();
        internal static List<GameObject> bodyPrefabs = new List<GameObject>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<SkillDef> skillDefs = new List<SkillDef>();
        public static List<Type> entityStates = new List<Type>();

        internal ContentPack contentPack = new ContentPack();
        public string identifier => "HERETICEXTENDED";

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;

            contentPack.buffDefs.Add(buffDefs.ToArray());
            contentPack.effectDefs.Add(effectDefs.ToArray());
            contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.networkedObjectPrefabs.Add(networkedObjectPrefabs.ToArray());

            contentPack.survivorDefs.Add(survivorDefs.ToArray());
            contentPack.bodyPrefabs.Add(bodyPrefabs.ToArray());
            contentPack.skillFamilies.Add(skillFamilies.ToArray());
            contentPack.skillDefs.Add(skillDefs.ToArray());
            contentPack.entityStateTypes.Add(entityStates.ToArray());

            //contentPack.eliteDefs.Add(Assets.eliteDefs.ToArray());
            //contentPack.unlockableDefs.Add(Unlockables.unlockableDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
