using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaseService
{
    List<SuspectSO> _all;

    public SuspectSO ActiveCase { get; private set; }

    public void LoadAll()
    {
        _all = Resources.LoadAll<SuspectSO>("Suspects").ToList();
    }

    public void LoadWeek(int week)
    {
        if (_all == null) LoadAll();
        ActiveCase = _all.FirstOrDefault(s => s.weekNumber == week);
    }

    public SuspectSO GetCase(int week)
    {
        if (_all == null) LoadAll();
        return _all.FirstOrDefault(s => s.weekNumber == week);
    }
}
