namespace MagicSchool.Contracts
{
    public readonly struct HealEvent
    {
        public readonly IEffectable Source;      // who healed
        public readonly IEffectable Target;      // who was healed
        public readonly HealKindEnum Kind;       // where the heal came from
        public readonly HealOutcome Outcome;

        public HealEvent(IEffectable source, IEffectable target, HealKindEnum kind, HealOutcome outcome)
        {
            Source = source;
            Target = target;
            Kind = kind;
            Outcome = outcome;
        }
    }
}
