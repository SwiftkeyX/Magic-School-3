using System.Collections;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// This class's SRP: To take care of trigger condition & skill order that was fired
    /// </summary>
    public class SkillTrigger
    {
        private int _currentStep = -1;

        // ============================================== Action ==============================================
        private void FireAction(SkillSO skill, Hero caster)
        {
            float maxCastTime = 0f;

            // Play every action in this step
            var steps = skill.Steps[_currentStep].ActionGroups;
            foreach (SkillActionGroup actionGroup in steps)
            {
                // play template action
                TemplateAction actionPrefab = actionGroup.TemplateAction;

                // FLAGGING: This is to complicated than it should be
                TemplateAction.Spawn(actionPrefab, actionGroup.Source, actionGroup.Target, caster, actionGroup.Effects);

                // Get cast time - it is use for other skill trigger condition
                float castTime = actionPrefab.CastTime;
                if (castTime > maxCastTime) maxCastTime = castTime;
            }

            PlayNextStep(skill, caster, maxCastTime);
        }

        private void PlayNextStep(SkillSO skill, Hero caster, float castTime)
        {
            _currentStep++;

            // guard
            if (_currentStep >= skill.Steps.Count) { _currentStep = -1; return; }

            // depend on enum, go different path
            SkillStep nextStep = skill.Steps[_currentStep];

            // OnExpired, the caster fire skill after the delay e.g. Galio
            if (nextStep.Trigger == TriggerEnum.OnExpired && castTime > 0f)
            {
                caster.StartCoroutine(FireActionAfterDelay(skill, caster, castTime));
            }

            // else if (enum) {} ...

            else
            {
                _currentStep = -1;
            }
        }

        // coroutine for casting delay action
        private IEnumerator FireActionAfterDelay(SkillSO skill, Hero caster, float delay)
        {
            yield return new WaitForSeconds(delay);
            FireAction(skill, caster);
        }

        // find current step's number
        private int IndexOfStep(SkillSO skill, SkillStep step)
        {
            for (int i = 0; i < skill.Steps.Count; i++)
            {
                if (skill.Steps[i] == step) return i;
            }
            return -1;
        }

        // ============================================== Trigger ==============================================
        // mana is full, OnCast skill is trigger 
        public bool OnCast(bool isManaFull, SkillSO skill, Hero caster)
        {
            if (isManaFull)
            {
                // OnCast skill is always step 0 
                _currentStep = 0;
                FireAction(skill, caster);
                return true;
            }

            return false;
        }

        // on kill enemy, OnKill skill is trigger
        public void OnKill(int hp, SkillSO skill, SkillStep step, Hero caster)
        {
            bool isTargetDead = (hp <= 0);

            if (isTargetDead)
            {
                _currentStep = IndexOfStep(skill, step);
                FireAction(skill, caster);
            }
        }
    }
}
