namespace MagicSchool.Contracts
{
    // How a modifier turns itself into a number.
    public enum ScalingEnum
    {
        // scale with flat amount
        // e.g. +20 DF is flat amount added into defence. 
        Flat,

        // scale with percentage
        // e.g. (AP, 50f) is "50% of their AP".
        Percentage,
    }
}
