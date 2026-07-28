/// <summary>
/// Skil use A LOT OF enum. We group all of them here for readability.
/// </summary>

// ================================================ Trigger ================================================
public enum TriggerEnum
{
    OnCast,
    OnKill,
}

// ================================================ Recipient ================================================
public enum EffectRecipientEnum
{
    Self,
    EnemiesInArea,
}

// ================================================ Effect ================================================
// All effect in the enum here are "modifier" (There is some effect aren't modifier e.g. attack, which is don't included here)
public enum ModifierEnum
{
    // ======================================= Buff =======================================
    BonusHP,
    DamageReduction,

    // ======================================= Debuff =======================================
    // ...

    // ======================================= Status =======================================
    Stun,
    Wound,
}
