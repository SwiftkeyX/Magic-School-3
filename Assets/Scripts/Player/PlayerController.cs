using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Core;
using MagicSchool.Combat.Placements;

namespace MagicSchool.Player
{
    /// FLAGGING: PlayerController ref to many module without using contract.
    /// But I can't do anything since I don't see the pattern well enough to start using the interface.
    /// So I just let it ref to other module directly.
    /// BUT I think if I start making other game, I would see the pattern more clear, and understand what interface player should use
    internal class PlayerController : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private BattleBoard _board;
        private IInspectorPanel _inspector;
        private IHeroCountPanel _heroCountPanel;
        private ISellZone _sellZone;
        private Dragging _dragging;
        private TeamEnum _team;
        private int HeroLimit => GameManager.Instance.HeroLimit;

        private const int CountHidden = -1;
        private int _shownHeroCount = CountHidden;
        private int _shownHeroLimit = CountHidden;

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
            _sellZone = behaviours.OfType<ISellZone>().FirstOrDefault();
            _dragging = new Dragging(_cam, _board, _sellZone, _team);
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            TryStartCombat();
            TryRestart();
            TryInspect();
            _dragging?.Tick();
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

        // Right-click a hero or an item to inspect it.
        private void TryInspect()
        {
            if (!PlayerInputSystem.InspectPressedThisFrame) return;

            // get which IInspectable that the pointer hit on
            Vector3 worldPos = PlayerInputSystem.GetMouseWorldPosition(_cam);
            IInspectable target = Picker.At<IInspectable>(worldPos);

            // inspect the target
            _inspector?.Inspect(target);
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

            int count = _board.CountTeamOnBoard(_team);
            if (count == _shownHeroCount && HeroLimit == _shownHeroLimit) return;

            _heroCountPanel.ShowHeroCount(count, HeroLimit);
            _shownHeroCount = count;
            _shownHeroLimit = HeroLimit;
        }
    }
}
