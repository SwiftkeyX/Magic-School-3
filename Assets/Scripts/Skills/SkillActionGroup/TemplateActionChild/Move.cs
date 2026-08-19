using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Engine;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Moves the caster itself - the jump half of a Jarvan-style dive.
    ///
    /// Unlike every other template action, the instance spawned here isn't the thing that does the
    /// work: it is a driver that pushes the CASTER's transform for _jumpDuration and then dies. Its
    /// OnExpired fires at the landing spot, so the next skill step (the AoE you land in) chains off
    /// it the same way a projectile's does.
    ///
    /// The travel itself is the same CurveMotion HeroWalk steps a hex with - only the curve and the
    /// duration differ, which is what makes it read as a jump instead of a walk.
    /// </summary>
    internal class Move : TemplateAction
    {
        [SerializeField] private int _moveRange = 2;                // hex radius the cluster is measured over
        [SerializeField] private float _jumpDuration = 0.5f;        // whole trip, however far it is - a jump isn't paced per hex like a walk
        // Progress along the path over the jump, same role as Hero's walk curve. Tune the arc here:
        // a slow-out/fast-in shape reads as a leap, a straight line reads as a slide.
        [SerializeField] private AnimationCurve _jumpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private IPlacement _landing;    // hex the caster ends up on, resolved before the cast is allowed to play
        private CurveMotion _jump;
        private bool _isJumping;

        // ======================================== override ========================================
        protected override void Play()
        {
            // this instance is only a driver, so park it on the caster - it never gets seen
            transform.position = GetSpawnPosition();

            // Reserved the landing hex before the actual jump
            // this ensure nobody else take our landing hex
            _landing.OnUnitReserved(_me);

            _jump = new CurveMotion(
                start: _source,
                end: _landing.transform.position,
                duration: _jumpDuration,
                curve: _jumpCurve);
            _isJumping = true;

            SetLifeTime();
        }

        // source = the hero that will be moved
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            if (source == ActionSourceEnum.Self)
            {
                _source = _me.transform.position;
            }

            // else if {} ...

            // FLAGGING: move template action are currently only work with self 
            // fallback
            else
            {
                _source = _me.transform.position;
                return false;
            }

            return true;
        }

        // aim = where the source will be moved at
        protected override bool ResolveAimTarget(AimTargetEnum aimTarget)
        {
            // source move at a hex which is most clustered with enemies
            if (aimTarget == AimTargetEnum.ClusteredCircle)
            {
                ICombatant target = _me.FindClusteredCircle(radius: _moveRange);
                if (target == null) return false;

                // land NEXT to the cluster, not on top of it - the hex that enemy stands on is taken
                _landing = _me.FindFreePlacementNextTo(target);
                if (_landing == null) return false;     // the cluster is walled in: nowhere to land, so don't cast

                _aimTarget = _landing.transform.position;
                return true;
            }

            // else if () ...

            // no landing spot means no jump - falling back to "move to where I already am" would
            // burn the cast on nothing
            Debug.LogWarning($"[Scriptable Object] Move can't aim at {aimTarget}", this);
            return false;
        }

        protected override Vector3 GetSpawnPosition() => _source;

        // Landing is what ends this action, not a timer - see Update(). The number is still
        // recorded so anything reading _lifetime sees how long the jump actually takes.
        protected override void SetLifeTime()
        {
            _lifetime = _jumpDuration;
        }

        // ======================================== private ========================================
        private void Update()
        {
            if (!_isJumping) return;

            _me.transform.position = _jump.Tick(Time.deltaTime);

            if (!_jump.IsFinished) return;

            _isJumping = false;

            // after finish landing, set current placement there
            TakeLandingPlacement();

            // die where the caster landed, so OnExpired hands the "landing spot" to the next step
            transform.position = _jump.End;
            DestroyMe();
        }

        private void TakeLandingPlacement()
        {
            IPlacement previous = _me.CurrentPlacement;

            if (previous != null) previous.OnUnitUnplaced(_me);
            _landing.OnUnitPlaced(_me);
        }
    }
}
