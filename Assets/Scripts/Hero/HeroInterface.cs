/// <summary>
/// The contracts other systems use to talk to a Hero
/// </summary>

// What a SkillEffect needs: damage, heal, and modifier.
public interface IDamageable
{
    void TakeDamage(int damage);
    void Heal(float amount);
    void AddModifier(Modifier modifier);

    bool IsAlive { get; }
}

// What the healthbar/ manabar needs.
public interface IStatReadout
{
    int CurrentHP { get; }
    int MaxHP { get; }
    int CurrentMana { get; }
    int MaxMana { get; }
}
