// Owns exactly one instance of each state for its hero. States carry per-hero mutable
// data (HeroIdle's grace-period timer, HeroWalk's walk progress), so they can't be shared
// stateless singletons across every hero on the board the way the state TYPE can be.
public class HeroStateMachine
{
    public HeroIdle Idle { get; }
    public HeroWalk Walk { get; }
    public HeroAttack Attack { get; }
    public HeroDead Dead { get; }

    public HeroState Current { get; private set; }
    public HeroStateType CurrentType => Current.StateType;

    public HeroStateMachine(Hero hero)
    {
        Idle = new HeroIdle(hero);
        Walk = new HeroWalk(hero);
        Attack = new HeroAttack(hero);
        Dead = new HeroDead(hero);
    }

    public void Start(HeroState initial)
    {
        Current = initial;
        Current.OnEnter();
    }

    public void ChangeState(HeroState next)
    {
        if (Current == next) return;
        Current?.OnExit();
        Current = next;
        Current.OnEnter();
    }

    public void Update() => Current?.OnUpdate();
}
