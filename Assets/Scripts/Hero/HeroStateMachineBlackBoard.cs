using System.Linq;
using UnityEngine;

// Movement/targeting data and queries shared by a Hero's state machine (Idle/Walk/Attack).
// Lives off Hero itself since nothing outside HeroStates/*.cs (and Hex.OnHeroPlaced, which
// writes the hex a hero is standing on/reserving) needs any of this.
public class HeroStateMachineBlackBoard
{
    // ================================================ dependency ================================================
    private readonly Hero _me;
    private readonly HeroDataInCombat _combatData;
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
    public Hex GetCurrentHex() => _combatData.CurrentHex;
    public Hex GetReservedHex() => _combatData.ReservedHex;
    public bool IsInCombat() => _combatData.CurrentHex != null;

    // ================================================ setter ================================================
    public void SetBoard(BattleBoard board) => _board = board;
    public void SetTeam(Team team) => _team = team;
    public void SetCurrentHex(Hex targetHex) => _combatData.SetCurrentHex(targetHex);
    public void SetReservedHex(Hex targetHex) => _combatData.SetReservedHex(targetHex);

    public HeroStateMachineBlackBoard(Hero hero, HeroDataInCombat combatData, float moveSpeed, AnimationCurve walkCurve, AnimationCurve attackCurve)
    {
        _me = hero;
        _combatData = combatData;
        _moveSpeed = moveSpeed;
        _walkCurve = walkCurve;
        _attackCurve = attackCurve;
    }

    // Hero have several stat. So we have dedicate section for this.
    #region Stat
    // ====================================== stat getter ======================================
    public int GetAtk() => _combatData.Atk;
    public int GetAttackDamage() => _combatData.ConsumeAttackDamage(_combatData.Atk);
    public float GetAttackSpeed() => _combatData.AttackSpeed;
    public int GetRange() => _combatData.Range;
    public int GetCurrentHP() => _combatData.CurrentHP;
    public int GetMaxHP() => _combatData.HP;
    public int GetCurrentMana() => _combatData.CurrentMana;
    public int GetMaxMana() => _combatData.MaxMana;

    // ====================================== stat setter ======================================
    public void GainMana(int amount) => _combatData.GainMana(amount);

    /// <summary>
    /// Take damage. Calculate damge using effective health pool formula.
    /// </summary>
    public void TakeDamage(int damage)
    {
        // EHP = HP * (1 + DF / 100) -> raw damage is worth less HP the more DF you have,
        // so divide by that same factor to get how much HP the hit actually removes.
        float mitigatedDamage = damage / (1f + _combatData.DF / 100f);
        int newHP = Mathf.Max(0, GetCurrentHP() - Mathf.RoundToInt(mitigatedDamage));
        _combatData.SetCurrentHP(newHP);
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
        Hero nearestEnemy = _combatData.NearestEnemy;
        if (nearestEnemy != null && tiedNearest.Contains(nearestEnemy)) return nearestEnemy;

        nearestEnemy = tiedNearest[Random.Range(0, tiedNearest.Count)];
        _combatData.SetNearestEnemy(nearestEnemy);
        return nearestEnemy;
    }
    #endregion
}
