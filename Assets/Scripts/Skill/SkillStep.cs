using UnityEngine;
using System;
using System.Collections.Generic;

namespace MagicSchool
{
    [Serializable]
    public class SkillStep
    {
        [SerializeField] private TriggerEnum _trigger;
        [SerializeField] private List<SkillPassiveGroup> _passiveGroups;
        [SerializeField] private List<SkillActiveGroup> _activeGroups;

        // ================================== getter ==================================
        public TriggerEnum Trigger => _trigger;
        public IReadOnlyList<SkillActionGroup> ActionGroups => _activeGroups;

        // ================================== setter ==================================
        public SkillStep(TriggerEnum trigger, List<SkillActionGroup> actionGroups)
        {
            _trigger = trigger;
            _activeGroups = actionGroups;
        }
    }
}
