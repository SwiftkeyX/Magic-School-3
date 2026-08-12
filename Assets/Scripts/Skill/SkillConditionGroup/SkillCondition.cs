using System;
using System.Collections.Generic;
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

        // the actual condition - read each child for more detail
        protected abstract bool IsMet(SkillStepContext context);

        // condition look at which hero?
        // FLAGGING: this is not used that widely, let move it to HasStatusCondition later
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

        // to tell the caller "Is all the condition met?"
        // ASKING: Is there a reason this need to be static?
        // FIXLATER: too much paramter ?
        public static ConditionResultEnum Ask(List<SkillCondition> conditions, SkillStepContext context)
        {
            if (conditions == null) return ConditionResultEnum.NoConditionFound;

            bool askedAnything = false;

            foreach (SkillCondition condition in conditions)
            {
                // guard
                if (condition == null) { Debug.LogError(""); continue; }

                askedAnything = true;

                // if either condition isn't satisfied, return conditionNotMet
                if (!condition.IsMet(context)) return ConditionResultEnum.ConditionIsNotMet;
            }

            return askedAnything
                ? ConditionResultEnum.ConditionIsMet
                : ConditionResultEnum.NoConditionFound;
        }
    }
}
