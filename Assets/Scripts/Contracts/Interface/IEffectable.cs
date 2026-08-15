namespace MagicSchool.Contracts
{
    // What a SkillEffect needs: damage, heal, and modifier.
    // HasStatus is what a SkillCondition asks about - "is this one transformed / wounded / stunned".
    public interface IEffectable
    {
        void TakeDamage(int damage);
        void Heal(float amount);
        void AddModifier(ICustomModifier modifier, float amplifier);
        bool HasStatus(ModifierEnum status);

        bool IsAlive { get; }
    }
}
