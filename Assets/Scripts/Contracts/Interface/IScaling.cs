namespace MagicSchool.Contracts
{
    // IScaling answer this: a amount total of stat/dmg/heal after the scaling calculation
    // How is scaling calucation look like? 
    // e.g. skill dmg = 500% AD + 200% AP
    // e.g. modifier buff to atk stat = 30% AP 
    public interface IScaling
    {
        ScalingEnum GetScalingEnum();                       // how the total should be applied - flat, or as percentage points
        float GetTotalAfterScaling(IHeroStats stats);       // Get a total amount of stat/dmg/heal after the scaling calculation
    }
}
