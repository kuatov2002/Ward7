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

    // UI state
    Vector2 _scrollPos;
    Vector2 _scrollPos2;
    string _contactResponse;
    string _selectedFollowUpAnswer;
    List<string> _outcomeHeadlines = new();
    bool _verdictChosen;

    GUIStyle _titleStyle;
    GUIStyle _headerStyle;
    GUIStyle _textStyle;
    GUIStyle _buttonStyle;
    GUIStyle _smallButtonStyle;
    GUIStyle _boxStyle;
    bool _stylesInited;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[GameManager]");
        go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);

        // Ensure a camera exists
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

        // Load all cases from Resources/Suspects/
        _cases.LoadAll();

        _screen = GameScreen.MainMenu;
    }

    void InitStyles()
    {
        if (_stylesInited) return;
        _stylesInited = true;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.9f, 0.85f, 0.7f) },
            wordWrap = true
        };

        _textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            wordWrap = true,
            richText = true,
            normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 16,
            fixedHeight = 40
        };

        _smallButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fixedHeight = 32
        };

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 10, 10)
        };
    }

    void OnGUI()
    {
        InitStyles();

        // Dark background
        GUI.backgroundColor = new Color(0.15f, 0.15f, 0.18f);

        float w = Screen.width;
        float h = Screen.height;
        float panelW = Mathf.Min(w - 40, 900);
        float panelX = (w - panelW) / 2f;

        GUILayout.BeginArea(new Rect(panelX, 20, panelW, h - 40));

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

    // ─────────────────── MAIN MENU ───────────────────
    void DrawMainMenu()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("PROFILE 7", _titleStyle);
        GUILayout.Space(10);
        GUILayout.Label("Detective Drama", new GUIStyle(_textStyle) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(40);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUILayout.Width(300));

        if (GUILayout.Button("NEW GAME", _buttonStyle))
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
        if (GUILayout.Button("CONTINUE", _buttonStyle))
        {
            _cases.LoadWeek(_state.CurrentWeek);
            RestoreScreen();
        }
        GUI.enabled = true;

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
    }

    void RestoreScreen()
    {
        switch (_state.CurrentDay)
        {
            case 0: StartOutcome(); break;
            case 1: _screen = GameScreen.Dossier; ResetUIState(); break;
            case 2: _screen = GameScreen.Evidence; ResetUIState(); break;
            case 3: _screen = GameScreen.Testimony; ResetUIState(); break;
            case 4: _screen = GameScreen.Interrogation; ResetUIState(); break;
            case 5: _screen = GameScreen.Briefing; ResetUIState(); break;
            default: _screen = GameScreen.MainMenu; break;
        }
    }

    void ResetUIState()
    {
        _scrollPos = Vector2.zero;
        _scrollPos2 = Vector2.zero;
        _contactResponse = null;
        _selectedFollowUpAnswer = null;
        _verdictChosen = false;
    }

    // ─────────────────── OUTCOME ───────────────────
    void StartOutcome()
    {
        ResetUIState();
        _outcomeHeadlines = _conseq.ResolveWeek(_state.CurrentWeek);
        _outcomeHeadlines = _outcomeHeadlines.Where(h => !string.IsNullOrEmpty(h)).ToList();
        _screen = GameScreen.Outcome;
    }

    void DrawOutcome()
    {
        GUILayout.Space(40);
        GUILayout.Label($"WEEK {_state.CurrentWeek} - MONDAY", _titleStyle);
        GUILayout.Space(20);

        if (_state.CurrentWeek == 1 && _outcomeHeadlines.Count == 0)
        {
            GUILayout.Label("A new case file has been placed on your desk.", _headerStyle);
        }
        else if (_outcomeHeadlines.Count == 0)
        {
            GUILayout.Label("-- No incidents --", _headerStyle);
        }
        else
        {
            GUILayout.Label("CONSEQUENCES OF YOUR PREVIOUS DECISIONS:", _headerStyle);
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
            GUILayout.Label($"Case: {suspect.displayName}", _headerStyle);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("PROCEED", _buttonStyle))
        {
            _state.AdvanceDay(); // day 0 -> 1
            _screen = GameScreen.Dossier;
            ResetUIState();
        }
    }

    // ─────────────────── DOSSIER (MONDAY) ───────────────────
    void DrawDossier()
    {
        var suspect = _cases.ActiveCase;
        if (suspect == null) return;

        GUILayout.Label($"MONDAY - DOSSIER: {suspect.displayName}", _headerStyle);
        GUILayout.Space(5);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        // Dossier text
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label(suspect.dossierText, _textStyle);
        GUILayout.EndVertical();

        GUILayout.Space(15);
        GUILayout.Label("CONTACTS (choose one to call):", _headerStyle);
        GUILayout.Space(5);

        bool alreadyChosen = _choices.IsChosen(_state.CurrentWeek, ChoiceType.Contact);
        string selectedContact = _choices.GetSelected(_state.CurrentWeek, ChoiceType.Contact);

        foreach (var contact in suspect.contacts)
        {
            bool isThis = selectedContact == contact.contactId;
            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label(contact.displayName, new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold });

            if (isThis)
            {
                GUILayout.Space(5);
                GUILayout.Label("[CALLED]", new GUIStyle(_textStyle) { normal = { textColor = Color.green } });
                GUILayout.Space(5);
                GUILayout.Label(contact.response, _textStyle);
            }
            else if (!alreadyChosen)
            {
                if (GUILayout.Button("Call", _smallButtonStyle, GUILayout.Width(120)))
                {
                    _choices.Commit(_state.CurrentWeek, ChoiceType.Contact, contact.contactId);
                    _contactResponse = contact.response;
                }
            }
            else
            {
                GUILayout.Label("[unavailable]", new GUIStyle(_textStyle) { normal = { textColor = Color.gray } });
            }

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.enabled = alreadyChosen;
        if (GUILayout.Button("NEXT DAY >>", _buttonStyle))
        {
            _state.AdvanceDay(); // day 1 -> 2
            _screen = GameScreen.Evidence;
            ResetUIState();
        }
        GUI.enabled = true;
    }

    // ─────────────────── EVIDENCE (TUESDAY) ───────────────────
    void DrawEvidence()
    {
        var suspect = _cases.ActiveCase;
        if (suspect == null) return;

        GUILayout.Label($"TUESDAY - EVIDENCE: {suspect.displayName}", _headerStyle);
        GUILayout.Space(5);
        GUILayout.Label("Review all evidence. Send ONE for detailed expertise.", _textStyle);
        GUILayout.Space(10);

        bool alreadySent = _choices.IsChosen(_state.CurrentWeek, ChoiceType.Evidence);
        string selectedEvidence = _choices.GetSelected(_state.CurrentWeek, ChoiceType.Evidence);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        foreach (var ev in suspect.evidence)
        {
            bool isThis = selectedEvidence == ev.evidenceId;
            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label(ev.title, new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Space(5);

            if (isThis)
            {
                GUILayout.Label("[SENT FOR EXPERTISE]", new GUIStyle(_textStyle) { normal = { textColor = Color.cyan } });
                GUILayout.Space(5);
                GUILayout.Label(ev.expertDescription, _textStyle);
            }
            else
            {
                GUILayout.Label(ev.baseDescription, _textStyle);
                GUILayout.Space(5);
                if (!alreadySent)
                {
                    if (GUILayout.Button("Send for Expertise", _smallButtonStyle, GUILayout.Width(200)))
                    {
                        _choices.Commit(_state.CurrentWeek, ChoiceType.Evidence, ev.evidenceId);
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(8);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.enabled = alreadySent;
        if (GUILayout.Button("NEXT DAY >>", _buttonStyle))
        {
            _state.AdvanceDay(); // day 2 -> 3
            _screen = GameScreen.Testimony;
            ResetUIState();
        }
        GUI.enabled = true;
    }

    // ─────────────────── TESTIMONY (WEDNESDAY) ───────────────────
    void DrawTestimony()
    {
        var suspect = _cases.ActiveCase;
        if (suspect == null) return;

        GUILayout.Label($"WEDNESDAY - TESTIMONIES: {suspect.displayName}", _headerStyle);
        GUILayout.Space(5);
        GUILayout.Label("Three sources. Request clarification from ONE.", _textStyle);
        GUILayout.Space(10);

        bool alreadyRequested = _choices.IsChosen(_state.CurrentWeek, ChoiceType.Testimony);
        string selectedWitness = _choices.GetSelected(_state.CurrentWeek, ChoiceType.Testimony);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        foreach (var t in suspect.testimonies)
        {
            bool isThis = selectedWitness == t.witnessName;
            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label(t.witnessName, new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Space(5);
            GUILayout.Label(t.baseTestimony, _textStyle);

            if (isThis)
            {
                GUILayout.Space(8);
                GUILayout.Label("[CLARIFICATION REQUESTED]", new GUIStyle(_textStyle) { normal = { textColor = Color.yellow } });
                GUILayout.Space(5);
                GUILayout.Label(t.clarification, _textStyle);
            }
            else if (!alreadyRequested)
            {
                GUILayout.Space(5);
                if (GUILayout.Button("Request Clarification", _smallButtonStyle, GUILayout.Width(200)))
                {
                    _choices.Commit(_state.CurrentWeek, ChoiceType.Testimony, t.witnessName);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(8);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.enabled = alreadyRequested;
        if (GUILayout.Button("NEXT DAY >>", _buttonStyle))
        {
            _state.AdvanceDay(); // day 3 -> 4
            _screen = GameScreen.Interrogation;
            ResetUIState();
        }
        GUI.enabled = true;
    }

    // ─────────────────── INTERROGATION (THURSDAY) ───────────────────
    void DrawInterrogation()
    {
        var suspect = _cases.ActiveCase;
        if (suspect == null) return;

        GUILayout.Label($"THURSDAY - INTERROGATION: {suspect.displayName}", _headerStyle);
        GUILayout.Space(5);

        bool followUpChosen = _choices.IsChosen(_state.CurrentWeek, ChoiceType.FollowUp);
        string selectedFollowUp = _choices.GetSelected(_state.CurrentWeek, ChoiceType.FollowUp);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        // Standard questions
        GUILayout.Label("STANDARD QUESTIONS:", new GUIStyle(_headerStyle) { fontSize = 16 });
        GUILayout.Space(5);

        if (suspect.standardQuestions != null)
        {
            foreach (var qa in suspect.standardQuestions)
            {
                GUILayout.BeginVertical(_boxStyle);
                GUILayout.Label($"-- {qa.question}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold });
                GUILayout.Space(3);
                GUILayout.Label(qa.answer, _textStyle);
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

        // Conditional questions (based on Mon-Wed choices)
        if (suspect.conditionalQuestions != null)
        {
            bool hasConditional = false;
            foreach (var cq in suspect.conditionalQuestions)
            {
                string sel = _choices.GetSelected(_state.CurrentWeek, cq.requiredChoiceType);
                if (sel == cq.requiredChoiceId)
                {
                    if (!hasConditional)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("UNLOCKED QUESTIONS:", new GUIStyle(_headerStyle) { fontSize = 16, normal = { textColor = Color.yellow } });
                        GUILayout.Space(5);
                        hasConditional = true;
                    }
                    GUILayout.BeginVertical(_boxStyle);
                    GUILayout.Label($"-- {cq.question}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold });
                    GUILayout.Space(3);
                    GUILayout.Label(cq.answer, _textStyle);
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }
        }

        // Follow-up questions (unique daily action)
        GUILayout.Space(15);
        GUILayout.Label("OFF-PROTOCOL QUESTION (choose one):", new GUIStyle(_headerStyle) { fontSize = 16, normal = { textColor = new Color(1f, 0.6f, 0.3f) } });
        GUILayout.Space(5);

        if (suspect.followUps != null)
        {
            foreach (var fu in suspect.followUps)
            {
                bool isThis = selectedFollowUp == fu.followUpId;
                GUILayout.BeginVertical(_boxStyle);

                if (isThis)
                {
                    GUILayout.Label($"-- {fu.question}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, normal = { textColor = Color.green } });
                    GUILayout.Space(3);
                    GUILayout.Label(fu.answer, _textStyle);
                }
                else if (!followUpChosen)
                {
                    if (GUILayout.Button(fu.question, _smallButtonStyle))
                    {
                        _choices.Commit(_state.CurrentWeek, ChoiceType.FollowUp, fu.followUpId);
                        _selectedFollowUpAnswer = fu.answer;
                    }
                }
                else
                {
                    GUILayout.Label($"-- {fu.question}", new GUIStyle(_textStyle) { normal = { textColor = Color.gray } });
                }

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.enabled = followUpChosen;
        if (GUILayout.Button("GO TO COMMISSION >>", _buttonStyle))
        {
            _state.AdvanceDay(); // day 4 -> 5
            _screen = GameScreen.Briefing;
            ResetUIState();
        }
        GUI.enabled = true;
    }

    // ─────────────────── BRIEFING (FRIDAY) ───────────────────
    void DrawBriefing()
    {
        var suspect = _cases.ActiveCase;
        if (suspect == null) return;

        GUILayout.FlexibleSpace();
        GUILayout.Label($"FRIDAY - COMMISSION", _titleStyle);
        GUILayout.Space(10);
        GUILayout.Label($"Case: {suspect.displayName}", new GUIStyle(_headerStyle) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(30);

        GUILayout.Label("Your verdict:", new GUIStyle(_textStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 18 });
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        var currentVerdict = _verdicts.GetVerdict();

        // GUILTY stamp
        var guiltyStyle = new GUIStyle(_buttonStyle)
        {
            fontSize = 24,
            fixedHeight = 80,
            fixedWidth = 250,
            fontStyle = FontStyle.Bold
        };
        if (currentVerdict == VerdictType.Guilty)
            guiltyStyle.normal.textColor = Color.red;

        if (GUILayout.Button("GUILTY", guiltyStyle))
        {
            _verdicts.SetVerdict(VerdictType.Guilty);
            _verdictChosen = true;
        }

        GUILayout.Space(40);

        // NOT GUILTY stamp
        var notGuiltyStyle = new GUIStyle(guiltyStyle);
        if (currentVerdict == VerdictType.NotGuilty)
            notGuiltyStyle.normal.textColor = Color.green;

        if (GUILayout.Button("NOT GUILTY", notGuiltyStyle))
        {
            _verdicts.SetVerdict(VerdictType.NotGuilty);
            _verdictChosen = true;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(40);

        GUI.enabled = _verdictChosen && _verdicts.GetVerdict() != VerdictType.None;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("SIGN PROTOCOL", new GUIStyle(_buttonStyle) { fontSize = 20, fixedHeight = 50, fixedWidth = 300 }))
        {
            _verdicts.CommitAll(suspect.suspectId, _state.CurrentWeek, suspect);
            _state.AdvanceDay(); // day 5 -> overflow -> day 0, week++

            if (_state.IsGameComplete)
            {
                _screen = GameScreen.Ending;
            }
            else
            {
                _cases.LoadWeek(_state.CurrentWeek);
                StartOutcome();
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.enabled = true;

        GUILayout.FlexibleSpace();
    }

    // ─────────────────── ENDING ───────────────────
    void DrawEnding()
    {
        GUILayout.Space(40);
        GUILayout.Label("FINAL REPORT", _titleStyle);
        GUILayout.Space(20);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        GUILayout.Label("Summary of all verdicts and their consequences:", _headerStyle);
        GUILayout.Space(10);

        foreach (var v in _save.Data.verdicts)
        {
            var suspect = _cases.GetCase(v.week);
            string name = suspect != null ? suspect.displayName : v.suspectId;
            string verdictStr = v.verdict == VerdictType.Guilty ? "GUILTY" : "NOT GUILTY";
            bool correct = suspect != null &&
                ((v.verdict == VerdictType.Guilty && suspect.isGuilty) ||
                 (v.verdict == VerdictType.NotGuilty && !suspect.isGuilty));

            GUILayout.BeginVertical(_boxStyle);
            GUILayout.Label($"Week {v.week}: {name}", new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, fontSize = 16 });
            GUILayout.Label($"Verdict: {verdictStr}", new GUIStyle(_textStyle)
            {
                normal = { textColor = correct ? Color.green : Color.red }
            });

            if (!correct && suspect != null)
            {
                string consequence = v.verdict == VerdictType.Guilty
                    ? suspect.consequenceGuilty
                    : suspect.consequenceNotGuilty;
                if (!string.IsNullOrEmpty(consequence))
                {
                    GUILayout.Space(3);
                    GUILayout.Label(consequence, _textStyle);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(8);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(20);
        if (GUILayout.Button("MAIN MENU", _buttonStyle))
        {
            _screen = GameScreen.MainMenu;
        }
    }
}
