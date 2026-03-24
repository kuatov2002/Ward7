using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
 
    SaveService          _save;
    GameStateService     _state;
    CaseService          _cases;
    ActionService        _actions;
    DeductionService     _deduction;
    VerdictService       _verdicts;
    ConsequenceService   _conseq;
    ContradictionService _contradictions;
 
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
        _save           = new SaveService();
        _save.Load();
        _state          = new GameStateService(_save);
        _cases          = new CaseService();
        _conseq         = new ConsequenceService(_save);
        _actions        = new ActionService(_save, _state);
        _deduction      = new DeductionService(_save);
        _contradictions = new ContradictionService(_save);
        _verdicts       = new VerdictService(_conseq, _save, _state);
 
        ServiceLocator.Register(_save);
        ServiceLocator.Register(_state);
        ServiceLocator.Register(_cases);
        ServiceLocator.Register(_actions);
        ServiceLocator.Register(_deduction);
        ServiceLocator.Register(_conseq);
        ServiceLocator.Register(_verdicts);
        ServiceLocator.Register(_contradictions);
 
        _cases.LoadAll();
    }
 
    public void StartNewGame()
    {
        _save.DeleteSave();
        _state          = new GameStateService(_save);
        _conseq         = new ConsequenceService(_save);
        _actions        = new ActionService(_save, _state);
        _deduction      = new DeductionService(_save);
        _contradictions = new ContradictionService(_save);
        _verdicts       = new VerdictService(_conseq, _save, _state);
 
        ServiceLocator.Register(_state);
        ServiceLocator.Register(_actions);
        ServiceLocator.Register(_deduction);
        ServiceLocator.Register(_conseq);
        ServiceLocator.Register(_verdicts);
        ServiceLocator.Register(_contradictions);
 
        _cases.LoadCase(1);
        if (_cases.ActiveCase != null)
        {
            _state.InitCase(_cases.ActiveCase.totalMoves);
            _deduction.SetActiveCase(_cases.ActiveCase);
        }
    }
 
    public void ContinueGame()
    {
        _cases.LoadCase(_state.CurrentCase);
        if (_cases.ActiveCase != null)
            _deduction.SetActiveCase(_cases.ActiveCase);
    }
 
    public bool HasSave() => _save.HasSave();
}