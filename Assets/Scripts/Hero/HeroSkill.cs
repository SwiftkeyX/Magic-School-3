using System.Collections;
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
            FireStep(0);
            return true;
        }

        // ============================================== Trigger condition ==============================================
        // on kill enemy, OnKill step is trigger
        private void TriggerOnKillStep(int hp, SkillStep step)
        {
            // bool isTargetDead = hp <= 0;

            // if (isTargetDead) FireAction(IndexOfStep(step));
        }

        // on previous skill expired, next step is trigger 
        private void TriggerOnExpiredStep(int nextIndex, float castTime)
        {
            _me.StartCoroutine(FireActionAfterDelay(nextIndex, castTime));
        }

        // ============================================== Action ==============================================
        // Fire skill in "step" order 
        private void FireStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _skill.Steps.Count) return;

            // step can have several action, play them all
            PlayAction(stepIndex);

            // fire next step
            FireNextStep(stepIndex + 1);
        }

        private void FireNextStep(int nextIndex)
        {
            // guard - ran off the end of the chain, nothing more to play
            if (nextIndex >= _skill.Steps.Count) return;

            // depend on enum, go different path
            SkillStep nextStep = _skill.Steps[nextIndex];

            // OnExpired, the caster fire skill after the delay e.g. Galio
            if (nextStep.Trigger == TriggerEnum.OnExpired && _castTime > 0f)
            {
                TriggerOnExpiredStep(nextIndex, _castTime);
            }

            // else if (enum) {} ...
        }

        // ============================================== helper ==============================================
        // Play every action in this step
        private void PlayAction(int stepIndex)
        {
            _castTime = 0f;

            // Play every action in this step
            var actionGroups = _skill.Steps[stepIndex].ActionGroups;
            foreach (SkillActionGroup actionGroup in actionGroups)
            {
                // get template action
                TemplateAction actionPrefab = actionGroup.TemplateAction;

                // FLAGGING: This is to complicated than it should be
                TemplateAction.Spawn(actionPrefab, actionGroup.Source, actionGroup.Target, _me, actionGroup.Effects);

                // Get cast time for this action - it is use for other skill trigger condition
                _castTime = actionPrefab.CastTime;
            }
        }

        // coroutine for casting delay action
        private IEnumerator FireActionAfterDelay(int stepIndex, float delay)
        {
            yield return new WaitForSeconds(delay);
            FireStep(stepIndex);
        }

    }
}
