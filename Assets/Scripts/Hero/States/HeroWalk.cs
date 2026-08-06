using UnityEngine;

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

    public HeroWalk(Hero hero, SkillSO skill, MovementConfig movement) : base(hero, skill)
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
        // If hp is below 0, transition to dead, WOW
        bool isMeDead = (_me.CurrentHP <= 0);
        if (isMeDead)
        {
            _me.StateMachine.ChangeState(HeroStateType.Dead);
            return;
        }

        if (_me.IsStunned)
        {
            _me.StateMachine.ChangeState(HeroStateType.Stunned);
            return;
        }

        // If walking animation finishes, transition to idle
        // FIXLATER: This is weird, so it will be changed later
        bool isWalkingFinished = _elapsed >= _duration;
        if (isWalkingFinished)
        {
            _me.transform.position = _end;
            _me.SetCurrentPlacement(_targetHex);
            _me.StateMachine.ChangeState(HeroStateType.Idle);
            return;
        }
    }

}
