// Entered whenever a Status/Stun StatModifier is active (see Blackboard.IsStunned()). Does
// nothing on OnUpdate, same shape as HeroDead, but it isn't terminal - the modifier's own
// Remaining timer (ticked by Hero.Update()) expires it, and this state notices and leaves.
using UnityEngine;

public class HeroStunned : HeroState
{
    public override HeroStateType StateType => HeroStateType.Stunned;

    public HeroStunned(Hero hero) : base(hero) { }

    public override void OnExit()
    {
        // Snap back in case a stun landed mid-attack-dash, same reasoning as HeroAttack.OnExit.
        _me.transform.position = _me.Blackboard.GetCurrentHex().transform.position;
    }

    public override void OnUpdate()
    {
        CheckSwitchState();
    }

    protected override void CheckSwitchState()
    {
        bool isMeDead = (_me.Blackboard.GetCurrentHP() <= 0);
        if (isMeDead)
        {
            _me.StateMachine.ChangeState(HeroStateType.Dead);
            return;
        }

        if (!_me.Blackboard.IsStunned())
        {
            _me.StateMachine.ChangeState(HeroStateType.Idle);
        }
    }
}
