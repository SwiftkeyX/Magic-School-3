namespace MagicSchool
{
    // What a template action needs to aim: ask the caster who to point at.
    public interface ITargeter
    {
        ICombatant FindNearestEnemy();
        ICombatant FindFurthestEnemy();
        ICombatant FindClusteredEnemy(int radius = 2);
    }
}
