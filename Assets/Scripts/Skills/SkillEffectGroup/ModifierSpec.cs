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
        private readonly Scaling _scaling;

        // e.g. buff Atk by 50% of the caster's AP  => (Attack, Percentage, { (MG, 50f) })
        // e.g. a flat 25% damage reduction         => (DamageReduction, Percentage, { (None, 25f) })
        // The two compose: { (None, 20f), (MG, 20f) } is "20 flat, plus 20% AP on top".
        public ModifierSpec(ModifierEnum modifier, ScalingEnum scalingType, IReadOnlyList<StatRatio> ratios)
        {
            _modifier = modifier;
            _scalingType = scalingType;
            _scaling = new Scaling(scalingType, ratios);
        }

        // FIXLATER: Make a dictionary that list which one is a status, which one isn't
        // If this modifier is status, there is no StatRatio. 
        public ModifierSpec(ModifierEnum modifier)
            : this(modifier, ScalingEnum.Percentage, null) { }

        public ModifierEnum GetModifierEnum() => _modifier;
        public ScalingEnum GetScalingEnum() => _scalingType;

        // return a pure bonus stat from this modifier 
        // e.g. this modifier grant +100 ATK
        public float GetBonusAmount(IHeroStats stats) => _scaling.GetTotalAfterScaling(stats);
    }
}
