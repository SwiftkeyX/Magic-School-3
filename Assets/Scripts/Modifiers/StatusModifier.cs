using MagicSchool.Contracts;

namespace MagicSchool.Modifiers
{
    public class StatusModifier : IModifier
    {
        private readonly ModifierEnum _status;

        public StatusModifier(ModifierEnum status)
        {
            _status = status;
        }

        public ModifierEnum GetModifierEnum() => _status;
        public ScalingEnum GetScalingEnum() => ScalingEnum.Percentage;
        public float GetBonusAmount(IHeroStats stats) => 0f;
    }
}
