using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ZoneAOE, CircleAOE, etc... are all legacy action.
/// later LegacyAction should be interface instead.
/// </summary>
[CreateAssetMenu(fileName = "LegacyAction", menuName = "Magic School 3/Legacy Action")]
public class LegacyAction : ScriptableObject
{
    [SerializeField] private LegacyActionEnum _actionName;
    [SerializeField] private GameObject _effect;    // the prefab for the skill
    [SerializeField] private float _castTime;       // the duration skill was cast

    // ==================================== public method ====================================
    // Hero that this would want is also Hero currentTarget, Hero furthestTarget and etc...
    // that'll be resolve later
    public void TriggerSkill(AimTarget aimTarget, Hero caster, List<SkillEffect> effects)
    {
        // find position using aim target
        Vector3 aimTargetPosition = ResolveAimTarget(aimTarget, caster);

        // play animation based on legacy action
        PlayLegacyAction(caster, effects, aimTargetPosition);
    }

    // ==================================== LegacyAction ====================================
    private void PlayLegacyAction(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        if (_actionName == LegacyActionEnum.ZoneAOE)
        {
            SpawnZoneAOE(caster, effects, aimTargetPosition);
        }

        else if (_actionName == LegacyActionEnum.CircleAOE)
        {
            SpawnCircleAOE(caster, effects, aimTargetPosition);
        }
    }

    private void SpawnZoneAOE(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        if (_effect == null) return;

        GameObject instance = Object.Instantiate(_effect, aimTargetPosition, Quaternion.identity);
        // assign prefab data at runtime
        ZoneAOE zone = instance.GetComponent<ZoneAOE>();
        if (zone != null) zone.Init(caster, effects, _castTime);
    }

    private void SpawnCircleAOE(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        if (_effect == null) return;

        GameObject instance = Object.Instantiate(_effect, aimTargetPosition, Quaternion.identity);
        CircleAOE circle = instance.GetComponent<CircleAOE>();
        if (circle != null) circle.Init(caster, effects, _castTime);
    }

    // ==================================== etc ====================================
    private Vector3 ResolveAimTarget(AimTarget aimTarget, Hero caster)
    {
        // find position using aim target
        if (aimTarget == AimTarget.Self)
        {
            return caster.transform.position;
        }

        return Vector3.zero;
    }
}
