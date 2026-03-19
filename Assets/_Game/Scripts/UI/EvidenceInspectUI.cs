using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Mini-game: inspect evidence zones. Limited clicks reveal details.
/// Enhanced with reveal animations, hover preview, and critical fact effects.
/// </summary>
public class EvidenceInspectUI : MonoBehaviour, IPanelController
{
    const string PanelName = "evidence-inspect-panel";
    int _currentEvIdx;
    readonly HashSet<string> _inspected = new();
    int _inspectionsLeft;
    string _lastRevealedZone;  // Track for animation

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _currentEvIdx = 0;
        _lastRevealedZone = null;
        LoadInspected();
        ShowCurrentEvidence();
    }

    void LoadInspected()
    {
        _inspected.Clear();
        var save = ServiceLocator.Get<SaveService>();
        foreach (var z in save.Data.inspectedZones)
            _inspected.Add(z);
    }

    void ShowCurrentEvidence()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var s = cases.ActiveCase;
        if (s == null || s.evidence == null || _currentEvIdx >= s.evidence.Length)
        {
            var save = ServiceLocator.Get<SaveService>();
            save.Data.evidenceInspectCompleted = true;
            save.Save();
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowPanel("evidence-panel");
            return;
        }

        var ev = s.evidence[_currentEvIdx];

        // Count remaining inspections
        int used = 0;
        int criticalFound = 0;
        int criticalTotal = 0;
        if (ev.zones != null)
        {
            for (int i = 0; i < ev.zones.Length; i++)
            {
                bool revealed = _inspected.Contains($"{ev.evidenceId}:{i}");
                if (revealed) used++;
                if (ev.zones[i].isCritical)
                {
                    criticalTotal++;
                    if (revealed) criticalFound++;
                }
            }
        }
        _inspectionsLeft = ev.maxInspections - used;

        var title = new Label($"ОСМОТР УЛИКИ: {ev.title}");
        title.AddToClassList("header");
        panel.Add(title);

        // ─── Status row ───
        var statusRow = new VisualElement();
        statusRow.style.flexDirection = FlexDirection.Row;
        statusRow.style.justifyContent = Justify.SpaceBetween;
        statusRow.style.marginBottom = 6;

        var inspLbl = new Label($"Осталось осмотров: {_inspectionsLeft}");
        inspLbl.AddToClassList("text");
        inspLbl.style.color = _inspectionsLeft > 1
            ? new Color(1f, 0.7f, 0f)
            : _inspectionsLeft == 1
                ? new Color(1f, 0.4f, 0.2f)
                : new Color(0.5f, 0.3f, 0.3f);
        statusRow.Add(inspLbl);

        // Critical facts counter (without spoiling which ones)
        if (criticalTotal > 0)
        {
            var critLbl = new Label($"Ключевых фактов: {criticalFound}/{criticalTotal}");
            critLbl.AddToClassList("text-small");
            critLbl.style.color = criticalFound == criticalTotal
                ? new Color(0.3f, 0.9f, 0.3f)
                : new Color(0.6f, 0.6f, 0.4f);
            statusRow.Add(critLbl);

            if (criticalFound == criticalTotal && criticalTotal > 0)
                UIAnimations.Pulse(critLbl, 2, 400);
        }

        panel.Add(statusRow);

        var progress = new Label($"Улика {_currentEvIdx + 1} из {s.evidence.Length}");
        progress.AddToClassList("text-small");
        progress.AddToClassList("text-dim");
        panel.Add(progress);

        panel.Add(Spacer(8));

        if (ev.zones == null || ev.zones.Length == 0)
        {
            var noZones = new Label("Нет зон для осмотра.");
            noZones.AddToClassList("text");
            panel.Add(noZones);
        }
        else
        {
            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.Center;

            for (int i = 0; i < ev.zones.Length; i++)
            {
                var zone = ev.zones[i];
                string zoneKey = $"{ev.evidenceId}:{i}";
                bool revealed = _inspected.Contains(zoneKey);
                int zi = i;
                bool justRevealed = _lastRevealedZone == zoneKey;

                var zoneEl = new VisualElement();
                zoneEl.AddToClassList("inspect-zone");

                if (revealed)
                {
                    zoneEl.AddToClassList(zone.isCritical ? "inspect-zone-critical" : "inspect-zone-revealed");
                    var detailLbl = new Label(zone.detail);
                    detailLbl.AddToClassList("text-small");
                    detailLbl.style.whiteSpace = WhiteSpace.Normal;
                    zoneEl.Add(detailLbl);

                    if (zone.isCritical)
                    {
                        var tag = new Label("[КЛЮЧЕВОЙ ФАКТ]");
                        tag.AddToClassList("text-small");
                        tag.AddToClassList("text-green");
                        zoneEl.Add(tag);

                        // Pulse critical facts
                        if (justRevealed)
                            UIAnimations.Pulse(tag, 3, 300);
                    }

                    // Animate the just-revealed zone
                    if (justRevealed)
                        UIAnimations.ScaleIn(zoneEl, 350);
                }
                else
                {
                    var btn = new Button(() => InspectZone(ev, zi));
                    btn.text = $"? {zone.label}";
                    btn.AddToClassList("inspect-zone-btn");
                    btn.SetEnabled(_inspectionsLeft > 0);
                    zoneEl.Add(btn);

                    // Hover highlight
                    if (_inspectionsLeft > 0)
                    {
                        btn.RegisterCallback<MouseEnterEvent>(evt => {
                            zoneEl.style.backgroundColor = new Color(0.15f, 0.3f, 0.15f, 0.9f);
                        });
                        btn.RegisterCallback<MouseLeaveEvent>(evt => {
                            zoneEl.style.backgroundColor = new Color(0.06f, 0.12f, 0.06f, 0.9f);
                        });
                    }
                }

                grid.Add(zoneEl);
            }

            panel.Add(grid);
        }

        panel.Add(Spacer(10));

        // ─── Inspection budget visual ───
        if (ev.maxInspections > 0)
        {
            var budgetRow = new VisualElement();
            budgetRow.style.flexDirection = FlexDirection.Row;
            budgetRow.style.justifyContent = Justify.Center;
            budgetRow.style.marginBottom = 8;

            for (int i = 0; i < ev.maxInspections; i++)
            {
                var pip = new Label("\u25CF");
                pip.style.fontSize = 12;
                pip.style.marginRight = 4;
                pip.style.color = i < used
                    ? new Color(0.3f, 0.3f, 0.3f)  // used — dim
                    : new Color(1f, 0.7f, 0f);       // remaining — bright
                budgetRow.Add(pip);
            }
            panel.Add(budgetRow);
        }

        var nextBtn = new Button(() => { _currentEvIdx++; _lastRevealedZone = null; ShowCurrentEvidence(); });
        nextBtn.text = _currentEvIdx < s.evidence.Length - 1 ? "СЛЕДУЮЩАЯ УЛИКА" : "ЗАВЕРШИТЬ ОСМОТР";
        nextBtn.AddToClassList("btn-wide");
        panel.Add(nextBtn);
    }

    void InspectZone(EvidenceData ev, int zoneIdx)
    {
        string key = $"{ev.evidenceId}:{zoneIdx}";
        if (_inspected.Contains(key)) return;

        _inspected.Add(key);
        _inspectionsLeft--;
        _lastRevealedZone = key;

        var save = ServiceLocator.Get<SaveService>();
        save.Data.inspectedZones.Add(key);
        save.Save();

        // Different sounds for critical vs normal
        if (ev.zones[zoneIdx].isCritical)
        {
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayStamp();
        }
        else
        {
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPaperFlip();
        }

        // Add critical zone facts as notes
        if (ev.zones[zoneIdx].isCritical)
        {
            var notes = ServiceLocator.Get<NoteService>();
            int w = ServiceLocator.Get<GameStateService>().CurrentWeek;
            notes.AddNote(w, ev.zones[zoneIdx].detail, $"inspect_{ev.evidenceId}");
        }

        ShowCurrentEvidence();
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    { var s = new VisualElement(); s.style.height = h; return s; }
}
