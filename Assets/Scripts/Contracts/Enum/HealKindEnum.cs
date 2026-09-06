namespace MagicSchool.Contracts
{
    // Where a heal came from. The mirror of DamageKindEnum.
    // e.g. Solace healing an ally is not the same fact as Vharn draining his own damage.
    public enum HealKindEnum
    {
        Skill = 0,
        Omnivamp = 1,
    }
}
