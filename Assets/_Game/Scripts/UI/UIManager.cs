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
    Label _controlsHint;
    Label _movesCounter;
    VisualElement _transitionOverlay;
    Label _transitionText;

    readonly Dictionary<string, VisualElement> _panels = new();
    string _activePanel;
    bool _initialized;

    readonly Dictionary<string, IPanelController> _controllers = new();

    // Frame-based input guard: block pointer events for 1 frame after panel opens
    int _panelOpenFrame = -1;

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
        if (_overlay == null) return;

        _crosshair = _root.Q<VisualElement>("crosshair");
        _dayHint = _root.Q<Label>("day-hint");
        _interactHint = _root.Q<Label>("interact-hint");
        _controlsHint = _root.Q<Label>("controls-hint");
        _movesCounter = _root.Q<Label>("moves-counter");
        _transitionOverlay = _root.Q<VisualElement>("transition-overlay");
        _transitionText = _root.Q<Label>("transition-text");

        string[] panelNames = {
            "main-menu-panel", "case-briefing-panel", "case-dossier-panel", "command-center-panel",
            "interrogation-panel", "location-panel", "database-panel",
            "confrontation-panel", "deduction-panel", "accusation-panel",
            "case-result-panel", "ending-panel"
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

        // Block pointer events for this frame so the click that opened the panel
        // doesn't also trigger a button inside the panel
        _panelOpenFrame = Time.frameCount;
        _overlay.pickingMode = PickingMode.Ignore;
        _panels[panelName].pickingMode = PickingMode.Ignore;

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

    static readonly HashSet<string> _uncloseable = new() {
        "main-menu-panel", "case-briefing-panel", "accusation-panel",
        "case-result-panel", "ending-panel"
    };

    public bool IsPanelOpen => _activePanel != null;

    void LateUpdate()
    {
        // Re-enable pointer events one frame after panel was opened
        if (_panelOpenFrame >= 0 && Time.frameCount > _panelOpenFrame)
        {
            _panelOpenFrame = -1;
            if (_overlay != null) _overlay.pickingMode = PickingMode.Position;
            if (_activePanel != null && _panels.ContainsKey(_activePanel))
                _panels[_activePanel].pickingMode = PickingMode.Position;
        }
    }

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

    public void ShowControlsHint()
    {
        EnsureInit();
        if (_controlsHint == null) return;
        _controlsHint.text = "Управление:\nМышь — осмотреться\nЛКМ / E — взаимодействие\nEsc / ПКМ — закрыть документ";
        _controlsHint.RemoveFromClassList("hidden");
        StartCoroutine(HideControlsAfter(8f));
    }

    IEnumerator HideControlsAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (_controlsHint != null)
            _controlsHint.AddToClassList("hidden");
    }

    public void UpdateMovesCounter(int moves, int pressurePenalty)
    {
        EnsureInit();
        if (_movesCounter == null) return;
        string penaltyStr = pressurePenalty > 0 ? $" (-{pressurePenalty})" : "";
        _movesCounter.text = $"ХОДЫ: {moves}{penaltyStr}";
        _movesCounter.RemoveFromClassList("hidden");

        // Color based on remaining moves
        if (moves <= 2)
            _movesCounter.style.color = new Color(0.9f, 0.2f, 0.2f);
        else if (moves <= 4)
            _movesCounter.style.color = new Color(1f, 0.7f, 0f);
        else
            _movesCounter.style.color = new Color(1f, 0.7f, 0f);
    }

    public void HideMovesCounter()
    {
        EnsureInit();
        if (_movesCounter != null)
            _movesCounter.AddToClassList("hidden");
    }

    public void PlayDayTransition(string text, System.Action onComplete)
    {
        StartCoroutine(DayTransitionRoutine(text, onComplete));
    }

    IEnumerator DayTransitionRoutine(string text, System.Action onComplete)
    {
        EnsureInit();
        if (_transitionOverlay == null) { onComplete?.Invoke(); yield break; }

        _transitionOverlay.RemoveFromClassList("hidden");
        _transitionOverlay.style.opacity = 0f;
        _transitionText.text = "";

        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            _transitionOverlay.style.opacity = Mathf.Clamp01(t / 0.5f);
            yield return null;
        }
        _transitionOverlay.style.opacity = 1f;

        _transitionText.text = text;
        yield return new WaitForSeconds(1.2f);

        onComplete?.Invoke();
        yield return new WaitForSeconds(0.3f);

        _transitionText.text = "";
        t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            _transitionOverlay.style.opacity = 1f - Mathf.Clamp01(t / 0.6f);
            yield return null;
        }
        _transitionOverlay.style.opacity = 0f;
        _transitionOverlay.AddToClassList("hidden");
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
