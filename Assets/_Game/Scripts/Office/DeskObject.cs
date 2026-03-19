using UnityEngine;

public class DeskObject : MonoBehaviour
{
    [Tooltip("Panel name to open in UIManager (e.g. dossier-panel, contact-panel)")]
    public string panelName;

    [Tooltip("-1 = always visible, 0 = outcome, 1 = monday, etc.")]
    public int visibleOnDay = -1;

    [Tooltip("If true, object is always visible when day >= visibleOnDay")]
    public bool visibleFromDayOnward;

    [Tooltip("For calendar: requires current day's choice to be made")]
    public bool isCalendar;

    Renderer _renderer;
    Color _baseColor;
    bool _highlighted;
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _baseColor = _renderer.material.color;
    }

    public void SetHighlight(bool on)
    {
        if (_highlighted == on || _renderer == null) return;
        _highlighted = on;
        if (on)
            _renderer.material.color = _baseColor * 1.5f;
        else
            _renderer.material.color = _baseColor;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public bool ShouldBeVisible(int currentDay)
    {
        if (visibleOnDay < 0) return true;
        if (visibleFromDayOnward) return currentDay >= visibleOnDay;
        return currentDay == visibleOnDay;
    }

    public void Interact()
    {
        if (isCalendar)
        {
            OfficeController.Instance.TryAdvanceDay();
            return;
        }

        if (!string.IsNullOrEmpty(panelName))
            OfficeController.Instance.OpenPanel(panelName);
    }
}
