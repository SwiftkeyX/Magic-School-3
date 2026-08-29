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
        public ScalingSourceEnum GetScalingSource() => _scaling.GetScalingSource();

        // return what this modifier grants, split into what adds and what multiplies
        // e.g. this modifier grant +100 ATK, or *2 AS
        public ScalingTotals GetTotals(IHeroStats stats, StatEnum? landingOn) => _scaling.GetTotals(stats, landingOn);
    }
}
