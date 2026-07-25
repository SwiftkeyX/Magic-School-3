using System.Linq;
using UnityEngine;

public class Hero : MonoBehaviour
{
    // ==================== Dependency ====================
    // Hero need to have board ref, in order to move toward the enemy, without it hero don't know enemy whereabout
    private BattleBoard _board;
    private SpriteRenderer _sprite;
    [SerializeField] private HeroDataSO _SOData;
    private HeroStateMachine _stateMachine;

    // ==================== Etc ====================
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // Hump-shaped (0 -> 1 -> 0): drives the attack dash out toward the enemy and back, not a one-way ease like _walkCurve.
    [SerializeField] private AnimationCurve _attackCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));
    private Team _team;

    // ==================== Runtime data ========================
    private HeroDataInCombat _combatData;

    // ==================== setter & getter ====================
    // False for a Hero sitting in Prefab Mode / not yet spawned via BattleBoard.SpawnHero() -
    // _combatData only exists once Init() has run, regardless of whether Play mode is active.
    public bool IsInitialized => _combatData != null;
    public Team Team => _team;
    public HeroDataSO Stat => _SOData;
    public HeroStateType State => _stateMachine.CurrentType;
    public BattleBoard Board => _board;
    public float MoveSpeed => _moveSpeed;
    public AnimationCurve WalkCurve => _walkCurve;
    public AnimationCurve AttackCurve => _attackCurve;
    public HeroStateMachine StateMachine => _stateMachine;


    #region Preparation
    public void Init(BattleBoard board, Team team)
    {
        _board = board;
        _team = team;
    }

    // move this hero when game are in preparation state
    public void MoveThisHeroInPreParationState(Placement placement)
    {
        transform.position = placement.transform.position;
        placement.OnHeroPlaced(this);
    }
    #endregion

    #region Life Cycle
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _stateMachine = new HeroStateMachine(this);

        // initialzie combat data
        _combatData = new HeroDataInCombat(_SOData);
    }

    void Start()
    {
        _stateMachine.Start(HeroStateType.Idle);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Combat) return;
        _stateMachine.Update();
    }
    #endregion

    #region Statemachine
    // ====================================== stat getter ======================================
    public int GetAtk() => _combatData.Atk;
    // Raw Atk, with the active skill's +50% applied (and consumed) if a cast is armed.
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

    // When hero die, set his sprite transparent to indicate that he is dead.
    public void SetDeadVisual()
    {
        Color c = _sprite.color;
        c.a = 0.3f;
        _sprite.color = c;
    }


    // ====================================== position ======================================
    public Hex GetCurrentHex() => _combatData.CurrentHex;
    public Hex GetReservedHex() => _combatData.ReservedHex;
    public void SetCurrentHex(Hex targetHex) => _combatData.SetCurrentHex(targetHex);
    public void SetReservedHex(Hex targetHex) => _combatData.SetReservedHex(targetHex);

    // How close two enemies' distances have to be to count as tied. Needed because two
    // geometrically-equal distances can still differ by a hair of floating-point error.
    private const float NearestEnemyTieEpsilon = 0.01f;

    // Picks nearest enemy (if there are several nearest enemies, random it).
    public Hero FindNearestEnemy()
    {
        var enemyDistances = _board.HeroesOnBoard
            .Where(target => target != this && target.Team != _team && target.State != HeroStateType.Dead)
            .Select(target => new { target, dist = Vector3.Distance(GetCurrentHex().transform.position, target.GetCurrentHex().transform.position) })
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

    #region Gizmo
    // draw gize between attacker and receiver to show which hero is attacking.
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !IsInitialized) return;
        if (State != HeroStateType.Attack) return;

        Hero target = _combatData.NearestEnemy;
        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.transform.position);
    }
    #endregion
}
