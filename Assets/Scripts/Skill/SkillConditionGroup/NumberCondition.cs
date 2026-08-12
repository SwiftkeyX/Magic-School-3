using System;
using UnityEngine;

namespace MagicSchool
{

    [Serializable]
    public class NumberCondition : SkillCondition
    {
        // shared with the other beats of this combo, on purpose
        private ComboTracker _combo;
        private int _matchBeat;

        // TRANSITIONAL: only here so the SerializeReference machinery can still construct one while
        // SkillSO exists. An instance made that way has no tracker and says so when asked.
        public NumberCondition() { }

        public NumberCondition(ConditionSubjectEnum subject, ComboTracker combo, int matchBeat) : base(subject)
        {
            _combo = combo;
            _matchBeat = matchBeat;
        }

        protected override bool IsMet(IDamageable caster, IDamageable recipient)
        {
            if (_combo == null)
            {
                Debug.LogError($"[{nameof(NumberCondition)}] was asked without a combo to read. A skill " +
                               "authored in a SkillSO cannot supply one - it has to be built by SkillLibrary.");
                return false;
            }

            return _combo.Beat == _matchBeat;
        }
    }
}
