// Cooldown between attacks is 1 / AttackSpeed seconds. Each landed hit deals _me's Atk as
// damage, grants ManaPerAttack mana, and plays a short dash-toward-and-back animation - same
// Lerp + AnimationCurve approach as HeroWalk, but AttackCurve is a 0 -> 1 -> 0 hump so it
// returns to the start instead of ending at the target. Re-checks every frame whether the
// nearest enemy is still within Range, so a hero resumes moving as soon as that stops being true.
using UnityEngine;

public class HeroAttack : HeroState
{
    public override HeroStateType StateType => HeroStateType.Attack;
    private const int ManaPerAttack = 10;
    private const float DashDuration = 0.2f;
    private const float DashDistance = 0.5f;

    private Hero _nearestEnemy;
    private float _aaCooldown = 0f;

    private Vector3 _dashStart;
    private Vector3 _dashPeak;
    private float _dashElapsed = -1f;

    private readonly MovementConfig _movement;

    public HeroAttack(Hero hero, SkillSO skill, MovementConfig movement) : base(hero, skill)
    {
        _movement = movement;
    }

    public override void OnEnter()
    {
        _aaCooldown = 0f;
        _dashElapsed = -1f;
    }

    public override void OnExit()
    {
        // Snap back in case we leave Attack mid-dash, so the hero doesn't get left off-hex-center.
        _me.transform.position = _me.Blackboard.GetCurrentHex().transform.position;
    }

    public override void OnUpdate()
    {
        _nearestEnemy = _me.Blackboard.FindNearestEnemy();

        // check if I have a target within range. if no, exit attack state
        CheckSwitchState();

        bool isInRange = _nearestEnemy != null && _me.Blackboard.GetCurrentHex().IsWithinRange(_nearestEnemy.Blackboard.GetCurrentHex(), _me.Blackboard.GetRange());
        if (!isInRange) return;

        // update aa timer
        _aaCooldown -= Time.deltaTime;

        // attack again if aa is reset
        bool isAaReset = (_aaCooldown <= 0f);
        if (isAaReset)
        {
            // apply damage to target
            _nearestEnemy.Blackboard.TakeDamage(_me.Blackboard.GetAttackDamage());

            // after attack, gain mana
            bool isManaCapped = _me.Blackboard.GainMana(ManaPerAttack);

            // if mana is full, trigger OnCast skill
            bool success = false;
            if (_currentStep != null) _me.Blackboard.Temp.TriggerSkill(_skill, _currentStep, isManaCapped);

            // if skill cast is success, pop skill effect 
            if (success) _me.Blackboard.Temp.PlaySkillCastEffect("Skill Activated!");

            // aa is now on cooldown
            _aaCooldown += 1f / _me.Blackboard.GetAttackSpeed();

            // attack animation: dash toward the enemy, then back to where we started
            AttackAnimation();
        }

        // update attack animation
        UpdateAttackAnimation();
    }

    protected override void CheckSwitchState()
    {
        // If hp is below 0, transition to dead, WOW
        bool isMeDead = (_me.Blackboard.GetCurrentHP() <= 0);
        if (isMeDead)
        {
            _me.StateMachine.ChangeState(HeroStateType.Dead);
            return;
        }

        if (_me.Blackboard.IsStunned())
        {
            _me.StateMachine.ChangeState(HeroStateType.Stunned);
            return;
        }

        // If the enemy is no longer within attack range, transition to idle
        bool isEnemyInRange = _nearestEnemy != null && _me.Blackboard.GetCurrentHex().IsWithinRange(_nearestEnemy.Blackboard.GetCurrentHex(), _me.Blackboard.GetRange());
        if (!isEnemyInRange)
        {
            _me.StateMachine.ChangeState(HeroStateType.Idle);
            return;
        }
    }

    private void AttackAnimation()
    {
        // attack animation: dash toward the enemy, then back to where we started
        _dashStart = _me.transform.position;
        Vector3 toEnemy = _nearestEnemy.transform.position - _dashStart;
        Vector3 direction = toEnemy.sqrMagnitude > 0f ? toEnemy.normalized : Vector3.zero;
        _dashPeak = _dashStart + direction * DashDistance;
        _dashElapsed = 0f;
    }

    private void UpdateAttackAnimation()
    {
        if (_dashElapsed < 0f) return;

        _dashElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_dashElapsed / DashDuration);
        _me.transform.position = Vector3.Lerp(_dashStart, _dashPeak, _movement.AttackCurve.Evaluate(t));

        if (t >= 1f) _dashElapsed = -1f;
    }
}
