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

    SaveService _save;
    GameStateService _state;
    CaseService _cases;
    DailyChoiceService _choices;
    VerdictService _verdicts;
    ConsequenceService _conseq;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[GameManager]");
        go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);
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
    }

    // ─── PUBLIC API for UI panels ───

    public void StartNewGame()
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
    }

    public void ContinueGame()
    {
        _cases.LoadWeek(_state.CurrentWeek);
    }

    public bool HasSave() => _save.HasSave();
}
