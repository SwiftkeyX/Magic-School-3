using System;
using System.Collections.Generic;

public class ActiveEffect
{
    public readonly SkillEffect Effect;
    public readonly Func<List<Hero>> ResolveRecipients;     // Recipients that got affect by the "effect"
    public float DurationStarted;       // use with EffectDuration, to be the start time of the skill. 
    public float CadenceLastTick;       // use with EffectCadence, to calculate the last time effect was apply according to EffectCadence 

    public ActiveEffect(SkillEffect effect, Func<List<Hero>> resolveRecipients)
    {
        Effect = effect;
        ResolveRecipients = resolveRecipients;
        DurationStarted = 0f;
        CadenceLastTick = 0f;
    }
}
