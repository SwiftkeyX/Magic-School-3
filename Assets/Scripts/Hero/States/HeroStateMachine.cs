namespace MagicSchool
{
    /// <summary>
    /// HeroStateMachine is state machine that control hero's behaviour.
    /// It's vanilla state machine, nothing special.
    /// </summary>
    public class HeroStateMachine
    {
        private readonly Hero _me;

        private readonly HeroIdle _idle;
        private readonly HeroAttack _attack;
        private readonly HeroDead _dead;
        private readonly HeroWalk _walk;
        private readonly HeroStunned _stunned;
        private readonly HeroCast _cast;

        public HeroState Current { get; private set; }

        public HeroStateType CurrentType => Current == null ? HeroStateType.Idle : Current.StateType;
        public HeroStateType PreviousType { get; private set; }

        public HeroStateMachine(Hero hero, MovementConfig movement)
        {
            _me = hero;
            Transition transition = new Transition(hero, movement);

            _idle = new HeroIdle(hero, transition);
            _walk = new HeroWalk(hero, movement, transition);
            _attack = new HeroAttack(hero, movement, transition);
            _dead = new HeroDead(hero, transition);
            _stunned = new HeroStunned(hero, transition);
            _cast = new HeroCast(hero, transition);
        }

        public void Start(HeroStateType initial)
        {
            Current = GetState(initial);
            Current.OnEnter();
        }

        public void ChangeState(HeroStateType next)
        {
            if (Current != null && next == CurrentType) return;

            PreviousType = CurrentType;

            Current?.OnExit();
            Current = GetState(next);
            Current.OnEnter();
        }

        public void Tick()
        {
            if (Current == null) return;

            // interrupt state e.g. stun, dead
            if (TryResolveInterrupt(out HeroStateType forced))
            {
                ChangeState(forced);

                // return early, so we don't update in the same frame
                return;
            }

            // update state
            Current.OnUpdate();
        }

        /// <summary>
        /// Global state transition.  
        /// Some of the transition are redundant in each state, make it a global transition by move it here.
        /// </summary>
        private bool TryResolveInterrupt(out HeroStateType forced)
        {
            forced = default;

            if (CurrentType == HeroStateType.Dead) return false;

            if (_me.CurrentHP <= 0)
            {
                forced = HeroStateType.Dead;
                return true;
            }

            bool notStun = CurrentType != HeroStateType.Stunned;
            if (_me.IsStunned && notStun)
            {
                forced = HeroStateType.Stunned;
                return true;
            }

            // FLAGGING: Being stun while casting skill is useless for the stun user, since skill effect is already fire.
            // if mana is full, trigger OnCast skill
            bool success = _me.TriggerSkill(_me.IsManaCapped());
            if (success)
            {
                forced = HeroStateType.Cast;
                return true;
            }

            return false;
        }

        private HeroState GetState(HeroStateType type)
        {
            switch (type)
            {
                case HeroStateType.Idle: return _idle;
                case HeroStateType.Walk: return _walk;
                case HeroStateType.Attack: return _attack;
                case HeroStateType.Dead: return _dead;
                case HeroStateType.Stunned: return _stunned;
                case HeroStateType.Cast: return _cast;
                default: return null;
            }
        }
    }
}
