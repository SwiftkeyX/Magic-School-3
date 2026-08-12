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

        // FIXLATER: I think IDamageable start to drift from its orignal meaning.
        // Could we implement new interface for IEffectable? It should inherit from IDamageable and add method of AddModifier() and HasStatus().
        protected override bool IsMet(SkillStepContext context)
        {
            // get context data
            IDamageable me = context.Me;
            IDamageable recipient = context.Recipient;

            // ask which unit?
            IDamageable subject = Subjected(me, recipient);

            // guard
            if (subject == null) return false;

            // this unit has/hasn't this staus
            return subject.HasStatus(_status) == _wantPresent;
        }
    }
}
