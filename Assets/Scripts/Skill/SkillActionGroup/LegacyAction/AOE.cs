using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AOE type for LegacyAction
/// </summary>
public abstract class AOE : LegacyAction
{
    protected Vector3 _source;
    protected Vector3 _aimTarget;

    // ======================================= protected =======================================
    /// <summary>
    /// source = 100% of the time doesn't mean anything in the spawn/aim term. BUT still important it tell us "who use this AOE"
    /// aim = where the AOE spawn
    /// </summary>
    protected override void ResolveSource(ActionSourceEnum source)
    {
        _source = _me.transform.position;
    }

    protected override void ResolveAimTarget(AimTargetEnum aimTarget)
    {
        // aim skill at self
        if (aimTarget == AimTargetEnum.Self)
        {
            _aimTarget = _me.transform.position;
        }

        // aim skill at current target
        else if (aimTarget == AimTargetEnum.Current)
        {
            Hero target = _me.Blackboard.FindNearestEnemy();
            _aimTarget = (target != null) ? target.transform.position : _me.transform.position;
        }

        // else if () ...

        // fallback
        else _aimTarget = _me.transform.position;
    }

    protected override void SpawnPrefab(Hero caster, List<SkillEffect> effects)
    {
        SpawnInstanceAt(_aimTarget, caster, effects);
    }
}