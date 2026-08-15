using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // The actual modifier class 
    // To contain modifier type, its amount, its duration in 1 place
    // FLAGGING: We use to need IModifier, but it don't neccessary anymore, since ModifierSpec is the only user. 
    // Let leave it for now though, maybe it'll see more used?.
    public class ModifierSpec : IModifier
    {
        private readonly ModifierEnum _modifier;
        private readonly StatScale _scaling;
        private readonly float _duration;

        public ModifierSpec(ModifierEnum modifier, ScalingEnum scalingType, float amount, float duration)
        {
            _modifier = modifier;
            _scaling = new StatScale(scalingType, amount);
            _duration = duration;
        }

        public ModifierEnum GetModifierEnum() => _modifier;
        public float GetAmount() => _scaling.Amount;
        public float GetDuration() => _duration;
        public ScalingEnum GetScalingEnum() => _scaling.ScalingType;

        // make copy of this instace with scaling
        public ModifierSpec WithAmount(float amount) =>
            new ModifierSpec(_modifier, _scaling.ScalingType, amount, _duration);
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
