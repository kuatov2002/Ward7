using System.Collections.Generic;
using System.Linq;

public class ConsequenceService
{
    readonly List<ScheduledConsequence> _queue;
    readonly SaveService _save;

    public ConsequenceService(SaveService save)
    {
        _save = save;
        _queue = save.Data.pending;
    }

    public void Schedule(CaseSO caseSO, CaseResult result, int currentCase)
    {
        CaseConsequenceData data = result switch
        {
            CaseResult.CorrectArrest => caseSO.consequenceCorrectArrest,
            CaseResult.WrongArrest => caseSO.consequenceWrongArrest,
            CaseResult.Unsolved => caseSO.consequenceUnsolved,
            CaseResult.WeakCase => caseSO.consequenceWeakCase,
            _ => null
        };

        if (data == null || string.IsNullOrEmpty(data.headlineText))
            return;

        _queue.Add(new ScheduledConsequence
        {
            caseId = caseSO.caseId,
            headlineText = data.headlineText,
            detailText = data.detailText,
            triggerCase = currentCase + 1
        });
        _save.Save();
    }

    public List<ScheduledConsequence> ResolveForCase(int caseNumber)
    {
        var due = _queue.Where(c => c.triggerCase == caseNumber).ToList();
        _queue.RemoveAll(c => c.triggerCase == caseNumber);
        _save.Save();
        return due;
    }

    public List<string> GetEscapedCriminals() => _save.Data.escapedCriminals;
}
