using UnityEngine;

namespace MagicSchool
{
    public class HeroAttack : HeroState
    {
        public override HeroStateType StateType => HeroStateType.Attack;
        private const int ManaPerAttack = 10;
        private const float DashDuration = 0.2f;
        private const float DashDistance = 0.5f;

        private ICombatant _nearestEnemy;
        private float _aaCooldown = 0f;

        private Vector3 _dashStart;
        private Vector3 _dashPeak;
        private float _dashElapsed = -1f;

        private readonly MovementConfig _movement;

        public HeroAttack(Hero hero, MovementConfig movement) : base(hero)
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
            _me.transform.position = _me.CurrentHex.transform.position;
        }

        public override void OnUpdate()
        {
            _nearestEnemy = _me.FindNearestEnemy();

            // check if I have a target within range. if no, exit attack state
            CheckSwitchState();

            // CheckSwitchState may switch the state. Then, attack state shouldn't continue.
            if (_me.StateType != StateType) return;

            // update aa timer
            _aaCooldown -= Time.deltaTime;

            // attack again if aa is reset
            bool isAaReset = (_aaCooldown <= 0f);
            if (isAaReset)
            {
                // apply damage to target
                _nearestEnemy.TakeDamage(_me.AttackDamage);

                // after attack, gain mana
                bool isManaCapped = _me.GainMana(ManaPerAttack);

                // if mana is full, trigger OnCast skill
                bool success = _me.TriggerSkill(isManaCapped);

                // if skill cast is success, pop skill effect
                if (success) _me.PlaySkillCastEffect("Skill Activated!");

                // aa is now on cooldown
                _aaCooldown += 1f / _me.AttackSpeed;

                // attack animation: dash toward the enemy, then back to where we started
                AttackAnimation();
            }

            // update attack animation
            UpdateAttackAnimation();
        }

        protected override void CheckSwitchState()
        {
            // If the enemy is no longer within attack range, transition to idle
            bool isEnemyInRange = _nearestEnemy != null && _nearestEnemy.IsAlive
                && _me.CurrentHex.IsWithinRange(_nearestEnemy.CurrentHex, _me.Range);
            if (!isEnemyInRange) _me.ChangeState(HeroStateType.Idle);
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
}
