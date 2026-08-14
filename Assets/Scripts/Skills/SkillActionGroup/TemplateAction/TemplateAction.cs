using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{

    /// <summary>
    /// TemplateAction = 1 part of skill that being played independently.
    /// e.g. create AOE, shoot projectile, cast buff/debuff
    /// </summary>
    public abstract class TemplateAction : MonoBehaviour
    {
        [SerializeField] protected float _castTime;                 // how long the caster is locked out of auto attacking
        protected ICombatant _me;
        protected List<SkillEffect> _effects;
        protected Hitbox _hitbox;
        protected Vector3 _source;
        protected Vector3 _aimTarget;

        // ==================================== universal to template action ====================================
        private event Action<SkillStepContext> OnExpired;     // Fire when a template action lifetime runout
        protected float _lifetime;
        private bool _hasExpired;

        // ==================================== from the step before ====================================
        // whatever the previous step produced, this'll be used by next step, to check trigger's condition
        protected SkillStepContext _fromPreviousStep;

        // ==================================== getter ====================================
        public float CastTime => _castTime;

        // ==================================== public method ====================================
        // try play template action. if play success, return true.
        // act as factory, since when this function is called, there is no real instance yet. 
        public static bool TryPlay(SkillActionGroup group, ICombatant caster,
        Action<SkillStepContext> onExpired = null, Action<SkillStepContext> onHit = null, SkillStepContext fromPreviousStep = null)
        {
            // change skill prefab into scene instace
            TemplateAction instance = Instantiate(group.TemplateAction);

            // config data from previous step
            instance._fromPreviousStep = fromPreviousStep;

            // init skill variable
            if (!instance.TryConfigure(caster, group.Effects, group.Source, group.Target))
            {
                // skill is not play
                Destroy(instance.gameObject);
                return false;
            }

            // other config - each template action wires up the triggers it can actually raise
            instance.SubscribeTriggers(onExpired, onHit);

            // skill is played
            instance.Play();
            return true;
        }

        // try initialize skill, if something went wrong, skill won't play
        private bool TryConfigure(ICombatant caster, List<SkillEffect> effects, ActionSourceEnum source, AimTargetEnum aimTarget)
        {
            // Not Init() before TryPlay(). 
            // because it need to instantiate the new instance first.
            Init(caster, effects);

            // find "source" using source enum
            if (!ResolveSource(source)) return false;

            // find "aim" using aim enum
            if (!ResolveAimTarget(aimTarget)) return false;

            return true;
        }

        private void Init(ICombatant caster, List<SkillEffect> effects)
        {
            _me = caster;
            _effects = effects;
        }


        // ==================================== abstract method ====================================
        // Each template action child have a different way to resolve how their skill was spawn/aim at.
        // read ResolveSource&ResolveAimTarget in each different's child for more detail
        // returns false if no valid source
        protected abstract bool ResolveSource(ActionSourceEnum source);

        // returns false if no valid target
        protected abstract bool ResolveAimTarget(AimTargetEnum aimTarget);

        // where this template action's instance should sit once source/aim are resolved.
        // 1) an AOE spawns at the aim target, 
        // 2) a projectile spawns at its source and travels from there.
        protected abstract Vector3 GetSpawnPosition();

        protected abstract void Play();


        // ==================================== OnExpired event ====================================
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

            OnExpired?.Invoke(new SkillStepContext(expiredPosition));
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

        // ==================================== Trigger wiring ====================================
        // OnExpired is the only trigger every template action can raise 
        // it was fired when the template action dies
        protected virtual void SubscribeTriggers(Action<SkillStepContext> onExpired, Action<SkillStepContext> onHit)
        {
            OnExpired += onExpired;
        }


        // ==================================== Hitbox ====================================
        protected void OnTriggerEnter2D(Collider2D other) => _hitbox?.OnTriggerEnter2D(other);
        protected void OnTriggerExit2D(Collider2D other) => _hitbox?.OnTriggerExit2D(other);


        // ==================================== Effect & Recipient ====================================
        // apply effect to the recipients
        protected void ApplyEffectToRecipients(SkillEffect effect, IReadOnlyList<IEffectable> recipients)
        {
            switch (effect.Recipient)
            {
                case EffectRecipientEnum.Self:
                    effect.ApplyEffect(new List<IEffectable> { _me });
                    break;

                case EffectRecipientEnum.SameToAimTarget:
                case EffectRecipientEnum.EnemiesInArea:
                case EffectRecipientEnum.EnemiesInPath:
                    effect.ApplyEffect(recipients);
                    break;
            }
        }

        // ===================================== helper =====================================
        protected virtual void SetLifeTime()
        {
            _lifetime = 0.5f;
            ExpireAfter(_lifetime);
        }
    }
}
