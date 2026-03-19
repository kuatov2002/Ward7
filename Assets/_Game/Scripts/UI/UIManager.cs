using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] UIDocument uiDocument;

    VisualElement _root;
    VisualElement _overlay;
    VisualElement _crosshair;
    Label _dayHint;
    Label _interactHint;

    readonly Dictionary<string, VisualElement> _panels = new();
    string _activePanel;
    bool _initialized;

    readonly Dictionary<string, IPanelController> _controllers = new();

    void Awake()
    {
        Instance = this;
    }

    void EnsureInit()
    {
        if (_initialized) return;

        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        _root = uiDocument.rootVisualElement;
        if (_root == null) return;

        _overlay = _root.Q<VisualElement>("overlay");
        if (_overlay == null) return; // UXML not loaded yet

        _crosshair = _root.Q<VisualElement>("crosshair");
        _dayHint = _root.Q<Label>("day-hint");
        _interactHint = _root.Q<Label>("interact-hint");

        string[] panelNames = {
            "main-menu-panel", "outcome-panel", "dossier-panel",
            "contact-panel", "evidence-panel", "testimony-panel",
            "interrogation-panel", "connection-panel", "timeline-panel",
            "briefing-panel", "ending-panel"
        };

        foreach (var n in panelNames)
        {
            var el = _root.Q<VisualElement>(n);
            if (el != null) _panels[n] = el;
        }

        _initialized = true;
    }

    public VisualElement GetRoot()
    {
        EnsureInit();
        return _root;
    }

    public void RegisterController(string panelName, IPanelController controller)
    {
        _controllers[panelName] = controller;
    }

    public void ShowPanel(string panelName)
    {
        EnsureInit();
        if (!_initialized || !_panels.ContainsKey(panelName)) return;

        if (_activePanel != null && _panels.ContainsKey(_activePanel))
        {
            _panels[_activePanel].AddToClassList("hidden");
            if (_controllers.ContainsKey(_activePanel))
                _controllers[_activePanel].OnHide();
        }

        _activePanel = panelName;
        _overlay.RemoveFromClassList("hidden");
        _panels[panelName].RemoveFromClassList("hidden");

        if (_controllers.ContainsKey(panelName))
            _controllers[panelName].OnShow();

        if (_crosshair != null) _crosshair.AddToClassList("hidden");
        if (_interactHint != null) _interactHint.AddToClassList("hidden");
        SetInteractionLocked(true);
    }

    public void HideAllPanels()
    {
        EnsureInit();
        if (!_initialized) return;

        if (_activePanel != null && _panels.ContainsKey(_activePanel))
        {
            _panels[_activePanel].AddToClassList("hidden");
            if (_controllers.ContainsKey(_activePanel))
                _controllers[_activePanel].OnHide();
        }
        _activePanel = null;
        if (_overlay != null) _overlay.AddToClassList("hidden");
        if (_crosshair != null) _crosshair.RemoveFromClassList("hidden");

        SetInteractionLocked(false);
    }

    // Panels that should NOT be closeable by Escape/RMB
    static readonly HashSet<string> _uncloseable = new() {
        "main-menu-panel", "outcome-panel", "briefing-panel", "ending-panel"
    };

    public bool IsPanelOpen => _activePanel != null;

    void Update()
    {
        if (_activePanel == null) return;
        if (_uncloseable.Contains(_activePanel)) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            HideAllPanels();
    }

    public void ShowInteractHint(string objectName)
    {
        EnsureInit();
        if (_interactHint == null) return;
        _interactHint.text = $"[E] {objectName}";
        _interactHint.RemoveFromClassList("hidden");
    }

    public void HideInteractHint()
    {
        if (_interactHint != null)
            _interactHint.AddToClassList("hidden");
    }

    public void ShowHint(string text)
    {
        EnsureInit();
        if (_dayHint == null) return;
        _dayHint.text = text;
        _dayHint.RemoveFromClassList("hidden");
        StartCoroutine(HideHintAfter(2f));
    }

    IEnumerator HideHintAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_dayHint != null)
            _dayHint.AddToClassList("hidden");
    }

    public void ShowDayLabel(string text)
    {
        EnsureInit();
        if (_dayHint == null) return;
        _dayHint.text = text;
        _dayHint.RemoveFromClassList("hidden");
    }

    public void HideDayLabel()
    {
        EnsureInit();
        if (_dayHint != null)
            _dayHint.AddToClassList("hidden");
    }

    void SetInteractionLocked(bool locked)
    {
        if (FirstPersonLook.Instance != null)
            FirstPersonLook.Instance.SetLocked(locked);

        var raycast = FindFirstObjectByType<RaycastInteraction>();
        if (raycast != null)
            raycast.SetLocked(locked);
    }
}

public interface IPanelController
{
    void OnShow();
    void OnHide();
}
