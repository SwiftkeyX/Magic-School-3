// Modifier types a StatModifier can carry - the buffs/debuffs/status combat code applies and
// queries directly (see Stat/ModifierList). The skill system that used to author these is being
// rebuilt; add values back here only once an active StatModifier actually needs them again.
public enum EffectDetail
{
    Stun,
    Wound,
    BonusHP,
    DamageReduction,
    AttackSpeed,
    MRShred,
    DEFShred,
}
