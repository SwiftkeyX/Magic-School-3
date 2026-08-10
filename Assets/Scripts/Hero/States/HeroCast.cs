using UnityEngine;

namespace MagicSchool
{
    public class HeroCast : HeroState
    {
        public override HeroStateType StateType => HeroStateType.Cast;

        private float _remaining;

        public HeroCast(Hero hero, Transition transition) : base(hero, transition) { }

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

            // guard
            if (nearestEnemy == null) { _me.ChangeState(HeroStateType.Idle); return; }

            // FIXLATER: I will check the consistent usage of _transition across otherr state later.
            if (_transition.CanAttack(nearestEnemy))
            {
                _me.ChangeState(HeroStateType.Attack);
                return;
            }

            if (_transition.CanWalk(nearestEnemy))
            {
                _me.ChangeState(HeroStateType.Walk);
            }

            else
            {
                _me.ChangeState(HeroStateType.Idle);
            }


        }

    }
}
