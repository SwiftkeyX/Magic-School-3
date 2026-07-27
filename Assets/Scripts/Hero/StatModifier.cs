// 1) One timed buff/debuff instance layered on top of a Stat's base values.
// 2) Amount's meaning depends on Detail (e.g. BonusHP/AttackSpeed/DamageReduction add, Stun/Wound just need to be
// present - Amount is unused for those).
// 3) Remaining counts down in TickModifiers;
// 4) float.PositiveInfinity means "lasts until something else removes it" (the sheet's "Permanent").
public class StatModifier
{
    public readonly EffectDetail Detail;
    public readonly float Amount;
    public float Remaining;

    public StatModifier(EffectDetail detail, float amount, float durationSeconds)
    {
        Detail = detail;
        Amount = amount;
        Remaining = durationSeconds <= 0f ? float.PositiveInfinity : durationSeconds;
    }
}
