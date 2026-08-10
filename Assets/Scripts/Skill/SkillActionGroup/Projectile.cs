using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Projectile was aim toward the "target", what determined which recipients get hit is based on the projectile type.
    /// </summary>
    public abstract class Projectile : TemplateAction
    {
        [SerializeField] protected float _speed = 8f;
        protected Vector3 _direction;
        const float PROJECTILELIFETIME = 10f;

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
                Debug.LogWarning("[Scriptable Object] Projectile can't be aim at self");
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

        protected virtual void GetAimDirection()
        {
            // lock projectile shoot direction 
            _direction = (_aimTarget - transform.position).normalized;

            // if no direction, destroy 
            if (_direction == Vector3.zero)
            {
                DestroyMe();
            }
        }

        protected virtual void Update()
        {
            transform.position += _direction * (_speed * Time.deltaTime);
        }

        // ======================================== private ========================================
        // set object lifetime - this is for fallback despawn if it never reaches anyone
        protected override void SetLifeTime()
        {
            _lifetime = PROJECTILELIFETIME;
            ExpireAfter(_lifetime);
        }
    }
}
