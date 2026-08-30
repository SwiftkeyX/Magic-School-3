using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Items;

namespace MagicSchool.UI
{
    /// The item offer shown after a stage is won: three items, of which the player keeps one.
    [RequireComponent(typeof(UIDocument))]
    internal class RewardPanelController : MonoBehaviour
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

        // ================= VisualElement ======================
        private VisualElement _rewardPanel;
        private List<VisualElement> _itemSlots;

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
        #endregion

        #region Private
        // Put one item's data into one slot. 
        private void AssignItemToSlot(VisualElement slot, ItemDataSO data)
        {
            // if the data is null, use empty slot style
            slot.EnableInClassList(EmptySlotClass, data == null);

            // put the item name in the ItemName container
            Label nameLabel = slot.Q<Label>("ItemName");
            if (nameLabel != null) nameLabel.text = data != null ? data.Name : string.Empty;

            // put the item description in the ItemDescription container
            Label descriptionLabel = slot.Q<Label>("ItemDescription");
            if (descriptionLabel != null) descriptionLabel.text = data != null ? data.Description : string.Empty;
        }

        private void SetShown(bool shown)
        {
            if (_rewardPanel == null) return;

            _rewardPanel.EnableInClassList(HiddenClass, !shown);
        }
        #endregion
    }
}
