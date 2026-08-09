
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Projectile was aim toward the "target", what determined which recipients get hit is based on the projectile type.
    /// </summary>
    public abstract class Projectile : TemplateAction
    {
        private float _lifetime = 10f;  // FLAGGING: lifetime of 10 sec is too long. Leave it for now.
        protected Vector3 _direction;

        // ======================================== override method ========================================
        protected override void Play()
        {
            // starting position
            transform.position = GetSpawnPosition();

            // find where the projectile shoot direction
            GetAimDirection();

            SetLifeTime();
        }

        // source = where projectile spawn from
        protected override void ResolveSource(ActionSourceEnum source)
        {
            // spawn projectile at self
            if (source == ActionSourceEnum.Self)
            {
                _source = _me.transform.position;
            }

            // else if ...
        }


        // aim = where projectile shoot direction is. Returns false if no valid target was found (caller skips the cast).
        protected override bool ResolveAimTarget(AimTargetEnum aimTarget)
        {
            // aim skill at self
            if (aimTarget == AimTargetEnum.Self)
            {
                _aimTarget = _me.transform.position;
            }

            // aim skill at current target
            else if (aimTarget == AimTargetEnum.Current)
            {
                ICombatant target = _me.FindNearestEnemy();
                if (target == null) return false;
                _aimTarget = target.transform.position;
            }

            else if (aimTarget == AimTargetEnum.Furthest)
            {
                ICombatant target = _me.FindFurthestEnemy();
                if (target == null) return false;
                _aimTarget = target.transform.position;
            }

            // else if () ...

            // fallback
            else _aimTarget = _me.transform.position;

            return true;
        }

        protected override Vector3 GetSpawnPosition() => _source;

        // ======================================== private ========================================
        private void GetAimDirection()
        {
            // lock the flight direction now - the target moving (or dying) later must not bend the shot
            _direction = (_aimTarget - transform.position).normalized;

            // aim and spawn ended up on the same spot, so there is no direction to fly in
            if (_direction == Vector3.zero)
            {
                Destroy(gameObject);
            }
        }

        // set object lifetime - this is for fallback despawn if it never reaches anyone
        private void SetLifeTime()
        {
            _lifetime = _castTime;
            Destroy(gameObject, _lifetime);
        }
    }
}
