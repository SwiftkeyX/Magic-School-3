// Attack() is still a stub - no damage, no death, no mana gain - so an adjacent pair just
// stands still for now. Re-checks every frame whether the nearest enemy is still adjacent,
// so a hero resumes moving as soon as that stops being true (target dies/retreats, once
// real combat exists).
public class HeroAttack : HeroState
{
    public override HeroStateType StateType => HeroStateType.Attack;

    public HeroAttack(Hero hero) : base(hero) { }

    public override void OnUpdate()
    {
        Hero nearestEnemy = Hero.FindNearestEnemy();
        if (nearestEnemy == null || !Hero.CurrentHex.GetNeighbors().Contains(nearestEnemy.CurrentHex))
        {
            Hero.StateMachine.ChangeState(HeroStateType.Idle);
            return;
        }

        // TODO: real combat - damage, attack speed, mana gain.
    }

    protected override void CheckSwitchState()
    {
        throw new System.NotImplementedException();
    }

}
