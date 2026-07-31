using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cast2 is the legacy action for instant self-effects (e.g. self-buffs).
/// No hitbox, no physical footprint - it applies its effects to the caster and is done.
/// e.g. Galio's Idol of Durand step 1.
/// </summary>
public class Cast2 : LegacyAction
{
    protected override float PlayLegacyAction(Hero caster, List<SkillEffect> effects, Vector3 aimTargetPosition)
    {
        _caster = caster;
        List<Hero> self = new List<Hero> { caster };

        foreach (SkillEffect effect in effects)
        {
            if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(self);
        }

        Destroy(gameObject);
        return _castTime;
    }
}
