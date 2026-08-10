using UnityEngine;

namespace MagicSchool
{
    public class HeroStunned : HeroState
    {
        public override HeroStateType StateType => HeroStateType.Stunned;

        public HeroStunned(Hero hero, Transition transition) : base(hero, transition) { }

        public override void OnExit()
        {
            // Snap back in case a stun landed mid-attack-dash, same reasoning as HeroAttack.OnExit.
            _me.transform.position = _me.CurrentHex.transform.position;
        }

        public override void OnUpdate()
        {
            CheckSwitchState();
        }

        protected override void CheckSwitchState()
        {
            if (!_me.IsStunned) _me.ChangeState(HeroStateType.Idle);
        }
    }
}
