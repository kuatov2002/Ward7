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

    public void Schedule(string suspectId, VerdictType verdict, int currentWeek, SuspectSO suspect)
    {
        string headline = verdict == VerdictType.Guilty
            ? suspect.consequenceGuilty
            : suspect.consequenceNotGuilty;

        if (string.IsNullOrEmpty(headline))
            return;

        _queue.Add(new ScheduledConsequence
        {
            suspectId = suspectId,
            headlineText = headline,
            triggerWeek = currentWeek + 1
        });
        _save.Save();
    }

    public List<string> ResolveWeek(int week)
    {
        var due = _queue.Where(c => c.triggerWeek == week).ToList();
        _queue.RemoveAll(c => c.triggerWeek == week);
        _save.Save();
        return due.Select(c => c.headlineText).ToList();
    }
}
