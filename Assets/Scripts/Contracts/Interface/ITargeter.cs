namespace MagicSchool.Contracts
{
    // ITargeter answer: Which enemy is the target base on specify condition.
    // e.g. skill want to aim at the Nearest/furthest enemy
    public interface ITargeter
    {
        ICombatant FindCurrentTarget();
        ICombatant FindNearestEnemy();
        ICombatant FindFurthestEnemy();
        ICombatant FindClusteredLaser(float beamHalfWidth);
        IPlacement FindClusteredLanding(int reachRange, float blastRadius, bool isJump);
    }
}
