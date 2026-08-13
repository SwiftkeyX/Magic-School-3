using System.Collections.Generic;
using MagicSchool;
using MagicSchool.Heroes;

namespace MagicSchool.Skills
{
    public class FirstHitProjectile : Projectile
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

            // tell the next step where this landed, before the destroy
            ReportHitPosition();

            // destroy first hit projectile once it hit target
            DestroyMe();
        }
    }
}