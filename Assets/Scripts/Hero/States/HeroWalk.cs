using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Placements;

namespace MagicSchool.Heroes.States
{
    // Walks one hex, one frame at a time - no coroutine, since the state machine already drives
    // everything off OnUpdate(). SetTarget must be called (by whoever transitions in, e.g.
    // HeroIdle via Hero.BeginWalkTo) before this state becomes current.
    public class HeroWalk : HeroState
    {
        public override HeroStateEnum StateType => HeroStateEnum.Walk;

        private Hex _targetHex;
        private Vector3 _start;
        private Vector3 _end;
        private float _duration;
        private float _elapsed;

        private readonly MovementConfig _movement;

        public HeroWalk(Hero hero, MovementConfig movement, Transition transition) : base(hero, transition)
        {
            _movement = movement;
        }

        public override void OnEnter()
        {
            StartStepTo(_me.ReservedHex);
        }

        public override void OnUpdate()
        {
            // walk according the movement's curve
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            _me.transform.position = Vector3.Lerp(_start, _end, _movement.WalkCurve.Evaluate(t));

            // if walk is finished, set new placement, check switch state
            bool isWalkingFinished = _elapsed >= _duration;
            if (isWalkingFinished)
            {
                _me.transform.position = _end;
                _me.SetCurrentPlacement(_targetHex);
            }

            CheckSwitchState();
        }

        protected override void CheckSwitchState()
        {
            // if walk isn't finished, return
            bool isWalkingFinished = _elapsed >= _duration;
            if (!isWalkingFinished) return;

            ICombatant nearestEnemy = _me.FindNearestEnemy();

            // guard
            if (nearestEnemy == null) { _me.ChangeState(HeroStateEnum.Idle); return; }

            if (_transition.CanAttack(nearestEnemy))
            {
                _me.ChangeState(HeroStateEnum.Attack);
                return;
            }

            if (_transition.CanWalk(nearestEnemy))
            {
                ResumeWalk();
                return;
            }

            else
            {
                _me.ChangeState(HeroStateEnum.Idle);
                return;
            }
        }

        private void StartStepTo(Hex hex)
        {
            _targetHex = hex;
            _start = _me.transform.position;
            _end = hex.transform.position;
            _duration = 1f / _movement.MoveSpeed;
            _elapsed = 0f;
        }

        // FLAGGING: This look completely like OnEnter() BUT later OnEnter() maybe have animation and other logic
        // so I separate it.
        private void ResumeWalk()
        {
            StartStepTo(_me.ReservedHex);
        }
    }
}
