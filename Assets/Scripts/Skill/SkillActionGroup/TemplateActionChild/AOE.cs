using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool
{
    /// <summary>
    /// AOE type for TemplateAction
    /// </summary>
    public abstract class AOE : TemplateAction
    {
        public enum AOEOffsetEnum { Center, Tip }
        [SerializeField] private AOEOffsetEnum _offset;

        // ======================================= protected =======================================
        protected override void Play()
        {
            // resolve rotation AOE placement first
            FaceAimTarget();

            // resolve transform AOE placement second
            transform.position = GetSpawnPosition();

            SetLifeTime();
        }

        // source = where AOE spawn.
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            // spawn on the caster
            if (source == ActionSourceEnum.Self)
            {
                _source = _me.transform.position;
            }

            // spawn on current target
            else if (source == ActionSourceEnum.Current)
            {
                ICombatant target = _me.CurrentTarget;
                if (target == null) return false;
                _source = target.transform.position;
            }

            // e.g. Teemo's and Karma's dart exploding on impact rather than back at the caster
            else if (source == ActionSourceEnum.WhereProjectileHit)
            {
                if (_fromPreviousStep?.Position == null) return false;
                _source = _fromPreviousStep.Position.Value;
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
                ICombatant target = _me.CurrentTarget;
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
            if (_offset == AOEOffsetEnum.Center) return _source;

            else if (_offset == AOEOffsetEnum.Tip)
            {
                // Tip: the near end sits on the source and the body runs out toward the aim, so shift
                // the centre out by half the shape's length. No direction to run along = nothing to do.
                Vector3 facing = _aimTarget - _source;
                if (facing.sqrMagnitude < 0.0001f) return _source;
                return _source + facing.normalized * HalfLengthAlongFacing();
            }

            // fallback - an unhandled offset still belongs on the source, not out at (1,1,1)
            return _source;
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
            transform.rotation = Quaternion.Euler(0f, 0f, degrees - 90f);
        }
    }
}
