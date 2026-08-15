using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // The actual modifier class 
    // To contain modifier type and its amount in 1 place.
    public class ModifierSpec : IModifier
    {
        private readonly ModifierEnum _modifier;
        private readonly StatScale _scaling;

        public ModifierSpec(ModifierEnum modifier, ScalingEnum scalingType, float amount)
        {
            _modifier = modifier;
            _scaling = new StatScale(scalingType, amount);
        }

        public ModifierEnum GetModifierEnum() => _modifier;
        public float GetAmount() => _scaling.Amount;
        public ScalingEnum GetScalingEnum() => _scaling.ScalingType;
    }

    // Specify scaling type the stat is using base on the modifier
    // e.g. Stat is increase by flat amount + 50
    // e.g. Stat is increase by percentage amount + 100 %  
    public readonly struct StatScale
    {
        public readonly ScalingEnum ScalingType; // Is the scaling Flat or Percentage?
        public readonly float Amount;

        public StatScale(ScalingEnum scalingType, float amount)
        {
            ScalingType = scalingType;
            Amount = amount;
        }
    }
}
