namespace MagicSchool.Contracts
{
    // IModifier to answer: How much this modifier give stat (stat after calcuation)?
    // e.g. Hero ask "hey, I have like 10 modifiers, give me how much stat, each modifier actualy give me"
    public interface IModifier
    {
        public ScalingTotals GetTotals(IHeroStats stats, StatEnum? landingOn);   // what this modifier gives, split into what adds and what multiplies
        public ModifierEnum GetModifierEnum();              // get modifier enum - to know what this modifier should behave
        public ScalingSourceEnum GetScalingSource();        // get whose stats it should be scaling off
    }
}
