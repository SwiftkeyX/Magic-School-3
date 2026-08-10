using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// ZoneAOE are template action that apply effect over time.
    /// Effect here was apply to the recipients on standing in the effect, if they walk out of it, they don't get effect re-apply.
    /// e.g. Garen, Silco, Swain
    /// </summary>
    public class ZoneAOE : AOE
    {
        private float _interval;
        const float VALUENOTASSIGN = -1f;

        // ======================================== override ==============================================
        protected override void Play()
        {
            // Read the interval BEFORE base.Play()
            ResolveInterval();

            base.Play();

            // one hitbox for the whole zone - every cadence effect on it shares one interval
            OnTickHitbox hitbox = new OnTickHitbox();
            hitbox.OnHit += HandleTick;
            hitbox.Init(_me);
            _hitbox = hitbox;

            // start applying effect every interval
            if (_interval != VALUENOTASSIGN) StartCoroutine(CadenceTick(hitbox, _interval));
        }

        // ZoneAOE have _lifetime as the same to effect duration
        protected override void SetLifeTime()
        {
            // guard
            if (_interval == VALUENOTASSIGN)
            {
                base.SetLifeTime();
                return;
            }

            _lifetime = _interval;
            Destroy(gameObject, _lifetime);
        }

        // ======================================== private ==============================================
        // temporarily guard
        // all cadence effects on this zone must share one interval - guard against a design
        // mistake where they don't, since only the first interval found actually gets used
        private void ResolveInterval()
        {
            _interval = VALUENOTASSIGN;
            foreach (SkillEffect effect in _effects)
            {
                if (!effect.Cadence.isCadence) continue;

                // if interval doesn't assign yet, assign it value
                if (_interval == VALUENOTASSIGN) _interval = effect.Cadence.cadenceInterval;

                // if interval value is assign already, we don't want 2 interval value, so log error
                else
                {
                    Debug.LogError($"{name}: cadence effects on the same zone must share one interval, found {_interval} and {effect.Cadence.cadenceInterval} - only {_interval} will be used", this);
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
