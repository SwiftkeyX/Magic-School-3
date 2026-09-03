using System.Collections.Generic;
using MagicSchool.Contracts;

namespace MagicSchool.Combat.Tracking
{
    // FIXLATER: I prefered this listen to player's event instead. let player fire event when they take damage.
    public class CombatTracker
    {
        private readonly Dictionary<IEffectable, CombatRecord> _round = new Dictionary<IEffectable, CombatRecord>();

        // ======================================== public ========================================
        // get combat record from the consumed unit
        public CombatRecord RoundOf(IEffectable unit) => RecordFor(_round, unit);

        // A round starts from nothing
        public void BeginRound() => _round.Clear();

        // record the damage dealt & damage taken
        public void RecordDamage(IEffectable source, IEffectable target, DamageKindEnum kind,
                                 int landed, int overkill, int mitigated)
        {
            if (source != null)
            {
                RecordFor(_round, source).AddDealt(kind, landed, overkill);
            }

            if (target != null)
            {
                RecordFor(_round, target).AddTaken(landed, mitigated);
            }
        }

        // record heal & heal reduction
        public void RecordHeal(IEffectable source, IEffectable target, int healed, int overhealed, int lostToWound)
        {
            if (source != null)
            {
                RecordFor(_round, source).AddHealingDone(healed, overhealed);
            }

            if (target != null)
            {
                RecordFor(_round, target).AddHealingReceived(healed, lostToWound);
            }
        }

        // ======================================== helper ========================================
        // context: combat record keep data for damage dealt, damage taken, heal
        // to read/write the combat record
        private static CombatRecord RecordFor(Dictionary<IEffectable, CombatRecord> table, IEffectable unit)
        {
            if (unit == null) return new CombatRecord();

            if (!table.TryGetValue(unit, out CombatRecord record))
            {
                record = new CombatRecord();
                table[unit] = record;
            }

            return record;
        }
    }
}
