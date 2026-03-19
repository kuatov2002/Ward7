using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Mini-game: testimony lines appear one by one. Press LIE when you spot a falsehood.
/// </summary>
public class LieDetectorUI : MonoBehaviour, IPanelController
{
    const string PanelName = "liedetector-panel";
    const float LineDisplayTime = 3.5f;

    string _witnessName;
    TestimonyLineData[] _lines;
    int _currentLine;
    int _trust;
    bool _running;
    bool _finished;
    readonly List<int> _caughtLies = new();
    float _lineTimer;
    bool _liePressed;

    void Start()
    {
        UIManager.Instance.RegisterController(PanelName, this);
    }

    public void StartForWitness(string witnessName, TestimonyLineData[] lines, int startTrust)
    {
        _witnessName = witnessName;
        _lines = lines;
        _trust = startTrust;
        _currentLine = 0;
        _caughtLies.Clear();
        _running = false;
        _finished = false;
        _liePressed = false;
        UIManager.Instance.ShowPanel(PanelName);
    }

    public void OnShow()
    {
        if (_lines == null || _lines.Length == 0)
        {
            UIManager.Instance.HideAllPanels();
            return;
        }
        _running = true;
        _currentLine = 0;
        _lineTimer = LineDisplayTime;
        _liePressed = false;
        BuildActive();
    }

    void Update()
    {
        if (!_running || _finished) return;

        _lineTimer -= Time.deltaTime;
        if (_lineTimer <= 0f)
        {
            // Time's up for this line
            AdvanceLine();
        }

        // Update timer bar visually
        UpdateTimerBar();
    }

    void BuildActive()
    {
        var root = UIManager.Instance.GetRoot();
        var panel = root.Q<VisualElement>(PanelName);
        panel.Clear();

        if (_finished || _currentLine >= _lines.Length || _trust <= 0)
        {
            ShowResults(panel);
            return;
        }

        var line = _lines[_currentLine];

        var title = new Label($"ПОКАЗАНИЯ: {_witnessName}");
        title.AddToClassList("header");
        panel.Add(title);

        // Trust bar
        var trustRow = new VisualElement();
        trustRow.AddToClassList("row");
        var trustLbl = new Label("Доверие: ");
        trustLbl.AddToClassList("text");
        trustRow.Add(trustLbl);
        for (int i = 0; i < _trust; i++)
        {
            var dot = new Label("\u2588");
            dot.style.color = new Color(0.3f, 0.8f, 0.3f);
            dot.style.fontSize = 14;
            dot.style.marginRight = 3;
            trustRow.Add(dot);
        }
        panel.Add(trustRow);

        // Timer bar
        var timerBg = new VisualElement();
        timerBg.name = "lie-timer-bg";
        timerBg.style.height = 4;
        timerBg.style.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
        timerBg.style.marginTop = 5;
        timerBg.style.marginBottom = 10;
        var timerFill = new VisualElement();
        timerFill.name = "lie-timer-fill";
        timerFill.style.height = 4;
        timerFill.style.width = Length.Percent(100);
        timerFill.style.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        timerBg.Add(timerFill);
        panel.Add(timerBg);

        // Progress
        var progress = new Label($"Фраза {_currentLine + 1} / {_lines.Length}");
        progress.AddToClassList("text-small");
        progress.AddToClassList("text-dim");
        panel.Add(progress);

        panel.Add(Spacer(15));

        // Current line — large text
        var lineBox = new VisualElement();
        lineBox.AddToClassList("box");
        lineBox.style.minHeight = 80;
        lineBox.style.justifyContent = Justify.Center;
        var lineLbl = new Label(line.text);
        lineLbl.AddToClassList("text");
        lineLbl.style.fontSize = 16;
        lineLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
        lineLbl.style.whiteSpace = WhiteSpace.Normal;
        lineBox.Add(lineLbl);
        panel.Add(lineBox);

        panel.Add(Spacer(15));

        // LIE button
        var lieBtn = new Button(() => OnLiePressed());
        lieBtn.text = "ЛОЖЬ!";
        lieBtn.AddToClassList("lie-btn");
        lieBtn.SetEnabled(!_liePressed);
        panel.Add(lieBtn);

        // Previously caught lies
        if (_caughtLies.Count > 0)
        {
            panel.Add(Spacer(10));
            var caughtHeader = new Label("Пойманная ложь:");
            caughtHeader.AddToClassList("text-small");
            caughtHeader.AddToClassList("text-red");
            panel.Add(caughtHeader);
            foreach (int li in _caughtLies)
            {
                var cl = new Label($"\u2022 {_lines[li].lieReason}");
                cl.AddToClassList("text-small");
                panel.Add(cl);
            }
        }
    }

