// Cooldown between attacks is 1 / AttackSpeed seconds. Each landed hit deals _me's Atk as
// damage, grants ManaPerAttack mana, and plays a short dash-toward-and-back animation - same
// Lerp + AnimationCurve approach as HeroWalk, but AttackCurve is a 0 -> 1 -> 0 hump so it
// returns to the start instead of ending at the target. Re-checks every frame whether the
// nearest enemy is still adjacent, so a hero resumes moving as soon as that stops being true.
// Going to 0 HP has no special handling yet - death (HeroDead state) isn't wired up in this pass.
using UnityEngine;

public class HeroAttack : HeroState
{
    public override HeroStateType StateType => HeroStateType.Attack;
    private const int ManaPerAttack = 10;
    private const float DashDuration = 0.2f;
    // How far toward the enemy hex the dash lunges, as a fraction of the distance between them.
    private const float DashDistanceFraction = 0.5f;

    private Hero _nearestEnemy;
    private float _aaCooldown = 0f;

    private Vector3 _dashStart;
    private Vector3 _dashPeak;
    private float _dashElapsed = -1f;

    public HeroAttack(Hero hero) : base(hero) { }

    public override void OnEnter()
    {
        _aaCooldown = 0f;
        _dashElapsed = -1f;
    }

    public override void OnExit()
    {
        // Snap back in case we leave Attack mid-dash, so the hero doesn't get left off-hex-center.
        _me.transform.position = _me.GetCurrentHex().transform.position;
    }

    public override void OnUpdate()
    {
        _nearestEnemy = _me.FindNearestEnemy();

        // check if I have target adjacent to me. if no, exit attack state
        CheckSwitchState();

        // update aa timer
        _aaCooldown -= Time.deltaTime;

        // attack again if aa is reset
        bool isAaReset = (_aaCooldown <= 0f);
        if (isAaReset)
        {
            // apply damage to target
            _nearestEnemy.TakeDamage(_me.GetAtk());

            // gain mana
            _me.GainMana(ManaPerAttack);

            // aa is cooldown
            _aaCooldown += 1f / _me.GetAttackSpeed();

            // attack animation: dash toward the enemy, then back to where we started
            AttackAnimation();
        }

        // update attack animation
        UpdateAttackAnimation();
    }

    protected override void CheckSwitchState()
    {
        // If hp is below 0, transition to dead, WOW
        bool isMeDead = (_me.GetCurrentHP() <= 0);
        if (isMeDead)
        {
            _me.StateMachine.ChangeState(HeroStateType.Dead);
            return;
        }

        // If there isn't a single enemy in the neighbors, transition to idle
        bool isEnemyMyNeighbor = _nearestEnemy != null && _me.GetCurrentHex().IsAdjacentTo(_nearestEnemy.GetCurrentHex());
        if (!isEnemyMyNeighbor)
        {
            _me.StateMachine.ChangeState(HeroStateType.Idle);
            return;
        }
    }

    private void AttackAnimation()
    {
        // attack animation: dash toward the enemy, then back to where we started
        _dashStart = _me.transform.position;
        _dashPeak = Vector3.Lerp(_dashStart, _nearestEnemy.transform.position, DashDistanceFraction);
        _dashElapsed = 0f;
    }

    private void UpdateAttackAnimation()
    {
        if (_dashElapsed < 0f) return;

        _dashElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_dashElapsed / DashDuration);
        _me.transform.position = Vector3.Lerp(_dashStart, _dashPeak, _me.AttackCurve.Evaluate(t));

        if (t >= 1f) _dashElapsed = -1f;
    }
}
