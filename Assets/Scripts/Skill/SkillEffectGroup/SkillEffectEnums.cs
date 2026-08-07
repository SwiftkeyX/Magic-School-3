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
        OnExpired = 2,
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
    // All effect in the enum here are "modifier" (There is some effect aren't modifier e.g. attack, which is don't included here)
    public enum ModifierEnum
    {
        // ======================================= Buff =======================================
        BonusHP = 0,
        Heal = 1,
        DamageReduction = 2,

        // ======================================= Debuff =======================================
        // ...

        // ======================================= Status =======================================
        Stun = 3,
        Wound = 4,
    }
}
