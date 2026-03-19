using UnityEngine;

public class OfficeController : MonoBehaviour
{
    public static OfficeController Instance { get; private set; }

    public DeskObject[] deskObjects;

    bool _gameStarted;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Hide all desk objects initially and show main menu
        foreach (var obj in deskObjects)
            if (obj != null) obj.SetVisible(false);

        // Delay one frame so UIManager.Start() has run
        StartCoroutine(ShowMainMenuDelayed());
    }

    System.Collections.IEnumerator ShowMainMenuDelayed()
    {
        yield return null;
        UIManager.Instance.ShowPanel("main-menu-panel");
    }

    public void RefreshDesk()
    {
        if (!_gameStarted) return;
        var state = ServiceLocator.Get<GameStateService>();
        int day = state.CurrentDay;
        foreach (var obj in deskObjects)
        {
            if (obj != null)
                obj.SetVisible(obj.ShouldBeVisible(day));
        }

        string[] dayNames = { "", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница" };
        if (day >= 1 && day <= 5)
            UIManager.Instance.ShowDayLabel($"Неделя {state.CurrentWeek} — {dayNames[day]}");
        else
            UIManager.Instance.HideDayLabel();

        // Update atmosphere lighting
        if (AtmosphereController.Instance != null)
            AtmosphereController.Instance.SetDayLighting(day);

        // Update evidence board
        if (EvidenceBoard.Instance != null)
            EvidenceBoard.Instance.RefreshFromChoices();
    }

    public void OnGameStarted()
    {
        _gameStarted = true;
        ServiceLocator.Get<GameStateService>().OnDayChanged += _ => RefreshDesk();
        RefreshDesk();

        // Start ambient sounds
        if (ProceduralAudio.Instance != null)
            ProceduralAudio.Instance.StartAmbient();
    }

    public void OpenPanel(string panelName)
    {
        UIManager.Instance.ShowPanel(panelName);
    }

    public void TryAdvanceDay()
    {
        var state = ServiceLocator.Get<GameStateService>();
        var choices = ServiceLocator.Get<DailyChoiceService>();
        int w = state.CurrentWeek;
        int d = state.CurrentDay;
        bool canAdvance = true;

        switch (d)
        {
            case 1: canAdvance = choices.IsChosen(w, ChoiceType.Contact); break;
            case 2: canAdvance = choices.IsChosen(w, ChoiceType.Evidence); break;
            case 3: canAdvance = choices.IsChosen(w, ChoiceType.Testimony); break;
            case 4: canAdvance = choices.IsChosen(w, ChoiceType.FollowUp); break;
            case 5: return;
        }

        if (!canAdvance)
        {
            UIManager.Instance.ShowHint("Сначала сделайте выбор");
            return;
        }

        state.AdvanceDay();
        HandleDayStart();
    }

    public void HandleDayStart()
    {
        RefreshDesk();
        var state = ServiceLocator.Get<GameStateService>();
        if (state.CurrentDay == 0)
        {
            UIManager.Instance.ShowPanel("outcome-panel");
        }
    }

    public void AfterVerdictCommit()
    {
        var state = ServiceLocator.Get<GameStateService>();
        state.AdvanceDay();
        if (state.IsGameComplete)
        {
            UIManager.Instance.ShowPanel("ending-panel");
        }
        else
        {
            ServiceLocator.Get<CaseService>().LoadWeek(state.CurrentWeek);
            HandleDayStart();
        }
    }
}
