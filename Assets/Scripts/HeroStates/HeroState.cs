// Base class for one behavior mode of a Hero (Idle, Walk, Attack, Dead). Each state decides
// its own exit condition and hands off to the next state itself via
// Hero.StateMachine.ChangeState(...), rather than a central dispatcher polling every hero's
// conditions each frame - that dispatcher is exactly the if-else pile this pattern replaces.
public abstract class HeroState
{
    protected readonly Hero Hero;

    protected HeroState(Hero hero)
    {
        Hero = hero;
    }

    public abstract HeroStateType StateType { get; }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void OnUpdate();
}
