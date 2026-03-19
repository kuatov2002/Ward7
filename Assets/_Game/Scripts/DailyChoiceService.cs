using System;
using System.Collections.Generic;
using System.Linq;

public class DailyChoiceService
{
    readonly List<DailyChoiceRecord> _choices;
    readonly SaveService _save;

    public DailyChoiceService(SaveService save)
    {
        _save = save;
        _choices = save.Data.dailyChoices;
    }

    public bool IsChosen(string suspectId, ChoiceType type) =>
        _choices.Any(c => c.suspectId == suspectId && c.choiceType == type);

    public void Commit(string suspectId, ChoiceType type, string selectedId)
    {
        if (IsChosen(suspectId, type))
            return; // silently ignore in prototype

        _choices.Add(new DailyChoiceRecord
        {
            suspectId = suspectId,
            choiceType = type,
            selectedId = selectedId
        });
        _save.Save();
    }

    public string GetSelected(string suspectId, ChoiceType type) =>
        _choices.FirstOrDefault(c =>
            c.suspectId == suspectId && c.choiceType == type)?.selectedId;
}
