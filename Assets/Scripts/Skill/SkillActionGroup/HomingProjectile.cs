using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    // FIXNOW: make homing projectile work as intended, it should lock the target enemy, and go past any enemy that isn't the target. 
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
            // if hero is hit isn't the target one, don't apply effect
            if ((ICombatant)hero != _target) return;

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
