public abstract class HeroState
{
    protected readonly Hero _me;
    // Which step to try casting. The SkillSO itself lives on HeroSkillRuntime now - states
    // only need to know which step they're on, not to carry the whole skill around.
    protected SkillStep _currentStep;

    protected HeroState(Hero hero, SkillSO skill)
    {
        _me = hero;
        // Some heroes (e.g. generic dummy/tank archetypes) have no SkillSO assigned - they just never cast.
        _currentStep = (skill != null && skill.Steps.Count > 0) ? skill.Steps[0] : null;
    }

    public abstract HeroStateType StateType { get; }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void OnUpdate();

    protected abstract void CheckSwitchState();
}
