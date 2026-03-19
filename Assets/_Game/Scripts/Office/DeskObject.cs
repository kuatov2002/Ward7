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
        if (_highlighted == on) return;
        _highlighted = on;
        if (_renderer != null)
        {
            _renderer.material.color = on ? _baseColor * 1.5f : _baseColor;
        }
        var anim = GetComponent<DeskObjectAnimator>();
        if (anim != null) anim.SetHighlighted(on);
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
        var audio = ProceduralAudio.Instance;

        if (isCalendar)
        {
            if (audio != null) audio.PlayPaperFlip();
            OfficeController.Instance.TryAdvanceDay();
            return;
        }

        if (!string.IsNullOrEmpty(panelName))
        {
            // Play contextual sound
            if (audio != null)
            {
                if (panelName.Contains("contact")) audio.PlayPhoneRing();
                else if (panelName.Contains("briefing")) audio.PlayStamp();
                else audio.PlayPaperFlip();
            }
            OfficeController.Instance.OpenPanel(panelName);
        }
    }
}
