using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;
using System;

namespace MagicSchool.Skills
{
    /// <summary>
    /// AOE type for TemplateAction
    /// </summary>
    internal abstract class AOE : TemplateAction
    {
        internal enum AOEOffsetEnum { Center, Tip }
        [SerializeField] private AOEOffsetEnum _offset;
        [SerializeField] private Sticky _sticky;

        // ======================================= override =======================================
        protected override void Play()
        {
            // resolve AOE's rotation first
            FaceAimTarget();

            // resolve AOE's transform second
            transform.position = GetSpawnPosition();

            SetLifeTime();

            InitRider();
        }

        // source = where AOE spawn.
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            // spawn on the caster
            if (source == ActionSourceEnum.Self)
            {
                _source = _me.transform.position;
                if (_sticky.IsSticky) _sticky.Source = _me;
            }

            // spawn on current target
            else if (source == ActionSourceEnum.Current)
            {
                ICombatant target = _me.FindCurrentTarget();
                if (target == null) return false;
                _source = target.transform.position;
                if (_sticky.IsSticky) _sticky.Source = target;
            }

            // spawn on where the previous projectile hit
            // e.g. Karma's dart exploding on impact
            else if (source == ActionSourceEnum.WhereProjectileHit)
            {
                if (_fromPreviousStep?.Position == null) return false;
                _source = _fromPreviousStep.Position.Value;
                if (_sticky.IsSticky) _sticky.Source = null;
            }

            // else if () ...

            // fallback
            else _source = _me.transform.position;

            return true;
        }

        // aim = where AOE was point at.
        // e.g. ConeAOE's tip point at current enemy
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
                ICombatant target = _me.FindCurrentTarget();
                if (target == null) return false;
                _aimTarget = target.transform.position;
            }

            // aim skill at furthest target
            else if (aimTarget == AimTargetEnum.Furthest)
            {
                ICombatant target = _me.FindFurthestEnemy();
                if (target == null) return false;
                _aimTarget = target.transform.position;
            }

            // aim skill at previous projectile hit position
            else if (aimTarget == AimTargetEnum.WhereProjectileHit)
            {
                if (_fromPreviousStep?.Position == null) return false;
                _aimTarget = _fromPreviousStep.Position.Value;
            }

            // else if () ...

            // fallback
            else _aimTarget = _me.transform.position;

            return true;
        }

        // return position where AOE was spawn
        // There's some nuiance => AOE could be place with offset
        // e.g. place the tip of box AOE at user, place the center of circle AOE at user
        protected override Vector3 GetSpawnPosition()
        {
            // the skill's center was place on source 
            if (_offset == AOEOffsetEnum.Center) return _source;

            // the skill's tip was place on source
            else if (_offset == AOEOffsetEnum.Tip)
            {
                Vector3 facing = _aimTarget - _source;
                if (facing.sqrMagnitude < 0.0001f) return _source;
                return _source + facing.normalized * HalfLengthAlongFacing();
            }

            // fallback - an unhandled offset still belongs on the source, not out at (1,1,1)
            return _source;
        }

        // The AOE could rides Move
        // e.g. Sion's AOE dies when his Move dies
        protected override void InitRider()
        {
            _rider = Rider.FindHostFor(this, _me);
        }

        // ======================================= abstract =======================================
        // When AOE hit someone, apply effect to recipient
        protected abstract void HandleAOEHit(ICombatant recipient);

        // ======================================= virtual =======================================
        protected virtual void Update()
        {
            // if sticky, the AOE should always follow the _source. 
            if (_sticky.IsSticky)
            {
                transform.position = _sticky.Source.transform.position;
            }
        }

        // ======================================= private =======================================
        /// How far the offset of this shape will be? e.g.
        private float HalfLengthAlongFacing()
        {
            float localHalf;
            Collider2D hitbox = GetComponent<Collider2D>();

            // BOXAOE - half a box's length, 
            if (hitbox is BoxCollider2D box) localHalf = box.size.y * 0.5f;

            // CIRCLEAOE - a circle's radius. 
            else if (hitbox is CircleCollider2D circle) localHalf = circle.radius;

            // else if {} ...

            // fallback
            else
            {
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                localHalf = sprite == null ? 0f : sprite.sprite.bounds.extents.y;
            }

            return localHalf * Mathf.Abs(transform.lossyScale.y);
        }

        // Point the AOE tip toward aim target
        private void FaceAimTarget()
        {
            Vector3 facing = _aimTarget - _source;
            if (facing.sqrMagnitude < 0.0001f) return;

            float degrees = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, degrees + 90f);
        }
    }

    [Serializable]
    internal struct Sticky
    {
        [SerializeField] internal bool IsSticky;
        internal ICombatant Source;
    }
}
