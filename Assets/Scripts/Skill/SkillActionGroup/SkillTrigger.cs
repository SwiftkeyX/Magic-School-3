using System.Collections.Generic;

/// <summary>
/// SkillTrigger are universal condition checker. 
/// MUST only be use by other skill system.
/// </summary>
public static class SkillTrigger
{
    // ============================================== Action ==============================================
    private static void FireAction(SkillStep step, Hero caster)
    {
        // cast the actual skill
        // foreach (SkillEffect effect in step.Effects)
        //     effect.ApplyEffect(ResolveRecipients(effect));

        foreach (SkillActionGroup actionGroup in step.ActionGroups)
        {
            // play legacy action (action name)
            actionGroup.LegacyAction.TriggerSkill(actionGroup.Target, caster, actionGroup.Effects);
        }
    }

    // ============================================== Trigger ==============================================
    public static bool OnCast(bool isManaFull, SkillStep step, Hero caster)
    {
        if (isManaFull)
        {
            FireAction(step, caster);
            return true;
        }

        return false;
    }

    public static void OnKill(int hp, SkillStep step, Hero caster)
    {
        bool isTargetDead = (hp <= 0);

        if (isTargetDead) FireAction(step, caster);
    }
}