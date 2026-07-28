using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public static class SkillTrigger
{
    // ============================================== Action ==============================================
    private static void FireAction(SkillStep step)
    {
        // cast the actual skill
        foreach (SkillEffect effect in step.Effects)
            effect.ApplyEffect(ResolveRecipients(effect));
    }

    // ============================================== Trigger ==============================================
    public static void OnCast(bool isManaFull, SkillStep step)
    {
        if (isManaFull) FireAction(step);
    }

    public static void OnKill(int hp, SkillStep step)
    {
        bool isTargetDead = (hp <= 0);

        if (isTargetDead) FireAction(step);
    }

    // ============================================== Recipient ==============================================
    private static List<Hero> ResolveRecipients(SkillEffect effect)
    {
        switch (effect.Recipient)
        {
            case EffectRecipientEnum.Self:
                return new List<Hero> { _me };
            case EffectRecipientEnum.EnemiesInArea:
                return EnemiesWithinRadius(effect.AoeRadius);
            default:
                return new List<Hero>();
        }
    }

    private static List<Hero> EnemiesWithinRadius(int radius)
    {
        Hex centerHex = _me.Blackboard.GetCurrentHex();
        if (centerHex == null) return new List<Hero>();

        return _me.Blackboard.Board.HeroesOnBoard
            .Where(h => h.Team != _me.Team && h.State != HeroStateType.Dead && centerHex.IsWithinRange(h.Blackboard.GetCurrentHex(), radius))
            .ToList();
    }
}