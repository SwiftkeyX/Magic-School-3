// Entered from Blackboard.TakeDamage() once HP hits 0. Dead heroes don't act - FindNearestEnemy and
// every hex-reservation check across the codebase already exclude State == Dead, so nothing
// else needs to know to leave this hero alone. Terminal state: nothing transitions out of it.
public class HeroDead : HeroState
{
    public override HeroStateType StateType => HeroStateType.Dead;

    public HeroDead(Hero hero) : base(hero) { }

    public override void OnEnter()
    {
        _me.Blackboard.Temp.SetDeadVisual();
    }

    public override void OnUpdate() { }

    protected override void CheckSwitchState() { }
}
