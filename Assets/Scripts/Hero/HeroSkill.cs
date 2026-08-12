using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{

    /// <summary>
    /// HeroSkill contain trigger condition for each hero skill.
    /// </summary>
    public class HeroSkill
    {
        private readonly Hero _me;
        private readonly SkillSO _skill;
        private float _castTime;

        private int _attacksMade;

        // ============================================== getter ==============================================
        public float GetCastTime() => _castTime;

        // Some heroes (e.g. generic dummy/tank archetypes) have no SkillSO assigned.
        public bool HasSkill => _skill != null && _skill.ActiveSteps.Count > 0;
        public bool HasPassive => _skill != null && _skill.PassiveSteps.Count > 0;

        public HeroSkill(Hero hero, SkillSO skill)
        {
            _me = hero;
            _skill = skill;
        }

        // ============================================== active & passive skill ==============================================
        // mana is full, active skill is trigger. 
        // Returns true if the skill cast successfully.
        public bool TriggerOnCastSkill(bool isManaCapped)
        {
            if (!isManaCapped || !HasSkill) return false;

            IReadOnlyList<SkillStep> steps = _skill.ActiveSteps;

            // OnCast skill is always step 0
            if (steps[0].Trigger != TriggerEnum.OnCast) return false;

            if (!FireStep(steps, 0)) return false;

            // if skill success, spend all mana
            _me.SpendMana();

            return true;
        }

        // passive skill could always be triggered if the condition is true  
        // Returns true if the skill cast successfully.
        public bool TriggerPassiveSkill(TriggerEnum trigger)
        {
            if (!HasPassive) return false;

            IReadOnlyList<SkillStep> steps = _skill.PassiveSteps;

            if (steps[0].Trigger != trigger) return false;

            bool played = FireStep(steps, 0);

            // FIXNOW: No need for _attackMade, 
            // 1) It doesn't generic enough to stay here.
            // 2) we already have SkillContext that could do this instead too.
            if (trigger == TriggerEnum.OnAttack) _attacksMade++;

            return played;
        }

        // ============================================== Trigger condition ==============================================
        // on kill enemy, OnKill step is trigger
        private void TriggerOnKillStep(int hp, SkillStep step)
        {
            // bool isTargetDead = hp <= 0;

            // if (isTargetDead) FireAction(IndexOfStep(step));
        }

        // on event invoke, related Trigger is fired.
        // return SkillStepContext - it is data from previous step that will be checked by this trigger
        private Action<SkillStepContext> TriggerNextStep(IReadOnlyList<SkillStep> steps, int nextIndex, TriggerEnum trigger)
        {
            // guard
            if (nextIndex >= steps.Count) return null;

            // if trigger type match with this step, fire next step
            if (steps[nextIndex].Trigger != trigger) return null;

            return context => FireStep(steps, nextIndex, context);
        }

        // ============================================== helper ==============================================
        // Fire skill in "step" order. Returns whether this step actually played anything.
        private bool FireStep(IReadOnlyList<SkillStep> steps, int stepIndex, SkillStepContext contextFromPreviousStep = null)
        {
            if (stepIndex < 0 || stepIndex >= steps.Count) return false;

            // step can have several template action, only template action was choose base on condition
            return PlayAction(steps, stepIndex, contextFromPreviousStep);
        }

        // Play a template action.
        private bool PlayAction(IReadOnlyList<SkillStep> steps, int stepIndex, SkillStepContext contextFromPreviousStep)
        {
            // initial event - this is handed to template action that's going to be played
            Action<SkillStepContext> onExpired = TriggerNextStep(steps, stepIndex + 1, TriggerEnum.OnExpired);
            Action<SkillStepContext> onHit = TriggerNextStep(steps, stepIndex + 1, TriggerEnum.OnHit);

            // FIXNOW: the context should be the same instance, using different instance'll only make it difficult to read.
            SkillStepContext askContext = new SkillStepContext(_me, _me.FindNearestEnemy(), combo: _attacksMade);

            // Play 1 template action
            IReadOnlyList<SkillActionGroup> actionGroups = steps[stepIndex].ActionGroups;
            foreach (SkillActionGroup actionGroup in actionGroups)
            {
                // check condition for current template action.
                // if condition is not met, skip this one, and go check the next template action.
                if (SkillCondition.Ask(actionGroup.Conditions, askContext) == ConditionResultEnum.ConditionIsNotMet) continue;

                // try play the template action
                bool played = TemplateAction.TryPlay(actionGroup.TemplateAction, actionGroup.Source, actionGroup.Target, _me, actionGroup.Effects,
                    onExpired, onHit, contextFromPreviousStep);

                // if one of the template action is played, stop
                if (played)
                {
                    _castTime = actionGroup.TemplateAction.CastTime;
                    return true;
                }
            }

            return false;
        }
    }
}
