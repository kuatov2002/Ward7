using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// PSX-style visuals via URP render scale reduction.
/// Lowers the internal rendering resolution for a pixelated look.
/// </summary>
public class PSXEffect : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1f)] float renderScale = 0.35f;

    float _originalScale;
    UniversalRenderPipelineAsset _urpAsset;

    void Start()
    {
        _urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (_urpAsset != null)
        {
            _originalScale = _urpAsset.renderScale;
            _urpAsset.renderScale = renderScale;
        }

        // Disable anti-aliasing for crispy pixels
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.allowMSAA = false;
            cam.allowHDR = false;
        }

        QualitySettings.antiAliasing = 0;
    }

    void OnDestroy()
    {
        // Restore original scale
        if (_urpAsset != null)
            _urpAsset.renderScale = _originalScale;
    }
}
