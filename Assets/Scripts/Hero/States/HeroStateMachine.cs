// Owns exactly one instance of each state for its hero. States carry per-hero mutable
// data (HeroIdle's grace-period timer, HeroWalk's walk progress), so they can't be shared
// stateless singletons across every hero on the board the way the state TYPE can be.
public class HeroStateMachine
{
    private readonly HeroIdle _idle;
    private readonly HeroAttack _attack;
    private readonly HeroDead _dead;
    private readonly HeroWalk _walk;
    private readonly HeroStunned _stunned;

    public HeroState Current { get; private set; }
    public HeroStateType CurrentType => Current.StateType;

    public HeroStateMachine(Hero hero)
    {
        _idle = new HeroIdle(hero);
        _walk = new HeroWalk(hero);
        _attack = new HeroAttack(hero);
        _dead = new HeroDead(hero);
        _stunned = new HeroStunned(hero);
    }

    public void Start(HeroStateType initial)
    {
        Current = GetState(initial);
        Current.OnEnter();
    }

    public void ChangeState(HeroStateType next)
    {
        if (Current != null && next == CurrentType) return;

        Current?.OnExit();
        Current = GetState(next);
        Current.OnEnter();
    }

    public void Update() => Current?.OnUpdate();

    private HeroState GetState(HeroStateType type)
    {
        switch (type)
        {
            case HeroStateType.Idle: return _idle;
            case HeroStateType.Walk: return _walk;
            case HeroStateType.Attack: return _attack;
            case HeroStateType.Dead: return _dead;
            case HeroStateType.Stunned: return _stunned;
            default: return null;
        }
    }
}
