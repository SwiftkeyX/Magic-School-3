// Vocabulary for the modular skill system, distilled from the `tft-skill` sheet's schema
// (Step -> Trigger/Condition -> Action Source/Action -> Aim Target -> Effects). Each enum here
// is a fixed vocabulary the sheet itself validates Hero rows against - see
// Magic School 2\.claude\scripts\tft-set9-skill-modularity\data\*.csv for the source.

public enum SkillType { Active, Passive }

// The event that fires a Step's Action. Sheet's full trigger vocabulary (trigger-types.csv);
// only a subset is used by Phase 1 content, the rest are here so new kits don't need a new enum.
public enum TriggerType
{
    GameStart,
    OnCast,
    AfterStep1,
    AfterStep2,
    AfterStep3,
    OnAttack,
    OnKill,
    OnDeath,
    OnBonusHPExpire,
    WhileChanneling,
    WhenTransformed,
    OnCastExpire,
    OnEnemyCast,
    OnEnemyEntersRange,
    OnEnemyKnockedUp,
    OnProjectileHit,
    OnShieldExpire,
    On3rdAttack,
    On10thAttack,
    On3rdCast,
    OnAllyAttack,
}

// A state gate that must be true for the Step's Action to fire. Small set for now - grows one
// value per new kit that actually needs it, same as the sheet's own Condition column does.
public enum ConditionType
{
    None,
    IfTransformed,
    IfNotTransformed,
}

// WHO performs the Action. Defaults to Self for every Phase 1 kit; Summon/Ally/StepNProjectile
// exist so the enum doesn't need reshaping the moment a summon-based kit is ported.
public enum ActionSourceType
{
    Self,
    Summon,
    Ally,
    StepNProjectile,
}

// Key into ActionModel's fixed Apply/Spawn/Motion/Behavior/Shape/Collision axes - one name,
// one lookup (see action-model.csv). Full 23-action vocabulary even though Phase 1 only uses
// a handful, since the axes never vary per-hero and the whole point is not restating them.
public enum ActionKey
{
    AutoAttackRanged,
    HomingProjectile,
    BounceHomingProjectile,
    FirstHitProjectile,
    PierceProjectile,
    GilgameshProjectile,
    SpawnAtTarget,
    CircleAOE,
    ConeAOE,
    BoxAOE,
    CustomAOE,
    LaserShot,
    SweepLaser,
    Charge,
    BounceCharge,
    GrabAndSlam,
    KnockBack,
    ReceiveProjectile,
    Cast,
    Move,
    MoveBehind,
    StaticSummon,
    ChargeSummon,
    HeroSummon,
    CurrentTargetLaser,
    BonusOnAA,
    QuickAA,
    ZoneAOE,
}

// Who the Action aims at - ABSOLUTE, never relative to Action Source. Small set covering
// Phase 1's 8 champions (aim-target-types.csv has ~25 entries; add as new kits need them).
public enum AimTargetType
{
    Self,
    Current,
    CurrentNew,
    Clustered,
}

// Who the Effect lands on. "SameAsAimTarget" mirrors the sheet's own convention of writing
// that instead of restating the aim (see column-explain.csv row 13).
public enum EffectRecipientType
{
    SameAsAimTarget,
    Self,
    EnemiesInArea,
    AlliesInPath,
}

// Six categories, per effect-types.csv (not four - Movement and Summon are real).
public enum EffectCategory
{
    Attack,
    Status,
    Buff,
    Debuff,
    Movement,
    Summon,
}

public enum EffectDetail
{
    // Attack
    Damage,
    TrueDamage,
    // Status
    Stun,
    Wound,
    // Buff
    Heal,
    BonusHP,
    DamageReduction,
    AttackSpeed,
    // Debuff
    MRShred,
    DEFShred,
    // Movement
    Reposition,
    Setup,
}

// How the Amount behaves. Full scaling-types.csv vocabulary - documentation-sized, cheap to
// keep complete even though Phase 1 only exercises a handful.
public enum ScalingType
{
    None,
    Stacking,
    Decay,
    FalloffPerHit,
    PerStack,
    PerTick,
    Burst,
    ConditionalBonus,
    Derived,
    PerTargetHit,
    Cap,
}

// How often an Effect re-applies. Once = instant; Periodic = re-applies every `cadenceSeconds`
// for the Effect's Duration (Zone AOE ticks, Galio's channel, Lux's laser all share this).
public enum EffectCadence
{
    Once,
    Periodic,
}
