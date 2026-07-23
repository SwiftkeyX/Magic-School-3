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

    public HeroWalk(Hero hero) : base(hero) { }

    public void SetTarget(Hex targetHex)
    {
        _targetHex = targetHex;
    }

    public override void OnEnter()
    {
        _start = Hero.transform.position;
        _end = _targetHex.transform.position;
        _duration = 1f / Hero.MoveSpeed;
        _elapsed = 0f;
    }

    public override void OnUpdate()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);
        Hero.transform.position = Vector3.Lerp(_start, _end, Hero.WalkCurve.Evaluate(t));

        if (_elapsed >= _duration)
        {
            Hero.transform.position = _end;
            Hero.SetCurrentHex(_targetHex);
            Hero.StateMachine.ChangeState(Hero.StateMachine.Idle);
        }
    }
}
