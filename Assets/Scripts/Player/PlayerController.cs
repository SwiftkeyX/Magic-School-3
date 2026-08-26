using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Core;
using MagicSchool.Combat.Heroes;
using MagicSchool.Combat.Placements;

namespace MagicSchool.Player
{
    internal class PlayerController : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        private bool IsHoldingHero => _heroHolded != null;
        private Hero _heroHolded;
        private Collider2D _heldHeroHitbox;
        private TeamEnum _team;
        private IInspectorPanel _inspector;

        void Awake()
        {
            _team = TeamEnum.Blue;
        }

        // FLAGGING: this wants to be a [SerializeField] and cannot be one - Unity does not
        // serialize interface fields, so the reference would just sit there null. Doing it
        // properly means a MonoBehaviour field plus a RequireInterface PropertyDrawer to stop
        // the wrong component being dragged in (ShowIfDrawer in Core/Editor is the pattern).
        // Deliberately not done: the scan runs once and costs nothing at runtime. The thing it
        // actually costs is that the wiring is invisible in the Inspector.
        void Start()
        {
            _inspector = FindAnyInspector();
        }

        private static IInspectorPanel FindAnyInspector()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IInspectorPanel>()
                .FirstOrDefault();
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            TryStartCombat();
            TryRestart();
            TryInspectHero();
            PlayerMoveHero();
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
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            Hero hero = hit != null ? hit.GetComponent<Hero>() : null;

            // right-clicking past every hero closes the panel rather than leaving it stale
            _inspector?.Inspect(hero);
        }


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

            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            Hero hero = hit != null ? hit.GetComponent<Hero>() : null;
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
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            Hex targetHex = hit != null ? hit.GetComponent<Hex>() : null;

            // if the user release hero on the hex, place hero on top of the hex
            if (targetHex != null && ValidateHex(targetHex))
            {
                GameManager.Instance.MoveHero(_heroHolded, targetHex);
                ReleaseHero();
                return;
            }

            // if the user not release on the placement, snap hero to the same placement
            CancelDrag();
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

        // If team of that hex is the same to hero's team, allow the placement
        // p.s. Hex could only belong to either Red or Blue team
        private bool ValidateHex(Hex targetHex)
        {
            bool correctTeam = targetHex.GetTeam() == this._team;
            if (!correctTeam) Debug.LogWarning("Player place hero in the wrong hex!");
            return correctTeam;
        }
    }
}
