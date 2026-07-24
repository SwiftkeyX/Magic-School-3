using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Use a empty main screen panel, then add each panel later:
/// 1) Bench Panel
/// 2) ...
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class BenchPanelController : MonoBehaviour
{
    // ================= SerializeField ======================
    [SerializeField] private VisualTreeAsset _benchPanelAsset;

    // ================= VisualElement ======================
    private VisualElement _benchPanel;
    private VisualElement _mainPanel;

    // ================= setter & getter ===================
    // ...

    private void OnEnable()
    {
        // get main screen panel
        UIDocument _main = GetComponent<UIDocument>();
        _mainPanel = _main.rootVisualElement;
        if (_mainPanel == null || _benchPanelAsset == null)
            return;

        // find bench panel's name
        VisualElement benchPanel = _benchPanelAsset.CloneTree()[0];
        benchPanel.RemoveFromHierarchy();

        // use bench panel's name - to find where the "bench panel" should be put inside the "main panel" 
        VisualElement benchPanelWhereAboutInMainPanel = _mainPanel.Q<VisualElement>(benchPanel.name);
        if (benchPanelWhereAboutInMainPanel == null)
        {
            Debug.LogWarning($"BenchPanelController: no element named '{benchPanel.name}' found in the main document.");
            return;
        }

        // put the bench panel in "it"
        benchPanelWhereAboutInMainPanel.Clear();
        benchPanelWhereAboutInMainPanel.Add(benchPanel);

        // get the reference for later use
        _benchPanel = benchPanel.Q<VisualElement>("BenchPanel");

        // Make hero slot draggable
        MakeHeroDraggable();
    }

    private void MakeHeroDraggable()
    {
        List<VisualElement> heroSlots = _benchPanel.Query<VisualElement>("HeroSlot").ToList();

        // register event to every hero slot.
        foreach (var heroSlot in heroSlots)
        {
            // PointerDownEvent = when you click and hold on "h"
            heroSlot.RegisterCallback<PointerDownEvent>(h =>
            {
                // CapturePointer = This element keep getting PointerMove/PointerUp event which is essential for dragging
                // (I don't know how it work)
                heroSlot.CapturePointer(h.pointerId);

                // testing
                Debug.Log($"Started dragging from slot: {heroSlot.name}");
            });
        }
    }
}
