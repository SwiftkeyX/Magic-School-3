using UnityEngine;

namespace MagicSchool
{
    // Walks one hex, one frame at a time - no coroutine, since the state machine already drives
    // everything off OnUpdate(). SetTarget must be called (by whoever transitions in, e.g.
    // HeroIdle via Hero.BeginWalkTo) before this state becomes current.
    public class HeroWalk : HeroState
    {
        public override HeroStateType StateType => HeroStateType.Walk;

        private Hex _targetHex;
        private Vector3 _start;
        private Vector3 _end;
        private float _duration;
        private float _elapsed;

        private readonly MovementConfig _movement;

        public HeroWalk(Hero hero, MovementConfig movement) : base(hero)
        {
            _movement = movement;
        }

        public override void OnEnter()
        {
            _targetHex = _me.ReservedHex;
            _start = _me.transform.position;
            _end = _targetHex.transform.position;
            _duration = 1f / _movement.MoveSpeed;
            _elapsed = 0f;
        }

        public override void OnUpdate()
        {
            // walk according the movement's curve
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            _me.transform.position = Vector3.Lerp(_start, _end, _movement.WalkCurve.Evaluate(t));

            CheckSwitchState();
        }

        protected override void CheckSwitchState()
        {
            // If walking animation finishes, transition to idle
            // FlAGGING: This is weird, so it will be changed later
            bool isWalkingFinished = _elapsed >= _duration;
            if (isWalkingFinished)
            {
                _me.transform.position = _end;
                _me.SetCurrentPlacement(_targetHex);
                _me.ChangeState(HeroStateType.Idle);
            }
        }
    }
}
