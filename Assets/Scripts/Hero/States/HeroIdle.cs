using System;
using UnityEngine;

namespace MagicSchool
{
    // Looks for a reason to stop being idle: an adjacent enemy to attack, or a valid step
    // toward the nearest enemy to start walking. Also owns the grace-period timer that gives
    // an undecided (non-Attack) blocking ally a moment to step aside before committing to a
    // longer detour.
    public class HeroIdle : HeroState
    {
        public override HeroStateType StateType => HeroStateType.Idle;

        // Timestamp a "this step doesn't look like progress" hold started, or -1f when not holding.
        private float _holdSince = -1f;

        private readonly MovementConfig _movement;

        private readonly Func<Hex, bool> _isHexBlocked;

        public HeroIdle(Hero hero, MovementConfig movement) : base(hero)
        {
            _movement = movement;

            // init func: to ask if this "hex" is reserved by me
            _isHexBlocked = hex => _me.IsHexReservedByOther(hex);
        }

        public override void OnEnter()
        {
            _holdSince = -1f;
        }

        public override void OnUpdate()
        {
            CheckSwitchState();
        }

        protected override void CheckSwitchState()
        {
            // Temporary, tagged once at its source - see the FIXLATER on HeroDataSO._isDummy.
            // Dummy never walks or attacks - it just stands there to be a target.
            if (_me.IsDummy) return;

            ICombatant nearestEnemy = _me.FindNearestEnemy();
            if (nearestEnemy == null) return;

            // If enemy is within attack range, stop moving, and transition to attack state
            if (_transition.TryAttack(nearestEnemy))
            {
                _me.ChangeState(HeroStateType.Attack);
                return;
            }

            // If there is ANY enemy that'll walk into my neighbors (adjacent), stop moving, and wait for him instead
            if (_transition.TryWalk(_me))
            {
                _me.SetReservedHex(targetHex);
                _me.ChangeState(HeroStateType.Walk);
                return;
            }

            else
            {
                _me.ChangeState(HeroStateType.Idle);
                return;
            }
        }

    }
}
