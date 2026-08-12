using System;
using UnityEngine;

namespace MagicSchool
{

    /// <summary>
    /// Aatrox's skill is a combo and it count combo number 1-3.
    /// This is condition for this hero. 
    /// FLAGGING: It might have change in the future when we included other hero with similar pattern. 
    /// </summary>
    [Serializable]
    public class NumberCondition : SkillCondition
    {
        private int _combo = 1;

        [SerializeField] private int _matchCombo;    // current combo number
        [SerializeField] private int _maxCombo;        // max combo number

        protected override bool IsMet(IDamageable caster, IDamageable recipient)
        {
            bool numberMatch = (_combo == _matchCombo);
            
            if (_combo < _maxCombo) _combo++;
            else _combo = 1;  
            
            return numberMatch;
        }
    }
}