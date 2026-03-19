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

    public bool IsChosen(int week, ChoiceType type) =>
        _choices.Any(c => c.week == week && c.choiceType == type);

    public void Commit(int week, ChoiceType type, string selectedId)
    {
        if (IsChosen(week, type))
            return;

        _choices.Add(new DailyChoiceRecord
        {
            week = week,
            choiceType = type,
            selectedId = selectedId
        });
        _save.Save();
    }

    public string GetSelected(int week, ChoiceType type) =>
        _choices.FirstOrDefault(c =>
            c.week == week && c.choiceType == type)?.selectedId;
}
