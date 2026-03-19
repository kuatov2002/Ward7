using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameScreen
{
    MainMenu,
    Outcome,
    Dossier,
    Evidence,
    Testimony,
    Interrogation,
    Briefing,
    Ending
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    GameScreen _screen = GameScreen.MainMenu;
    SaveService _save;
    GameStateService _state;
    CaseService _cases;
    DailyChoiceService _choices;
    VerdictService _verdicts;
    ConsequenceService _conseq;

    Vector2 _scrollPos;
    List<string> _outcomeHeadlines = new();
    bool _verdictChosen;

    GUIStyle _titleStyle, _headerStyle, _textStyle, _buttonStyle, _smallButtonStyle, _boxStyle;
    bool _stylesInited;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[GameManager]");
        go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);
        if (Camera.main == null)
        {
            var camGo = new GameObject("[MainCamera]");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            DontDestroyOnLoad(camGo);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitServices();
    }

    void InitServices()
    {
        ServiceLocator.Clear();
        _save = new SaveService();
        _save.Load();
        _state = new GameStateService(_save);
        _cases = new CaseService();
        _conseq = new ConsequenceService(_save);
        _choices = new DailyChoiceService(_save);
        _verdicts = new VerdictService(_conseq, _save);
        ServiceLocator.Register(_save);
        ServiceLocator.Register(_state);
        ServiceLocator.Register(_cases);
        ServiceLocator.Register(_choices);
        ServiceLocator.Register(_conseq);
        ServiceLocator.Register(_verdicts);
        _cases.LoadAll();
        _screen = GameScreen.MainMenu;
    }

    void InitStyles()
    {
        if (_stylesInited) return;
        _stylesInited = true;
        _titleStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        _headerStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.9f, 0.85f, 0.7f) }, wordWrap = true };
        _textStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 15, wordWrap = true, richText = true, normal = { textColor = new Color(0.85f, 0.85f, 0.85f) } };
        _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, fixedHeight = 40 };
        _smallButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fixedHeight = 32 };
        _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10) };
    }

    void OnGUI()
    {
        InitStyles();
        GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f);
        float panelW = Mathf.Min(Screen.width - 40, 900);
        float panelX = (Screen.width - panelW) / 2f;
        GUILayout.BeginArea(new Rect(panelX, 20, panelW, Screen.height - 40));
        switch (_screen)
        {
            case GameScreen.MainMenu: DrawMainMenu(); break;
            case GameScreen.Outcome: DrawOutcome(); break;
            case GameScreen.Dossier: DrawDossier(); break;
            case GameScreen.Evidence: DrawEvidence(); break;
            case GameScreen.Testimony: DrawTestimony(); break;
            case GameScreen.Interrogation: DrawInterrogation(); break;
            case GameScreen.Briefing: DrawBriefing(); break;
            case GameScreen.Ending: DrawEnding(); break;
        }
        GUILayout.EndArea();
    }

    void ResetUI() { _scrollPos = Vector2.zero; _verdictChosen = false; }

    // ─── MAIN MENU ───
    void DrawMainMenu()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("PROFILE 7", _titleStyle);
        GUILayout.Space(10);
        GUILayout.Label("Детективная драма", new GUIStyle(_textStyle) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(40);
        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUILayout.Width(300));
        if (GUILayout.Button("НОВАЯ ИГРА", _buttonStyle))
        {
            _save.DeleteSave();
            _state = new GameStateService(_save);
            _choices = new DailyChoiceService(_save);
            _conseq = new ConsequenceService(_save);
            _verdicts = new VerdictService(_conseq, _save);
            ServiceLocator.Register(_state);
            ServiceLocator.Register(_choices);
            ServiceLocator.Register(_conseq);
            ServiceLocator.Register(_verdicts);
            _cases.LoadWeek(1);
            StartOutcome();
        }
        GUILayout.Space(10);
        GUI.enabled = _save.HasSave();
        if (GUILayout.Button("ПРОДОЛЖИТЬ", _buttonStyle))
        {
            _cases.LoadWeek(_state.CurrentWeek);
            RestoreScreen();
        }
        GUI.enabled = true;
        GUILayout.EndVertical(); GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    void RestoreScreen()
    {
        switch (_state.CurrentDay)
        {
            case 0: StartOutcome(); break;
            case 1: _screen = GameScreen.Dossier; ResetUI(); break;
            case 2: _screen = GameScreen.Evidence; ResetUI(); break;
            case 3: _screen = GameScreen.Testimony; ResetUI(); break;
            case 4: _screen = GameScreen.Interrogation; ResetUI(); break;
            case 5: _screen = GameScreen.Briefing; ResetUI(); break;
            default: _screen = GameScreen.MainMenu; break;
        }
    }

    // ─── OUTCOME ───
    void StartOutcome()
    {
        ResetUI();
        _outcomeHeadlines = _conseq.ResolveWeek(_state.CurrentWeek)
            .Where(h => !string.IsNullOrEmpty(h)).ToList();
        _screen = GameScreen.Outcome;
    }

    void DrawOutcome()
    {
        GUILayout.Space(40);
        GUILayout.Label($"НЕДЕЛЯ {_state.CurrentWeek} — ПОНЕДЕЛЬНИК", _titleStyle);
        GUILayout.Space(20);
        if (_state.CurrentWeek == 1 && _outcomeHeadlines.Count == 0)
            GUILayout.Label("На ваш стол легло новое дело.", _headerStyle);
        else if (_outcomeHeadlines.Count == 0)
            GUILayout.Label("— Без происшествий —", _headerStyle);
        else
        {
            GUILayout.Label("ПОСЛЕДСТВИЯ ВАШИХ РЕШЕНИЙ:", _headerStyle);
            GUILayout.Space(10);
            foreach (var h in _outcomeHeadlines)
            {
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.Label(h, _textStyle);
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }
        var suspect = _cases.ActiveCase;
        if (suspect != null)
        {
            GUILayout.Space(20);
            GUILayout.Label($"Дело: {suspect.displayName}", _headerStyle);
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ДАЛЕЕ", _buttonStyle))
        {
            _state.AdvanceDay();
            _screen = GameScreen.Dossier;
            ResetUI();
        }
    }

    // ─── DOSSIER ───
    void DrawDossier()
    {
        var s = _cases.ActiveCase; if (s == null) return;
        int w = _state.CurrentWeek;
        GUILayout.Label($"ПОНЕДЕЛЬНИК — ДОСЬЕ: {s.displayName}", _headerStyle);
        GUILayout.Space(5);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label(s.dossierText, _textStyle);
        GUILayout.EndVertical();
        GUILayout.Space(15);
        GUILayout.Label("КОНТАКТЫ (выберите одного для звонка):", _headerStyle);
        GUILayout.Space(5);
        bool done = _choices.IsChosen(w, ChoiceType.Contact);
        string sel = _choices.GetSelected(w, ChoiceType.Contact);
        foreach (var c in s.contacts)
        {
            bool mine = sel == c.contactId;
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label(c.displayName, new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold });
            if (mine)
            {
                GUILayout.Label("[ЗВОНОК СДЕЛАН]", new GUIStyle(_textStyle) { normal = { textColor = Color.green } });
                GUILayout.Space(3);
                GUILayout.Label(c.response, _textStyle);
            }
            else if (!done)
            {
                if (GUILayout.Button("Позвонить", _smallButtonStyle, GUILayout.Width(140)))
                    _choices.Commit(w, ChoiceType.Contact, c.contactId);
            }
            else
                GUILayout.Label("[недоступен]", new GUIStyle(_textStyle) { normal = { textColor = Color.gray } });
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUI.enabled = done;
        if (GUILayout.Button("СЛЕДУЮЩИЙ ДЕНЬ >>", _buttonStyle))
        { _state.AdvanceDay(); _screen = GameScreen.Evidence; ResetUI(); }
        GUI.enabled = true;
    }

    // ─── EVIDENCE ───
    void DrawEvidence()
    {
        var s = _cases.ActiveCase; if (s == null) return;
        int w = _state.CurrentWeek;
        GUILayout.Label($"ВТОРНИК — УЛИКИ: {s.displayName}", _headerStyle);
        GUILayout.Space(5);
        GUILayout.Label("Изучите все улики. Отправьте ОДНУ на экспертизу.", _textStyle);
        GUILayout.Space(10);
        bool done = _choices.IsChosen(w, ChoiceType.Evidence);
        string sel = _choices.GetSelected(w, ChoiceType.Evidence);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        foreach (var ev in s.evidence)
        {
            bool mine = sel == ev.evidenceId;
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label(ev.title, new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Space(5);
            if (mine)
            {
                GUILayout.Label("[ОТПРАВЛЕНО НА ЭКСПЕРТИЗУ]", new GUIStyle(_textStyle) { normal = { textColor = Color.cyan } });
                GUILayout.Space(5);
                GUILayout.Label(ev.expertDescription, _textStyle);
            }
            else
            {
                GUILayout.Label(ev.baseDescription, _textStyle);
                if (!done)
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button("Отправить на экспертизу", _smallButtonStyle, GUILayout.Width(220)))
                        _choices.Commit(w, ChoiceType.Evidence, ev.evidenceId);
                }
            }
            GUILayout.EndVertical();
            GUILayout.Space(8);
        }
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUI.enabled = done;
        if (GUILayout.Button("СЛЕДУЮЩИЙ ДЕНЬ >>", _buttonStyle))
        { _state.AdvanceDay(); _screen = GameScreen.Testimony; ResetUI(); }
        GUI.enabled = true;
    }

    // ─── TESTIMONY ───
    void DrawTestimony()
    {
        var s = _cases.ActiveCase; if (s == null) return;
        int w = _state.CurrentWeek;
        GUILayout.Label($"СРЕДА — ПОКАЗАНИЯ: {s.displayName}", _headerStyle);
        GUILayout.Space(5);
        GUILayout.Label("Три источника. Запросите уточнение у ОДНОГО.", _textStyle);
        GUILayout.Space(10);
        bool done = _choices.IsChosen(w, ChoiceType.Testimony);
        string sel = _choices.GetSelected(w, ChoiceType.Testimony);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        foreach (var t in s.testimonies)
        {
            bool mine = sel == t.witnessName;
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label(t.witnessName, new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Space(5);
            GUILayout.Label(t.baseTestimony, _textStyle);
            if (mine)
            {
                GUILayout.Space(8);
                GUILayout.Label("[УТОЧНЕНИЕ ЗАПРОШЕНО]", new GUIStyle(_textStyle) { normal = { textColor = Color.yellow } });
                GUILayout.Space(5);
                GUILayout.Label(t.clarification, _textStyle);
            }
            else if (!done)
            {
                GUILayout.Space(5);
                if (GUILayout.Button("Запросить уточнение", _smallButtonStyle, GUILayout.Width(220)))
                    _choices.Commit(w, ChoiceType.Testimony, t.witnessName);
            }
            GUILayout.EndVertical();
            GUILayout.Space(8);
        }
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUI.enabled = done;
        if (GUILayout.Button("СЛЕДУЮЩИЙ ДЕНЬ >>", _buttonStyle))
        { _state.AdvanceDay(); _screen = GameScreen.Interrogation; ResetUI(); }
        GUI.enabled = true;
    }

    // ─── INTERROGATION ───
    void DrawInterrogation()
    {
        var s = _cases.ActiveCase; if (s == null) return;
        int w = _state.CurrentWeek;
        GUILayout.Label($"ЧЕТВЕРГ — ДОПРОС: {s.displayName}", _headerStyle);
        GUILayout.Space(5);
        bool fuDone = _choices.IsChosen(w, ChoiceType.FollowUp);
        string fuSel = _choices.GetSelected(w, ChoiceType.FollowUp);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        // Standard
        GUILayout.Label("СТАНДАРТНЫЕ ВОПРОСЫ:", new GUIStyle(_headerStyle) { fontSize = 16 });
        GUILayout.Space(5);
        if (s.standardQuestions != null)
            foreach (var qa in s.standardQuestions)
            {
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.Label($"— {qa.question}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold });
                GUILayout.Space(3);
                GUILayout.Label(qa.answer, _textStyle);
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

        // Conditional
        if (s.conditionalQuestions != null)
        {
            bool hasAny = false;
            foreach (var cq in s.conditionalQuestions)
            {
                if (_choices.GetSelected(w, cq.requiredChoiceType) == cq.requiredChoiceId)
                {
                    if (!hasAny)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("РАЗБЛОКИРОВАННЫЕ ВОПРОСЫ:", new GUIStyle(_headerStyle) { fontSize = 16, normal = { textColor = Color.yellow } });
                        GUILayout.Space(5);
                        hasAny = true;
                    }
                    GUILayout.BeginVertical(_boxStyle);
                    GUILayout.Label($"— {cq.question}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold });
                    GUILayout.Space(3);
                    GUILayout.Label(cq.answer, _textStyle);
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }
        }

        // Follow-up
        GUILayout.Space(15);
        GUILayout.Label("ВОПРОС ВНЕ ПРОТОКОЛА (выберите один):",
            new GUIStyle(_headerStyle) { fontSize = 16, normal = { textColor = new Color(1f, 0.6f, 0.3f) } });
        GUILayout.Space(5);
        if (s.followUps != null)
            foreach (var fu in s.followUps)
            {
                bool mine = fuSel == fu.followUpId;
                GUILayout.BeginVertical(_boxStyle);
                if (mine)
                {
                    GUILayout.Label($"— {fu.question}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, normal = { textColor = Color.green } });
                    GUILayout.Space(3);
                    GUILayout.Label(fu.answer, _textStyle);
                }
                else if (!fuDone)
                {
                    if (GUILayout.Button(fu.question, _smallButtonStyle))
                        _choices.Commit(w, ChoiceType.FollowUp, fu.followUpId);
                }
                else
                    GUILayout.Label($"— {fu.question}", new GUIStyle(_textStyle) { normal = { textColor = Color.gray } });
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUI.enabled = fuDone;
        if (GUILayout.Button("ПЕРЕЙТИ К КОМИССИИ >>", _buttonStyle))
        { _state.AdvanceDay(); _screen = GameScreen.Briefing; ResetUI(); }
        GUI.enabled = true;
    }

    // ─── BRIEFING ───
    void DrawBriefing()
    {
        var s = _cases.ActiveCase; if (s == null) return;
        GUILayout.FlexibleSpace();
        GUILayout.Label("ПЯТНИЦА — КОМИССИЯ", _titleStyle);
        GUILayout.Space(10);
        GUILayout.Label($"Дело: {s.displayName}", new GUIStyle(_headerStyle) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(30);
        GUILayout.Label("Ваш вердикт:", new GUIStyle(_textStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 18 });
        GUILayout.Space(20);
        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        var cur = _verdicts.GetVerdict();
        var gStyle = new GUIStyle(_buttonStyle) { fontSize = 24, fixedHeight = 80, fixedWidth = 250, fontStyle = FontStyle.Bold };
        if (cur == VerdictType.Guilty) gStyle.normal.textColor = Color.red;
        if (GUILayout.Button("ВИНОВЕН", gStyle)) { _verdicts.SetVerdict(VerdictType.Guilty); _verdictChosen = true; }
        GUILayout.Space(40);
        var ngStyle = new GUIStyle(gStyle);
        ngStyle.normal.textColor = cur == VerdictType.NotGuilty ? Color.green : gStyle.normal.textColor;
        if (cur != VerdictType.Guilty) ngStyle.normal.textColor = cur == VerdictType.NotGuilty ? Color.green : GUI.skin.button.normal.textColor;
        if (GUILayout.Button("НЕ ВИНОВЕН", ngStyle)) { _verdicts.SetVerdict(VerdictType.NotGuilty); _verdictChosen = true; }
        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
        GUILayout.Space(40);
        GUI.enabled = _verdictChosen && cur != VerdictType.None;
        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        if (GUILayout.Button("ПОДПИСАТЬ ПРОТОКОЛ", new GUIStyle(_buttonStyle) { fontSize = 20, fixedHeight = 50, fixedWidth = 320 }))
        {
            _verdicts.CommitAll(s.suspectId, _state.CurrentWeek, s);
            _state.AdvanceDay();
            if (_state.IsGameComplete) { _screen = GameScreen.Ending; }
            else { _cases.LoadWeek(_state.CurrentWeek); StartOutcome(); }
        }
        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
    }

    // ─── ENDING ───
    void DrawEnding()
    {
        GUILayout.Space(40);
        GUILayout.Label("ФИНАЛЬНЫЙ ОТЧЁТ", _titleStyle);
        GUILayout.Space(20);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);
        GUILayout.Label("Все вердикты и их последствия:", _headerStyle);
        GUILayout.Space(10);
        foreach (var v in _save.Data.verdicts)
        {
            var suspect = _cases.GetCase(v.week);
            string name = suspect != null ? suspect.displayName : v.suspectId;
            string vStr = v.verdict == VerdictType.Guilty ? "ВИНОВЕН" : "НЕ ВИНОВЕН";
            bool correct = suspect != null &&
                ((v.verdict == VerdictType.Guilty && suspect.isGuilty) ||
                 (v.verdict == VerdictType.NotGuilty && !suspect.isGuilty));
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"Неделя {v.week}: {name}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Label($"Вердикт: {vStr}", new GUIStyle(_textStyle) { normal = { textColor = correct ? Color.green : Color.red } });
            if (!correct && suspect != null)
            {
                string c = v.verdict == VerdictType.Guilty ? suspect.consequenceGuilty : suspect.consequenceNotGuilty;
                if (!string.IsNullOrEmpty(c)) { GUILayout.Space(3); GUILayout.Label(c, _textStyle); }
            }
            GUILayout.EndVertical();
            GUILayout.Space(8);
        }
        GUILayout.EndScrollView();
        GUILayout.Space(20);
        if (GUILayout.Button("ГЛАВНОЕ МЕНЮ", _buttonStyle)) _screen = GameScreen.MainMenu;
    }
}
