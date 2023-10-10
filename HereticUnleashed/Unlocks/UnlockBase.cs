using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HereticUnchained.Unlocks
{
    public abstract class UnlockBase<T> : UnlockBase where T : UnlockBase<T>
    {
        public static T instance { get; private set; }

        public UnlockBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting UnlockBase was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class UnlockBase : ModdedUnlockable
    {
        public static string Token = HereticPlugin.TokenName + "UNLOCK_";
        public abstract string UnlockLangTokenName { get; }
        public abstract string UnlockName { get; }
        public abstract string AchievementName { get; }
        public abstract string AchievementDesc { get; }

        public override bool ForceDisable => false;
        public override string AchievementIdentifier => Token + UnlockLangTokenName + "_ACHIEVEMENT_ID";

        public override string UnlockableIdentifier => Token + UnlockLangTokenName + "_REWARD_ID";

        public override string AchievementNameToken => Token + UnlockLangTokenName + "_ACHIEVEMENT_NAME";

        public override string AchievementDescToken => Token + UnlockLangTokenName + "_ACHIEVEMENT_DESC";

        public override string UnlockableNameToken => Token + UnlockLangTokenName + "_UNLOCKABLE_NAME";
        public virtual bool HideUnlock => false;

        internal Sprite GetSpriteProvider(string iconName)
        {
            return null;// HereticPlugin.iconBundle.LoadAsset<Sprite>(HereticPlugin.iconsPath + iconName + ".png");
        }

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("HereticBody");
        }

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            LanguageAPI.Add(AchievementNameToken, "Heretic: " + AchievementName);
            LanguageAPI.Add(AchievementDescToken, "As The Heretic, " + AchievementDesc);
            LanguageAPI.Add(UnlockableNameToken, "Heretic: " + UnlockName);
        }
    }
}
