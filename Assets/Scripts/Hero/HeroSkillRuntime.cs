namespace MagicSchool
{
    public class HeroSkillRuntime
    {
        private readonly Hero _me;
        private readonly SkillSO _skill;
        private readonly SkillTrigger _skillTrigger = new SkillTrigger();

        // Some heroes (e.g. generic dummy/tank archetypes) have no SkillSO assigned.
        public bool HasSkill => _skill != null && _skill.Steps.Count > 0;

        public HeroSkillRuntime(Hero hero, SkillSO skill)
        {
            _me = hero;
            _skill = skill;
        }

        // Returns true if the skill cast successfully
        public bool TriggerSkill(bool isManaCapped)
        {
            if (!HasSkill) return false;
            if (_skill.Steps[0].Trigger != TriggerEnum.OnCast) return false;

            return _skillTrigger.OnCast(isManaCapped, _skill,  _me);
        }
    }
}
