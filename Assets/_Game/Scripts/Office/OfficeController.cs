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

        // Music intensity: calm → building → intense
        if (ProceduralMusic.Instance != null)
        {
            float[] intensity = { 0.2f, 0.3f, 0.4f, 0.5f, 0.7f, 1.0f };
            ProceduralMusic.Instance.SetIntensity(day >= 0 && day <= 5 ? intensity[day] : 0.5f);
        }

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

        // Show controls hint for first-time players
        UIManager.Instance.ShowControlsHint();

        // Start music
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.StartMusic();
    }

    public void OpenPanel(string panelName)
    {
        var save = ServiceLocator.Get<SaveService>();
        var cases = ServiceLocator.Get<CaseService>();

        // Redirect to mini-game if not completed
        if (panelName == "dossier-panel" && save != null && !save.Data.documentCompareCompleted)
        {
            var s = cases?.ActiveCase;
            if (s != null && s.documentCompare != null && s.documentCompare.lines != null && s.documentCompare.lines.Length > 0)
            {
                UIManager.Instance.ShowPanel("doccompare-panel");
                return;
            }
        }

        if (panelName == "evidence-panel" && save != null && !save.Data.evidenceInspectCompleted)
        {
            var s = cases?.ActiveCase;
            if (s != null && s.evidence != null && s.evidence.Length > 0 && s.evidence[0].zones != null && s.evidence[0].zones.Length > 0)
            {
                UIManager.Instance.ShowPanel("evidence-inspect-panel");
                return;
            }
        }

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
            case 4:
                var press = ServiceLocator.Get<PressureService>();
                bool bluffBlock = press != null && press.BluffFailed;
                bool pressShutdown = false;
                var activeCase = ServiceLocator.Get<CaseService>().ActiveCase;
                if (press != null && activeCase != null)
                    pressShutdown = press.IsShutdown(activeCase.pressureThreshold);
                canAdvance = choices.IsChosen(w, ChoiceType.FollowUp) || bluffBlock || pressShutdown;
                break;
            case 5: return;
        }

        if (!canAdvance)
        {
            string[] hints = {
                "",
                "Позвоните контакту (телефон на столе)",
                "Выберите приоритетную улику (папки на столе)",
                "Запросите уточнение у свидетеля (компьютер)",
                "Завершите допрос (блокнот на столе)"
            };
            string hint = d >= 1 && d <= 4 ? hints[d] : "Сначала сделайте выбор";
            UIManager.Instance.ShowHint(hint);
            return;
        }

        // Day transition with fade
        string[] dayNames = { "", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница" };
        int nextDay = d + 1;
        if (nextDay > 5) nextDay = 0;
        string transText = nextDay >= 1 && nextDay <= 5
            ? dayNames[nextDay]
            : $"Неделя {state.CurrentWeek}";

        UIManager.Instance.PlayDayTransition(transText, () => {
            state.AdvanceDay();
            HandleDayStart();
        });
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
        UIManager.Instance.PlayDayTransition("Протокол подписан...", () => {
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
        });
    }
}
