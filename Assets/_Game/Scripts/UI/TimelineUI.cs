using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Timeline panel: shows a pre-built chronology of events.
/// Player must find contradictions by clicking pairs of events that don't add up.
/// </summary>
public class TimelineUI : MonoBehaviour, IPanelController
{
    const string PanelName = "timeline-panel";

    string _selectedEntry;
    readonly List<string> _found = new();
    int _attemptsUsed;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        var save = ServiceLocator.Get<SaveService>();
        _found.Clear();
        _found.AddRange(save.Data.foundContradictions);
        _attemptsUsed = save.Data.contradictionAttemptsUsed;
        _selectedEntry = null;
        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var choices = ServiceLocator.Get<DailyChoiceService>();
        var state = ServiceLocator.Get<GameStateService>();
        var s = cases.ActiveCase;
        if (s == null || s.timelineEntries == null || s.timelineEntries.Length == 0) return;
        int w = state.CurrentWeek;

        // Filter visible entries
        var visible = new List<TimelineEntryData>();
        foreach (var e in s.timelineEntries)
        {
            if (DiscoveryHelper.IsDiscovered(e.alwaysVisible, e.requiredChoiceType, e.requiredChoiceId, w, choices))
                visible.Add(e);
        }

        // Close row
        var closeRow = new VisualElement();
        closeRow.AddToClassList("close-row");
        var closeBtn = new Button(() => UIManager.Instance.HideAllPanels());
        closeBtn.text = "\u2715";
        closeBtn.AddToClassList("btn-close");
        closeRow.Add(closeBtn);
        panel.Add(closeRow);

        var title = new Label("ХРОНОЛОГИЯ СОБЫТИЙ");
        title.AddToClassList("header");
        panel.Add(title);

        int totalContr = s.timelineContradictions != null ? s.timelineContradictions.Length : 0;
        int remaining = s.maxContradictionAttempts - _attemptsUsed;

        var sub = new Label(_selectedEntry != null
            ? "Теперь кликните на событие которое противоречит выбранному."
            : "Изучите хронологию. Кликните на одно событие, затем на другое — если они противоречат друг другу, нестыковка будет зафиксирована. Например: свидетель говорит что подозреваемый уехал, но журнал показывает его вход позже.");
        sub.AddToClassList("text");
        sub.AddToClassList("text-dim");
        panel.Add(sub);

        var info = new Label($"Нестыковок найдено: {_found.Count}/{totalContr}  |  Попыток осталось: {remaining}");
        info.AddToClassList("text");
        info.AddToClassList("text-amber");
        panel.Add(info);

        panel.Add(Spacer(8));

        // ─── TIMELINE ENTRIES ───
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 400;
        scroll.style.flexGrow = 1;

        foreach (var entry in visible)
        {
            bool isSelected = _selectedEntry == entry.entryId;
            bool isPartOfFound = IsInFoundContradiction(entry.entryId);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 3;
            row.style.paddingLeft = 4;
            row.style.paddingRight = 4;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;
            row.style.backgroundColor = isSelected
                ? new Color(0.15f, 0.3f, 0.15f)
                : isPartOfFound
                    ? new Color(0.25f, 0.1f, 0.1f)
                    : new Color(0.03f, 0.06f, 0.03f);
            row.style.borderLeftWidth = 3;
            row.style.borderLeftColor = isPartOfFound
                ? new Color(0.8f, 0.3f, 0.3f)
                : new Color(0.15f, 0.3f, 0.15f);

            // Time label
            var timeLbl = new Label(entry.time);
            timeLbl.AddToClassList("text-bold");
            timeLbl.style.width = 55;
            timeLbl.style.color = new Color(0.5f, 0.9f, 0.5f);
            timeLbl.style.flexShrink = 0;
            row.Add(timeLbl);

            // Description (clickable)
            string eid = entry.entryId;
            var descBtn = new Button(() => OnEntryClicked(eid, s));
            descBtn.text = entry.description;
            descBtn.AddToClassList("timeline-entry-btn");
            if (isSelected)
                descBtn.AddToClassList("timeline-entry-selected");
            descBtn.SetEnabled(remaining > 0 || _selectedEntry != null);
            row.Add(descBtn);

            // Source tag
            if (!string.IsNullOrEmpty(entry.source))
            {
                var srcLbl = new Label(entry.source);
                srcLbl.AddToClassList("text-small");
                srcLbl.style.color = new Color(0.3f, 0.5f, 0.3f);
                srcLbl.style.flexShrink = 0;
                srcLbl.style.marginLeft = 8;
                srcLbl.style.alignSelf = Align.Center;
                row.Add(srcLbl);
            }

            scroll.Add(row);
        }

