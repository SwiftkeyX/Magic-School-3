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
            ApplySelfEffects(caster, effects);
            SpawnAoeZone(caster, effects);
        }
    }

    // Self-targeted effects don't need a spatial check - apply them straight to the caster.
    private void ApplySelfEffects(Hero caster, List<SkillEffect> effects)
    {
        if (effects == null) return;

        foreach (SkillEffect effect in effects)
        {
            if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(new List<Hero> { caster });
        }
    }

    // EnemiesInArea effects are resolved by AoeZone via its trigger collider once the prefab spawns.
    private void SpawnAoeZone(Hero caster, List<SkillEffect> effects)
    {
        if (_effect == null) return;

        List<SkillEffect> areaEffects = effects?.Where(e => e.Recipient == EffectRecipientEnum.EnemiesInArea).ToList();
        if (areaEffects == null || areaEffects.Count == 0) return;

        GameObject instance = Object.Instantiate(_effect, _aimTargetPosition, Quaternion.identity);
        AoeZone zone = instance.GetComponent<AoeZone>();
        if (zone != null) zone.Init(caster, areaEffects);
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
