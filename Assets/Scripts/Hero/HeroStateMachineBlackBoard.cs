using UnityEngine;

/// <summary>
/// HeroStateMachineBlackBoard don't contain any real logic SIMILAR to Hero:
/// 1) It act like a additional Glue which don't contain any real logic. 
/// 
/// 2) Why would we need this class when we have Hero as a glue? 
/// We need this class because HeroStateMachine want a share data between each state. 
/// BUT we don't want to pass Hero to those state because it's also contain additional unrelated data.
/// So we create this class to be a dedicated glue for HeroStatMachine. 
/// </summary>
public class HeroStateMachineBlackBoard
{
    // ================================================ dependency ================================================
    private readonly Hero _me;
    private readonly HeroDataRuntime _runtimeData;
    private BattleBoard _board;
    private readonly BlackboardTemp _temp;

    // ================================================ movement ================================================
    private float _moveSpeed;
    private AnimationCurve _walkCurve;

    // ================================================ combat ================================================
    private Team _team;
    private AnimationCurve _attackCurve;
    private readonly FindEnemy _findEnemy;


    // ================================================ getter ================================================
    // === other dependency ===
    public BattleBoard Board => _board;
    public float MoveSpeed => _moveSpeed;
    public AnimationCurve WalkCurve => _walkCurve;
    public AnimationCurve AttackCurve => _attackCurve;
    public Team Team => _team;
    public BlackboardTemp Temp => _temp;    // Grab-bag for logic that doesn't have an obvious home yet - see BlackboardTemp for why.
    public bool IsDummy => _runtimeData.IsDummy;
    
    // === placement ===
    public Hex GetCurrentHex() => _runtimeData.CurrentPlacement as Hex;
    public Hex GetReservedHex() => _runtimeData.ReservedHex;
    public bool IsInCombat() => _runtimeData.CurrentPlacement is Hex;
    public Placement CurrentPlacement() => _runtimeData.CurrentPlacement;
    
    // === find enemy ===
    public Hero FindNearestEnemy() => _findEnemy.FindNearestEnemy();
    public Hero FindFurthestEnemy() => _findEnemy.FindFurthestEnemy();


    // ================================================ setter ================================================
    public void SetBoard(BattleBoard board) => _board = board;
    public void SetTeam(Team team) => _team = team;
    public void SetReservedHex(Hex targetHex) => _runtimeData.SetReservedHex(targetHex);
    public void SetCurrentPlacement(Placement placement) => _runtimeData.SetCurrentPlacement(placement);

    public HeroStateMachineBlackBoard(Hero hero, HeroDataRuntime runtimeData, SpriteRenderer sprite, float moveSpeed, AnimationCurve walkCurve, AnimationCurve attackCurve)
    {
        _me = hero;
        _runtimeData = runtimeData;
        _moveSpeed = moveSpeed;
        _walkCurve = walkCurve;
        _attackCurve = attackCurve;
        _temp = new BlackboardTemp(hero, sprite);
        _findEnemy = new FindEnemy(hero, runtimeData);
    }

    // Hero have several stat. So we have dedicate section for this.
    #region Stat
    // ====================================== stat getter ======================================
    public int GetAtk() => _runtimeData.Atk;
    public int GetAttackDamage() => _runtimeData.Atk;
    public float GetAttackSpeed() => _runtimeData.AttackSpeed;
    public int GetRange() => _runtimeData.Range;
    public int GetCurrentHP() => _runtimeData.CurrentHP;
    public int GetMaxHP() => _runtimeData.HP;
    public int GetCurrentMana() => _runtimeData.CurrentMana;
    public int GetMaxMana() => _runtimeData.MaxMana;
    public bool IsStunned() => _runtimeData.IsStunned;
    public bool IsWounded() => _runtimeData.IsWounded;

    // ====================================== stat setter ======================================
    public bool GainMana(int amount) => _runtimeData.GainMana(amount);      // return true if mana if capped
    public void AddModifier(Modifier modifier) => _runtimeData.AddModifier(modifier);
    public void TickModifiers(float deltaTime) => _runtimeData.TickModifiers(deltaTime);
    public void Heal(float amount)
    {
        int healed = CombatMath.Heal(amount, GetCurrentHP(), _runtimeData.IsWounded);
        _runtimeData.SetCurrentHP(healed);
    }

    public void TakeDamage(int damage)
    {
        int newHP = CombatMath.TakeDamage(damage, _runtimeData.DF, _runtimeData.DamageReductionPercent, GetCurrentHP());
        _runtimeData.SetCurrentHP(newHP);
    }
    #endregion
}
