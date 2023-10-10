using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HereticUnchained.CoreModules
{
    internal static class Survivors
    {
        internal static List<SurvivorDef> survivorDefinitions = new List<SurvivorDef>();

        internal static SurvivorDef RegisterNewSurvivor(SurvivorDefInfo survivorDefInfo)
        {
            SurvivorDef survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            survivorDef.bodyPrefab = survivorDefInfo.bodyPrefab;
            survivorDef.displayPrefab = survivorDefInfo.displayPrefab;
            survivorDef.primaryColor = survivorDefInfo.primaryColor;
            survivorDef.displayNameToken = survivorDefInfo.displayNameToken;
            survivorDef.descriptionToken = survivorDefInfo.descriptionToken;
            survivorDef.outroFlavorToken = survivorDefInfo.outroFlavorToken;
            survivorDef.mainEndingEscapeFailureFlavorToken = survivorDefInfo.mainEndingEscapeFailureFlavorToken;
            survivorDef.desiredSortPosition = survivorDefInfo.desiredSortPosition;
            survivorDef.unlockableDef = survivorDefInfo.unlockableDef;
            survivorDef.hidden = false;

            Assets.RegisterSurvivorDef(survivorDef);

            return survivorDef;
        }

        internal struct SurvivorDefInfo
        {
            internal GameObject bodyPrefab;
            internal GameObject displayPrefab;
            internal Color primaryColor;
            internal string displayNameToken;
            internal string descriptionToken;
            internal string outroFlavorToken;
            internal string mainEndingEscapeFailureFlavorToken;
            internal float desiredSortPosition;
            internal bool hidden;
            internal UnlockableDef unlockableDef;
        }
    }
}
