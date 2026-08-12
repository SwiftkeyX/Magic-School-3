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
        private readonly SkillDefinition _skill;
        private float _castTime;

        // ============================================== getter ==============================================
        public float GetCastTime() => _castTime;

        // Some heroes (e.g. generic dummy/tank archetypes) have no skill at all.
        public bool HasSkill => _skill != null && _skill.ActiveSteps.Count > 0;
        public bool HasPassive => _skill != null && _skill.PassiveSteps.Count > 0;

        public HeroSkill(Hero hero, SkillDefinition skill)
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

            // invoke Aatrox's combo counter
            // FLAGGING: Don't sure if it should stay here. Let's look at it again when the pattern is more clear. 
            if (played) _skill.InvokeTrigger(trigger);

            return played;
        }

        // ============================================== Trigger condition ==============================================
        // on kill enemy, OnKill step is trigger
        private void TriggerOnKillStep(int hp, SkillStep step)
        {
            // bool isTargetDead = hp <= 0;

            // if (isTargetDead) FireAction(IndexOfStep(step));
        }

        // This function create a event that was handed to TemplateAction.
        // on event invoke, next TemplateAction is played, and related context is sent to be used for next TemplateAction.
        // e.g. Projectile invoke "OnHit" event => Send projectile hit position to next TemplateAction
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

        // Play a ActionGroup
        private bool PlayAction(IReadOnlyList<SkillStep> steps, int stepIndex, SkillStepContext contextFromPreviousStep)
        {
            // initial event - this is handed to template action that's going to be played
            Action<SkillStepContext> onExpired = TriggerNextStep(steps, stepIndex + 1, TriggerEnum.OnExpired);
            Action<SkillStepContext> onHit = TriggerNextStep(steps, stepIndex + 1, TriggerEnum.OnHit);

            // Choose 1 TemplateAction from ActionGroup
            IReadOnlyList<SkillActionGroup> actionGroups = steps[stepIndex].ActionGroups;
            foreach (SkillActionGroup actionGroup in actionGroups)
            {
                // check condition for current TemplateAction. 
                // if condition is not met, skip this one, and go check the next template action.
                // FIXNOW: send FindNearestEnemy() is just straight wrong.
                // I try look into pattern more, and found that condition for template action don't need recipient.
                // just make recipient = null here.
                if (SkillCondition.Ask(actionGroup.Conditions, _me, _me.FindNearestEnemy()) == ConditionResultEnum.ConditionIsNotMet) continue;

                // try play the template action
                // ASKING: what is amount of paramter? could we just send ActionGroup in? 
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
