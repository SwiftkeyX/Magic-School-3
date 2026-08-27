using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Core;
using MagicSchool.Combat.Heroes;
using MagicSchool.Combat.Placements;

namespace MagicSchool.Player
{
    /// <summary>
    /// FLAGGING: PlayerController ref to many module without using contract.
    /// But I can't do anything since I don't see the pattern well enough to start using the interface.
    /// So I just let it ref to other module directly.
    /// BUT I think if I start making other game, I would see the pattern more clear, and understand what interface player should use 
    /// </summary>
    internal class PlayerController : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private BattleBoard _board;
        private IInspectorPanel _inspector;
        private IHeroCountPanel _heroCountPanel;
        private TeamEnum _team;
        private int HeroLimit => GameManager.Instance.HeroLimit;

        private const int CountHidden = -1;
        private int _shownHeroCount = CountHidden;
        private int _shownHeroLimit = CountHidden;

        // ===================================== holding hero var ============================== 
        private bool IsHoldingHero => _heroHolded != null;
        private Hero _heroHolded;
        private Collider2D _heldHeroHitbox;

        void Awake()
        {
            _team = TeamEnum.Blue;
        }

        // FLAGGING: Use findObjectByType maybe mess up later.
        void Start()
        {
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _inspector = behaviours.OfType<IInspectorPanel>().FirstOrDefault();
            _heroCountPanel = behaviours.OfType<IHeroCountPanel>().FirstOrDefault();
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            TryStartCombat();
            TryRestart();
            TryInspectHero();
            PlayerMoveHero();
            RefreshHeroCount();
        }

        // BLOCKED on: a "Start Battle" UI button. => Now use manual space-bar to trigger the game for easy testing.
        // Lives on the player side so starting a battle is something input asks for, rather than
        // something GameManager reaches into input to discover.
        private void TryStartCombat()
        {
            if (!PlayerInputSystem.SpacePressedThisFrame) return;

            if (GameManager.Instance.Phase == GamePhaseEnum.Result)
            {
                GameManager.Instance.ContinueFromResult();
                return;
            }

            GameManager.Instance.StartCombat();
        }

        // quick restart
        private void TryRestart()
        {
            if (!PlayerInputSystem.RestartPressedThisFrame) return;

            GameManager.Instance.Restart();
        }

        // Right-click a hero to inspect it.
        private void TryInspectHero()
        {
            if (!PlayerInputSystem.InspectPressedThisFrame) return;

            // get which hero pointer hit on
            Vector3 worldPos = PlayerInputSystem.GetMouseWorldPosition(_cam);
            Hero hero = PickAt<Hero>(worldPos);

            // right-clicking past every hero closes the panel rather than leaving it stale
            _inspector?.Inspect(hero);
        }

        // FLAGGING: move hero section could be move to its own file 
        // =================================== move hero ==================================
        // left click hero to drag it around.
        private void PlayerMoveHero()
        {
            if (GameManager.Instance.Phase != GamePhaseEnum.Preparation)
            {
                if (IsHoldingHero) CancelDrag();
                return;
            }

            // get world position from current pointer position (mouse)
            Vector3 worldPos = PlayerInputSystem.GetMouseWorldPosition(_cam);

            // if not holding hero, polling until player click on one of the hero
            if (!IsHoldingHero)
            {
                TryPickUpHero(worldPos);
                return;
            }

            // while holdin hero, hero'll move following the pointer
            _heroHolded.transform.position = new Vector3(worldPos.x, worldPos.y, _heroHolded.transform.position.z);

            // if still holding hero, return
            bool stillHolding = PlayerInputSystem.IsPointerDown && !PlayerInputSystem.DragReleasedThisFrame;
            if (stillHolding) return;

            // if releasing the hero, drop it
            DropHero(worldPos);
        }

