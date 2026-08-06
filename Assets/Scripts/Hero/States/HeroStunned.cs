using UnityEngine;

public class HeroStunned : HeroState
{
    public override HeroStateType StateType => HeroStateType.Stunned;

    public HeroStunned(Hero hero, SkillSO skill) : base(hero, skill) { }

    public override void OnExit()
    {
        // Snap back in case a stun landed mid-attack-dash, same reasoning as HeroAttack.OnExit.
        _me.transform.position = _me.CurrentHex.transform.position;
    }

    public override void OnUpdate()
    {
        CheckSwitchState();
    }

    protected override void CheckSwitchState()
    {
        bool isMeDead = (_me.CurrentHP <= 0);
        if (isMeDead)
        {
            _me.StateMachine.ChangeState(HeroStateType.Dead);
            return;
        }

        if (!_me.IsStunned)
        {
            _me.StateMachine.ChangeState(HeroStateType.Idle);
        }
    }
}
