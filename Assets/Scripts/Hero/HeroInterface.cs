using UnityEngine;

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

// contract for hero stat.
public interface IHeroStats
{
    int CurrentHP { get; }
    int MaxHP { get; }
    int CurrentMana { get; }
    int MaxMana { get; }
    int AttackDamage { get; }
    float AttackSpeed { get; }
    int Range { get; }
}

// contract for moving hero on the board e.g. when we move hero on our own
public interface IPlaceable
{
    Transform transform { get; }

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
    Hero FindClusteredEnemy(int radius = 2);
}
