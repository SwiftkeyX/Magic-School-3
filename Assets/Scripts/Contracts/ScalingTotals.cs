namespace MagicSchool.Contracts
{
    // A scaling's terms, sorted into the two piles that have to be combined in order:
    // everything that ADDS to the stat, and everything that MULTIPLIES what the adding produced.
    //
    // e.g. (base 1.0 AS) + a flat 0.3 item + Fang's "+100% of his own AS"
    //      => Additive 0.3, MultiplierPercent 100  =>  (1.0 + 0.3) * 2.0
    public readonly struct ScalingTotals
    {
        // flat terms, plus terms derived from a DIFFERENT stat - both land as a number to add.
        public readonly float Additive;

        // terms derived from the same stat the modifier lands on, in percent. Several of them add
        // up rather than compounding: +100% and +15% is *2.15, not *2.30.
        public readonly float MultiplierPercent;

        public ScalingTotals(float additive, float multiplierPercent)
        {
            Additive = additive;
            MultiplierPercent = multiplierPercent;
        }

        public static ScalingTotals operator *(ScalingTotals totals, float amplifier)
            => new ScalingTotals(totals.Additive * amplifier, totals.MultiplierPercent * amplifier);
    }
}
