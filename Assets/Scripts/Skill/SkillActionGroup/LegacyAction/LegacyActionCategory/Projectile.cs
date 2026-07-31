
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile was aim toward the "target", what determined which recipients get hit is based on the projectile type.
/// </summary>
public abstract class Projectile : LegacyAction
{
    // normally, destroy upon projectile impact
    // But here is also, fallback lifetime, in case something go wrong 
    private float _lifetime = 10f;
    private float _speed = 8f;

    protected Vector3 _source;
    protected Vector3 _aimTarget;


    // ======================================== override method ========================================
    // source = where projectile spawn from
    protected override void ResolveSource(ActionSourceEnum source)
    {
        // spawn projectile at self
        if (source == ActionSourceEnum.Self)
        {
            _source = _me.transform.position;
        }

        // else if ...
    }


    // aim = where projectile shoot direction is
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
            _aimTarget = target.transform.position;
        }

        // else if () ...

        // fallback
        else _aimTarget = _me.transform.position;
    }

    protected override void SpawnPrefab(Hero caster, List<SkillEffect> effects)
    {
        SpawnInstanceAt(_source, caster, effects);
    }
}