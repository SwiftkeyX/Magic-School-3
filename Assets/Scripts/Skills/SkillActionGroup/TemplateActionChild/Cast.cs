using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Cast is the template action for instant self-effects (e.g. self-buffs).
    /// No hitbox, no physical footprint - it applies its effects to the caster and is done.
    /// e.g. Galio's Idol of Durand step 1.
    /// </summary>
    public class Cast : TemplateAction
    {
        private ICombatant _target;
        private int _keepAlive;     // the amount of "thing" running on this TemplateAction

        // source mean nothing to Cast.
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            if (source == ActionSourceEnum.Self)
            {
                _source = _me.transform.position;
            }

            // spawn on current target
            else if (source == ActionSourceEnum.Current)
            {
                ICombatant target = _me.CurrentTarget;
                if (target == null) return false;
                _source = target.transform.position;
            }

            // else if () ...

            // fallback
            else _source = _me.transform.position;

            return true;
        }

        // aim = which unit the cast was targeting
        protected override bool ResolveAimTarget(AimTargetEnum aimTarget)
        {
            // aim skill at self
            if (aimTarget == AimTargetEnum.Self)
            {
                _target = _me;
            }

            // aim skill at current target
            else if (aimTarget == AimTargetEnum.Current)
            {
                _target = _me.CurrentTarget;
                if (_target == null) return false;
            }

            // aim skill at furthest target
            else if (aimTarget == AimTargetEnum.Furthest)
            {
                _target = _me.FindFurthestEnemy();
                if (_target == null) return false;
            }

            // else if () ...

            // fallback
            else _target = _me;

            return true;
        }

        protected override Vector3 GetSpawnPosition() => _source;


        protected override void Play()
        {
            foreach (SkillEffect effect in _effects)
            {
                if (effect.Recipient != EffectRecipientEnum.Self) continue;

                // if cadence, apply effect over time.
                // e.g. galio heal
                if (effect.Cadence.isCadence)
                {
                    StartCoroutine(PerHeroCadenceTick(effect, _me));
                }

                // if not cadence, apply effect once.
                else
                {
                    effect.ApplyEffect(new List<IEffectable> { _target });

                    // get longest effect duration
                    float duration = LongestModifierDuration(effect);

                    // set effect duration.
                    if (duration > 0) StartCoroutine(ExpireAfterModifier(duration));
                }
            }

            // if there's still something running, don't destroy yet
            if (_keepAlive == 0) DestroyMe();
        }

        // get the longest effect duration from this TemplateAction
        private float LongestModifierDuration(SkillEffect effect)
        {
            if (!(effect is ModifierSkillEffect modifierEffect)) return 0f;

            float longest = 0f;
            foreach (ModifierSpec modifier in modifierEffect.Modifiers)
            {
                if (modifier == null) continue;

                if (modifier.GetDuration() > longest) longest = modifier.GetDuration();
            }

            return longest;
        }

        private IEnumerator ExpireAfterModifier(float duration)
        {
            // another thing run
            _keepAlive++;

            // wait
            yield return new WaitForSeconds(duration);

            // another thing dies
            _keepAlive--;

            // if there's still something running, don't destroy yet
            if (_keepAlive == 0) DestroyMe();
        }

        // Cadence Tick are use by several template action
        // so we unified thing by move it here.
        // FLAGGING: But it should be move later since not all template action need it. maybe to interface?
        private IEnumerator PerHeroCadenceTick(SkillEffect effect, ICombatant hero)
        {
            _keepAlive++;

            WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);
            List<ICombatant> recipients = new List<ICombatant> { hero };
            float elapsed = 0f;

            while (elapsed < effect.Cadence.cadenceDuration)
            {
                yield return wait;
                elapsed += effect.Cadence.cadenceInterval;

                if (hero == null || hero.StateType == HeroStateEnum.Dead) break;
                ApplyEffectToRecipients(effect, recipients);
            }

            // one of cadence effect die
            _keepAlive--;

            // if there's still something running, don't destroy yet
            if (_keepAlive == 0) DestroyMe();
        }
    }
}
