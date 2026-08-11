using System;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// A question a skill asks before it does something. One predicate, used in two places that do
    /// different things with the answer:
    ///
    /// 1) on a SkillActionGroup, it gates - a group whose condition fails is skipped, and the step
    ///    falls through to the next group. That is how a template action gets swapped for another
    ///    one, e.g. Aatrox throwing a box while transformed and a circle otherwise.
    ///
    /// 2) on a SkillEffect, it amplifies - the effect still lands, but scaled up when the answer is
    ///    yes, e.g. +30% damage against a wounded target.
    ///
    /// The consequence differs, the question does not, so it is written once here.
    /// </summary>
    [Serializable]
    public abstract class SkillCondition
    {
        [SerializeField] protected ConditionSubjectEnum _subject;

        public ConditionSubjectEnum Subject => _subject;

        // ...
        protected abstract bool IsMet(IDamageable caster, IDamageable recipient);

        // condition look at which hero?
        protected IDamageable Subjected(IDamageable caster, IDamageable recipient)
        {
            if (_subject == ConditionSubjectEnum.Caster) return caster;

            else if (_subject == ConditionSubjectEnum.Recipient) return recipient;

            else
            {
                // logerror; 
                return null;
            }
        }

        // to tell the caller "Is condition met?"
        // ASKING: Is there a reason this need to be static?
        public static ConditionResultEnum Ask(SkillCondition condition, IDamageable caster, IDamageable recipient)
        {
            // nobody wrote a condition on this group or effect
            if (condition == null) return ConditionResultEnum.NoConditionFound;

            // condition is met/not met
            return condition.IsMet(caster, recipient)
                ? ConditionResultEnum.ConditionIsMet
                : ConditionResultEnum.ConditionIsNotMet;
        }
    }

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
