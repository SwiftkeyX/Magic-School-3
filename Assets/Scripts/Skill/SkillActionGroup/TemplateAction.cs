using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    public abstract class TemplateAction : MonoBehaviour
    {
        [SerializeField] protected float _castTime;                 // how long the caster is locked out of auto attacking
        protected Hero _me;
        protected List<SkillEffect> _effects;
        protected Hitbox _hitbox;
        protected Vector3 _source;
        protected Vector3 _aimTarget;

        // ==================================== universal template action ====================================
        private event Action<Vector3> OnExpired;     // Fire when a template action lifetime runout
        protected float _lifetime;
        private bool _hasExpired;

        // ==================================== specific to projectile ====================================
        // FLAGGING: Maybe I have to move this somewhere else?
        private event Action<Vector3> OnHit;         // Fire the first time it lands on someone e.g. on projectile hit
        protected Vector3? _onHitPosition;           // where the OnHit event was hit
        private bool _hasReportedHit;

        // ==================================== getter ====================================
        public float CastTime => _castTime;

        // ==================================== public method ====================================
        public static bool TryPlay(TemplateAction prefab, ActionSourceEnum source, AimTargetEnum aimTarget, Hero caster, List<SkillEffect> effects,
            Action<Vector3> onExpired = null, Action<Vector3> onHit = null, Vector3? lastHitPoint = null)
        {
            // change skill prefab into scene instace
            TemplateAction instance = Instantiate(prefab);

            // FLAGGING: I think this begin to be too complicated. I'll see what I can do.
            // before configuring - aiming at WhereProjectileHit needs it
            instance._onHitPosition = lastHitPoint;

            // init skill variable
            if (!instance.TryConfigure(caster, effects, source, aimTarget))
            {
                // skill is not play
                Destroy(instance.gameObject);
                return false;
            }

            // other config
            instance.OnExpired += onExpired;
            instance.OnHit += onHit;

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
        // Every template action ends the same way => through here
        // to make sure OnExpired always gets fired. 
        // Never call Destroy() on a template action directly.
        protected void DestroyMe()
        {
            // guard
            if (_me == null) return;

            // guard
            if (_hasExpired) return;
            _hasExpired = true;

            Vector3 expiredPosition = transform.position;

            Destroy(gameObject);

            OnExpired?.Invoke(expiredPosition);
        }

        // helper for projectile to report the hit position
        protected void ReportHitPosition()
        {
            if (_hasReportedHit) return;
            _hasReportedHit = true;

            OnHit?.Invoke(transform.position);
        }

        protected void ExpireAfter(float delay)
        {
            StartCoroutine(ExpireRoutine(delay));
        }

        private IEnumerator ExpireRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            DestroyMe();
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

        // ===================================== helper =====================================
        protected virtual void SetLifeTime()
        {
            _lifetime = 0.5f;
            ExpireAfter(_lifetime);
        }
    }
}
