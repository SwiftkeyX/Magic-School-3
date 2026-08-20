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
        [SerializeField] protected int _aimRange = 4;               // how far out, in hexes, a hex-aimed shot may look

        // FLAGGING: The AOE's radius from next step was also use here. But it was not actually fetch from one. 
        // how move was resolve, kinda need the effectRange of the other skill step involve
        [SerializeField] protected float _effectRange = 1.25f;
        // FLAGGING: similar to _effectRange
        [SerializeField] protected float _beamHalfWidth = 0.8f;

        protected Vector3 _direction;
        protected ICombatant _target;   // who the projectile is aiming at?
        protected Transform _aimAt;     // what will projectile fly at?
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
        protected override void ApplyTuning(Tuning tuning)
        {
            base.ApplyTuning(tuning);
            if (tuning == null) return;

            if (tuning.Range.HasValue) _aimRange = tuning.Range.Value;
            if (tuning.EffectRange.HasValue) _effectRange = tuning.EffectRange.Value;
            if (tuning.LaneHalfWidth.HasValue) _beamHalfWidth = tuning.LaneHalfWidth.Value;
        }

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

                _aimAt = _target.transform;
            }

            // aim skill at furthest enemy from self
            else if (aimTarget == AimTargetEnum.Furthest)
            {
                _target = _me.FindFurthestEnemy();
                if (_target == null) return false;

                _aimAt = _target.transform;
            }

            // aim down the line that passes through the most enemies
            else if (aimTarget == AimTargetEnum.ClusteredLaser)
            {
                int finalDistance = (int)(_speed * PROJECTILELIFETIME);
                _target = _me.FindClusteredLaser(finalDistance, _beamHalfWidth);
                if (_target == null) return false;

                _aimAt = _target.transform;
            }

            // aim at clustered that measure by specify radius
            else if (aimTarget == AimTargetEnum.ClusteredCircle)
            {
                IPlacement blastCentre = _me.FindClusteredCircle(_aimRange, _effectRange, isJump: false);
                if (blastCentre == null) return false;

                _aimAt = blastCentre.transform;
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
            if (_target != null && !IsTargetAlive) return;

            if (_aimAt == null) return;

            // destination = whatever it was aimed at, read fresh so a moving one stays tracked
            _aimTarget = _aimAt.position;

            // lock projectile shoot direction 
            _direction = (_aimTarget - transform.position).normalized;

            // if no direction, destroy 
            if (_direction == Vector3.zero)
            {
                DestroyMe();
            }
        }

        // update projectile position to make it visually fly at target
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
