using UnityEngine;

namespace MagicSchool.Combat.Heroes
{
    // Pure damage/heal math, kept out of Stat and HeroStateMachineBlackBoard so both stay
    // data + accessors rather than calculators.
    internal static class CombatMath
    {
        // EHP = HP * (1 + DF / 100) -> raw damage is worth less HP the more DF you have,
        // so divide by that same factor to get how much HP the hit actually removes.
        // A skill's Damage Reduction buff shaves a further percentage off, after armor mitigation.
        private static int CalculateDamage(int rawDamage, int defense, float damageReductionPercent)
        {
            float mitigated = rawDamage / (1f + defense / 100f);
            mitigated *= 1f - damageReductionPercent / 100f;
            return Mathf.RoundToInt(mitigated);
        }

        // Wound halves incoming healing (simplified from "reduce healing" per effect-types.csv,
        // since this game has no finer-grained heal-reduction percentage authored yet).
        private static int CalculateHeal(float amount, bool isWounded)
        {
            float healed = isWounded ? amount * 0.5f : amount;
            return Mathf.RoundToInt(healed);
        }

        public static int Heal(float amount, int currentHP, bool isWounded)
        {
            int healed = CalculateHeal(amount, isWounded);
            return currentHP + healed;
        }

        public static int TakeDamage(int damage, int defend, float dmgReduction, int currentHP)
        {
            int mitigatedDamage = CalculateDamage(damage, defend, dmgReduction);
            int newHP = Mathf.Max(0, currentHP - mitigatedDamage);
            return newHP;
        }
    }
}
