using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Travels in a straight line through everyone in its path - the aim target only sets the
    /// direction, it is not a destination, so the shot carries on past whoever it hits first.
    /// </summary>
    public class PiercingProjectile : Projectile
    {
        // ======================================== override method ==============================================
        protected override void Play()
        {
            base.Play();

            // initialize hitbox: apply effects once, on first contact
            OnContactHitbox onceHitbox = new OnContactHitbox();
            onceHitbox.OnHit += HandleHit;
            _hitbox = onceHitbox;
            _hitbox.Init(_me);
        }


        // ======================================== private ==============================================
        private void HandleHit(Hero hero)
        {
            List<Hero> recipients = new List<Hero> { hero };
            foreach (SkillEffect effect in _effects)
            {
                ApplyEffectToRecipients(effect, recipients);
            }

        }

    }
}
