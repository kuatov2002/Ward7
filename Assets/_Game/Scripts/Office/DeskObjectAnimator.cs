using UnityEngine;

public class DeskObjectAnimator : MonoBehaviour
{
    public enum AnimationType { None, PhoneVibrate, GlowPulse, StampReady }

    public AnimationType animType = AnimationType.None;

    Vector3 _basePos;
    Vector3 _baseScale;
    float _time;
    bool _isHighlighted;
    Renderer _renderer;
    Color _baseColor;

    void Start()
    {
        _basePos = transform.localPosition;
        _baseScale = transform.localScale;
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _baseColor = _renderer.material.color;
    }

    void Update()
    {
        _time += Time.deltaTime;

        switch (animType)
        {
            case AnimationType.PhoneVibrate:
                AnimatePhoneVibrate();
                break;
            case AnimationType.GlowPulse:
                AnimateGlowPulse();
                break;
            case AnimationType.StampReady:
                AnimateStampReady();
                break;
        }
    }

    public void SetHighlighted(bool on)
    {
        _isHighlighted = on;
        if (on)
            transform.localScale = _baseScale * 1.08f;
        else
            transform.localScale = _baseScale;
    }

    void AnimatePhoneVibrate()
    {
        float vibrate = Mathf.Sin(_time * 30f) * 0.003f * Mathf.Abs(Mathf.Sin(_time * 2f));
        transform.localPosition = _basePos + new Vector3(vibrate, 0f, vibrate * 0.5f);
    }

    void AnimateGlowPulse()
    {
        if (_renderer == null) return;
        float pulse = (Mathf.Sin(_time * 2f) + 1f) * 0.5f;
        _renderer.material.color = Color.Lerp(_baseColor, _baseColor * 1.4f, pulse * 0.3f);
    }

    void AnimateStampReady()
    {
        float bob = Mathf.Sin(_time * 1.5f) * 0.005f;
        transform.localPosition = _basePos + new Vector3(0f, bob, 0f);
    }
}
