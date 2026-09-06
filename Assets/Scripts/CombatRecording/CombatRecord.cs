using MagicSchool.Contracts;

namespace MagicSchool.CombatRecording
{
    public class CombatRecord : ICombatRecord
    {
        // ===================== as the source: what this unit did to others =====================
        public int DamageDealt { get; private set; }        // AutoAttackDamage + SkillDamage
        public int AutoAttackDamage { get; private set; }
        public int SkillDamage { get; private set; }
        public int Overkill { get; private set; }           // landed on a corpse's last hit, past 0 HP
        public int HealingDone { get; private set; }        // SkillHealing + OmnivampHealing
        public int SkillHealing { get; private set; }
        public int OmnivampHealing { get; private set; }    // drained back out of its own damage
        public int Overhealing { get; private set; }        // healing past the target's MaxHP

        // ===================== as the target: what others did to this unit =====================
        public int DamageTaken { get; private set; }
        public int DamageMitigated { get; private set; }    // what DF and Damage Reduction saved
        public int HealingReceived { get; private set; }
        public int HealingLostToWound { get; private set; }

        internal void AddDealt(DamageKindEnum kind, int landed, int overkill)
        {
            DamageDealt += landed;
            Overkill += overkill;

            if (kind == DamageKindEnum.AutoAttack) AutoAttackDamage += landed;
            else if (kind == DamageKindEnum.Skill) SkillDamage += landed;
        }

        internal void AddTaken(int landed, int mitigated)
        {
            DamageTaken += landed;
            DamageMitigated += mitigated;
        }

        internal void AddHealingDone(HealKindEnum kind, int healed, int overhealed)
        {
            HealingDone += healed;
            Overhealing += overhealed;

            if (kind == HealKindEnum.Skill) SkillHealing += healed;
            else if (kind == HealKindEnum.Omnivamp) OmnivampHealing += healed;
        }

        internal void AddHealingReceived(int healed, int lostToWound)
        {
            HealingReceived += healed;
            HealingLostToWound += lostToWound;
        }
    }
}
