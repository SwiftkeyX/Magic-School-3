namespace MagicSchool.Skills
{
    // ActionSourceEnum/AimTargetEnum are serialized into SkillActionGroup (inside SkillSO assets) as
    // raw ints - always assign explicit values so inserting a new member later can't silently remap
    // what an existing asset's stored int means.
    public enum ActionSourceEnum
    {
        Self = 0,
        Ally = 1,
        Summon = 2,
        WhereProjectileHit = 3,     // spawn where the previous step's projectile landed
        Current = 4,
        // ...
    }

    public enum AimTargetEnum
    {
        Self = 0,
        Current = 1,
        Furthest = 2,
        Clustered = 3,
        WhereProjectileHit = 4,

        // ...
    }
}
