using System.Collections.Generic;

namespace MagicSchool
{
    public class SkillStep
    {
        private TriggerEnum _trigger;
        private List<SkillActionGroup> _actionGroups;

        // ================================== getter ==================================
        public TriggerEnum Trigger => _trigger;
        public IReadOnlyList<SkillActionGroup> ActionGroups => _actionGroups;

        // ================================== setter ==================================
        public SkillStep(TriggerEnum trigger, List<SkillActionGroup> actionGroups)
        {
            _trigger = trigger;
            _actionGroups = actionGroups;
        }
    }
}
