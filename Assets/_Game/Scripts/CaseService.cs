using System.Collections.Generic;
using System.Linq;

public class CaseService
{
    readonly List<SuspectSO> _all = new();

    public SuspectSO ActiveCase { get; private set; }

    public void RegisterCase(SuspectSO suspect)
    {
        _all.Add(suspect);
    }

    public void LoadWeek(int week)
    {
        ActiveCase = _all.FirstOrDefault(s => s.weekNumber == week);
    }

    public SuspectSO GetCase(int week)
    {
        return _all.FirstOrDefault(s => s.weekNumber == week);
    }
}
