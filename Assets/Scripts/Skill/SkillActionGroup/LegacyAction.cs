using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LegacyAction
{
    [SerializeField] private LegacyActionEnum _actionName;
    [SerializeField] private GameObject _effect;
    private Vector3 _aimTargetPosition;

    // Hero that this would want is also Hero currentTarget, Hero furthestTarget and etc...
    // that'll be resolve later
    public void PlayLegacyAction(AimTarget aimTarget, Hero caster, List<SkillEffect> effects)
    {
        // find position using aim target
        ResolveAimTarget(aimTarget, caster);

        // play animation based on legacy action
        if (_actionName == LegacyActionEnum.ZoneAOE)
        {
            SpawnAoeZone(caster, effects);
        }
    }

    private void SpawnAoeZone(Hero caster, List<SkillEffect> effects)
    {
        if (_effect == null) return;

        GameObject instance = Object.Instantiate(_effect, _aimTargetPosition, Quaternion.identity);
        // assign prefab data at runtime
        AoeZone zone = instance.GetComponent<AoeZone>(); 
        if (zone != null) zone.Init(caster, effects);
    }

    private void ResolveAimTarget(AimTarget aimTarget, Hero caster)
    {
        // find position using aim target
        if (aimTarget == AimTarget.Self)
        {
            _aimTargetPosition = caster.transform.position;
        }
    }
}
