/// <summary>
/// The narrow contracts other systems use to talk to a Hero, so they don't have to reach
/// through Hero.Blackboard and see all forty of its members to use four of them.
///
/// Hero implements these itself (its whole job is being glue), which means the skill system
/// and the UI depend on a handful of methods that describe what they need - not on the
/// internal shape of the hero's blackboard.
/// </summary>

// What a skill effect needs: something it can damage, heal, or attach a modifier to.
// Deliberately says nothing about hexes, teams, or state machines.
public interface IDamageable
{
    void TakeDamage(int damage);
    void Heal(float amount);
    void AddModifier(Modifier modifier);

    // Checked instead of a `== null` test at the call site: once a recipient is typed as this
    // interface, `==` is plain reference equality and would MISS a destroyed GameObject,
    // because Unity's fake-null only applies when the static type is a UnityEngine.Object.
    bool IsAlive { get; }
}

// What the world-space bars need: current-over-max, nothing else.
public interface IStatReadout
{
    int CurrentHP { get; }
    int MaxHP { get; }
    int CurrentMana { get; }
    int MaxMana { get; }
}
