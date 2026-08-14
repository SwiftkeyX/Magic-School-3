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
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            _source = _me.transform.position;
            return true;
        }

        protected override bool ResolveAimTarget(AimTargetEnum aimTarget) => true;

        protected override Vector3 GetSpawnPosition() => _source;

        private int _cadenceRunning;        // counting the amount of cadence effect still running

        protected override void Play()
        {
            // initialize local variable
            // FLAGGING: Cast always applies to the caster (for now)
            List<ICombatant> self = new List<ICombatant> { _me };

            foreach (SkillEffect effect in _effects)
            {
                if (effect.Recipient != EffectRecipientEnum.Self) continue;

                // if cadence, apply effect over time.
                // e.g. galio heal
                if (effect.Cadence.isCadence)
                {
                    _cadenceRunning++;
                    StartCoroutine(PerHeroCadenceTick(effect, _me));
                }

                // if not cadence, apply effect once.
                else
                {
                    effect.ApplyEffect(self);
                }
            }

            // if there's still cadence effect run, don't destroy yet
            if (_cadenceRunning == 0) DestroyMe();
        }

        // Cadence Tick are use by several template action
        // so we unified thing by move it here.
        // FLAGGING: But it should be move later since not all template action need it. maybe to interface?
        private IEnumerator PerHeroCadenceTick(SkillEffect effect, ICombatant hero)
        {
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
            _cadenceRunning--; 

            // if there's still cadence effect run, don't destroy yet
            if (_cadenceRunning == 0) DestroyMe();
        }
    }
}
