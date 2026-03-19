using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BriefingUI : MonoBehaviour, IPanelController
{
    const string PanelName = "briefing-panel";
    const int MaxArguments = 3;

    VerdictType _selected = VerdictType.None;
    readonly HashSet<string> _selectedArgs = new();
    bool _showJustification;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void OnShow()
    {
        _selected = VerdictType.None;
        _selectedArgs.Clear();
        _showJustification = false;
        BuildPanel();
    }

    void BuildPanel()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var cases = ServiceLocator.Get<CaseService>();
        var notes = ServiceLocator.Get<NoteService>();
        var state = ServiceLocator.Get<GameStateService>();
        var s = cases.ActiveCase;
        if (s == null) return;
        int w = state.CurrentWeek;

        var title = new Label("ПЯТНИЦА — КОМИССИЯ");
        title.AddToClassList("title");
        panel.Add(title);

        panel.Add(Spacer());

        var caseName = new Label($"Дело: {s.displayName}");
        caseName.AddToClassList("header-center");
        panel.Add(caseName);

        // ─── NOTES REMINDER ───
        var playerNotes = notes.GetNotes(w);
        if (playerNotes.Count > 0)
        {
            panel.Add(Spacer(15));
            var notesHeader = new Label($"ВАШИ ЗАМЕТКИ ({playerNotes.Count}):");
            notesHeader.AddToClassList("text-bold");
            notesHeader.AddToClassList("text-yellow");
            panel.Add(notesHeader);

            var notesScroll = new ScrollView(ScrollViewMode.Vertical);
            notesScroll.style.maxHeight = 120;
            foreach (var n in playerNotes)
            {
                var noteLabel = new Label($"\u2022 {TruncateText(n.text, 80)}");
                noteLabel.AddToClassList("text-small");
                notesScroll.Add(noteLabel);
            }
            panel.Add(notesScroll);
        }

        panel.Add(Spacer(20));

        if (!_showJustification)
        {
            // ─── VERDICT SELECTION ───
            var verdictLabel = new Label("Ваш вердикт:");
            verdictLabel.AddToClassList("text");
            verdictLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            verdictLabel.style.fontSize = 18;
            panel.Add(verdictLabel);
            panel.Add(Spacer(15));

            var row = new VisualElement();
            row.AddToClassList("row-center");

            var guiltyBtn = new Button(() => { _selected = VerdictType.Guilty; BuildPanel(); });
            guiltyBtn.text = "ВИНОВЕН";
            guiltyBtn.AddToClassList("btn-verdict");
            if (_selected == VerdictType.Guilty) guiltyBtn.AddToClassList("btn-verdict-guilty-selected");
            row.Add(guiltyBtn);

            var ngBtn = new Button(() => { _selected = VerdictType.NotGuilty; BuildPanel(); });
            ngBtn.text = "НЕ ВИНОВЕН";
            ngBtn.AddToClassList("btn-verdict");
            if (_selected == VerdictType.NotGuilty) ngBtn.AddToClassList("btn-verdict-not-guilty-selected");
            row.Add(ngBtn);
            panel.Add(row);

            panel.Add(Spacer(20));

            bool hasArgs = s.arguments != null && s.arguments.Length > 0;
            var nextBtn = new Button(() => {
                if (hasArgs) { _showJustification = true; BuildPanel(); }
                else CommitVerdict(s, w);
            });
            nextBtn.text = hasArgs ? "ОБОСНОВАТЬ ВЕРДИКТ" : "ПОДПИСАТЬ ПРОТОКОЛ";
            nextBtn.AddToClassList("btn-sign");
            nextBtn.SetEnabled(_selected != VerdictType.None);
            panel.Add(nextBtn);
        }
        else
        {
            // ─── JUSTIFICATION PHASE ───
            string vStr = _selected == VerdictType.Guilty ? "ВИНОВЕН" : "НЕ ВИНОВЕН";
            var verdictTag = new Label($"Вердикт: {vStr}");
            verdictTag.AddToClassList("text-bold");
            verdictTag.AddToClassList(_selected == VerdictType.Guilty ? "text-red" : "text-green");
            verdictTag.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(verdictTag);

            panel.Add(Spacer(10));

            var instrLabel = new Label($"Выберите до {MaxArguments} ключевых доводов для обоснования:");
            instrLabel.AddToClassList("text");
            instrLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(instrLabel);

            // Show investigation bonus info
            panel.Add(Spacer(10));

            var argsScroll = new ScrollView(ScrollViewMode.Vertical);
            argsScroll.style.maxHeight = 300;

            var choices = ServiceLocator.Get<DailyChoiceService>();
            int availableCount = 0;
            int lockedCount = 0;

            foreach (var arg in s.arguments)
            {
                bool discovered = DiscoveryHelper.IsDiscovered(
                    arg.alwaysVisible, arg.requiredChoiceType, arg.requiredChoiceId, w, choices);

                if (!discovered)
                {
                    lockedCount++;
                    var lockedBox = new VisualElement();
                    lockedBox.AddToClassList("box");
                    lockedBox.style.opacity = 0.3f;
                    var lockLbl = new Label("??? [Факт не раскрыт]");
                    lockLbl.AddToClassList("text");
                    lockLbl.AddToClassList("text-dim");
                    lockedBox.Add(lockLbl);
                    argsScroll.Add(lockedBox);
                    continue;
                }

                availableCount++;
                bool isSelected = _selectedArgs.Contains(arg.argumentId);
                var box = new VisualElement();
                box.AddToClassList("box");
                if (isSelected)
                {
                    box.style.borderLeftWidth = 3;
                    box.style.borderLeftColor = new Color(0.3f, 0.8f, 0.3f);
                    box.style.backgroundColor = new Color(0.2f, 0.3f, 0.2f, 0.5f);
                }

                string aid = arg.argumentId;
                var btn = new Button(() => {
                    if (_selectedArgs.Contains(aid))
                        _selectedArgs.Remove(aid);
                    else if (_selectedArgs.Count < MaxArguments)
                        _selectedArgs.Add(aid);
                    BuildPanel();
                });
                btn.text = (isSelected ? "\u2713 " : "\u25CB ") + arg.text;
                btn.AddToClassList("btn-small");
                btn.style.whiteSpace = WhiteSpace.Normal;
                btn.style.height = StyleKeyword.Auto;
                btn.style.paddingTop = 6;
                btn.style.paddingBottom = 6;
                if (isSelected) btn.style.color = new Color(0.5f, 1f, 0.5f);
                box.Add(btn);

                argsScroll.Add(box);
            }

            panel.Add(argsScroll);
            panel.Add(Spacer(5));

            if (lockedCount > 0)
            {
                var lockInfo = new Label($"Скрыто доводов: {lockedCount} (не все факты раскрыты в ходе расследования)");
                lockInfo.AddToClassList("text-small");
                lockInfo.AddToClassList("text-red");
                lockInfo.style.unityTextAlign = TextAnchor.MiddleCenter;
                panel.Add(lockInfo);
            }

            panel.Add(Spacer(5));

            var countLabel = new Label($"Выбрано: {_selectedArgs.Count}/{MaxArguments}");
            countLabel.AddToClassList("text");
            countLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(countLabel);

            panel.Add(Spacer(10));

            var signBtn = new Button(() => CommitVerdict(s, w));
            signBtn.text = "ПОДПИСАТЬ ПРОТОКОЛ";
            signBtn.AddToClassList("btn-sign");
            signBtn.SetEnabled(_selectedArgs.Count > 0);
            panel.Add(signBtn);

            // Back button
            var backBtn = new Button(() => { _showJustification = false; BuildPanel(); });
            backBtn.text = "Назад к вердикту";
            backBtn.AddToClassList("btn-small");
            backBtn.style.marginTop = 8;
            panel.Add(backBtn);
        }
    }

    void CommitVerdict(SuspectSO s, int week)
    {
        // Calculate justification score & count mismatches
        int score = 0;
        int maxScore = 0;
        int mismatches = 0;
        if (s.arguments != null)
        {
            foreach (var arg in s.arguments)
            {
                if (!_selectedArgs.Contains(arg.argumentId)) continue;
                bool matches = (_selected == VerdictType.Guilty && arg.supportsGuilty)
                            || (_selected == VerdictType.NotGuilty && !arg.supportsGuilty);
                if (matches)
                    score += arg.weight;
                else
                    mismatches++;
            }

            // Max possible score from top 3 matching arguments
            var sorted = new System.Collections.Generic.List<int>();
            foreach (var arg in s.arguments)
            {
                bool m = (_selected == VerdictType.Guilty && arg.supportsGuilty)
                      || (_selected == VerdictType.NotGuilty && !arg.supportsGuilty);
                if (m) sorted.Add(arg.weight);
            }
            sorted.Sort((a, b) => b.CompareTo(a));
            for (int i = 0; i < Mathf.Min(MaxArguments, sorted.Count); i++)
                maxScore += sorted[i];
        }

        var verdicts = ServiceLocator.Get<VerdictService>();
        verdicts.SetVerdict(_selected);
        verdicts.CommitAll(s.suspectId, week, s, score);

        if (ProceduralAudio.Instance != null)
            ProceduralAudio.Instance.PlayStamp();

        // Show commission reaction instead of immediately closing
        ShowCommissionReaction(s, score, maxScore, mismatches);
    }

    void ShowCommissionReaction(SuspectSO s, int score, int maxScore, int mismatches)
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        var title = new Label("РЕШЕНИЕ КОМИССИИ");
        title.AddToClassList("title");
        panel.Add(title);
        panel.Add(Spacer(20));

        // Score feedback
        float ratio = maxScore > 0 ? (float)score / maxScore : 0f;
        string reaction;
        string reactionClass;

        if (ratio >= 0.8f && mismatches == 0)
        {
            reaction = "Блестящее обоснование. Комиссия полностью удовлетворена вашей аргументацией.";
            reactionClass = "text-green";
        }
        else if (ratio >= 0.5f)
        {
            reaction = "Обоснование принято. Комиссия отмечает убедительность ключевых доводов.";
            reactionClass = "text-yellow";
        }
        else if (mismatches >= 2)
        {
            reaction = "Комиссия обеспокоена. Некоторые из ваших доводов противоречат выбранному вердикту. Ваша компетентность под вопросом.";
            reactionClass = "text-red";
        }
        else
        {
            reaction = "Слабое обоснование. Комиссия приняла вердикт, но выразила сомнения в глубине анализа.";
            reactionClass = "text-red";
        }

        var reactionLabel = new Label(reaction);
        reactionLabel.AddToClassList("header");
        reactionLabel.AddToClassList(reactionClass);
        reactionLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        panel.Add(reactionLabel);

        panel.Add(Spacer(15));

        var scoreLabel = new Label($"Оценка обоснования: {score}/{maxScore}");
        scoreLabel.AddToClassList("text");
        scoreLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        scoreLabel.style.fontSize = 18;
        panel.Add(scoreLabel);

        if (mismatches > 0)
        {
            panel.Add(Spacer(5));
            var mismatchLabel = new Label($"Противоречивых доводов: {mismatches}");
            mismatchLabel.AddToClassList("text");
            mismatchLabel.AddToClassList("text-red");
            mismatchLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            panel.Add(mismatchLabel);
        }

        panel.Add(Spacer(30));

        var continueBtn = new Button(() => {
            UIManager.Instance.HideAllPanels();
            OfficeController.Instance.AfterVerdictCommit();
        });
        continueBtn.text = "ПРОДОЛЖИТЬ";
        continueBtn.AddToClassList("btn-wide");
        panel.Add(continueBtn);
    }

    public void OnHide()
    {
        _selected = VerdictType.None;
        _selectedArgs.Clear();
        _showJustification = false;
    }

    static string TruncateText(string text, int maxLen)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLen) return text;
        return text.Substring(0, maxLen) + "...";
    }

    static VisualElement Spacer(int h = 15)
    {
        var s = new VisualElement();
        s.style.height = h;
        return s;
    }
}
