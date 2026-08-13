using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Heroes;

namespace MagicSchool.Skills
{
    /// <summary>
    /// ZoneAOE are template action that apply effect over time.
    /// Effect here was apply to the recipients on standing in the effect, if they walk out of it, they don't get effect re-apply.
    /// e.g. Garen, Silco, Swain
    /// </summary>
    public class ZoneAOE : AOE
    {
        private float _interval;
        private float _duration;
        const float VALUENOTASSIGN = -1f;

        // ======================================== override ==============================================
        protected override void Play()
        {
            // FLAGGING: This shouldn't be check everytime, it should be initialize somewhere once.
            // Read cadence BEFORE base.Play()
            ResolveCadence();

            base.Play();

            // one hitbox for the whole zone - every cadence effect on it shares one interval
            OnTickHitbox hitbox = new OnTickHitbox();
            hitbox.OnHit += HandleTick;
            hitbox.Init(_me);
            _hitbox = hitbox;

            // start applying effect every interval
            StartCoroutine(CadenceTick(hitbox, _interval));
        }

        // ZoneAOE have _lifetime as the same to cadence duration
        protected override void SetLifeTime()
        {
            // guard
            if (_duration == VALUENOTASSIGN) { base.SetLifeTime(); return; }

            _lifetime = _duration;
            ExpireAfter(_lifetime);
        }

        // ======================================== private ==============================================
        // Assign value to interval, and duration.
        // To guard - cadence effects on this zone must share one interval
        private void ResolveCadence()
        {
            _interval = VALUENOTASSIGN;
            _duration = VALUENOTASSIGN;
            foreach (SkillEffect effect in _effects)
            {
                if (!effect.Cadence.isCadence) continue;

                // if interval doesn't assign yet, assign it value
                if (_interval == VALUENOTASSIGN)
                {
                    _interval = effect.Cadence.cadenceInterval;
                    _duration = effect.Cadence.cadenceDuration;
                }

                // if interval/duration value is assign already, we don't want 2 value, so log error.
                else
                {
                    if (effect.Cadence.cadenceInterval != _interval)
                        Debug.LogError($"{name}: cadence effects on the same zone must share one interval, found {_interval} and {effect.Cadence.cadenceInterval} - only {_interval} will be used", this);

                    if (effect.Cadence.cadenceDuration != _duration)
                        Debug.LogError($"{name}: cadence effects on the same zone must share one duration, found {_duration} and {effect.Cadence.cadenceDuration} - only {_duration} will be used", this);
                }
            }
        }

        // a hero currently in the zone got ticked - apply every cadence effect to them
        private void HandleTick(Hero hero)
        {
            List<Hero> recipients = new List<Hero> { hero };
            foreach (SkillEffect effect in _effects)
            {
                if (effect.Cadence.isCadence) ApplyEffectToRecipients(effect, recipients);
            }
        }

        // Global cadence tick - on a fixed schedule from spawn, no initial collision needed to start ticking.
        private IEnumerator CadenceTick(OnTickHitbox hitbox, float interval)
        {
            WaitForSeconds wait = new WaitForSeconds(interval);

            while (true)
            {
                yield return wait;
                hitbox.FireTick();
            }
        }
    }
}
