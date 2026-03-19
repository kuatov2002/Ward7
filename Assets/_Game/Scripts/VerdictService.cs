using System;

public class VerdictService
{
    VerdictType _pending = VerdictType.None;
    readonly ConsequenceService _consequences;
    readonly SaveService _save;

    public VerdictService(ConsequenceService consequences, SaveService save)
    {
        _consequences = consequences;
        _save = save;
    }

    public void SetVerdict(VerdictType verdict) => _pending = verdict;
    public VerdictType GetVerdict() => _pending;

    public void CommitAll(string suspectId, int currentWeek, SuspectSO suspect)
    {
        if (_pending == VerdictType.None)
            return;

        _consequences.Schedule(suspectId, _pending, currentWeek, suspect);

        _save.Data.verdicts.Add(new VerdictRecord
        {
            suspectId = suspectId,
            verdict = _pending,
            week = currentWeek
        });
        _save.Save();
        _pending = VerdictType.None;
    }
}
