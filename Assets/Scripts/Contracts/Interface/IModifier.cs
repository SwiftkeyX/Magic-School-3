namespace MagicSchool.Contracts
{
    public interface IModifier
    {
        public float GetAmount();
        public ModifierEnum GetModifierEnum();
        public ScalingEnum GetScalingEnum();
    }
}
