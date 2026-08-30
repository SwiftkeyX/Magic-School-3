using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Contracts;
using MagicSchool.Items;

namespace MagicSchool.UI
{
    // FIXLATER: I notice that most of our UI panel do the same thing. Could we apply DRY here?
    /// The item offer shown after a stage is won: three items, of which the player keeps one.
    [RequireComponent(typeof(UIDocument))]
    internal class RewardPanelController : MonoBehaviour, IRewardPanel
    {
        // style for empty reward slot 
        private const string EmptySlotClass = "item-slot--empty";

        // style for hiding the container
        private const string HiddenClass = "is-hidden";

        // ================= SerializeField ======================
        [SerializeField] private VisualTreeAsset _rewardPanelAsset;

        [Tooltip("The three items offered. Fewer than the card has slots is fine - the rest go dim.")]
        [SerializeField] private List<ItemDataSO> _offeredItems = new List<ItemDataSO>();

        [Tooltip("Draw the card as soon as the scene starts. Off once a stage win is what opens it.")]
        [SerializeField] private bool _showOnStart = true;

        [Tooltip("Where a picked item lands in the world, for the player to drag onto a hero.")]
        [SerializeField] private Vector3 _spawnPosition = new Vector3(-5.6f, -2f, 0f);

        [Tooltip("How far along x each further pick lands, so they do not pile up on one spot.")]
        [SerializeField] private float _spawnSpacing = 0.6f;

        // ================= VisualElement ======================
        private VisualElement _rewardPanel;
        
        // the slot of the offered item
        private List<VisualElement> _itemSlots;

        // the items
        private readonly Dictionary<VisualElement, ItemDataSO> _items = new Dictionary<VisualElement, ItemDataSO>();

        // ================= other ======================
        // how many picks have been spawned, so the next spawned one lands beside the previous one
        private int _spawnedCount;

        // ================= getter ======================
        public bool IsShown => _rewardPanel != null && !_rewardPanel.ClassListContains(HiddenClass);

        #region Initialize Panel
        private void OnEnable()
        {
            // get main screen panel
            UIDocument main = GetComponent<UIDocument>();
            VisualElement mainPanel = main.rootVisualElement;
            if (mainPanel == null || _rewardPanelAsset == null) return;

            // put this panel in the slot of the main panel that shares its name
            VisualElement rewardPanel = PanelMounter.MountInMainPanel(mainPanel, _rewardPanelAsset);
            if (rewardPanel == null) return;

            _rewardPanel = rewardPanel;
            _itemSlots = rewardPanel.Query<VisualElement>("ItemSlot").ToList();

            // registered once here rather than per offer, so re-filling the card cannot stack
            // a second callback on the same slot
            foreach (VisualElement slot in _itemSlots)
                slot.RegisterCallback<ClickEvent>(_ => Pick(slot));

            ShowOffer(_offeredItems);
            SetShown(_showOnStart);
        }
        #endregion

        #region Public
        // Put a set of items on the card. 
        public void ShowOffer(IReadOnlyList<ItemDataSO> offer)
        {
            if (_itemSlots == null) return;

            for (int slot = 0; slot < _itemSlots.Count; slot++)
            {
                ItemDataSO data = offer != null && slot < offer.Count ? offer[slot] : null;

                AssignItemToSlot(_itemSlots[slot], data);
            }
        }

        public void Show() => SetShown(true);
        public void Hide() => SetShown(false);

        // === IRewardPanel ===
        public void ShowReward()
        {
            if (!HasAnyOffer()) return;

            Show();
        }

        // the card is only up while a pick is still owed - Pick hides it
        public bool IsChoosing => IsShown;
        #endregion

        #region Private
        private bool HasAnyOffer()
        {
            foreach (ItemDataSO data in _items.Values)
                if (data != null) return true;

            return false;
        }

        // Put one item's data into one slot. 
        private void AssignItemToSlot(VisualElement slot, ItemDataSO data)
        {
            _items[slot] = data;

            // if the data is null, use empty slot style
            slot.EnableInClassList(EmptySlotClass, data == null);

            // put the item name in the ItemName container
            Label nameLabel = slot.Q<Label>("ItemName");
            if (nameLabel != null) nameLabel.text = data != null ? data.Name : string.Empty;

            // put the item description in the ItemDescription container
            Label descriptionLabel = slot.Q<Label>("ItemDescription");
            if (descriptionLabel != null) descriptionLabel.text = data != null ? data.Description : string.Empty;
        }

        // Clicking an offer, to spawns that item into the world and closes the card. 
        private void Pick(VisualElement slot)
        {
            if (!IsShown) return;

            // an empty slot has nothing to give
            if (!_items.TryGetValue(slot, out ItemDataSO data) || data == null) return;

            Vector3 where = _spawnPosition + new Vector3(_spawnedCount * _spawnSpacing, 0f, 0f);
            if (Item.Spawn(data, where) == null) return;

            _spawnedCount++;
            Hide();
        }

        private void SetShown(bool shown)
        {
            if (_rewardPanel == null) return;

            _rewardPanel.EnableInClassList(HiddenClass, !shown);
        }
        #endregion
    }
}
