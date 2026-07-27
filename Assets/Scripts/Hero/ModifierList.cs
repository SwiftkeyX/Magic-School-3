using System;
using System.Collections.Generic;

// Owns the accumulation/query/expiry logic for a Stat's active StatModifiers, so Stat itself
// doesn't iterate or mutate a raw list - it just holds one of these and calls through it.
public class ModifierList
{
    private readonly List<StatModifier> _modifiers = new List<StatModifier>();

    public void Add(StatModifier modifier) => _modifiers.Add(modifier);

    public float Sum(EffectDetail detail)
    {
        float sum = 0f;
        foreach (StatModifier modifier in _modifiers)
            if (modifier.Detail == detail) sum += modifier.Amount;
        return sum;
    }

    public bool Has(EffectDetail detail)
    {
        foreach (StatModifier modifier in _modifiers)
            if (modifier.Detail == detail) return true;
        return false;
    }

    public void Tick(float deltaTime, Action<StatModifier> onExpired)
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            StatModifier modifier = _modifiers[i];
            if (float.IsPositiveInfinity(modifier.Remaining)) continue;

            modifier.Remaining -= deltaTime;
            if (modifier.Remaining <= 0f)
            {
                _modifiers.RemoveAt(i);
                onExpired?.Invoke(modifier);
            }
        }
    }
}
