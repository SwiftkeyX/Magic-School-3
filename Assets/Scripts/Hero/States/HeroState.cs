// Base class for one behavior mode of a Hero (Idle, Walk, Attack, Dead). Each state decides
// its own exit condition and hands off to the next state itself via
// Hero.StateMachine.ChangeState(...), rather than a central dispatcher polling every hero's
// conditions each frame - that dispatcher is exactly the if-else pile this pattern replaces.
public abstract class HeroState
{
    protected readonly Hero _me;
    protected readonly SkillSO _skill;
    protected SkillStep _currentStep;

    protected HeroState(Hero hero, SkillSO skill)
    {
        _me = hero;
        _skill = skill;
        // Some heroes (e.g. generic dummy/tank archetypes) have no SkillSO assigned - they just never cast.
        _currentStep = (skill != null && skill.Steps.Count > 0) ? skill.Steps[0] : null;
    }

    public abstract HeroStateType StateType { get; }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void OnUpdate();

    protected abstract void CheckSwitchState();
}
