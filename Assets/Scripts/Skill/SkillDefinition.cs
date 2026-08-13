using System;
using System.Collections.Generic;

namespace MagicSchool
{
    /// <summary>
    /// A whole container for 1 skill.
    /// SkillDefinition contain list of SkillStep for both passive & active skill.
    /// skill are separated into step, those step are working together in order, to create a actual skill.
    /// </summary>
    public class SkillDefinition
    {
        public string SkillName { get; }

        // triggered by OnCast once mana caps
        public IReadOnlyList<SkillStep> ActiveSteps { get; }

        // triggered by whatever the hero does (attack, combat start, ...)
        public IReadOnlyList<SkillStep> PassiveSteps { get; }

        // event to control skill behaviour e.g. combo counter, (no more usage yet) ...
        public event Action<TriggerEnum> Triggered;

        public SkillDefinition(string skillName, List<SkillStep> activeSteps = null, List<SkillStep> passiveSteps = null)
        {
            SkillName = skillName;
            ActiveSteps = activeSteps ?? new List<SkillStep>();
            PassiveSteps = passiveSteps ?? new List<SkillStep>();
        }

        // inject caster into class that need it.
        // E.g. SkillCondition
        public void Init(IEffectable caster)
        {
            foreach (SkillStep step in ActiveSteps) step.Init(caster);

            foreach (SkillStep step in PassiveSteps) step.Init(caster);
        }

        public void InvokeTrigger(TriggerEnum trigger) => Triggered?.Invoke(trigger);
    }
}
