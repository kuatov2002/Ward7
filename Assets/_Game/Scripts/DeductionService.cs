using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Free-form deduction: player selects a suspect + up to 3 supporting fragments.
/// Validation checks if among selected fragments there is a valid motive,
/// opportunity, and evidence — without telling the player WHICH is which.
/// </summary>
public class DeductionService
{
    readonly SaveService _save;
    CaseSO _activeCase;

    public DeductionService(SaveService save) => _save = save;

    public void SetActiveCase(CaseSO c) => _activeCase = c;

    // ── Fragment revelation ──
    public void RevealFragment(string fragmentId)
    {
        if (string.IsNullOrEmpty(fragmentId)) return;
        if (_save.Data.revealedFragments.Contains(fragmentId)) return;
        _save.Data.revealedFragments.Add(fragmentId);

        // Auto-reveal suspect fragments for related person
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
                        _save.Data.revealedFragments.Add(sf.fragmentId);
                }
            }
        }
        _save.Save();
    }

    public List<string> GetRevealedFragments() => _save.Data.revealedFragments;
    public bool IsRevealed(string id)           => _save.Data.revealedFragments.Contains(id);

    // ── Accusation ──
    public void SetAccused(string personId)
    {
        _save.Data.accusedPersonId = personId;
        _save.Save();
    }

    public string GetAccused() => _save.Data.accusedPersonId;

    public void SetAccusationFragments(List<string> fragmentIds)
    {
        _save.Data.accusationFragments = new List<string>(fragmentIds);
        _save.Save();
    }

    public List<string> GetAccusationFragments() => _save.Data.accusationFragments ?? new();

    public bool IsAccusationReady() =>
        !string.IsNullOrEmpty(_save.Data.accusedPersonId) &&
        _save.Data.accusationFragments?.Count == 3;

    // ── Validation ──
    /// <summary>
    /// Validates the free-form accusation.
    /// Looks for motive / opportunity / evidence among the 3 selected fragments,
    /// checking isTrue for each type independently.
    /// </summary>
    public CaseResult ValidateAccusation(CaseSO c)
    {
        if (!IsAccusationReady()) return CaseResult.Unsolved;
        if (c?.fragments == null) return CaseResult.Unsolved;

        string accused     = _save.Data.accusedPersonId;
        var    selected    = _save.Data.accusationFragments;

        // Is the correct person accused?
        bool correctPerson = accused == c.trueCulpritId;

        // Among selected fragments, count how many are "true" for their type
        // (i.e., they genuinely constitute motive/opportunity/evidence for the true culprit)
        int correctFragments = selected.Count(fid => {
            var frag = c.fragments.FirstOrDefault(f => f.fragmentId == fid);
            return frag != null && frag.isTrue;
        });

        // Determine result
        if (correctPerson && correctFragments == 3) return CaseResult.CorrectArrest;
        if (correctPerson && correctFragments >= 2) return CaseResult.WeakCase;
        if (!correctPerson)                          return CaseResult.WrongArrest;

        return CaseResult.WeakCase;
    }

    // ── Compatibility shims (used by old code paths) ──
    [System.Obsolete("Use free-form accusation instead")]
    public void PlaceOnChain(FragmentType slot, string fragmentId) { }
    [System.Obsolete]
    public string GetChainSlot(FragmentType slot) => "";
    [System.Obsolete]
    public bool IsChainComplete() => IsAccusationReady();
    [System.Obsolete]
    public CaseResult ValidateChain(CaseSO c) => ValidateAccusation(c);
    [System.Obsolete]
    public string GetAccusedPersonId(CaseSO c) => GetAccused();

    public void ResetChain()
    {
        _save.Data.accusedPersonId      = "";
        _save.Data.accusationFragments  = new List<string>();
        _save.Data.revealedFragments.Clear();
        _save.Data.physicalFragments.Clear();
        _save.Data.resolvedContradictions.Clear();
        _save.Data.askedQuestions.Clear();
        _save.Data.inspectedZones.Clear();
        _save.Data.madeQueries.Clear();
        _save.Data.doneConfrontations.Clear();
        _save.Save();
    }
}