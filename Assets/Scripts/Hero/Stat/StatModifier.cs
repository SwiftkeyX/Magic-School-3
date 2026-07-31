using System.Collections.Generic;

public interface Modifier
{
    public float GetAmount();
    public ModifierEnum GetModifier();
    public float GetDuration();
}

/// <summary>
/// StatModifier are a helper for Stat:
/// 1) It contain all logic for calculating those modifier into final stat
/// 2) Update those modifier duration
/// 3) Remember all modifier for this hero
/// </summary>
public class StatModifier
{
    private const float Permanent = -1f;

    private readonly Stat _stat;
    private readonly List<ActiveModifier> _modifiers = new List<ActiveModifier>();

    public StatModifier(Stat stat)
    {
        _stat = stat;
    }

    // =================================== setter ===================================
    // update all current modifier duration, removing any that just expired
    public void Tick(float deltaTime)
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            // if permanent, skip timer
            if (float.IsPositiveInfinity(_modifiers[i].Remaining)) continue;

            // update timer
            _modifiers[i].Remaining -= deltaTime;

            // if timer is expired, remove modifier
            if (_modifiers[i].Remaining <= 0f) _modifiers.RemoveAt(i);
        }
    }

    // add new modifier
    public void AddModifier(Modifier modifier)
    {
        _modifiers.Add(new ActiveModifier(modifier));
    }

    // =================================== modified stat getter ===================================
    public int ModifiedHP()
    {
        int newHP = _stat.BaseHP;
        foreach (var modifier in _modifiers)
        {
            if (modifier.Source.GetModifier() == ModifierEnum.Heal)
            {
                newHP += (int)modifier.Source.GetAmount();
            }
        }

        return newHP;
    }

    public int ModifiedAtk()
    {
        return _stat.BaseAtk;
    }

    public int ModifiedDF()
    {
        return _stat.BaseDF;
    }

    public int ModifiedMG()
    {
        return _stat.BaseMG;
    }

    public int ModifiedMR()
    {
        return _stat.BaseMR;
    }

    public float ModifiedAttackSpeed()
    {
        return _stat.BaseAttackSpeed;
    }

    public int ModifiedRange()
    {
        return _stat.BaseRange;
    }

    public int ModifiedStartMana()
    {
        return _stat.BaseStartMana;
    }

    public int ModifiedMaxMana()
    {
        return _stat.BaseMaxMana;
    }

    // =================================== modifier helper ===================================
    public bool HasModifier(ModifierEnum type)
    {
        foreach (var modifier in _modifiers)
            if (modifier.Source.GetModifier() == type) return true;
        return false;
    }

    public float SumModifier(ModifierEnum type)
    {
        float sum = 0f;
        foreach (var modifier in _modifiers)
            if (modifier.Source.GetModifier() == type) sum += modifier.Source.GetAmount();
        return sum;
    }

    // =================================== active modifier ===================================
    private class ActiveModifier
    {
        public readonly Modifier Source;    // remember its modifier type
        public float Remaining;             // remember its duration

        public ActiveModifier(Modifier source)
        {
            Source = source;

            float duration = source.GetDuration();

            Remaining = (duration == Permanent) ? float.PositiveInfinity : duration;
        }
    }
}