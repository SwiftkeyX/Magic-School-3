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
        private readonly Dictionary<StatEnum, float> _base = new Dictionary<StatEnum, float>();

        private int _currentHP;
        private int _currentMana;

        // ========================================== base stat getter ==========================================
        public float GetBaseStat(StatEnum type) => _base[type];

        // ========================================== modify stat getter ==========================================
        public float GetFinalStat(StatEnum type) => _statModifier.GetStatModifier(type, _base[type]);

        public int HP => Mathf.RoundToInt(GetFinalStat(StatEnum.HP));
        public int Atk => Mathf.RoundToInt(GetFinalStat(StatEnum.Atk));
        public int DF => Mathf.RoundToInt(GetFinalStat(StatEnum.DF));
        public int MG => Mathf.RoundToInt(GetFinalStat(StatEnum.MG));
        public int MR => Mathf.RoundToInt(GetFinalStat(StatEnum.MR));
        public float AttackSpeed => GetFinalStat(StatEnum.AttackSpeed);
        public int Range => Mathf.RoundToInt(GetFinalStat(StatEnum.Range));
        public int StartMana => Mathf.RoundToInt(GetFinalStat(StatEnum.StartMana));
        public int MaxMana => Mathf.RoundToInt(GetFinalStat(StatEnum.MaxMana));
        public float DamageReductionPercent => Mathf.RoundToInt(GetFinalStat(StatEnum.DamageReduction));   // FLAGGING: let make this a stat for now. if it doesn't, adjust later.

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

        public void AddModifier(IModifier modifier) => _statModifier.AddModifier(modifier);
        public void TickModifiers(float deltaTime) => _statModifier.Tick(deltaTime);


        public Stat(HeroDataSO stat)
        {
            _base[StatEnum.HP] = stat.HP;
            _base[StatEnum.Atk] = stat.Atk;
            _base[StatEnum.DF] = stat.DF;
            _base[StatEnum.MG] = stat.MG;
            _base[StatEnum.MR] = stat.MR;
            _base[StatEnum.AttackSpeed] = stat.AttackSpeed;
            _base[StatEnum.Range] = stat.Range;
            _base[StatEnum.StartMana] = stat.StartMana;
            _base[StatEnum.MaxMana] = stat.MaxMana;
            _base[StatEnum.DamageReduction] = 0f;

            _currentHP = HP;
            _currentMana = StartMana;
        }
    }
}
