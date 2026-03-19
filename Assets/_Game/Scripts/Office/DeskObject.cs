using UnityEngine;

public class DeskObject : MonoBehaviour
{
    [Tooltip("Panel name to open in UIManager")]
    public string panelName;

    [Tooltip("Player-facing name shown in interact hint")]
    public string displayName;

    [Tooltip("-1 = always visible, 0 = outcome, 1 = monday, etc.")]
    public int visibleOnDay = -1;

    [Tooltip("If true, object is always visible when day >= visibleOnDay")]
    public bool visibleFromDayOnward;

    [Tooltip("For calendar: requires current day's choice to be made")]
    public bool isCalendar;

    Renderer _renderer;
    Renderer[] _allRenderers;
    Color _baseColor;
    bool _highlighted;
    float _flickerTimer;
    bool _flickerState;

    static readonly int ColorID = Shader.PropertyToID("_Color");
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _renderer = GetComponentInChildren<Renderer>();
        _allRenderers = GetComponentsInChildren<Renderer>();
        if (_renderer != null)
            _baseColor = _renderer.sharedMaterial.color;
    }

    void Update()
    {
        if (!_highlighted) return;

        // PSX-style flicker highlight
        _flickerTimer += Time.deltaTime;
        if (_flickerTimer >= 0.08f)
        {
            _flickerTimer = 0f;
            _flickerState = !_flickerState;
            float mult = _flickerState ? 1.8f : 1.3f;
            Color c = _baseColor * mult;
            _mpb.SetColor(ColorID, c);
            foreach (var r in _allRenderers)
            {
                if (r != null)
                    r.SetPropertyBlock(_mpb);
            }
        }
    }

    public void SetHighlight(bool on)
    {
        if (_highlighted == on) return;
        _highlighted = on;
        _flickerTimer = 0f;
        if (!on)
        {
            foreach (var r in _allRenderers)
            {
                if (r != null)
                    r.SetPropertyBlock(null);
            }
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
