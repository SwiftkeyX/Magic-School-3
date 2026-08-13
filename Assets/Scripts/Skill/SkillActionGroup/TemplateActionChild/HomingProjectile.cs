using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Heroes;

namespace MagicSchool.Skills
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
            // if hero is hit isn't the target one, don't apply effect
            if ((ICombatant)hero != _target) return;

            List<Hero> recipients = new List<Hero> { hero };
            foreach (SkillEffect effect in _effects)
            {
                ApplyEffectToRecipients(effect, recipients);
            }

            // tell the next step where this landed, before the destroy
            ReportHitPosition();

            // destroy homing projectile once it hit target
            DestroyMe();
        }

    }
}
