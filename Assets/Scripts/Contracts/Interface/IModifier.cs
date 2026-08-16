namespace MagicSchool.Contracts
{
    public interface IModifier
    {
        public float GetBonusAmount(IHeroStats stats);
        public ModifierEnum GetModifierEnum();
        public ScalingEnum GetScalingEnum();
    }
}
