using UnityEngine;

namespace MagicSchool.Combat.Heroes
{
    // Pure damage/heal math, kept out of Stat and HeroStateMachineBlackBoard so both stay
    // data + accessors rather than calculators.
    internal static class CombatMath
    {
        // calculate final damage after mitigation
        // final damage = damage that use to to reduce the HP directly
        public static int DamageAfterMitigation(int rawDamage, int defense, float damageReductionPercent)
        {
            // Effective health pool formula: EHP = HP * (1 + DF / 100)
            float mitigated = rawDamage / (1f + defense / 100f);

            // damage reduction will reduce the damage by percentage 
            mitigated *= 1f - damageReductionPercent / 100f;
            
            return Mathf.RoundToInt(mitigated);
        }

        // calculate final heal
        // final heal = amount of HP increase directly
        public static int HealAfterMitigation(float amount, bool isWounded)
        {
            // wound reduce healing by 50%
            float healed = isWounded ? amount * 0.5f : amount;
            return Mathf.RoundToInt(healed);
        }
    }
}
