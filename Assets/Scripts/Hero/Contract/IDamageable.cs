namespace MagicSchool
{
    // What a SkillEffect needs: damage, heal, and modifier.
    // HasStatus is what a SkillCondition asks about - "is this one transformed / wounded / stunned".
    public interface IDamageable
    {
        void TakeDamage(int damage);
        void Heal(float amount);
        void AddModifier(IModifier modifier);
        bool HasStatus(ModifierEnum status);

        bool IsAlive { get; }
    }
}
