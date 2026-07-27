using System;
using System.Collections.Generic;

// One live periodic-effect instance a SkillRuntime is ticking - Zone AOE's damage tick, Galio's
// channel heal, Lux's laser DoT. ResolveRecipients is re-evaluated every tick rather than
// captured once, because an AOE's occupants can change between ticks (Zone AOE's whole point,
// per effect-types.csv) while a locked single-target effect's resolver simply returns the same
// Hero every time. Duration <= 0 means "lasts until removed some other way" (same convention as
// StatModifier's Remaining), so a passive aura (Swain's) never times out on its own.
public class ActiveEffect
{
    public readonly SkillEffect Effect;
    public readonly Func<List<Hero>> ResolveRecipients;
    public float Elapsed;
    public float SinceLastTick;

    public ActiveEffect(SkillEffect effect, Func<List<Hero>> resolveRecipients)
    {
        Effect = effect;
        ResolveRecipients = resolveRecipients;
        Elapsed = 0f;
        SinceLastTick = 0f;
    }
}
