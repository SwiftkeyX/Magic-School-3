namespace MagicSchool
{
    public class HeroDead : HeroState
    {
        public override HeroStateEnum StateType => HeroStateEnum.Dead;

        public HeroDead(Hero hero, Transition transition) : base(hero, transition) { }

        public override void OnEnter()
        {
            _me.SetDeadVisual();
        }

        public override void OnUpdate() { }

        protected override void CheckSwitchState() { }
    }
}
