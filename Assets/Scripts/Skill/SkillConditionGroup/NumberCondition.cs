using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("_currentNumber")]
        [SerializeField] private int _matchCombo;    // the beat this one answers to

        [FormerlySerializedAs("_maxNumber")]
        [SerializeField] private int _maxCombo;      // how many beats the whole combo has

        protected override bool IsMet(SkillStepContext context)
        {
            // guard
            if (_maxCombo <= 0 || context.Combo == null) return false;

            int beat = context.Combo.Value % _maxCombo + 1;

            return beat == _matchCombo;
        }
    }
}