using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MagicSchool
{

    /// <summary>
    /// Aatrox's skill is a combo and it count combo number 1-3.
    /// This is condition for this hero. 
    /// FIXLATER: We have try implement this. But it change the skill system structure too much. 
    /// I'll come back later.
    /// </summary>
    [Serializable]
    public class NumberCondition : SkillCondition
    {
        private int _combo = 1;

        // Aatrox.asset still stores these two under the names they were first saved with, so without
        // the bridge both read 0 - and a max of 0 reads as "this beat never comes up", which looks
        // like a dead passive rather than a missing value.
        [FormerlySerializedAs("_currentNumber")]
        [SerializeField] private int _matchCombo;    // current combo number

        [FormerlySerializedAs("_maxNumber")]
        [SerializeField] private int _maxCombo;        // max combo number

        public NumberCondition() { }

        public NumberCondition(ConditionSubjectEnum subject, int matchCombo, int maxCombo) : base(subject)
        {
            _matchCombo = matchCombo;
            _maxCombo = maxCombo;
        }

        protected override bool IsMet(IDamageable caster, IDamageable recipient)
        {
            bool numberMatch = (_combo == _matchCombo);
            
            if (_combo < _maxCombo) _combo++;
            else _combo = 1;  
            
            return numberMatch;
        }
    }
}