namespace MagicSchool.Contracts
{
    // ITargeter answer: Which enemy is the target base on specify condition.
    // e.g. skill want to aim at the Nearest/furthest enemy
    public interface ITargeter
    {
        ICombatant CurrentTarget { get; }

        ICombatant FindNearestEnemy();
        ICombatant FindFurthestEnemy();
        ICombatant FindClusteredEnemy(int radius = 2);
    }
}
