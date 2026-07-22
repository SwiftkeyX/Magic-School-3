using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Use a empty main screen panel, then add each panel later:
/// 1) Bench Panel
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class BenchPanelController : MonoBehaviour
{
    // we'll add trait, shop, hero panel later
    [SerializeField] private VisualTreeAsset benchPanelAsset;

    public VisualElement BenchPanel { get; private set; }

    private void OnEnable()
    {
        // get main screen panel
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (root == null || benchPanelAsset == null)
            return;

        // find where each panel should be put inside the main screen panel
        var panel = benchPanelAsset.CloneTree()[0];
        panel.RemoveFromHierarchy();

        var panelWhereAbout = root.Q<VisualElement>(panel.name);
        if (panelWhereAbout == null)
        {
            Debug.LogWarning($"BenchPanelController: no element named '{panel.name}' found in the main document.");
            return;
        }

        // put the panel in that whereabout
        panelWhereAbout.Clear();
        panelWhereAbout.Add(panel);

        // get the reference for later use
        BenchPanel = panel.Q<VisualElement>("BenchPanel");
    }
}
