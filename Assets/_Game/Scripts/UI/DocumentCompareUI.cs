using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Mini-game: two documents side by side, find discrepancies.
/// Enhanced with hover highlighting, mark animations, and result animations.
/// </summary>
public class DocumentCompareUI : MonoBehaviour, IPanelController
{
    const string PanelName = "doccompare-panel";
    readonly HashSet<int> _marked = new();
    bool _submitted;
    int _hoveredRow = -1;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _marked.Clear();
        _submitted = false;
        _hoveredRow = -1;
        Build();
    }

    void Build()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var s = cases.ActiveCase;
        if (s == null || s.documentCompare == null || s.documentCompare.lines == null)
        {
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowPanel("dossier-panel");
            return;
        }

        var dc = s.documentCompare;

        var title = new Label("СОПОСТАВЛЕНИЕ ДОКУМЕНТОВ");
        title.AddToClassList("header");
        panel.Add(title);

        var sub = new Label(_submitted
            ? "Результат проверки:"
            : "Сравните два документа. Кликните на строки с расхождениями, затем подтвердите.");
        sub.AddToClassList("text");
        sub.AddToClassList("text-dim");
        panel.Add(sub);

        panel.Add(Spacer(8));

        // Headers row
        var headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        var leftH = new Label(dc.leftTitle);
        leftH.AddToClassList("text-bold");
        leftH.AddToClassList("text-amber");
        leftH.style.width = Length.Percent(48);
        headerRow.Add(leftH);
        var spacerV = new VisualElement();
        spacerV.style.width = Length.Percent(4);
        headerRow.Add(spacerV);
        var rightH = new Label(dc.rightTitle);
        rightH.AddToClassList("text-bold");
        rightH.AddToClassList("text-amber");
        rightH.style.width = Length.Percent(48);
        headerRow.Add(rightH);
        panel.Add(headerRow);

        panel.Add(Spacer(5));

        // Lines
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.maxHeight = 350;

        for (int i = 0; i < dc.lines.Length; i++)
        {
            var line = dc.lines[i];
            int idx = i;
            bool marked = _marked.Contains(i);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 3;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;

            if (_submitted)
            {
                bool wasMarked = marked;
                bool correct = wasMarked && line.isDiscrepancy;
                bool missed = !wasMarked && line.isDiscrepancy;
                bool falseAlarm = wasMarked && !line.isDiscrepancy;

                Color bg = correct ? new Color(0.1f, 0.3f, 0.1f)
                    : missed ? new Color(0.3f, 0.15f, 0.1f)
                    : falseAlarm ? new Color(0.3f, 0.25f, 0.05f)
                    : new Color(0.03f, 0.06f, 0.03f);
                row.style.backgroundColor = bg;

                // Result icon
                string icon = correct ? "[OK]" : missed ? "[!!]" : falseAlarm ? "[X]" : "";
                if (!string.IsNullOrEmpty(icon))
                {
                    var iconLbl = new Label(icon);
                    iconLbl.style.width = 30;
                    iconLbl.AddToClassList("text-small");
                    iconLbl.style.color = correct ? new Color(0.3f, 0.9f, 0.3f) :
                        missed ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.9f, 0.7f, 0.2f);
                    row.Add(iconLbl);
                }

                string leftColor = line.isDiscrepancy ? "text-red" : "text-green";
                AddLineCell(row, line.leftText, leftColor, 45);
                AddSpacer(row, 4);
                AddLineCell(row, line.rightText, leftColor, 45);

                // Staggered result animation
                UIAnimations.FadeIn(row, 100 + i * 50);
            }
            else
            {
                row.style.backgroundColor = marked
                    ? new Color(0.25f, 0.15f, 0.05f)
                    : new Color(0.03f, 0.06f, 0.03f);
                row.style.borderLeftWidth = marked ? 3 : 0;
                row.style.borderLeftColor = new Color(1f, 0.6f, 0.2f);

                // Hover effect — highlight both sides simultaneously
                row.RegisterCallback<MouseEnterEvent>(evt => {
                    if (_submitted) return;
                    _hoveredRow = idx;
                    row.style.backgroundColor = marked
                        ? new Color(0.3f, 0.2f, 0.08f)
                        : new Color(0.08f, 0.12f, 0.08f);
                    row.style.borderRightWidth = 1;
                    row.style.borderRightColor = new Color(0.5f, 0.5f, 0.3f);
                });
                row.RegisterCallback<MouseLeaveEvent>(evt => {
                    if (_submitted) return;
                    _hoveredRow = -1;
                    row.style.backgroundColor = marked
                        ? new Color(0.25f, 0.15f, 0.05f)
                        : new Color(0.03f, 0.06f, 0.03f);
                    row.style.borderRightWidth = 0;
                });

                row.RegisterCallback<ClickEvent>(evt => {
                    if (_submitted) return;
                    if (_marked.Contains(idx))
                    {
                        _marked.Remove(idx);
                        // Unmark animation
                        row.style.borderLeftWidth = 0;
                    }
                    else
                    {
                        _marked.Add(idx);
                        // Mark animation - flash
                        UIAnimations.FlashBorder(row, new Color(1f, 0.8f, 0.2f), 300);
                    }
                    if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPaperFlip();
                    Build();
                });

                // Mark indicator
                if (marked)
                {
                    var markIcon = new Label("[*]");
                    markIcon.style.width = 24;
                    markIcon.AddToClassList("text-small");
                    markIcon.style.color = new Color(1f, 0.6f, 0.2f);
                    row.Add(markIcon);
                }

                AddLineCell(row, line.leftText, "text", marked ? 45 : 48);
                AddSpacer(row, marked ? 3 : 4);
                AddLineCell(row, line.rightText, "text", marked ? 45 : 48);
            }

            scroll.Add(row);
        }

        panel.Add(scroll);
        panel.Add(Spacer(8));

        if (_submitted)
        {
            int correct = 0, missed = 0, falseAlarm = 0;
            var save = ServiceLocator.Get<SaveService>();
            var notes = ServiceLocator.Get<NoteService>();
            int w = ServiceLocator.Get<GameStateService>().CurrentWeek;

            for (int i = 0; i < dc.lines.Length; i++)
            {
                bool wasMarked = _marked.Contains(i);
                if (wasMarked && dc.lines[i].isDiscrepancy)
                {
                    correct++;
                    if (!string.IsNullOrEmpty(dc.lines[i].revealFact))
                        notes.AddNote(w, dc.lines[i].revealFact, "document_compare");
                    save.Data.foundDiscrepancies.Add($"{i}");
                }
                else if (!wasMarked && dc.lines[i].isDiscrepancy) missed++;
                else if (wasMarked && !dc.lines[i].isDiscrepancy) falseAlarm++;
            }
            save.Data.documentCompareCompleted = true;
            save.Save();

            int total = 0;
            foreach (var l in dc.lines) if (l.isDiscrepancy) total++;

            var resultLabel = new Label($"Расхождений найдено: {correct}/{total}" +
                (falseAlarm > 0 ? $"  |  Ложных отметок: {falseAlarm}" : "") +
                (missed > 0 ? $"  |  Пропущено: {missed}" : ""));
            resultLabel.AddToClassList("text");
            resultLabel.AddToClassList(correct == total ? "text-green" : "text-yellow");
            resultLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            resultLabel.style.fontSize = 16;
            panel.Add(resultLabel);
            UIAnimations.ScaleIn(resultLabel, 400);

            // Perfect score celebration
            if (correct == total && falseAlarm == 0)
            {
                panel.Add(Spacer(5));
                var perfectLbl = new Label("БЕЗУПРЕЧНЫЙ АНАЛИЗ!");
                perfectLbl.AddToClassList("text-bold");
                perfectLbl.style.color = new Color(1f, 0.8f, 0.2f);
                perfectLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(perfectLbl);
                UIAnimations.Pulse(perfectLbl, 3, 400);
            }

            panel.Add(Spacer(10));

            var continueBtn = new Button(() => {
                UIManager.Instance.HideAllPanels();
                UIManager.Instance.ShowPanel("dossier-panel");
            });
            continueBtn.text = "ПЕРЕЙТИ К ДОСЬЕ";
            continueBtn.AddToClassList("btn-wide");
            panel.Add(continueBtn);
        }
        else
        {
            var markedCount = new Label($"Отмечено расхождений: {_marked.Count}");
            markedCount.AddToClassList("text");
            markedCount.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(markedCount);

            panel.Add(Spacer(5));

            var confirmBtn = new Button(() => { _submitted = true; Build(); });
            confirmBtn.text = "ПОДТВЕРДИТЬ РАСХОЖДЕНИЯ";
            confirmBtn.AddToClassList("btn-wide");
            confirmBtn.SetEnabled(_marked.Count > 0);
            panel.Add(confirmBtn);
        }
    }

    void AddLineCell(VisualElement row, string text, string cls, float widthPct)
    {
        var lbl = new Label(text);
        lbl.AddToClassList(cls);
        lbl.style.width = Length.Percent(widthPct);
        lbl.style.whiteSpace = WhiteSpace.Normal;
        row.Add(lbl);
    }

    void AddSpacer(VisualElement row, float pct)
    {
        var s = new VisualElement();
        s.style.width = Length.Percent(pct);
        row.Add(s);
    }

    public void OnHide() { }

    static VisualElement Spacer(int h = 10)
    { var s = new VisualElement(); s.style.height = h; return s; }
}
