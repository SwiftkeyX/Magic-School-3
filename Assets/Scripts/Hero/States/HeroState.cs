namespace MagicSchool
{

    // BLOCKED: We should have state for casting skill specifically. But I still don't sure though. maybe we don't?
    public abstract class HeroState
    {
        protected readonly Hero _me;

        protected HeroState(Hero hero)
        {
            _me = hero;
        }

        public abstract HeroStateType StateType { get; }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public abstract void OnUpdate();
        protected abstract void CheckSwitchState();
    }
}
