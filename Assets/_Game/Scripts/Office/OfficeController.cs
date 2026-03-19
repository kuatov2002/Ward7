using UnityEngine;

public class OfficeController : MonoBehaviour
{
    public static OfficeController Instance { get; private set; }

    public DeskObject[] deskObjects;

    UIManager _ui;
    GameStateService _state;
    DailyChoiceService _choices;
    bool _gameStarted;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _state = ServiceLocator.Get<GameStateService>();
        _choices = ServiceLocator.Get<DailyChoiceService>();
        _ui = UIManager.Instance;

        _state.OnDayChanged += _ => RefreshDesk();

        // Hide all desk objects initially and show main menu
        foreach (var obj in deskObjects)
            if (obj != null) obj.SetVisible(false);

        // Delay one frame so UIManager.Start() has run
        StartCoroutine(ShowMainMenuDelayed());
    }

    System.Collections.IEnumerator ShowMainMenuDelayed()
    {
        yield return null;
        _ui.ShowPanel("main-menu-panel");
    }

    public void RefreshDesk()
    {
        if (!_gameStarted) return;
        int day = _state.CurrentDay;
        foreach (var obj in deskObjects)
        {
            if (obj != null)
                obj.SetVisible(obj.ShouldBeVisible(day));
        }

        // Show day label
        string[] dayNames = { "", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница" };
        if (day >= 1 && day <= 5)
            _ui.ShowDayLabel($"Неделя {_state.CurrentWeek} — {dayNames[day]}");
        else
            _ui.HideDayLabel();
    }

    public void OnGameStarted()
    {
        _gameStarted = true;
        RefreshDesk();
    }

    public void OpenPanel(string panelName)
    {
        _ui.ShowPanel(panelName);
    }

    public void TryAdvanceDay()
    {
        int w = _state.CurrentWeek;
        int d = _state.CurrentDay;
        bool canAdvance = true;

        switch (d)
        {
            case 1: canAdvance = _choices.IsChosen(w, ChoiceType.Contact); break;
            case 2: canAdvance = _choices.IsChosen(w, ChoiceType.Evidence); break;
            case 3: canAdvance = _choices.IsChosen(w, ChoiceType.Testimony); break;
            case 4: canAdvance = _choices.IsChosen(w, ChoiceType.FollowUp); break;
            case 5: return; // verdict is handled by BriefingUI
        }

        if (!canAdvance)
        {
            _ui.ShowHint("Сначала сделайте выбор");
            return;
        }

        _state.AdvanceDay();
        HandleDayStart();
    }

    public void HandleDayStart()
    {
        RefreshDesk();
        int d = _state.CurrentDay;
        if (d == 0)
        {
            _ui.ShowPanel("outcome-panel");
        }
    }

    public void AfterVerdictCommit()
    {
        _state.AdvanceDay();
        var cases = ServiceLocator.Get<CaseService>();
        if (_state.IsGameComplete)
        {
            _ui.ShowPanel("ending-panel");
        }
        else
        {
            cases.LoadWeek(_state.CurrentWeek);
            HandleDayStart();
        }
    }
}
