using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    public abstract class TemplateAction : MonoBehaviour
    {
        // FIXNOW: cast time is stub. Have to be implement.
        [SerializeField] protected float _castTime;                 // the duration skill was cast
        protected Hero _me;
        protected List<SkillEffect> _effects;
        protected Hitbox _hitbox;
        protected Vector3 _source;     
        protected Vector3 _aimTarget;   
        
        // ==================================== other ====================================
        private event Action OnExpired;     // Fire when a template action lifetime runout
        protected float _lifetime = 10f;      // FLAGGING: lifetime of 10 sec is too long. Leave it for now.

        // ==================================== getter ====================================
        public float CastTime => _castTime;

        // ==================================== public method ====================================
        public static bool TryPlay(TemplateAction prefab, ActionSourceEnum source, AimTargetEnum aimTarget, Hero caster, List<SkillEffect> effects, Action onExpired = null)
        {
            // change skill prefab into scene instace
            TemplateAction instance = Instantiate(prefab);

            // init skill variable
            if (!instance.TryConfigure(caster, effects, source, aimTarget))
            {
                // skill is not play
                Destroy(instance.gameObject);
                return false;
            }

            // other config
            instance.OnExpired += onExpired;

            // skill is played
            instance.Play();
            return true;
        }

        // try initialize skill, if something went wrong, skill won't play
        private bool TryConfigure(Hero caster, List<SkillEffect> effects, ActionSourceEnum source, AimTargetEnum aimTarget)
        {
            Init(caster, effects);

            // find "source" using source enum
            ResolveSource(source);

            // find "aim" using aim enum
            if (!ResolveAimTarget(aimTarget)) return false;

            return true;
        }

        private void Init(Hero caster, List<SkillEffect> effects)
        {
            _me = caster;
            _effects = effects;
        }


        // ==================================== abstract method ====================================
        // Each template action child have a different way to resolve how their skill was spawn/aim at.
        // read ResolveSource&ResolveAimTarget in each different's child for more detail
        protected abstract void ResolveSource(ActionSourceEnum source);

        // returns false if no valid target could be resolved (e.g. Current-targeted with no enemy left) - caller skips the cast
        protected abstract bool ResolveAimTarget(AimTargetEnum aimTarget);

        // where this template action's instance should sit once source/aim are resolved.
        // 1) an AOE spawns at the aim target, 
        // 2) a projectile spawns at its source and travels from there.
        protected abstract Vector3 GetSpawnPosition();

        protected abstract void Play();


        // ==================================== Expire ====================================
        // Every template action ends the same way - it gets destroyed
        // FIXNOW: still not implement
        protected void DestroyMe()
        {
            // the scene is being torn down, not the template action running out
            if (_me == null) return;

            Destroy(gameObject);

            OnExpired?.Invoke();
        }


        // ==================================== Hitbox ====================================
        protected void OnTriggerEnter2D(Collider2D other) => _hitbox?.OnTriggerEnter2D(other);
        protected void OnTriggerExit2D(Collider2D other) => _hitbox?.OnTriggerExit2D(other);


        // ==================================== Effect & Recipient ====================================
        // apply effect to the recipients
        protected void ApplyEffectToRecipients(SkillEffect effect, List<Hero> recipients)
        {
            switch (effect.Recipient)
            {
                case EffectRecipientEnum.Self:
                    effect.ApplyEffect(new List<Hero> { _me });
                    break;

                case EffectRecipientEnum.SameToAimTarget:
                case EffectRecipientEnum.EnemiesInArea:
                case EffectRecipientEnum.EnemiesInPath:
                    effect.ApplyEffect(recipients);
                    break;
            }
        }

        // Cadence Tick are use by several template action
        // so we unified thing by move it here. 
        // FIXLATER: But it should be move later since not all template action need it. maybe to interface?
        protected IEnumerator CadenceTick(HealSkillEffect effect, List<Hero> recipients)
        {
            WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);
            float elapsed = 0f;

            while (elapsed < effect.Duration)
            {
                yield return wait;
                elapsed += effect.Cadence.cadenceInterval;

                effect.ApplyEffect(recipients);
            }

            // destroy once duration is expired
            DestroyMe();
        }
    }
}
