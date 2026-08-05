// ActionSourceEnum/AimTargetEnum are serialized into SkillActionGroup (inside SkillSO assets) as
// raw ints - always assign explicit values so inserting a new member later can't silently remap
// what an existing asset's stored int means.
public enum ActionSourceEnum
{
    Self = 0,
    Ally = 1,
    Summon = 2,

    // ...
}

public enum LegacyActionEnum
{
    // ====================== AOE ======================
    ZoneAOE,
    CircleAOE,

    // ====================== Laser ======================
    LaserShot,

    // ======================== Etc ======================
    Cast,

    // ...
}

public enum AimTargetEnum
{
    Self = 0,
    Current = 1,
    Furthest = 2,
    Clustered = 3,

    // ...
}