// Base class for one behavior mode of a Hero (Idle, Walk, Attack, Dead). Each state decides
// its own exit condition and hands off to the next state itself via
// Hero.StateMachine.ChangeState(...), rather than a central dispatcher polling every hero's
// conditions each frame - that dispatcher is exactly the if-else pile this pattern replaces.
public abstract class HeroState
{
    protected readonly Hero _me;

    protected HeroState(Hero hero)
    {
        _me = hero;
    }

    public abstract HeroStateType StateType { get; }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void OnUpdate();

    protected abstract void CheckSwitchState();
}
