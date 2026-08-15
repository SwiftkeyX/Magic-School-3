using MagicSchool.Contracts;

namespace MagicSchool.Skills
{
    // The actual modifier class 
    // To contain modifier type, its amount, its duration in 1 place
    // FLAGGING: We use to need IModifier, but it don't neccessary anymore, since ModifierSpec is the only user. 
    // Let leave it for now though, maybe it'll see more used?.
    public class ModifierSpec : IModifier
    {
        private ModifierEnum _modifier;
        private StatScale _scaling;
        private float _duration;

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

        // FIXLATER: The scaling should be resolve in ModifierResolver.
        // // basically scale the modifier's value up 
        // public IModifier Scaled(float multiplier)
        // {
        //     // scale amount base on multiplier
        //     _scaling.Amount *= multiplier;

        //     // return self
        //     return this;
        // }
    }

    // Scale the modifier's amount up base on which stat it scale on  
    public class StatScale
    {
        public ScalingEnum ScalingType; // Is the scaling Flat or Percentage?
        public float Amount;
        public StatScale(ScalingEnum scalingType, float amount)
        {
            ScalingType = scalingType;
            Amount = amount;
        }
    }
}