using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // The actual modifier class 
    // To contain modifier type and its amount in 1 place.
    public class ModifierSpec : IModifier
    {
        private readonly ModifierEnum _modifier;
        private readonly ScalingEnum _scalingType;
        private readonly float _amount;      // the amount written straight in, when there is no ratio
        private readonly Scaling _scaling;   // how to derive the amount instead, when there is

        // e.g. buff Atk by a written +50            => (Attack, Flat, amount: 50f)
        // e.g. buff Atk by 50% of the caster's AP   => (Attack, Percentage, ratios: { (MG, 50f) })
        public ModifierSpec(ModifierEnum modifier, ScalingEnum scalingType, float amount = 0f,
                            IReadOnlyList<StatRatio> ratios = null)
        {
            _modifier = modifier;
            _scalingType = scalingType;
            _amount = amount;
            _scaling = (ratios == null || ratios.Count == 0) ? null : new Scaling(scalingType, ratios);
        }

        public ModifierEnum GetModifierEnum() => _modifier;
        public ScalingEnum GetScalingEnum() => _scalingType;

        public float GetBonusAmount(IHeroStats stats)
            => _scaling == null ? _amount : _scaling.GetTotalAfterScaling(stats);
    }
}
