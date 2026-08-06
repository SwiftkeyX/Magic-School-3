public class HeroSkillRuntime
{
    private readonly Hero _me;
    private readonly SkillSO _skill;
    private readonly SkillTrigger _skillTrigger = new SkillTrigger();

    // Some heroes (e.g. generic dummy/tank archetypes) have no SkillSO assigned.
    public bool HasSkill => _skill != null;
    public SkillSO Skill => _skill;

    public HeroSkillRuntime(Hero hero, SkillSO skill)
    {
        _me = hero;
        _skill = skill;
    }

    // Returns true if the skill cast successfully
    public bool TriggerSkill(SkillStep currentStep, bool isManaCapped)
    {
        if (!HasSkill || currentStep == null) return false;
        if (currentStep.Trigger != TriggerEnum.OnCast) return false;

        return _skillTrigger.OnCast(isManaCapped, _skill, currentStep, _me);
    }
}