        panel.Add(scroll);

        // ─── FOUND CONTRADICTIONS ───
        if (_found.Count > 0)
        {
            panel.Add(Spacer(10));
            var foundHeader = new Label("ОБНАРУЖЕННЫЕ НЕСТЫКОВКИ:");
            foundHeader.AddToClassList("text-bold");
            foundHeader.AddToClassList("text-red");
            panel.Add(foundHeader);
            panel.Add(Spacer(4));

            foreach (var pair in _found)
            {
                var parts = pair.Split('|');
                if (parts.Length != 2) continue;
                var contr = FindContradiction(parts[0], parts[1], s);
                if (contr == null) continue;

                var entryA = GetEntry(parts[0], s);
                var entryB = GetEntry(parts[1], s);
                if (entryA == null || entryB == null) continue;

                var box = new VisualElement();
                box.AddToClassList("box");
                box.style.borderLeftWidth = 3;
                box.style.borderLeftColor = new Color(0.8f, 0.3f, 0.3f);

                var pairLbl = new Label($"{entryA.time} {entryA.description}  \u2260  {entryB.time} {entryB.description}");
                pairLbl.AddToClassList("text-bold");
                pairLbl.style.color = new Color(1f, 0.5f, 0.4f);
                box.Add(pairLbl);

                var explLbl = new Label(contr.explanation);
                explLbl.AddToClassList("text");
                box.Add(explLbl);

                panel.Add(box);
            }
        }
    }

    void OnEntryClicked(string entryId, SuspectSO s)
    {
        if (_selectedEntry == null)
        {
            _selectedEntry = entryId;
            BuildPanel();
            return;
        }

        if (_selectedEntry == entryId)
        {
            _selectedEntry = null;
            BuildPanel();
            return;
        }

        // Try pair
        string pairKey = MakePairKey(_selectedEntry, entryId);
        _selectedEntry = null;

        if (_found.Contains(pairKey))
        {
            BuildPanel();
            return;
        }

        _attemptsUsed++;
        var save = ServiceLocator.Get<SaveService>();
        save.Data.contradictionAttemptsUsed = _attemptsUsed;

        var contr = FindContradiction(pairKey.Split('|')[0], pairKey.Split('|')[1], s);
        if (contr != null)
        {
            _found.Add(pairKey);
            save.Data.foundContradictions.Add(pairKey);
            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayPaperFlip();
        }
        else
        {
            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayStamp();
        }

        save.Save();
        BuildPanel();
    }

    bool IsInFoundContradiction(string entryId)
    {
        return _found.Any(p => p.Contains(entryId));
    }

    public int GetFoundCount() => _found.Count;

    static string MakePairKey(string a, string b)
    {
        return string.Compare(a, b) < 0 ? $"{a}|{b}" : $"{b}|{a}";
    }

    static TimelineContradictionData FindContradiction(string a, string b, SuspectSO s)
    {
        if (s.timelineContradictions == null) return null;
        return s.timelineContradictions.FirstOrDefault(c =>
            (c.entryA == a && c.entryB == b) || (c.entryA == b && c.entryB == a));
    }

    static TimelineEntryData GetEntry(string id, SuspectSO s)
    {
        return s.timelineEntries?.FirstOrDefault(e => e.entryId == id);
    }

    public void OnHide() { _selectedEntry = null; }

    static VisualElement Spacer(int h = 10)
    {
        var el = new VisualElement();
        el.style.height = h;
        return el;
    }
}
