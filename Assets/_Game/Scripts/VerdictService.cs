public class VerdictService
{
    readonly ConsequenceService _consequences;
    readonly SaveService _save;
    readonly GameStateService _state;

    public VerdictService(ConsequenceService consequences, SaveService save, GameStateService state)
    {
        _consequences = consequences;
        _save = save;
        _state = state;
    }

    public void CommitAccusation(CaseSO caseSO, CaseResult result, string accusedPersonId)
    {
        _consequences.Schedule(caseSO, result, _state.CurrentCase);

        _save.Data.caseResults.Add(new CaseResultRecord
        {
            caseId = caseSO.caseId,
            result = result,
            accusedPersonId = accusedPersonId,
            caseNumber = _state.CurrentCase
        });

        // Track escaped criminals
        if (result == CaseResult.WrongArrest || result == CaseResult.WeakCase)
        {
            if (!string.IsNullOrEmpty(caseSO.trueCulpritId))
                _save.Data.escapedCriminals.Add(caseSO.trueCulpritId);
        }

        // Press penalty for unsolved cases
        if (result == CaseResult.Unsolved)
        {
            _state.AddPressPenalty(1);
        }

        _save.Save();
    }

    public void CommitUnsolved(CaseSO caseSO)
    {
        CommitAccusation(caseSO, CaseResult.Unsolved, null);
    }
}
