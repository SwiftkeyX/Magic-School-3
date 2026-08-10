namespace MagicSchool
{
    /// <summary>
    /// Skil use A LOT OF enum. We group all of them here for readability.
    /// </summary>

    // These enums are serialized into SkillSO assets as raw ints - always assign explicit values so
    // inserting a new member later can't silently remap what an existing asset's stored int means.

    // ================================================ Trigger ================================================
    public enum TriggerEnum
    {
        OnCast = 0,
        OnKill = 1,
        OnExpired = 2,      // once the previous step expired
        OnHit = 3,          // once the previous step hit someone 
    }

    // ================================================ Recipient ================================================
    public enum EffectRecipientEnum
    {
        Self = 0,
        EnemiesInArea = 1,
        SameToAimTarget = 2,
        EnemiesInPath = 3,
    }

    // ================================================ Effect ================================================
    public enum ModifierEnum
    {
        // ======================================= Buff =======================================
        BonusHP = 0,
        DamageReduction = 2,
        AttackSpeed = 5,
        Attack = 6,
        Defend = 7,
        AP = 8,

        // ======================================= Debuff =======================================
        // ...

        // ======================================= Status =======================================
        Stun = 3,
        Wound = 4,
    }
}
