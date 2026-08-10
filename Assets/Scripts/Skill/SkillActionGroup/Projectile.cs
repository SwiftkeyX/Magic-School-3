using System;
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
        protected ICombatant _target;
        const float PROJECTILELIFETIME = 10f;

        // ==================================== OnHit event ====================================
        private event Action<SkillStepContext> OnHit;        // Fire the first time projectile lands on someone
        private bool _hasReportedHit;

        protected override void SubscribeTriggers(Action<SkillStepContext> onExpired, Action<SkillStepContext> onHit)
        {
            base.SubscribeTriggers(onExpired, onHit);

            OnHit += onHit;
        }

        // Report where the projectile hit
        protected void ReportHitPosition()
        {
            if (_hasReportedHit) return;
            _hasReportedHit = true;

            OnHit?.Invoke(new SkillStepContext(transform.position));
        }

        protected bool IsTargetAlive => _target != null && _target.IsAlive;

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
                Debug.LogWarning("[Scriptable Object] Projectile can't be aim at self", this);
                return false;
            }

            // aim skill at current target
            else if (aimTarget == AimTargetEnum.Current)
            {
                _target = _me.FindNearestEnemy();
                if (_target == null) return false;
            }

            else if (aimTarget == AimTargetEnum.Furthest)
            {
                _target = _me.FindFurthestEnemy();
                if (_target == null) return false;
            }

            // else if () ...

            // nothing to shoot at, so don't shoot 
            else
            {
                Debug.LogWarning($"[Scriptable Object] Projectile can't aim at {aimTarget}", this);
                return false;
            }

            return true;
        }

        protected override Vector3 GetSpawnPosition() => _source;

        protected virtual void GetAimDirection()
        {
            // if target die beforehand, return => let projectile keep heading forward 
            if (!IsTargetAlive) return;

            _aimTarget = _target.transform.position;

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
