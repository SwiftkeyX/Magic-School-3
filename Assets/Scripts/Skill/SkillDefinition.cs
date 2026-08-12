using System.Collections.Generic;

namespace MagicSchool
{
    /// <summary>
    /// A whole container for 1 skill.
    /// </summary>
    public class SkillDefinition
    {
        public string SkillName { get; }

        // triggered by OnCast once mana caps
        public IReadOnlyList<SkillStep> ActiveSteps { get; }

        // triggered by whatever the hero does (attack, combat start, ...)
        public IReadOnlyList<SkillStep> PassiveSteps { get; }

        public SkillDefinition(string skillName, List<SkillStep> activeSteps = null, List<SkillStep> passiveSteps = null)
        {
            SkillName = skillName;
            ActiveSteps = activeSteps ?? new List<SkillStep>();
            PassiveSteps = passiveSteps ?? new List<SkillStep>();
        }
    }
}
