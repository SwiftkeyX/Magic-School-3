using System;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// "Is the subject carrying this status right now?"
    /// E.g. transformed, wounded, stunned, ...
    /// </summary>
    [Serializable]
    public class HasStatusCondition : SkillCondition
    {
        [SerializeField] private ModifierEnum _status;
        [SerializeField] private bool _wantPresent = true;

        protected override bool IsMet(IDamageable caster, IDamageable recipient)
        {
            // ask which unit?
            IDamageable subject = Subjected(caster, recipient);

            // guard
            if (subject == null) return false;

            // this unit has/hasn't this staus
            return subject.HasStatus(_status) == _wantPresent;
        }
    }
}