        // pick up whichever hero the player just pressed on
        private void TryPickUpHero(Vector3 worldPos)
        {
            if (!PlayerInputSystem.DragPressedThisFrame) return;

            Hero hero = PickAt<Hero>(worldPos);
            if (hero == null) return;

            _heroHolded = hero;

            // turn off hero hitbox while holding him
            // context: hero drop logic use pointer to scan for placement hitbox,
            // but while holding hero, pointer only get hero hitbox from scanning
            // so we turn off hero hitbox.
            _heldHeroHitbox = hero.GetComponent<Collider2D>();
            if (_heldHeroHitbox != null) _heldHeroHitbox.enabled = false;
        }

        // put the hero down where the player releasing
        private void DropHero(Vector3 worldPos)
        {
            IPlacement targetPlacement = PickAt<IPlacement>(worldPos);

            // if the user release hero on the placement, place hero on top of the placement
            if (targetPlacement != null && ValidatePlacement(targetPlacement))
            {
                GameManager.Instance.MoveHero(_heroHolded, targetPlacement);
                ReleaseHero();
            }

            // if the user not release on the placement, snap hero to the same placement
            else
            {
                CancelDrag();
            }
        }

        // snap this holded hero back to its old placement
        private void CancelDrag()
        {
            IPlacement placement = _heroHolded.CurrentPlacement;

            // OnUnitPlaced re-seats the transform, so this is the snap back
            if (placement != null) placement.OnUnitPlaced(_heroHolded);

            ReleaseHero();
        }

        // when releasing hero, reset var
        private void ReleaseHero()
        {
            if (_heldHeroHitbox != null) _heldHeroHitbox.enabled = true;

            _heldHeroHitbox = null;
            _heroHolded = null;
        }

        // context: we use OverLapPoint to pick up a collider, 
        // and check if which hero this collider belong to?
        // OverLapPoint can return both hero/placement.
        // problem: Sometime when placement and hero is presented at the same spot, 
        // the hero can't be drag or inspect.
        // PickAt() is to ensure hero is always picked.
        private static T PickAt<T>(Vector3 worldPos) where T : class
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);

            foreach (Collider2D hit in hits)
            {
                T found = hit.GetComponent<T>();
                if (found != null) return found;
            }

            return null;
        }

        // check the placement before placing the hero
        private bool ValidatePlacement(IPlacement placement)
        {
            bool validate = false;

            // if placement is bench, allow the placement
            if (placement is BenchSlot)
            {
                validate = true;
            }

            // if placement is hex, check the team, hero limit 
            // if correct, allow the placement
            // p.s. Hex could only belong to either Red or Blue team
            else if (placement is Hex)
            {
                Hex targetHex = (Hex)placement;
                bool correctTeam = targetHex.GetTeam() == this._team;

                // if the hero placing here is the new hero that arn't already on the board, 
                // check if new hero count is over the allow hero limit.
                bool isThisHeroTracked = _board.HeroesOnBoard.Any(hero => (Hero)hero == _heroHolded);
                int myHeroCount = CountMyHeroes();
                if (!isThisHeroTracked) myHeroCount += 1;
                bool isHeroesCountOverFlow = myHeroCount > HeroLimit;

                validate = correctTeam && !isHeroesCountOverFlow;
            }

            return validate;
        }

        // FLAGGING: hero count section could move to its own file
        // =================================== hero count ==================================
        private void RefreshHeroCount()
        {
            if (_heroCountPanel == null) return;

            if (GameManager.Instance.Phase != GamePhaseEnum.Preparation)
            {
                if (_shownHeroCount == CountHidden) return;

                _heroCountPanel.HideHeroCount();
                _shownHeroCount = CountHidden;
                return;
            }

            int count = CountMyHeroes();
            if (count == _shownHeroCount && HeroLimit == _shownHeroLimit) return;

            _heroCountPanel.ShowHeroCount(count, HeroLimit);
            _shownHeroCount = count;
            _shownHeroLimit = HeroLimit;
        }

        private int CountMyHeroes()
        {
            int count = 0;
            foreach (ICombatant hero in _board.HeroesOnBoard)
            {
                if (hero.Team == _team) count++;
            }
            return count;
        }
    }
}
