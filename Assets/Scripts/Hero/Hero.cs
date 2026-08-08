using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Hero don't have any logic inside it BUT:
    /// 1) It's the ONLY Monobehavior for the Hero, so it's here so we could make hero interact with Unity.
    /// 2) it act like a glue, which mean itself don't contain any real logic.
    /// 
    /// FLAGGING: We implement several interface that we thought would be useful e.g. IDamageable, IHeroStats, IPlaceable, ITargeter.
    /// But only IDamageable see a real usage. Other was no usage at all.
    /// We decide to leave it here, since it should have benefit when we implement other unit e.g. summon, etc...
    /// </summary>
    public class Hero : MonoBehaviour, ICombatant, IHeroStats, ITargeter
    {
        // ======================================== Dependency ========================================
        private HeroDataSO _SOData;
        private HeroStateMachine _stateMachine;
        private BattleBoard _board;
        private FindEnemy _findEnemy;
        private HeroVisuals _visuals;
        private HeroSkillRuntime _skillRuntime;

        // ======================================== Etc ========================================
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private AnimationCurve _walkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        // Hump-shaped (0 -> 1 -> 0): drives the attack dash out toward the enemy and back, not a one-way ease like _walkCurve.
        [SerializeField] private AnimationCurve _attackCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

        // ======================================== Runtime data ========================================
        private HeroDataRuntime _runtimeData;
        private Team _team;
        private Stat Stat => _runtimeData.Stat;

        // ======================================== other getter ========================================
        public bool IsInitialized => _runtimeData != null;
        public Team Team => _team;
        public HeroStateType StateType => _stateMachine.CurrentType;
        public bool IsDummy => _runtimeData.IsDummy;

        // ======================================== state ========================================
        public void ChangeState(HeroStateType next) => _stateMachine.ChangeState(next);

        // ======================================== board ========================================
        public ICombatant WhoReservedThisHex(Hex hex) => _board.WhoReservedThisHex(hex);
        public bool IsHexReservedByOther(Hex hex) => _board.IsReservedByOther(hex, this);
        // A hero knows which board it belongs to, so HeroMover doesn't need telling.
        // Null-checked (not `?.`) because `?.` skips Unity's fake-null: a destroyed board would
        // pass the check and then throw. Guarded at all because a hero can exist before a board,
        // e.g. spawned onto the bench.
        public void TrackOnBoard() { if (_board != null) _board.TrackThisHero(this); }
        public void UntrackFromBoard() { if (_board != null) _board.UntrackThisHero(this); }

        // ======================================== visuals ========================================
        public void SetDeadVisual() => _visuals.SetDeadVisual();
        public void PlaySkillCastEffect(string skillName) => _visuals.PlaySkillCastEffect(skillName);

        // ======================================== skill ========================================
        public bool TriggerSkill(bool isManaCapped) => _skillRuntime.TriggerSkill(isManaCapped);

        // ======================================== stat ========================================
        public bool GainMana(int amount) => Stat.AddMana(amount);      // return true if mana if capped
        public void TickModifiers(float deltaTime) => Stat.TickModifiers(deltaTime);

        // ======================================== interface method ========================================
        // === IDamageable ===
        public bool IsAlive => this != null && IsInitialized && StateType != HeroStateType.Dead;
        public void AddModifier(Modifier modifier) => Stat.AddModifier(modifier);

        public void Heal(float amount)
        {
            int healed = CombatMath.Heal(amount, Stat.CurrentHP, Stat.IsWounded);
            Stat.SetCurrentHP(healed);
        }

        public void TakeDamage(int damage)
        {
            int newHP = CombatMath.TakeDamage(damage, Stat.DF, Stat.DamageReductionPercent, Stat.CurrentHP);
            Stat.SetCurrentHP(newHP);
        }

        // === IHeroStats ===
        public int CurrentHP => Stat.CurrentHP;
        public int MaxHP => Stat.HP;
        public int CurrentMana => Stat.CurrentMana;
        public int MaxMana => Stat.MaxMana;
        public int AttackDamage => Stat.Atk;
        public float AttackSpeed => Stat.AttackSpeed;
        public int Range => Stat.Range;
        public bool IsStunned => Stat.IsStunned;
        public bool IsWounded => Stat.IsWounded;

        // === IPlaceable ===
        public Hex CurrentHex => _runtimeData.CurrentPlacement as Hex;
        public Hex ReservedHex => _runtimeData.ReservedHex;
        public Placement CurrentPlacement => _runtimeData.CurrentPlacement;
        public bool IsInCombat => _runtimeData.CurrentPlacement is Hex;
        public void SetReservedHex(Hex hex)
        {
            if (_board != null) _board.UpdateReservation(this, _runtimeData.ReservedHex, hex);

            _runtimeData.SetReservedHex(hex);
        }
        public void SetCurrentPlacement(Placement placement) => _runtimeData.SetCurrentPlacement(placement);

        // === ITargeter ===
        public ICombatant FindNearestEnemy() => _findEnemy.FindNearestEnemy();
        public ICombatant FindFurthestEnemy() => _findEnemy.FindFurthestEnemy();
        public ICombatant FindClusteredEnemy(int radius = 2) => _findEnemy.FindClusteredEnemy(radius);


        // ======================================== life cycle ========================================
        #region Life Cycle
        // Give everything a hero needs to exist, in one call. 
        public void Init(HeroDataSO data, BattleBoard board, Team team)
        {
            _SOData = data;
            _board = board;
            _team = team;

            _runtimeData = new HeroDataRuntime(_SOData);
            _visuals = GetComponent<HeroVisuals>();
            _skillRuntime = new HeroSkillRuntime(this, _SOData.Skill);
            _findEnemy = new FindEnemy(this, _runtimeData, _board);
            _stateMachine = new HeroStateMachine(this, _SOData.Skill, new MovementConfig(_moveSpeed, _walkCurve, _attackCurve));
        }

        void Start()
        {
            if (!IsInitialized) return;

            _stateMachine.Start(HeroStateType.Idle);
        }

        void Update()
        {
            if (!IsInitialized) return;

            // if combat not start, return
            if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Combat) return;

            // Some hero are not on BattleBoard but was in the bench. They don't consider in combat.
            if (!IsInCombat) return;

            TickModifiers(Time.deltaTime);

            _stateMachine.Tick();
        }
        #endregion

        // ======================================== gizmo ========================================
        #region Gizmo
        // draw gize between attacker and receiver to show which hero is attacking.
        void OnDrawGizmos()
        {
            if (!Application.isPlaying || !IsInitialized) return;
            if (StateType != HeroStateType.Attack) return;

            ICombatant target = _runtimeData.NearestEnemy;
            if (target == null || !target.IsAlive) return;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
        #endregion
    }
}
