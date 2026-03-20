using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaseService
{
    List<CaseSO> _all;

    public CaseSO ActiveCase { get; private set; }

    public void LoadAll()
    {
        _all = Resources.LoadAll<CaseSO>("Cases").ToList();
    }

    public void LoadCase(int caseNumber)
    {
        if (_all == null) LoadAll();
        ActiveCase = _all.FirstOrDefault(c => c.caseNumber == caseNumber);
    }

    public CaseSO GetCase(int caseNumber)
    {
        if (_all == null) LoadAll();
        return _all.FirstOrDefault(c => c.caseNumber == caseNumber);
    }
}
