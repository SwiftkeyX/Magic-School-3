using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// The contracts other systems use to talk to a Hero
    /// </summary>

    // What a SkillEffect needs: damage, heal, and modifier.
    public interface IDamageable
    {
        void TakeDamage(int damage);
        void Heal(float amount);
        void AddModifier(IModifier modifier);
        bool HasStatus(ModifierEnum status);

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

    // What a template action needs to aim: ask the caster who to point at.
    public interface ITargeter
    {
        ICombatant FindNearestEnemy();
        ICombatant FindFurthestEnemy();
        ICombatant FindClusteredEnemy(int radius = 2);
    }

    // Contract for BattleBoard to talk to Hero
    // BattleBoard need to answer 1 thing "What is all the hero on the board"
    // So it need talk to only talk to "some part" of the Hero via this contract.
    public interface ICombatant : IPlaceable, IDamageable
    {
        TeamEnum Team { get; }
        HeroStateEnum StateType { get; }

        // A unit should knows which board it's on
        void TrackOnBoard();
        void UntrackFromBoard();
    }
}
