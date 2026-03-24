using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class DetectedContradiction
{
    public string personId;
    public int    questionIndex;
    public string claimText;                      // what they said (the lie)
    public string truthText;                      // actual truth (revealed on resolve)
    public List<string> contradictingFragmentIds; // physical evidence that exposes the lie
    public string resolveKey => $"{personId}:{questionIndex}";
}

/// <summary>
/// Detects when a player has BOTH a lie testimony AND physical evidence that contradicts it.
/// This is the trigger for meaningful confrontations.
/// </summary>
public class ContradictionService
{
    readonly SaveService _save;

    public ContradictionService(SaveService save) => _save = save;

    // ── Called when LocationInspect or Database reveals a fragment ──
    public void RegisterPhysicalFragment(string fragmentId)
    {
        if (_save.Data.physicalFragments.Contains(fragmentId)) return;
        _save.Data.physicalFragments.Add(fragmentId);
        _save.Save();
    }

    public bool IsPhysical(string fragmentId) =>
        _save.Data.physicalFragments.Contains(fragmentId);

    // ── Core: find active (unresolved) contradictions ──
    public List<DetectedContradiction> GetActive(CaseSO c, ActionService actions)
    {
        var result = new List<DetectedContradiction>();
        if (c?.interrogations == null || c.fragments == null) return result;

        // Index physical fragments by relatedPersonId
        var physByPerson = c.fragments
            .Where(f => _save.Data.physicalFragments.Contains(f.fragmentId)
                     && !string.IsNullOrEmpty(f.relatedPersonId))
            .GroupBy(f => f.relatedPersonId)
            .ToDictionary(g => g.Key, g => g.Select(f => f.fragmentId).ToList());

        foreach (var interr in c.interrogations)
        {
            for (int qi = 0; qi < (interr.questions?.Length ?? 0); qi++)
            {
                var q = interr.questions[qi];
                if (!q.isLie) continue;
                if (!actions.IsQuestionAsked(interr.targetPersonId, qi)) continue;

                string key = $"{interr.targetPersonId}:{qi}";
                if (_save.Data.resolvedContradictions.Contains(key)) continue;

                if (!physByPerson.TryGetValue(interr.targetPersonId, out var contradicting))
                    continue;

                result.Add(new DetectedContradiction
                {
                    personId                 = interr.targetPersonId,
                    questionIndex            = qi,
                    claimText                = q.answerText,
                    truthText                = q.truthText,
                    contradictingFragmentIds = contradicting
                });
            }
        }

        return result;
    }

    // ── Returns true if this confrontation pair has at least one active contradiction ──
    public bool CanConfront(string personA, string personB, CaseSO c, ActionService actions)
    {
        var active = GetActive(c, actions);
        return active.Any(ct => ct.personId == personA || ct.personId == personB);
    }

    // ── Resolve contradictions for both people in a confrontation ──
    // Returns list of contradiction keys that were resolved
    public List<DetectedContradiction> ResolveForConfrontation(
        string personA, string personB, CaseSO c, ActionService actions)
    {
        var active   = GetActive(c, actions);
        var resolved = active.Where(ct => ct.personId == personA || ct.personId == personB).ToList();

        foreach (var ct in resolved)
        {
            if (!_save.Data.resolvedContradictions.Contains(ct.resolveKey))
                _save.Data.resolvedContradictions.Add(ct.resolveKey);
        }
        _save.Save();
        return resolved;
    }
}
