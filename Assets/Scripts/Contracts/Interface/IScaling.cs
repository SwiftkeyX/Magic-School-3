namespace MagicSchool.Contracts
{
    // IScaling answer this: a amount total of stat/dmg/heal after the scaling calculation
    // How is scaling calucation look like? 
    // e.g. skill dmg = 500% AD + 200% AP
    // e.g. modifier buff to atk stat = 30% AP 
    public interface IScaling
    {
        ScalingSourceEnum GetScalingSource();               // whose stats the ratios read - the caster, or the other being hit by the skill
        float GetTotalAfterScaling(IHeroStats stats);       // Get a total amount of stat/dmg/heal after the scaling calculation

        // The same terms, split into what adds and what multiplies.
        // "landingOn" is the stat the total is going into, and it is what tells the two apart: a
        // term reading that same stat is a multiplier, one reading another stat is an amount to
        // add. Damage and healing land on no stat, so they pass null and everything adds.
        ScalingTotals GetTotals(IHeroStats stats, StatEnum? landingOn);
    }
}
