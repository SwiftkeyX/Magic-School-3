using System.Collections.Generic;

// Fixed per-action axes - Apply/Spawn/Collision never vary per-hero, they're a property of the
// action's NAME (see action-model.csv's own reasoning: these lived as per-row Hero columns for
// one day and moved out because every hero using an action agreed on them). SkillRuntime looks
// this up once per action execution instead of authoring it again on every SkillStep.

public enum ApplyType { DirectApply, Hitbox }
public enum SpawnType { None, AtUser, AtTarget }
public enum CollisionType { None, TargetOnly, Area, PierceAll, FirstHit, Self, FlankPair }

public readonly struct ActionModelEntry
{
    public readonly ApplyType Apply;
    public readonly SpawnType Spawn;
    public readonly CollisionType Collision;

    public ActionModelEntry(ApplyType apply, SpawnType spawn, CollisionType collision)
    {
        Apply = apply;
        Spawn = spawn;
        Collision = collision;
    }
}

public static class ActionModel
{
    private static readonly Dictionary<ActionKey, ActionModelEntry> _entries = new Dictionary<ActionKey, ActionModelEntry>
    {
        { ActionKey.AutoAttackRanged, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.TargetOnly) },
        { ActionKey.HomingProjectile, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.TargetOnly) },
        { ActionKey.BounceHomingProjectile, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.TargetOnly) },
        { ActionKey.FirstHitProjectile, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.FirstHit) },
        { ActionKey.PierceProjectile, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.PierceAll) },
        { ActionKey.GilgameshProjectile, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.PierceAll) },
        { ActionKey.SpawnAtTarget, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtTarget, CollisionType.TargetOnly) },
        { ActionKey.CircleAOE, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtTarget, CollisionType.Area) },
        { ActionKey.ConeAOE, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtTarget, CollisionType.Area) },
        { ActionKey.BoxAOE, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtTarget, CollisionType.Area) },
        { ActionKey.CustomAOE, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtTarget, CollisionType.Area) },
        { ActionKey.LaserShot, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.PierceAll) },
        { ActionKey.SweepLaser, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.PierceAll) },
        { ActionKey.Charge, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.PierceAll) },
        { ActionKey.BounceCharge, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.TargetOnly) },
        { ActionKey.GrabAndSlam, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.FlankPair) },
        { ActionKey.KnockBack, new ActionModelEntry(ApplyType.DirectApply, SpawnType.None, CollisionType.None) },
        { ActionKey.ReceiveProjectile, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.Self) },
        { ActionKey.Cast, new ActionModelEntry(ApplyType.DirectApply, SpawnType.None, CollisionType.None) },
        { ActionKey.Move, new ActionModelEntry(ApplyType.DirectApply, SpawnType.None, CollisionType.None) },
        { ActionKey.MoveBehind, new ActionModelEntry(ApplyType.DirectApply, SpawnType.None, CollisionType.None) },
        { ActionKey.StaticSummon, new ActionModelEntry(ApplyType.DirectApply, SpawnType.AtTarget, CollisionType.None) },
        { ActionKey.ChargeSummon, new ActionModelEntry(ApplyType.DirectApply, SpawnType.None, CollisionType.None) },
        { ActionKey.HeroSummon, new ActionModelEntry(ApplyType.DirectApply, SpawnType.AtTarget, CollisionType.None) },
        { ActionKey.CurrentTargetLaser, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.TargetOnly) },
        { ActionKey.BonusOnAA, new ActionModelEntry(ApplyType.DirectApply, SpawnType.None, CollisionType.None) },
        { ActionKey.QuickAA, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtUser, CollisionType.TargetOnly) },
        { ActionKey.ZoneAOE, new ActionModelEntry(ApplyType.Hitbox, SpawnType.AtTarget, CollisionType.Area) },
    };

    public static ActionModelEntry Get(ActionKey key) => _entries[key];
}
