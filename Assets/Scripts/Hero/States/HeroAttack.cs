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

        private Vector3 _dashStart;
        private Vector3 _dashPeak;
        private float _dashElapsed = -1f;

        private readonly MovementConfig _movement;

        public HeroAttack(Hero hero, MovementConfig movement, Transition transition) : base(hero, transition)
        {
            _movement = movement;
        }

        public override void OnEnter()
        {
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

            // CheckSwitchState may switch the state. Then, attack state shouldn't continue.
            if (_me.StateType != StateType) return;

            // attack again if aa is reset. The timer is always runs in the background
            if (_me.IsAttackReady)
            {
                // apply damage to target
                _nearestEnemy.TakeDamage(_me.AttackDamage);

                // after attack, gain mana
                _me.GainMana(ManaPerAttack);

                // aa is now on cooldown
                _me.SpendAttack();

                // attack animation: dash toward the enemy, then back to where we started
                AttackAnimation();
            }

            // update attack animation
            UpdateAttackAnimation();
            
            CheckSwitchState();
        }

        protected override void CheckSwitchState()
        {
            // guard
            if (_nearestEnemy == null) { _me.ChangeState(HeroStateType.Idle); return; }

            // resume attack
            if (_transition.CanAttack(_nearestEnemy))
            {
                return;
            }

            if (_transition.CanWalk(_nearestEnemy))
            {
                _me.ChangeState(HeroStateType.Walk);
            }

            else
            {
                _me.ChangeState(HeroStateType.Idle);
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
}
