using System;
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
        
        // ============================================== getter ==============================================
        public float GetCastTime() => _castTime;

        // Some heroes (e.g. generic dummy/tank archetypes) have no SkillSO assigned.
        public bool HasSkill => _skill != null && _skill.Steps.Count > 0;

        public HeroSkill(Hero hero, SkillSO skill)
        {
            _me = hero;
            _skill = skill;
        }

        // mana is full, skill is trigger. Returns true if the skill cast successfully.
        public bool TriggerOnCastSkill(bool isManaCapped)
        {
            if (!isManaCapped || !HasSkill) return false;

            if (_skill.Steps[0].Trigger != TriggerEnum.OnCast) return false;

            // OnCast skill is always step 0
            if (!FireStep(0)) return false;

            // if skill success, spend all mana
            _me.SpendMana();

            return true;
        }

        public void TriggerPassiveSkill()
        {
            
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
        private Action<SkillStepContext> TriggerNextStep(int nextIndex, TriggerEnum trigger)
        {
            // guard
            if (nextIndex >= _skill.Steps.Count) return null;

            // if trigger type match with this step, fire next step
            if (_skill.Steps[nextIndex].Trigger != trigger) return null;

            return context => FireStep(nextIndex, context);
        }

        // ============================================== helper ==============================================
        // Fire skill in "step" order. Returns whether this step actually played anything.
        private bool FireStep(int stepIndex, SkillStepContext contextFromPreviousStep = null)
        {
            if (stepIndex < 0 || stepIndex >= _skill.Steps.Count) return false;

            // step can have several template action, only template action was choose base on condition
            return PlayAction(stepIndex, contextFromPreviousStep);
        }

        // Play every template action in this step.
        private bool PlayAction(int stepIndex, SkillStepContext contextFromPreviousStep)
        {
            // initial event - this is hand to template action
            Action<SkillStepContext> onExpired = TriggerNextStep(stepIndex + 1, TriggerEnum.OnExpired);
            Action<SkillStepContext> onHit = TriggerNextStep(stepIndex + 1, TriggerEnum.OnHit);

            // Play 1 template action
            var actionGroups = _skill.Steps[stepIndex].ActionGroups;
            foreach (SkillActionGroup actionGroup in actionGroups)
            {
                // try play the skill
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
