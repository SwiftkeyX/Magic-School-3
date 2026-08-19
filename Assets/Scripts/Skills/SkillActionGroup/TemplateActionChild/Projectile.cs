using System;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Projectile was aim toward the "target", what determined which recipients get hit is based on the projectile type.
    /// </summary>
    public abstract class Projectile : TemplateAction
    {
        [SerializeField] protected float _speed = 8f;
        [SerializeField] protected float _size = 1f;
        [SerializeField] protected float _beamHalfWidth = 0.8f;
        protected Vector3 _direction;
        protected ICombatant _target;
        const float PROJECTILELIFETIME = 10f;

        // ==================================== OnHit event ====================================
        private event Action<SkillStepContext> OnHit;        // Fire the first time projectile lands on someone
        private bool _hasReportedHit;

        // Projectile have OnHit event
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

            SetProjectileSize(_size);

            // find where the projectile shoot direction
            GetAimDirection();

            // set rotation
            FaceAimTarget();

            SetLifeTime();
        }

        // source = where projectile spawn from
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            // spawn projectile at self
            if (source == ActionSourceEnum.Self)
            {
                _source = _me.transform.position;
            }

            // else if ...

            // fallback - a projectile always has somewhere to come from, so this never fails
            else _source = _me.transform.position;

            return true;
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
                _target = _me.FindCurrentTarget();
                if (_target == null) return false;
            }

            // aim skill at furthest enemy from self
            else if (aimTarget == AimTargetEnum.Furthest)
            {
                _target = _me.FindFurthestEnemy();
                if (_target == null) return false;
            }

            // aim down the line that passes through the most enemies
            else if (aimTarget == AimTargetEnum.ClusteredLaser)
            {
                _target = _me.FindClusteredLaser(_beamHalfWidth);
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

        // set object lifetime - this is for fallback despawn if it never reaches anyone
        protected override void SetLifeTime()
        {
            _lifetime = PROJECTILELIFETIME;
            ExpireAfter(_lifetime);
        }

        // =========================================== virtual ===========================================
        protected virtual void GetAimDirection()
        {
            // if target die beforehand, return => let projectile keep heading forward 
            if (!IsTargetAlive) return;

            // destination = enemy position
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
        private const float Default = 1f;
        private void SetProjectileSize(float size = Default)
        {
            this.transform.localScale = new Vector3(size, size, this.transform.localScale.z);
        }

        // FLAGGING: This is duplicated of AOE's method
        // Point the AOE tip toward aim target
        private void FaceAimTarget()
        {
            Vector3 facing = _aimTarget - _source;
            if (facing.sqrMagnitude < 0.0001f) return;

            float degrees = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, degrees + 90f);
        }
    }
}
