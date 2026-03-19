using System;

public class GameStateService
{
    public int CurrentWeek { get; private set; }
    public int CurrentDay { get; private set; }

    public event Action<int> OnDayChanged;
    public event Action<int> OnWeekChanged;

    readonly SaveService _save;

    public GameStateService(SaveService save)
    {
        _save = save;
        CurrentWeek = save.Data.currentWeek;
        CurrentDay = save.Data.currentDay;
    }

    public void AdvanceDay()
    {
        CurrentDay++;
        if (CurrentDay > 5)
        {
            CurrentDay = 0;
            CurrentWeek++;
            _save.Data.currentWeek = CurrentWeek;
            OnWeekChanged?.Invoke(CurrentWeek);
        }
        _save.Data.currentDay = CurrentDay;
        _save.Save();
        OnDayChanged?.Invoke(CurrentDay);
    }

    public void SetDay(int day)
    {
        CurrentDay = day;
        _save.Data.currentDay = day;
        _save.Save();
    }

    public bool IsGameComplete => CurrentWeek > 8;
}
