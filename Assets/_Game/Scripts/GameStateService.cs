using System;

public class GameStateService
{
    public int CurrentCase { get; private set; }
    public int MovesRemaining { get; private set; }
    public int PressPenalty { get; private set; }

    public event Action<int> OnMovesChanged;
    public event Action<int> OnCaseChanged;

    readonly SaveService _save;

    public GameStateService(SaveService save)
    {
        _save = save;
        CurrentCase = save.Data.currentCase;
        MovesRemaining = save.Data.movesRemaining;
        PressPenalty = save.Data.pressPenalty;
    }

    public void InitCase(int totalMoves)
    {
        int effective = totalMoves - PressPenalty;
        if (effective < 3) effective = 3; // minimum 3 moves
        MovesRemaining = effective;
        _save.Data.movesRemaining = MovesRemaining;
        _save.Save();
        OnMovesChanged?.Invoke(MovesRemaining);
    }

    public bool CanSpend(int cost)
    {
        return MovesRemaining >= cost;
    }

    public void SpendMoves(int cost)
    {
        MovesRemaining -= cost;
        if (MovesRemaining < 0) MovesRemaining = 0;
        _save.Data.movesRemaining = MovesRemaining;
        _save.Save();
        OnMovesChanged?.Invoke(MovesRemaining);
    }

    public bool IsOutOfMoves => MovesRemaining <= 0;

    public void AdvanceCase()
    {
        CurrentCase++;
        _save.Data.currentCase = CurrentCase;
        _save.Save();
        OnCaseChanged?.Invoke(CurrentCase);
    }

    public void AddPressPenalty(int amount)
    {
        PressPenalty += amount;
        _save.Data.pressPenalty = PressPenalty;
        _save.Save();
    }

    public void ResetForNewCase()
    {
        _save.Data.revealedFragments.Clear();
        _save.Data.chainMotive = "";
        _save.Data.chainOpportunity = "";
        _save.Data.chainEvidence = "";
        _save.Data.chainSuspect = "";
        _save.Data.askedQuestions.Clear();
        _save.Data.inspectedZones.Clear();
        _save.Data.madeQueries.Clear();
        _save.Data.doneConfrontations.Clear();
        _save.Data.actions.RemoveAll(a => a.caseNumber == CurrentCase);
        _save.Save();
    }

    public bool IsGameComplete =>
        ServiceLocator.Get<CaseService>().GetCase(CurrentCase) == null;
}
