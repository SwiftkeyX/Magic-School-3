using UnityEngine;
using System;
using System.Collections.Generic;


[Serializable]
public class SkillStep
{
    [SerializeField] private TriggerEnum _trigger;
    [SerializeField] private List<SkillEffect> _effects;

    // ================================== getter ==================================
    public TriggerEnum Trigger => _trigger;
    public IReadOnlyList<SkillEffect> Effects => _effects;

    // ================================== setter ==================================
    public SkillStep(TriggerEnum trigger, List<SkillEffect> effects)
    {
        _trigger = trigger;
        _effects = effects;
    }
}
