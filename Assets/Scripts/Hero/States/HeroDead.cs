public class HeroDead : HeroState
{
    public override HeroStateType StateType => HeroStateType.Dead;

    public HeroDead(Hero hero, SkillSO skill) : base(hero, skill) { }

    public override void OnEnter()
    {
        _me.SetDeadVisual();
    }

    public override void OnUpdate() { }

    protected override void CheckSwitchState() { }
}
