
/// <summary>
/// HeroStateMachine is state machine that control hero's behaviour.
/// It's vanilla state machine, nothing special.
/// </summary>
public class HeroStateMachine
{
    private readonly HeroIdle _idle;
    private readonly HeroAttack _attack;
    private readonly HeroDead _dead;
    private readonly HeroWalk _walk;
    private readonly HeroStunned _stunned;

    public HeroState Current { get; private set; }
    public HeroStateType CurrentType => Current.StateType;

    public HeroStateMachine(Hero hero, HeroStateMachineBlackBoard blackboard, SkillSO skill, MovementConfig movement)
    {
        _idle = new HeroIdle(hero, blackboard, skill, movement);
        _walk = new HeroWalk(hero, blackboard, skill, movement);
        _attack = new HeroAttack(hero, blackboard, skill, movement);
        _dead = new HeroDead(hero, blackboard, skill);
        _stunned = new HeroStunned(hero, blackboard, skill);
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

    public void Tick() => Current?.OnUpdate();

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
