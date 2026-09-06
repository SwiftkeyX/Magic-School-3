namespace MagicSchool.Contracts
{
    // One piece of a modifier - a modifier is a list of these sharing one duration, see IModifier.
    // IModifierPiece to answer: How much this piece give stat (stat after calcuation)?
    // e.g. Hero ask "hey, I have like 10 pieces, give me how much stat, each piece actualy give me"
    public interface IModifierPiece
    {
        public float GetBonusAmount(IHeroStats stats);      // get bonus amount from this piece (bonus amount is the final amount after modifier calculation)
        public ModifierEnum GetModifierEnum();              // get modifier enum - to know what this piece should behave
        public ScalingSourceEnum GetScalingSource();        // get whose stats it should be scaling off
    }
}