    void OnLiePressed()
    {
        if (_liePressed || _finished) return;
        _liePressed = true;

        var line = _lines[_currentLine];
        if (line.isLie)
        {
            _caughtLies.Add(_currentLine);
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPaperFlip();
        }
        else
        {
            _trust--;
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayStamp();
        }

        // Brief pause then advance
        StartCoroutine(DelayedAdvance(0.8f));
    }

    IEnumerator DelayedAdvance(float delay)
    {
        // Show result briefly
        BuildActive();
        yield return new WaitForSeconds(delay);
        AdvanceLine();
    }

    void AdvanceLine()
    {
        _currentLine++;
        _liePressed = false;
        _lineTimer = LineDisplayTime;

        if (_currentLine >= _lines.Length || _trust <= 0)
        {
            _finished = true;
            _running = false;
            SaveResults();
        }

        BuildActive();
    }

    void UpdateTimerBar()
    {
        var root = UIManager.Instance.GetRoot();
        var fill = root.Q<VisualElement>("lie-timer-fill");
        if (fill != null)
        {
            float pct = Mathf.Clamp01(_lineTimer / LineDisplayTime) * 100f;
            fill.style.width = Length.Percent(pct);
            fill.style.backgroundColor = pct > 50 ? new Color(0.3f, 0.7f, 0.3f)
                : pct > 25 ? new Color(0.7f, 0.6f, 0.2f)
                : new Color(0.8f, 0.2f, 0.2f);
        }
    }

    void SaveResults()
    {
        var save = ServiceLocator.Get<SaveService>();
        var notes = ServiceLocator.Get<NoteService>();
        int w = ServiceLocator.Get<GameStateService>().CurrentWeek;

        foreach (int li in _caughtLies)
        {
            save.Data.caughtLies.Add($"{_witnessName}:{li}");
            if (!string.IsNullOrEmpty(_lines[li].lieReason))
                notes.AddNote(w, _lines[li].lieReason, $"lie_{_witnessName}");
        }
        save.Save();
    }

    void ShowResults(VisualElement panel)
    {
        var title = new Label($"РЕЗУЛЬТАТ: {_witnessName}");
        title.AddToClassList("header");
        panel.Add(title);

        panel.Add(Spacer(10));

        if (_trust <= 0)
        {
            var warn = new Label("Свидетель потерял доверие и отказался продолжать.");
            warn.AddToClassList("text");
            warn.AddToClassList("text-red");
            panel.Add(warn);
        }

        int totalLies = 0;
        foreach (var l in _lines) if (l.isLie) totalLies++;

        var result = new Label($"Ложь поймана: {_caughtLies.Count}/{totalLies}");
        result.AddToClassList("text");
        result.AddToClassList(_caughtLies.Count == totalLies ? "text-green" : "text-yellow");
        result.style.unityTextAlign = TextAnchor.MiddleCenter;
        panel.Add(result);

        if (_caughtLies.Count > 0)
        {
            panel.Add(Spacer(8));
            foreach (int li in _caughtLies)
            {
                var box = new VisualElement();
                box.AddToClassList("box");
                box.style.borderLeftWidth = 3;
                box.style.borderLeftColor = new Color(0.8f, 0.3f, 0.3f);
                var lieLbl = new Label($"\"{_lines[li].text}\"");
                lieLbl.AddToClassList("text");
                lieLbl.AddToClassList("text-red");
                box.Add(lieLbl);
                var reason = new Label(_lines[li].lieReason);
                reason.AddToClassList("text-small");
                box.Add(reason);
                panel.Add(box);
            }
        }

        panel.Add(Spacer(15));

        var continueBtn = new Button(() => {
            UIManager.Instance.HideAllPanels();
            UIManager.Instance.ShowPanel("testimony-panel");
        });
        continueBtn.text = "ВЕРНУТЬСЯ К ПОКАЗАНИЯМ";
        continueBtn.AddToClassList("btn-wide");
        panel.Add(continueBtn);
    }

    public void OnHide()
    {
        _running = false;
    }

    static VisualElement Spacer(int h = 10)
    { var s = new VisualElement(); s.style.height = h; return s; }
}
