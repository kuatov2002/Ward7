public class PressureService
{
    readonly SaveService _save;

    public PressureService(SaveService save)
    {
        _save = save;
    }

    public int CurrentPressure => _save.Data.currentPressure;
    public bool BluffFailed => _save.Data.bluffFailed;

    public void AddPressure(int amount)
    {
        _save.Data.currentPressure += amount;
        if (_save.Data.currentPressure < 0)
            _save.Data.currentPressure = 0;
        _save.Save();
    }

    public bool IsShutdown(int threshold)
    {
        return threshold > 0 && _save.Data.currentPressure >= threshold;
    }

    public void SetBluffFailed()
    {
        _save.Data.bluffFailed = true;
        _save.Save();
    }

    public void ResetForWeek()
    {
        _save.Data.currentPressure = 0;
        _save.Data.bluffFailed = false;
        _save.Save();
    }
}
