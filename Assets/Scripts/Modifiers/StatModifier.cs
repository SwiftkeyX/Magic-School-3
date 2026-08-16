using MagicSchool.Contracts;

namespace MagicSchool.Modifiers
{
    // The actual modifier class 
    // To contain modifier type and its amount in 1 place.
    public class StatModifier : IModifier
    {
        private readonly ModifierEnum _modifier;
        private readonly IScaling _scaling;

        public StatModifier(ModifierEnum modifier, IScaling scaling)
        {
            _modifier = modifier;
            _scaling = scaling;
        }

        public ModifierEnum GetModifierEnum() => _modifier;
        public ScalingEnum GetScalingEnum() => _scaling.GetScalingEnum();

        // return a pure bonus stat from this modifier 
        // e.g. this modifier grant +100 ATK
        public float GetBonusAmount(IHeroStats stats) => _scaling.GetTotalAfterScaling(stats);
    }
}
