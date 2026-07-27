using System;
using System.Collections.Generic;
using UnityEngine;

// Authored skill data - one asset per hero. A skill is an ordered list of Steps (mirrors the sheet's Step column); 
// each Step fires one Action when its Trigger/Condition are met and applies one or more Effects. 
// See the plan's schema mapping for the sheet columns this covers.
[CreateAssetMenu(fileName = "Skill", menuName = "Magic School 3/Skill Definition")]
public class SkillDefinitionSO : ScriptableObject
{
    [SerializeField] private string _skillName = "Skill";
    [SerializeField] private List<SkillStep> _steps = new List<SkillStep>();

    public string SkillName => _skillName;
    public IReadOnlyList<SkillStep> Steps => _steps;

    // Authoring-time only (editor tooling that builds these assets from the sheet's champion
    // data) - the Inspector already edits _steps directly via SerializedProperty regardless.
    public void SetSteps(List<SkillStep> steps) => _steps = steps;
}

/// <summary>
/// SkillStep = step in google sheet. 
/// </summary>
[Serializable]
public class SkillStep
{
    [SerializeField] private int _step;
    [SerializeField] private SkillType _skillType;
    [SerializeField] private TriggerType _trigger;
    [SerializeField] private ConditionType _condition = ConditionType.None;
    [SerializeField] private ActionSourceType _actionSource = ActionSourceType.Self;
    [SerializeField] private ActionKey _action;
    [SerializeField] private AimTargetType _aimTarget;
    // Radius in hexes, for AOE actions only (Circle/Zone AOE). Ignored by non-AOE actions.
    [SerializeField] private int _aoeRadius;
    // The aim target must be within this range (hexes) for the step to fire.
    [SerializeField] private int _skillRange;
    [SerializeField] private float _castSeconds;
    [SerializeField] private List<SkillEffect> _effects = new List<SkillEffect>();

    public int Step => _step;
    public SkillType SkillType => _skillType;
    public TriggerType Trigger => _trigger;
    public ConditionType Condition => _condition;
    public ActionSourceType ActionSource => _actionSource;
    public ActionKey Action => _action;
    public AimTargetType AimTarget => _aimTarget;
    public int AoeRadius => _aoeRadius;
    public int SkillRange => _skillRange;
    public float CastSeconds => _castSeconds;
    public IReadOnlyList<SkillEffect> Effects => _effects;

    // Authoring-time only - see SkillDefinitionSO.SetSteps.
    public SkillStep(int step, SkillType skillType, TriggerType trigger, ConditionType condition,
        ActionSourceType actionSource, ActionKey action, AimTargetType aimTarget,
        int aoeRadius, int skillRange, float castSeconds, List<SkillEffect> effects)
    {
        _step = step;
        _skillType = skillType;
        _trigger = trigger;
        _condition = condition;
        _actionSource = actionSource;
        _action = action;
        _aimTarget = aimTarget;
        _aoeRadius = aoeRadius;
        _skillRange = skillRange;
        _castSeconds = castSeconds;
        _effects = effects;
    }
}

/// <summary>
/// SkillEffect = Effect in google sheet. 
/// </summary>
[Serializable]
public class SkillEffect
{
    [SerializeField] private EffectRecipientType _recipient;
    [SerializeField] private EffectCategory _category;
    [SerializeField] private EffectDetail _detail;
    // Flat authored value - the sheet's per-star Amount (e.g. "160/240/360% AP") collapses to
    // one number here since this game has no star/level system. Retune on the asset directly.
    [SerializeField] private float _amount;
    [SerializeField] private ScalingType _scaling = ScalingType.None;
    [SerializeField] private EffectCadence _cadence = EffectCadence.Once;
    // Tick interval in seconds, only meaningful when Cadence == Periodic.
    [SerializeField] private float _cadenceSeconds;
    // How long the Effect lasts, in seconds. 0 = instant, nothing to expire.
    [SerializeField] private float _duration;

    public EffectRecipientType Recipient => _recipient;
    public EffectCategory Category => _category;
    public EffectDetail Detail => _detail;
    public float Amount => _amount;
    public ScalingType Scaling => _scaling;
    public EffectCadence Cadence => _cadence;
    public float CadenceSeconds => _cadenceSeconds;
    public float Duration => _duration;

    // Authoring-time only - see SkillDefinitionSO.SetSteps.
    public SkillEffect(EffectRecipientType recipient, EffectCategory category, EffectDetail detail,
        float amount, ScalingType scaling, EffectCadence cadence, float cadenceSeconds, float duration)
    {
        _recipient = recipient;
        _category = category;
        _detail = detail;
        _amount = amount;
        _scaling = scaling;
        _cadence = cadence;
        _cadenceSeconds = cadenceSeconds;
        _duration = duration;
    }
}
