using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    public class HomingProjectile : Projectile
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

        protected override void Update()
        {                        
            // homing projectile update target position every frame, so it never miss on target enemy
            GetAimDirection();

            base.Update();
        }

        // ======================================== private ==============================================

        private void HandleHit(Hero hero)
        {
            List<Hero> recipients = new List<Hero> { hero };
            foreach (SkillEffect effect in _effects)
            {
                ApplyEffectToRecipients(effect, recipients);
            }

            // destroy homing projectile once it hit target
            DestroyMe();
        }

    }
}
