using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Mini-game: inspect evidence zones. Limited clicks reveal details.
/// </summary>
public class EvidenceInspectUI : MonoBehaviour, IPanelController
{
    const string PanelName = "evidence-inspect-panel"; // matches UXML name
    int _currentEvIdx;
    readonly HashSet<string> _inspected = new();
    int _inspectionsLeft;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _currentEvIdx = 0;
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
            // Done inspecting all evidence — go to evidence selection
            var save = ServiceLocator.Get<SaveService>();
            save.Data.evidenceInspectCompleted = true;
            save.Save();
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowPanel("evidence-panel");
            return;
        }

        var ev = s.evidence[_currentEvIdx];

        // Count remaining inspections for this evidence
        int used = 0;
        if (ev.zones != null)
        {
            for (int i = 0; i < ev.zones.Length; i++)
                if (_inspected.Contains($"{ev.evidenceId}:{i}")) used++;
        }
        _inspectionsLeft = ev.maxInspections - used;

        var title = new Label($"ОСМОТР УЛИКИ: {ev.title}");
        title.AddToClassList("header");
        panel.Add(title);

        var sub = new Label($"Кликните на зоны для осмотра. Осталось осмотров: {_inspectionsLeft}");
        sub.AddToClassList("text");
        sub.AddToClassList("text-amber");
        panel.Add(sub);

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
            // Grid of zones
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
                    }
                }
                else
                {
                    var btn = new Button(() => InspectZone(ev, zi));
                    btn.text = $"? {zone.label}";
                    btn.AddToClassList("inspect-zone-btn");
                    btn.SetEnabled(_inspectionsLeft > 0);
                    zoneEl.Add(btn);
                }

                grid.Add(zoneEl);
            }

            panel.Add(grid);
        }

        panel.Add(Spacer(10));

        var nextBtn = new Button(() => { _currentEvIdx++; ShowCurrentEvidence(); });
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

        var save = ServiceLocator.Get<SaveService>();
        save.Data.inspectedZones.Add(key);
        save.Save();

        if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPaperFlip();

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
