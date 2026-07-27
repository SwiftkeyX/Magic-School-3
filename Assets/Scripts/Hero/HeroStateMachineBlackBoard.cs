using System.Linq;
using UnityEngine;

// BlackBoard's SRP is contain the data every statemachine use. And BlackBoard itself shouldn't have any real logic, it should act like getter & setter for other class.
// BlackBoard is similar to Hero in the sense that we dump all data here when we don't know where to put it & both shouldn't contain any real logic.
// But most of the time, we are going to violated its SRP, since we really like dump data into them.
public class HeroStateMachineBlackBoard
{
    // ================================================ dependency ================================================
    private readonly Hero _me;
    private readonly HeroDataRuntime _runtimeData;
    private BattleBoard _board;

    // ================================================ movement ================================================
    private float _moveSpeed;
    private AnimationCurve _walkCurve;
    private const float NearestEnemyTieEpsilon = 0.01f;     // How close two enemies' distances have to be to count as tied. 

    // ================================================ combat ================================================
    private Team _team;
    private AnimationCurve _attackCurve;

    // ================================================ getter ================================================
    public BattleBoard Board => _board;
    public float MoveSpeed => _moveSpeed;
    public AnimationCurve WalkCurve => _walkCurve;
    public AnimationCurve AttackCurve => _attackCurve;
    public Team Team => _team;
    // CurrentHex is a derived view of CurrentPlacement, not separately stored - it's the
    // hex the hero is on if CurrentPlacement is a Hex, or null if it's a BenchSlot (or unset).
    public Hex GetCurrentHex() => _runtimeData.CurrentPlacement as Hex;
    public Hex GetReservedHex() => _runtimeData.ReservedHex;
    public bool IsInCombat() => _runtimeData.CurrentPlacement is Hex;
    public Placement CurrentPlacement => _runtimeData.CurrentPlacement;

    // ================================================ setter ================================================
    public void SetBoard(BattleBoard board) => _board = board;
    public void SetTeam(Team team) => _team = team;
    public void SetReservedHex(Hex targetHex) => _runtimeData.SetReservedHex(targetHex);
    public void SetCurrentPlacement(Placement placement) => _runtimeData.SetCurrentPlacement(placement);

    public HeroStateMachineBlackBoard(Hero hero, HeroDataRuntime runtimeData, float moveSpeed, AnimationCurve walkCurve, AnimationCurve attackCurve)
    {
        _me = hero;
        _runtimeData = runtimeData;
        _moveSpeed = moveSpeed;
        _walkCurve = walkCurve;
        _attackCurve = attackCurve;
    }

    // Hero have several stat. So we have dedicate section for this.
    #region Stat
    // ====================================== stat getter ======================================
    public int GetAtk() => _runtimeData.Atk;
    public int GetAttackDamage() => _runtimeData.ConsumeAttackDamage(_runtimeData.Atk);
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
    public void AddModifier(StatModifier modifier) => _runtimeData.AddModifier(modifier);
    public void TickModifiers(float deltaTime, System.Action<StatModifier> onExpired) => _runtimeData.TickModifiers(deltaTime, onExpired);
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

    #region Find Enemy
    // Picks nearest enemy (if there are several nearest enemies, random it).
    public Hero FindNearestEnemy()
    {
        var enemyDistances = _board.HeroesOnBoard
            .Where(target =>
            {
                bool notTargetMyself = target != _me;
                bool notTargetFriend = target.Team != _me.Team;
                bool notTargetDead = target.State != HeroStateType.Dead;
                bool notTargetGuyNotInCombat = target.Blackboard.IsInCombat();
                return notTargetMyself && notTargetFriend && notTargetDead && notTargetGuyNotInCombat;
            })
            .Select(target => new { target, dist = Vector3.Distance(GetCurrentHex().transform.position, target.Blackboard.GetCurrentHex().transform.position) })
            .ToList();

        if (enemyDistances.Count == 0) return null;

        float nearestDist = enemyDistances.Min(e => e.dist);
        var tiedNearest = enemyDistances.Where(e => e.dist <= nearestDist + NearestEnemyTieEpsilon).Select(e => e.target).ToList();

        // Sticks with the previous pick across calls as long as it's still tied for nearest, so the target
        // doesn't flicker between equally-near enemies frame to frame.
        Hero nearestEnemy = _runtimeData.NearestEnemy;
        if (nearestEnemy != null && tiedNearest.Contains(nearestEnemy)) return nearestEnemy;

        nearestEnemy = tiedNearest[Random.Range(0, tiedNearest.Count)];
        _runtimeData.SetNearestEnemy(nearestEnemy);
        return nearestEnemy;
    }
    #endregion
}
