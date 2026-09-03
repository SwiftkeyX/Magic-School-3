namespace MagicSchool.Contracts
{
    // ICombatRecord answers: over one round, what were this unit's numbers that will be shown to the player?
    public interface ICombatRecord
    {
        // === what this unit did to others ===
        int DamageDealt { get; }            // AutoAttackDamage + SkillDamage
        int AutoAttackDamage { get; }
        int SkillDamage { get; }
        int Overkill { get; }
        int HealingDone { get; }
        int Overhealing { get; }

        // === what others did to this unit ===
        int DamageTaken { get; }
        int DamageMitigated { get; }
        int HealingReceived { get; }
        int HealingLostToWound { get; }
    }
}
