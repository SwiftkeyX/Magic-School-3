using System.Collections;
using UnityEngine;

/// <summary>
/// SkillTrigger tracks one hero's progress through their skill's steps (e.g. Cast -> OnExpired).
/// Owned per-hero (see BlackboardTemp) rather than static, since _currentStep is per-cast state -
/// a shared/static instance would let two heroes' casts (or two casts by the same hero) stomp
/// on each other's progress.
/// </summary>
public class SkillTrigger
{
    private int _currentStep = -1;

    // ============================================== Action ==============================================
    private void FireAction(SkillSO skill, SkillStep step, Hero caster)
    {
        float maxCastTime = 0f;

        // Play every action in this step
        foreach (SkillActionGroup actionGroup in step.ActionGroups)
        {
            // play legacy action
            float castTime = actionGroup.LegacyAction.TriggerSkill(actionGroup.Target, caster, actionGroup.Effects);
            if (castTime > maxCastTime) maxCastTime = castTime;
        }

        NextAction(skill, caster, maxCastTime);
    }

    private void NextAction(SkillSO skill, Hero caster, float castTime)
    {
        _currentStep++;

        // guard
        if (_currentStep >= skill.Steps.Count) { _currentStep = -1; return; }

        // depend on enum, go different path
        SkillStep nextStep = skill.Steps[_currentStep];
        if (nextStep.Trigger == TriggerEnum.OnExpired && castTime > 0f)
        {
            caster.StartCoroutine(FireActionAfterDelay(skill, nextStep, caster, castTime));
        }

        // else if (enum) {} ...

        else
        {
            _currentStep = -1;
        }
    }

    // coroutine for casting delay action
    private IEnumerator FireActionAfterDelay(SkillSO skill, SkillStep step, Hero caster, float delay)
    {
        yield return new WaitForSeconds(delay);
        FireAction(skill, step, caster);
    }

    private static int IndexOfStep(SkillSO skill, SkillStep step)
    {
        for (int i = 0; i < skill.Steps.Count; i++)
        {
            if (skill.Steps[i] == step) return i;
        }
        return -1;
    }

    // ============================================== Trigger ==============================================
    public bool OnCast(bool isManaFull, SkillSO skill, SkillStep step, Hero caster)
    {
        if (isManaFull)
        {
            _currentStep = IndexOfStep(skill, step);
            FireAction(skill, step, caster);
            return true;
        }

        return false;
    }

    public void OnKill(int hp, SkillSO skill, SkillStep step, Hero caster)
    {
        bool isTargetDead = (hp <= 0);

        if (isTargetDead)
        {
            _currentStep = IndexOfStep(skill, step);
            FireAction(skill, step, caster);
        }
    }
}
