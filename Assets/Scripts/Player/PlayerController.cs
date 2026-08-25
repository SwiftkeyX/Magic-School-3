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
        private bool _isHeroHolded = false;
        private Hero _heroHolded;
        private TeamEnum _team;

        void Awake()
        {
            _team = TeamEnum.Blue;
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            TryStartCombat();
            TryInspectHero();
            PlayerMoveHero();
        }

        // Right-click a hero to inspect it.
        private void TryInspectHero()
        {
            if (!PlayerInputSystem.InspectPressedThisFrame) return;

            // get which hero pointer hit on
            Vector3 worldPos = PlayerInputSystem.GetMouseWorldPosition(_cam);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            Hero hero = hit != null ? hit.GetComponent<Hero>() : null;

            if (hero == null) HeroSelection.Clear();
            else HeroSelection.Select(hero);
        }

        // BLOCKED on: a "Start Battle" UI button. => Now use manual space-bar to trigger the game for easy testing.
        // Lives on the player side so starting a battle is something input asks for, rather than
        // something GameManager reaches into input to discover.
        private void TryStartCombat()
        {
            if (!PlayerInputSystem.SpacePressedThisFrame) return;

            GameManager.Instance.StartCombat();
        }

        // left click hero to drag it around.
        private void PlayerMoveHero()
        {
            // if not in preparation, return;
            if (GameManager.Instance.Phase != GamePhaseEnum.Preparation) return;

            // get world position from current pointer position (mouse)
            Vector3 worldPos = PlayerInputSystem.GetMouseWorldPosition(_cam);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            // if hero is not holded by the player, polling until player click a hero
            if (!_isHeroHolded)
            {
                // if the user click/hold on the hero, continue 
                _heroHolded = hit.GetComponent<Hero>();
                bool isHeroHit = (_heroHolded != null);
                bool isHeroClicked = PlayerInputSystem.DragPressedThisFrame && isHeroHit;
                if (isHeroClicked)
                {
                    _isHeroHolded = PlayerInputSystem.IsPointerDown;
                }
            }

            // if hero in holded now, find which hex player will place the hero on
            if (_isHeroHolded)
            {
                // calculate _isHeroHolded for the next frame
                _isHeroHolded = PlayerInputSystem.IsPointerDown;

                // if the user release hero on the hex, place hero on top of the hex 
                Hex targetHex = hit.GetComponent<Hex>();
                bool isRelease = PlayerInputSystem.DragReleasedThisFrame;
                bool isHexHit = (targetHex != null);
                if (isRelease && isHexHit && ValidateHex(targetHex))
                {
                    GameManager.Instance.MoveHero(_heroHolded, targetHex);
                }
            }
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
