using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineUI : MonoBehaviour, IPanelController
{
    const string PanelName = "timeline-panel";
    const float SlotInterval = 0.5f; // 30-minute slots (bigger, easier to click)
    const float Tolerance = 0.55f; // ±33 minutes tolerance

    readonly Dictionary<string, float> _placements = new();
    string _selectedEventId;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _placements.Clear();
        var save = ServiceLocator.Get<SaveService>();
        foreach (var p in save.Data.timelinePlacements)
        {
            var parts = p.Split(':');
            if (parts.Length == 2 && float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float h))
                _placements[parts[0]] = h;
        }
        _selectedEventId = null;
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
        if (s == null || s.timelineEvents == null || s.timelineEvents.Length == 0) return;
        int w = state.CurrentWeek;

        // Filter to only discovered events
        var discoveredEvents = new List<TimelineEventData>();
        foreach (var ev in s.timelineEvents)
        {
            if (DiscoveryHelper.IsDiscovered(ev.alwaysVisible, ev.requiredChoiceType,
                ev.requiredChoiceId, w, choices))
                discoveredEvents.Add(ev);
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

        if (discoveredEvents.Count == 0)
        {
            var noData = new Label("Недостаточно данных. Соберите больше улик и показаний.");
            noData.AddToClassList("text");
            noData.AddToClassList("text-dim");
            panel.Add(noData);
            return;
        }

        var sub = new Label(_selectedEventId != null
            ? "Теперь кликните на временной слот для размещения."
            : "Выберите событие, затем кликните на шкале времени.");
        sub.AddToClassList("text");
        sub.AddToClassList("text-dim");
        panel.Add(sub);

        int correct = CountCorrect(s, discoveredEvents);
        var score = new Label($"Размещено: {_placements.Count}/{discoveredEvents.Count}  |  Верно: {correct}");
        score.AddToClassList("text");
        score.AddToClassList("text-amber");
        panel.Add(score);

        panel.Add(Spacer(8));

        // ─── EVENT CARDS (select one) ───
        var cardRow = new VisualElement();
        cardRow.style.flexDirection = FlexDirection.Row;
        cardRow.style.flexWrap = Wrap.Wrap;
        cardRow.style.marginBottom = 8;

        foreach (var ev in discoveredEvents)
        {
            bool placed = _placements.ContainsKey(ev.eventId);
            bool selected = _selectedEventId == ev.eventId;

            var cardBtn = new Button(() => {
                _selectedEventId = (_selectedEventId == ev.eventId) ? null : ev.eventId;
                BuildPanel();
            });
            cardBtn.text = ev.description;
            cardBtn.AddToClassList("timeline-card");

            if (selected)
                cardBtn.AddToClassList("timeline-card-selected");
            else if (placed)
            {
                bool isOk = IsPlacementCorrect(ev);
                cardBtn.AddToClassList(isOk ? "timeline-card-correct" : "timeline-card-placed");
            }

            cardRow.Add(cardBtn);
        }
        panel.Add(cardRow);

        // ─── TIMELINE SCALE ───
        float startH = s.timelineStartHour;
        float endH = s.timelineEndHour;
        if (endH < startH) endH += 24f;
        float totalHours = endH - startH;
        int slotCount = Mathf.RoundToInt(totalHours / SlotInterval);

        var timelineBox = new VisualElement();
        timelineBox.style.backgroundColor = new Color(0.02f, 0.04f, 0.02f);
        timelineBox.style.paddingTop = 6;
        timelineBox.style.paddingBottom = 6;
        timelineBox.style.paddingLeft = 4;
        timelineBox.style.paddingRight = 4;
        timelineBox.style.borderTopWidth = timelineBox.style.borderBottomWidth = 1;
        timelineBox.style.borderTopColor = timelineBox.style.borderBottomColor = new Color(0.15f, 0.3f, 0.15f);

        // Hour labels
        var hourRow = new VisualElement();
        hourRow.style.flexDirection = FlexDirection.Row;
        for (float h = Mathf.Floor(startH); h <= Mathf.Ceil(endH); h += 1f)
        {
            float displayH = h >= 24f ? h - 24f : h;
            var lbl = new Label($"{(int)displayH:00}:00");
            lbl.AddToClassList("text-small");
            lbl.style.flexGrow = 1;
            lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            lbl.style.color = new Color(0.3f, 0.6f, 0.3f);
            hourRow.Add(lbl);
        }
        timelineBox.Add(hourRow);

        // Slot row
        var slotRow = new VisualElement();
        slotRow.style.flexDirection = FlexDirection.Row;
        slotRow.style.height = 55;

        for (int i = 0; i < slotCount; i++)
        {
            float rawHour = startH + i * SlotInterval;
            float slotHour = rawHour >= 24f ? rawHour - 24f : rawHour;

            var slot = new VisualElement();
            slot.style.flexGrow = 1;
            slot.style.height = 55;
            slot.style.borderRightWidth = 1;
            slot.style.borderRightColor = (i % 2 == 1)
                ? new Color(0.12f, 0.22f, 0.12f)
                : new Color(0.06f, 0.12f, 0.06f);
            slot.style.backgroundColor = new Color(0.03f, 0.06f, 0.03f);

            // Highlight on hover
            slot.RegisterCallback<PointerEnterEvent>(evt => {
                if (_selectedEventId != null)
                    ((VisualElement)evt.target).style.backgroundColor = new Color(0.08f, 0.15f, 0.08f);
            });
            slot.RegisterCallback<PointerLeaveEvent>(evt => {
                ((VisualElement)evt.target).style.backgroundColor = new Color(0.03f, 0.06f, 0.03f);
            });

            // Check if event placed here
            var placedHere = _placements.FirstOrDefault(p => Mathf.Abs(p.Value - slotHour) < 0.01f);
            if (!string.IsNullOrEmpty(placedHere.Key))
            {
                var ev = discoveredEvents.FirstOrDefault(e => e.eventId == placedHere.Key);
                if (ev == null) ev = s.timelineEvents.FirstOrDefault(e => e.eventId == placedHere.Key);
                if (ev != null)
                {
                    bool ok = IsPlacementCorrect(ev);
                    slot.style.backgroundColor = ok
                        ? new Color(0.08f, 0.25f, 0.08f)
                        : new Color(0.3f, 0.25f, 0.04f);

                    var evLbl = new Label(Truncate(ev.description, 16));
                    evLbl.AddToClassList("text-small");
                    evLbl.style.color = ok ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.9f, 0.3f);
                    evLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                    evLbl.style.whiteSpace = WhiteSpace.Normal;
                    evLbl.style.fontSize = 10;
                    slot.Add(evLbl);
                }
            }

            // Half-hour label at bottom
            int mins = (int)((slotHour % 1f) * 60f);
            int hrs = (int)slotHour;
            var timeLbl = new Label($"{hrs:00}:{mins:00}");
            timeLbl.style.fontSize = 8;
            timeLbl.style.color = new Color(0.2f, 0.4f, 0.2f);
            timeLbl.style.unityTextAlign = TextAnchor.LowerCenter;
            timeLbl.style.position = Position.Absolute;
            timeLbl.style.bottom = 1;
            timeLbl.style.left = 0;
            timeLbl.style.right = 0;
            slot.Add(timeLbl);

            float capturedHour = slotHour;
            slot.RegisterCallback<ClickEvent>(evt => OnSlotClicked(capturedHour, s, discoveredEvents));
            slotRow.Add(slot);
        }

        timelineBox.Add(slotRow);
        panel.Add(timelineBox);

        // Reset button
        panel.Add(Spacer(8));
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;

        var resetBtn = new Button(() => {
            _placements.Clear();
            var sv = ServiceLocator.Get<SaveService>();
            sv.Data.timelinePlacements.Clear();
            sv.Save();
            BuildPanel();
        });
        resetBtn.text = "СБРОСИТЬ";
        resetBtn.AddToClassList("btn-small");
        resetBtn.style.color = new Color(0.8f, 0.3f, 0.3f);
        row.Add(resetBtn);

        panel.Add(row);
    }

    void OnSlotClicked(float hour, SuspectSO s, List<TimelineEventData> discovered)
    {
        if (string.IsNullOrEmpty(_selectedEventId)) return;

        _placements.Remove(_selectedEventId);
        var existing = _placements.FirstOrDefault(p => Mathf.Abs(p.Value - hour) < 0.01f);
        if (!string.IsNullOrEmpty(existing.Key))
            _placements.Remove(existing.Key);

        _placements[_selectedEventId] = hour;
        _selectedEventId = null;

        SavePlacements();

        if (ProceduralAudio.Instance != null)
            ProceduralAudio.Instance.PlayPaperFlip();

        BuildPanel();
    }

    void SavePlacements()
    {
        var save = ServiceLocator.Get<SaveService>();
        save.Data.timelinePlacements.Clear();
        foreach (var kv in _placements)
            save.Data.timelinePlacements.Add(
                $"{kv.Key}:{kv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        save.Save();
    }

    bool IsPlacementCorrect(TimelineEventData ev)
    {
        if (!_placements.TryGetValue(ev.eventId, out float placed)) return false;
        return Mathf.Abs(NormHour(ev.correctHour) - NormHour(placed)) < Tolerance;
    }

    public int CountCorrect(SuspectSO s, List<TimelineEventData> discovered = null)
    {
        var events = discovered ?? s.timelineEvents?.ToList() ?? new List<TimelineEventData>();
        int count = 0;
        foreach (var ev in events)
            if (IsPlacementCorrect(ev)) count++;
        return count;
    }

    static float NormHour(float h)
    {
        while (h < 0) h += 24f;
        while (h >= 24f) h -= 24f;
        return h;
    }

    static string Truncate(string t, int m) =>
        string.IsNullOrEmpty(t) || t.Length <= m ? t : t.Substring(0, m) + "..";

    public void OnHide() { _selectedEventId = null; }

    static VisualElement Spacer(int h = 10)
    {
        var el = new VisualElement();
        el.style.height = h;
        return el;
    }
}
