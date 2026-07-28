using UnityEngine;
using System;
using System.Collections.Generic;


[Serializable]
public class SkillStep
{
    [SerializeField] private TriggerEnum _trigger;
    [SerializeField] private List<SkillActionGroup> _actionGroups;

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
