using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI animation helpers for VisualElements — shake, pulse, fade-in, flash.
/// All use USS transitions or scheduled callbacks (no coroutines needed).
/// </summary>
public static class UIAnimations
{
    /// <summary>
    /// Shake an element horizontally. Uses schedule callbacks.
    /// </summary>
    public static void Shake(VisualElement el, float intensity = 6f, int durationMs = 300)
    {
        int steps = durationMs / 30;
        int step = 0;

        el.schedule.Execute(() =>
        {
            if (step >= steps)
            {
                el.style.translate = new Translate(0, 0);
                return;
            }
            float offset = Mathf.Sin(step * Mathf.PI * 2f) * intensity * (1f - (float)step / steps);
            el.style.translate = new Translate(offset, 0);
            step++;
        }).Every(30).Until(() => step >= steps);
    }

    /// <summary>
    /// Pulse an element's opacity. Loops count times.
    /// </summary>
    public static void Pulse(VisualElement el, int count = 3, int periodMs = 400)
    {
        int totalSteps = count * 2;
        int step = 0;

        el.schedule.Execute(() =>
        {
            if (step >= totalSteps)
            {
                el.style.opacity = 1f;
                return;
            }
            el.style.opacity = step % 2 == 0 ? 0.4f : 1f;
            step++;
        }).Every(periodMs / 2).Until(() => step >= totalSteps);
    }

    /// <summary>
    /// Flash a border color briefly then revert.
    /// </summary>
    public static void FlashBorder(VisualElement el, Color flashColor, int durationMs = 400)
    {
        var origColor = el.resolvedStyle.borderLeftColor;
        el.style.borderLeftColor = flashColor;
        el.style.borderRightColor = flashColor;
        el.style.borderTopColor = flashColor;
        el.style.borderBottomColor = flashColor;

        el.schedule.Execute(() =>
        {
            el.style.borderLeftColor = origColor;
            el.style.borderRightColor = origColor;
            el.style.borderTopColor = origColor;
            el.style.borderBottomColor = origColor;
        }).ExecuteLater(durationMs);
    }

    /// <summary>
    /// Fade in an element from 0 to 1 opacity.
    /// </summary>
    public static void FadeIn(VisualElement el, int durationMs = 300)
    {
        el.style.opacity = 0;
        int steps = durationMs / 30;
        int step = 0;

        el.schedule.Execute(() =>
        {
            step++;
            el.style.opacity = Mathf.Clamp01((float)step / steps);
        }).Every(30).Until(() => step >= steps);
    }

    /// <summary>
    /// Scale-up entrance animation (0.8 → 1.0 scale).
    /// </summary>
    public static void ScaleIn(VisualElement el, int durationMs = 250)
    {
        el.style.scale = new Scale(new Vector3(0.85f, 0.85f, 1f));
        el.style.opacity = 0;
        int steps = durationMs / 25;
        int step = 0;

        el.schedule.Execute(() =>
        {
            step++;
            float t = Mathf.Clamp01((float)step / steps);
            float eased = 1f - (1f - t) * (1f - t); // ease-out quad
            el.style.scale = new Scale(Vector3.Lerp(new Vector3(0.85f, 0.85f, 1f), Vector3.one, eased));
            el.style.opacity = eased;
        }).Every(25).Until(() => step >= steps);
    }

    /// <summary>
    /// Slide in from left.
    /// </summary>
    public static void SlideInLeft(VisualElement el, int durationMs = 300, float distance = 30f)
    {
        el.style.translate = new Translate(-distance, 0);
        el.style.opacity = 0;
        int steps = durationMs / 25;
        int step = 0;

        el.schedule.Execute(() =>
        {
            step++;
            float t = Mathf.Clamp01((float)step / steps);
            float eased = 1f - (1f - t) * (1f - t);
            el.style.translate = new Translate(-distance * (1f - eased), 0);
            el.style.opacity = eased;
        }).Every(25).Until(() => step >= steps);
    }
}
