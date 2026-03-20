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
        foreach (var obj in deskObjects)
            if (obj != null) obj.SetVisible(false);

        StartCoroutine(ShowMainMenuDelayed());
    }

    System.Collections.IEnumerator ShowMainMenuDelayed()
    {
        yield return null;
        UIManager.Instance.ShowPanel("main-menu-panel");
    }

    public void OnGameStarted()
    {
        _gameStarted = true;
        RefreshDesk();

        if (ProceduralAudio.Instance != null)
            ProceduralAudio.Instance.StartAmbient();

        UIManager.Instance.ShowControlsHint();

        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.StartMusic();

        // Listen to moves changes for atmosphere
        var state = ServiceLocator.Get<GameStateService>();
        state.OnMovesChanged += OnMovesChanged;

        // Update HUD
        UIManager.Instance.UpdateMovesCounter(state.MovesRemaining, state.PressPenalty);
    }

    void OnMovesChanged(int remaining)
    {
        var state = ServiceLocator.Get<GameStateService>();
        var cases = ServiceLocator.Get<CaseService>();

        UIManager.Instance.UpdateMovesCounter(remaining, state.PressPenalty);

        // Music intensity based on investigation progress (moves spent)
        if (ProceduralMusic.Instance != null && cases.ActiveCase != null)
        {
            float progress = 1f - (float)remaining / cases.ActiveCase.totalMoves;
            ProceduralMusic.Instance.SetIntensity(0.2f + progress * 0.8f);
        }

        // Atmosphere based on progress
        if (AtmosphereController.Instance != null)
        {
            float progress = cases.ActiveCase != null
                ? 1f - (float)remaining / cases.ActiveCase.totalMoves
                : 0.5f;
            int fakeDay = Mathf.Clamp(Mathf.RoundToInt(progress * 5f), 1, 5);
            AtmosphereController.Instance.SetDayLighting(fakeDay);
        }
    }

    public void RefreshDesk()
    {
        if (!_gameStarted) return;

        // All desk objects always visible (no day-based visibility)
        foreach (var obj in deskObjects)
        {
            if (obj != null) obj.SetVisible(true);
        }

        var state = ServiceLocator.Get<GameStateService>();
        UIManager.Instance.ShowDayLabel($"Дело #{state.CurrentCase}");

        // Update evidence board
        if (EvidenceBoard.Instance != null)
            EvidenceBoard.Instance.RefreshFromFragments();
    }

    public void OpenPanel(string panelName)
    {
        UIManager.Instance.ShowPanel(panelName);
    }

    public void AfterCaseComplete()
    {
        var state = ServiceLocator.Get<GameStateService>();
        var cases = ServiceLocator.Get<CaseService>();

        state.ResetForNewCase();
        state.AdvanceCase();

        if (state.IsGameComplete)
        {
            UIManager.Instance.ShowPanel("ending-panel");
        }
        else
        {
            cases.LoadCase(state.CurrentCase);
            if (cases.ActiveCase != null)
                state.InitCase(cases.ActiveCase.totalMoves);
            UIManager.Instance.ShowPanel("case-briefing-panel");
        }
    }
}
