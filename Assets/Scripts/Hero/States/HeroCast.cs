using UnityEngine;

namespace MagicSchool
{
    public class HeroCast : HeroState
    {
        public override HeroStateType StateType => HeroStateType.Cast;

        private float _remaining;

        public HeroCast(Hero hero) : base(hero) { }

        public override void OnEnter()
        {
            // get cast time of skill
            _remaining = _me.GetCastTime();

            // if skill cast is success, pop skill effect
            _me.PlaySkillCastEffect("Skill Activated!");
        }

        public override void OnUpdate()
        {
            _remaining -= Time.deltaTime;

            CheckSwitchState();
        }

        protected override void CheckSwitchState()
        {
            // if still casting, return
            if (_remaining > 0f) return;

            ICombatant nearestEnemy = _me.FindNearestEnemy();

            // If enemy is within attack range, stop moving, and transition to attack state
            if (nearestEnemy != null && IsEnemyInAttackRange(nearestEnemy))
            {
                _me.ChangeState(HeroStateType.Attack);
                return;
            }

            // else if ()
            // transition to walk state
            // FIXLATER: let implement this later, now we just delegate walk logic to idle state which doesn't sound like to me.
            // every state should know how to transition to walk
            // FIXLATER: I also see that transition logic is redundant. So let also implement Transition class later.

            // if enemy is not in attack range (or there's no enemy), go idle
            _me.ChangeState(HeroStateType.Idle);
        }

    }
}
