using System.Collections.Generic;
using System.Linq;

public class DeductionService
{
    readonly SaveService _save;

    public DeductionService(SaveService save)
    {
        _save = save;
    }

    CaseSO _activeCase;

    public void SetActiveCase(CaseSO caseSO) => _activeCase = caseSO;

    public void RevealFragment(string fragmentId)
    {
        if (string.IsNullOrEmpty(fragmentId)) return;
        if (_save.Data.revealedFragments.Contains(fragmentId)) return;
        _save.Data.revealedFragments.Add(fragmentId);

        // Auto-reveal suspect fragments for the related person
        if (_activeCase?.fragments != null)
        {
            var frag = _activeCase.fragments.FirstOrDefault(f => f.fragmentId == fragmentId);
            if (frag != null && !string.IsNullOrEmpty(frag.relatedPersonId))
            {
                foreach (var sf in _activeCase.fragments)
                {
                    if (sf.fragmentType == FragmentType.Suspect
                        && sf.relatedPersonId == frag.relatedPersonId
                        && !_save.Data.revealedFragments.Contains(sf.fragmentId))
                    {
                        _save.Data.revealedFragments.Add(sf.fragmentId);
                    }
                }
            }
        }

        _save.Save();
    }

    public List<string> GetRevealedFragments() => _save.Data.revealedFragments;

    public bool IsRevealed(string fragmentId) =>
        _save.Data.revealedFragments.Contains(fragmentId);

    // Chain management
    public void PlaceOnChain(FragmentType slot, string fragmentId)
    {
        switch (slot)
        {
            case FragmentType.Motive:
                _save.Data.chainMotive = fragmentId;
                break;
            case FragmentType.Opportunity:
                _save.Data.chainOpportunity = fragmentId;
                break;
            case FragmentType.Evidence:
                _save.Data.chainEvidence = fragmentId;
                break;
            case FragmentType.Suspect:
                _save.Data.chainSuspect = fragmentId;
                break;
        }
        _save.Save();
    }

    public void RemoveFromChain(FragmentType slot)
    {
        PlaceOnChain(slot, "");
    }

    public string GetChainSlot(FragmentType slot)
    {
        return slot switch
        {
            FragmentType.Motive => _save.Data.chainMotive,
            FragmentType.Opportunity => _save.Data.chainOpportunity,
            FragmentType.Evidence => _save.Data.chainEvidence,
            FragmentType.Suspect => _save.Data.chainSuspect,
            _ => ""
        };
    }

    public bool IsChainComplete()
    {
        return !string.IsNullOrEmpty(_save.Data.chainMotive)
            && !string.IsNullOrEmpty(_save.Data.chainOpportunity)
            && !string.IsNullOrEmpty(_save.Data.chainEvidence)
            && !string.IsNullOrEmpty(_save.Data.chainSuspect);
    }

    public CaseResult ValidateChain(CaseSO caseSO)
    {
        if (!IsChainComplete())
            return CaseResult.Unsolved;

        // A slot is correct if the placed fragment has isTrue == true for its type
        bool motiveCorrect = IsFragmentTrue(caseSO, _save.Data.chainMotive);
        bool oppCorrect = IsFragmentTrue(caseSO, _save.Data.chainOpportunity);
        bool evidCorrect = IsFragmentTrue(caseSO, _save.Data.chainEvidence);
        bool suspCorrect = IsFragmentTrue(caseSO, _save.Data.chainSuspect);

        int correctCount = (motiveCorrect ? 1 : 0) + (oppCorrect ? 1 : 0)
                         + (evidCorrect ? 1 : 0) + (suspCorrect ? 1 : 0);

        if (correctCount == 4)
            return CaseResult.CorrectArrest;

        if (suspCorrect && correctCount >= 2)
            return CaseResult.WeakCase; // right person, weak evidence

        return CaseResult.WrongArrest;
    }

    static bool IsFragmentTrue(CaseSO caseSO, string fragmentId)
    {
        if (string.IsNullOrEmpty(fragmentId) || caseSO.fragments == null)
            return false;
        var frag = System.Array.Find(caseSO.fragments, f => f.fragmentId == fragmentId);
        return frag != null && frag.isTrue;
    }

    /// <summary>
    /// Get the person ID from the suspect fragment placed in the chain.
    /// Returns null if no suspect fragment is placed.
    /// </summary>
    public string GetAccusedPersonId(CaseSO caseSO)
    {
        string suspFragId = _save.Data.chainSuspect;
        if (string.IsNullOrEmpty(suspFragId) || caseSO.fragments == null)
            return null;

        var frag = caseSO.fragments.FirstOrDefault(f => f.fragmentId == suspFragId);
        return frag?.relatedPersonId;
    }

    public void ResetChain()
    {
        _save.Data.chainMotive = "";
        _save.Data.chainOpportunity = "";
        _save.Data.chainEvidence = "";
        _save.Data.chainSuspect = "";
        _save.Data.revealedFragments.Clear();
        _save.Save();
    }
}
