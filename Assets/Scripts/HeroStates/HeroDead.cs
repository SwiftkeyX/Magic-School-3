// Placeholder for when real combat/death is implemented - dead heroes don't act. Nothing
// transitions into this state yet.
public class HeroDead : HeroState
{
    public override HeroStateType StateType => HeroStateType.Dead;

    public HeroDead(Hero hero) : base(hero) { }

    public override void OnUpdate() { }
}
