using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Stat of a hero.
    ///
    /// This class should only contain data and its getter & setter only.
    /// NO LOGIC.
    /// </summary>
    public class Stat
    {
        // ========================================== dependency ==========================================
        private readonly ModifierResolver _statModifier = new ModifierResolver();

        // ========================================== base stat ==========================================
        private readonly Dictionary<StatType, float> _base = new Dictionary<StatType, float>();

        private int _currentHP;
        private int _currentMana;

        // ========================================== base stat getter ==========================================
        public float GetBaseStat(StatType type) => _base[type];

        // ========================================== modify stat getter ==========================================
        public float GetFinalStat(StatType type) => _statModifier.GetStatModifier(type, _base[type]);

        public int HP => Mathf.RoundToInt(GetFinalStat(StatType.HP));
        public int Atk => Mathf.RoundToInt(GetFinalStat(StatType.Atk));
        public int DF => Mathf.RoundToInt(GetFinalStat(StatType.DF));
        public int MG => Mathf.RoundToInt(GetFinalStat(StatType.MG));
        public int MR => Mathf.RoundToInt(GetFinalStat(StatType.MR));
        public float AttackSpeed => GetFinalStat(StatType.AttackSpeed);
        public int Range => Mathf.RoundToInt(GetFinalStat(StatType.Range));
        public int StartMana => Mathf.RoundToInt(GetFinalStat(StatType.StartMana));
        public int MaxMana => Mathf.RoundToInt(GetFinalStat(StatType.MaxMana));
        public float DamageReductionPercent => Mathf.RoundToInt(GetFinalStat(StatType.DamageReduction));   // FLAGGING: let make this a stat for now. if it doesn't, adjust later.

        public int CurrentHP => _currentHP;
        public int CurrentMana => _currentMana;

        public bool HasStatus(ModifierEnum type) => _statModifier.GetStatusModifier(type);

        public bool IsStunned => HasStatus(ModifierEnum.Stun);
        public bool IsWounded => HasStatus(ModifierEnum.Wound);

        // ========================================== setter ==========================================
        public void SetCurrentHP(int value) => _currentHP = Mathf.Clamp(value, 0, HP);
        public void AddMana(int amount) => _currentMana += amount;
        public bool IsManaCapped() => _currentMana >= MaxMana;
        public void SpendMana() => _currentMana = 0;

        public void AddModifier(Modifier modifier) => _statModifier.AddModifier(modifier);
        public void TickModifiers(float deltaTime) => _statModifier.Tick(deltaTime);


        public Stat(HeroDataSO stat)
        {
            _base[StatType.HP] = stat.HP;
            _base[StatType.Atk] = stat.Atk;
            _base[StatType.DF] = stat.DF;
            _base[StatType.MG] = stat.MG;
            _base[StatType.MR] = stat.MR;
            _base[StatType.AttackSpeed] = stat.AttackSpeed;
            _base[StatType.Range] = stat.Range;
            _base[StatType.StartMana] = stat.StartMana;
            _base[StatType.MaxMana] = stat.MaxMana;
            _base[StatType.DamageReduction] = 0f;

            _currentHP = HP;
            _currentMana = StartMana;
        }
    }
}
