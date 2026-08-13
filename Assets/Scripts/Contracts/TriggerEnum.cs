namespace MagicSchool
{
    // Serialized into SkillSO assets as a raw int - always assign explicit values so inserting a
    // new member later can't silently remap what an existing asset's stored int means.
    public enum TriggerEnum
    {
        OnCast = 0,
        OnKill = 1,
        OnExpired = 2,      // once the previous step expired
        OnHit = 3,          // once the previous step hit someone 
        OnAttack = 4,       // once hero auto attack
    }
}
