using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Modifiers;

namespace MagicSchool.Combat.Heroes.Stats
{
    /// <summary>
    /// Stat of a hero.
    ///
    /// This class should only contain data and its getter & setter only.
    /// NO LOGIC.
    /// </summary>
    internal class Stat
    {
        // ========================================== dependency ==========================================
        private readonly ModifierResolver _statModifier = new ModifierResolver();

        // ========================================== base stat ==========================================
        private readonly Dictionary<StatEnum, float> _base = new Dictionary<StatEnum, float>();

        private int _currentHP;
        private int _currentMana;

        // ========================================== base stat getter ==========================================
        public float GetBaseStat(StatEnum type) => _base[type];

        // ========================================== modify stat getter ==========================================
        public float GetFinalStat(StatEnum type) => _statModifier.GetStatModifier(type, _base[type]);

        public int MaxHP => Mathf.RoundToInt(GetFinalStat(StatEnum.MaxHP));
        public int Atk => Mathf.RoundToInt(GetFinalStat(StatEnum.ATK));
        public int DF => Mathf.RoundToInt(GetFinalStat(StatEnum.DF));
        public int MG => Mathf.RoundToInt(GetFinalStat(StatEnum.AP));
        public int MR => Mathf.RoundToInt(GetFinalStat(StatEnum.MR));
        public float AttackSpeed => GetFinalStat(StatEnum.AS);
        public int Range => Mathf.RoundToInt(GetFinalStat(StatEnum.Range));
        public int StartMana => Mathf.RoundToInt(GetFinalStat(StatEnum.StartMana));
        public int MaxMana => Mathf.RoundToInt(GetFinalStat(StatEnum.MaxMana));
        public float DamageReductionPercent => Mathf.RoundToInt(GetFinalStat(StatEnum.DamageReduction));   // FLAGGING: let make this a stat for now. if it doesn't, adjust later.

        public int CurrentHP => _currentHP;
        public int CurrentMana => _currentMana;

        public bool HasStatus(ModifierEnum type) => _statModifier.GetStatusModifier(type);

        public int ActiveModifierCount => _statModifier.ActiveCount;
        public float ModifierRemaining(int index) => _statModifier.GetRemainingDuration(index);

        public bool IsStunned => HasStatus(ModifierEnum.Stun);
        public bool IsWounded => HasStatus(ModifierEnum.Wound);

        // ========================================== setter ==========================================
        public void SetCurrentHP(int value) => _currentHP = Mathf.Clamp(value, 0, MaxHP);
        // FIXLATER: I kinda skeptical about this one, but maybe it was okay.
        // let see later.
        public void SeedPools()
        {
            _currentHP = MaxHP;
            _currentMana = Mathf.Min(StartMana, MaxMana);
        }
        public void AddMana(int amount) => _currentMana += amount;
        public bool IsManaCapped() => _currentMana >= MaxMana;
        public void SpendMana() => _currentMana = 0;
        public void AddModifier(ICustomModifier modifier, float amplifier, IHeroStats casterStats, IHeroStats recipientStats) => _statModifier.AddModifier(modifier, amplifier, casterStats, recipientStats);
        public bool RemoveModifier(ICustomModifier modifier) => _statModifier.RemoveModifier(modifier);
        public void TickModifiers(float deltaTime) => _statModifier.Tick(deltaTime);


        public Stat(HeroDataSO stat)
        {
            _base[StatEnum.MaxHP] = stat.HP;
            _base[StatEnum.ATK] = stat.Atk;
            _base[StatEnum.DF] = stat.DF;
            _base[StatEnum.AP] = stat.MG;
            _base[StatEnum.MR] = stat.MR;
            _base[StatEnum.AS] = stat.AttackSpeed;
            _base[StatEnum.Range] = stat.Range;
            _base[StatEnum.StartMana] = stat.StartMana;
            _base[StatEnum.MaxMana] = stat.MaxMana;
            _base[StatEnum.DamageReduction] = 0f;

            SeedPools();
        }
    }
}
