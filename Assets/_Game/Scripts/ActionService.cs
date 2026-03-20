using System.Collections.Generic;
using System.Linq;

public class ActionService
{
    readonly SaveService _save;
    readonly GameStateService _state;

    public ActionService(SaveService save, GameStateService state)
    {
        _save = save;
        _state = state;
    }

    static readonly Dictionary<ActionType, int> ActionCosts = new()
    {
        { ActionType.Interrogation, 2 },
        { ActionType.LocationInspect, 1 },
        { ActionType.DatabaseQuery, 1 },
        { ActionType.Confrontation, 3 }
    };

    public int GetCost(ActionType type) => ActionCosts.TryGetValue(type, out int c) ? c : 1;

    public bool CanPerform(ActionType type)
    {
        return _state.CanSpend(GetCost(type));
    }

    public void CommitAction(ActionType type, string targetId)
    {
        int cost = GetCost(type);
        _state.SpendMoves(cost);

        _save.Data.actions.Add(new ActionRecord
        {
            actionType = type,
            targetId = targetId,
            caseNumber = _state.CurrentCase
        });
        _save.Save();
    }

    public List<ActionRecord> GetActions(int caseNumber)
    {
        return _save.Data.actions.Where(a => a.caseNumber == caseNumber).ToList();
    }

    public bool HasPerformed(ActionType type, string targetId)
    {
        int c = _state.CurrentCase;
        return _save.Data.actions.Any(a =>
            a.caseNumber == c && a.actionType == type && a.targetId == targetId);
    }

    // Interrogation tracking
    public void MarkQuestionAsked(string personId, int questionIndex)
    {
        string key = $"{_state.CurrentCase}:{personId}:{questionIndex}";
        if (!_save.Data.askedQuestions.Contains(key))
        {
            _save.Data.askedQuestions.Add(key);
            _save.Save();
        }
    }

    public bool IsQuestionAsked(string personId, int questionIndex)
    {
        string key = $"{_state.CurrentCase}:{personId}:{questionIndex}";
        return _save.Data.askedQuestions.Contains(key);
    }

    // Location tracking
    public void MarkZoneInspected(string locationId, int zoneIndex)
    {
        string key = $"{_state.CurrentCase}:{locationId}:{zoneIndex}";
        if (!_save.Data.inspectedZones.Contains(key))
        {
            _save.Data.inspectedZones.Add(key);
            _save.Save();
        }
    }

    public bool IsZoneInspected(string locationId, int zoneIndex)
    {
        string key = $"{_state.CurrentCase}:{locationId}:{zoneIndex}";
        return _save.Data.inspectedZones.Contains(key);
    }

    // Database tracking
    public void MarkQueryMade(string queryId)
    {
        string key = $"{_state.CurrentCase}:{queryId}";
        if (!_save.Data.madeQueries.Contains(key))
        {
            _save.Data.madeQueries.Add(key);
            _save.Save();
        }
    }

    public bool IsQueryMade(string queryId)
    {
        string key = $"{_state.CurrentCase}:{queryId}";
        return _save.Data.madeQueries.Contains(key);
    }

    // Confrontation tracking
    public void MarkConfrontationDone(string personA, string personB)
    {
        string key = MakeConfrontationKey(personA, personB);
        if (!_save.Data.doneConfrontations.Contains(key))
        {
            _save.Data.doneConfrontations.Add(key);
            _save.Save();
        }
    }

    public bool IsConfrontationDone(string personA, string personB)
    {
        string key = MakeConfrontationKey(personA, personB);
        return _save.Data.doneConfrontations.Contains(key);
    }

    string MakeConfrontationKey(string a, string b)
    {
        return string.Compare(a, b) < 0
            ? $"{_state.CurrentCase}:{a}|{b}"
            : $"{_state.CurrentCase}:{b}|{a}";
    }
}
