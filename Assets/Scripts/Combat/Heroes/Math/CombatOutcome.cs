using MagicSchool.Contracts;

namespace MagicSchool.Combat.Heroes
{
    // all of those are use with CombatMath.cs
    // FIXLATER: I notice Math folder can actually be decoupling from the combat module
    // let move math folder to be under Combat folder, and let it have asmdef for CombatMath module.

    public readonly struct DamageOutcome
    {
        public readonly int NewHP;
        public readonly int Landed;         // HP actually removed
        public readonly int Overkill;       // the part of the hit that fell past 0 HP
        public readonly int Mitigated;      // what DF and Damage Reduction saved

        public DamageOutcome(int newHP, int landed, int overkill, int mitigated)
        {
            NewHP = newHP;
            Landed = landed;
            Overkill = overkill;
            Mitigated = mitigated;
        }
    }

    public readonly struct HealOutcome
    {
        public readonly int NewHP;
        public readonly int Healed;         // HP actually gained
        public readonly int Overhealed;     // the part that fell past MaxHP
        public readonly int LostToWound;    // what Wound halved away

        public HealOutcome(int newHP, int healed, int overhealed, int lostToWound)
        {
            NewHP = newHP;
            Healed = healed;
            Overhealed = overhealed;
            LostToWound = lostToWound;
        }
    }

    public readonly struct DamageEvent
    {
        public readonly IEffectable Source;      // who dealt it. null if nothing authored the hit
        public readonly IEffectable Target;      // who took it
        public readonly DamageKindEnum Kind;
        public readonly DamageOutcome Outcome;

        public DamageEvent(IEffectable source, IEffectable target, DamageKindEnum kind, DamageOutcome outcome)
        {
            Source = source;
            Target = target;
            Kind = kind;
            Outcome = outcome;
        }
    }

    public readonly struct HealEvent
    {
        public readonly IEffectable Source;      // who healed
        public readonly IEffectable Target;      // who was healed
        public readonly HealOutcome Outcome;

        public HealEvent(IEffectable source, IEffectable target, HealOutcome outcome)
        {
            Source = source;
            Target = target;
            Outcome = outcome;
        }
    }
}
