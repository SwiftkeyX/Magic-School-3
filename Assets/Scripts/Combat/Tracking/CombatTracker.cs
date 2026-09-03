using System.Collections.Generic;
using MagicSchool.Contracts;
using MagicSchool.Combat.Heroes;

namespace MagicSchool.Combat.Tracking
{
    // FIXLATER: tracker could also be decoupling from the combat module, let it have its own asmdef, "CombatRecorder" module
    // The last thing in the way is DamageEvent/HealEvent, which still live in Combat.Heroes.
    public class CombatRecorder
    {
        private readonly Dictionary<IEffectable, CombatRecord> _round = new Dictionary<IEffectable, CombatRecord>();

        // ======================================== public ========================================
        // get combat record from the consumed unit
        public CombatRecord RoundOf(IEffectable unit) => RecordFor(_round, unit);

        // A round starts from nothing
        public void BeginRound() => _round.Clear();

        // ======================================== listener ========================================
        // Subscribed to a hero's OnDamaged/OnHealed
        public void Record(DamageEvent e)
        {
            if (e.Source != null) RecordFor(_round, e.Source).AddDealt(e.Kind, e.Outcome.Landed, e.Outcome.Overkill);
            if (e.Target != null) RecordFor(_round, e.Target).AddTaken(e.Outcome.Landed, e.Outcome.Mitigated);
        }
        public void Record(HealEvent e)
        {
            if (e.Source != null) RecordFor(_round, e.Source).AddHealingDone(e.Outcome.Healed, e.Outcome.Overhealed);
            if (e.Target != null) RecordFor(_round, e.Target).AddHealingReceived(e.Outcome.Healed, e.Outcome.LostToWound);
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
