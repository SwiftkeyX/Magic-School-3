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

// Where a hero is standing. Used by Hex/BenchSlot when placing.
public interface IPlaceable
{
    Hex CurrentHex { get; }
    Hex ReservedHex { get; }
    Placement CurrentPlacement { get; }
    bool IsInCombat { get; }        // false when standing somewhere that isn't a Hex, e.g. the bench

    void SetReservedHex(Hex hex);
    void SetCurrentPlacement(Placement placement);
}

// What a skill action needs to aim: ask the caster who to point at.
public interface ITargeter
{
    Hero FindNearestEnemy();
    Hero FindFurthestEnemy();
}
