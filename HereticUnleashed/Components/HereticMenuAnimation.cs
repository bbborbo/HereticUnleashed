using EntityStates;
using RoR2;
using UnityEngine;

namespace HereticUnchained.Components
{
    public class HereticMenuAnimation : MonoBehaviour
    {
        private uint playID;

        private void OnEnable()
        {
            this.PlayEffect();
        }

        private void PlayEffect()
        {
            this.playID = Util.PlaySound(EntityStates.Heretic.SpawnState.spawnSoundString, base.gameObject);
            EffectManager.SimpleEffect(EntityStates.Heretic.SpawnState.effectPrefab, transform.position + Vector3.up, Quaternion.identity, false);
            this.PlayAnimation("Body", "Spawn", "Spawn.playbackRate", EntityStates.Heretic.SpawnState.duration);

            CharacterModel characterModel = this.transform.GetComponent<CharacterModel>();
            if (characterModel && EntityStates.Heretic.SpawnState.overlayMaterial)
            {
                TemporaryOverlay temporaryOverlay = this.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = EntityStates.Heretic.SpawnState.overlayDuration;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = EntityStates.Heretic.SpawnState.overlayMaterial;
                temporaryOverlay.inspectorCharacterModel = characterModel;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }
        }

        private void OnDestroy()
        {
            if (this.playID != 0) AkSoundEngine.StopPlayingID(this.playID);
        }

        private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration)
        {
            Animator modelAnimator = this.GetComponent<Animator>();
            if (modelAnimator)
            {
                EntityStates.EntityState.PlayAnimationOnAnimator(modelAnimator, layerName, animationStateName, playbackRateParam, duration);
            }
            else
            {
                Debug.Log("Model animator not found!");
            }
        }
    }
}