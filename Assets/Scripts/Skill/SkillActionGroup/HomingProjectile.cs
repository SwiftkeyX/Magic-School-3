using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    public class HomingProjectile : Projectile
    {
        private ICombatant _target;

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

        protected override bool ResolveAimTarget(AimTargetEnum aimTarget)
        {
            // aim skill at current target
            if (aimTarget == AimTargetEnum.Current)
            {
                _target = _me.FindNearestEnemy();
                if (_target == null) return false;
            }

            else if (aimTarget == AimTargetEnum.Furthest)
            {
                _target = _me.FindFurthestEnemy();
                if (_target == null) return false;
            }

            return true;
        }

        protected override void GetAimDirection()
        {
            // homing projectile update target position every frame, so it could land straight on target enemy
            _aimTarget = _target.transform.position;

            // lock projectile shoot direction 
            _direction = (_aimTarget - transform.position).normalized;

            // if no direction, destroy 
            if (_direction == Vector3.zero)
            {
                DestroyMe();
            }
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
