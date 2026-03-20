using UnityEngine;

public class DeskObject : MonoBehaviour
{
    [Tooltip("Panel name to open in UIManager")]
    public string panelName;

    [Tooltip("Player-facing name shown in interact hint")]
    public string displayName;

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

    // Panels that require context data set before opening — redirect to command center
    static readonly System.Collections.Generic.HashSet<string> _contextPanels = new() {
        "database-panel", "interrogation-panel", "location-panel",
        "confrontation-panel", "accusation-panel"
    };

    public void Interact()
    {
        var audio = ProceduralAudio.Instance;

        if (!string.IsNullOrEmpty(panelName))
        {
            if (audio != null)
            {
                if (panelName.Contains("database")) audio.PlayPhoneRing();
                else audio.PlayPaperFlip();
            }

            // Context-dependent panels must go through the command center
            string target = _contextPanels.Contains(panelName)
                ? "command-center-panel"
                : panelName;
            OfficeController.Instance.OpenPanel(target);
        }
    }
}
