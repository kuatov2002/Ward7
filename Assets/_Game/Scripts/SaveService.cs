using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ChoiceType { Contact, Evidence, Testimony, FollowUp, Bluff }
public enum VerdictType { None, Guilty, NotGuilty }

[Serializable]
public class DailyChoiceRecord
{
    public int week;
    public ChoiceType choiceType;
    public string selectedId;
}

[Serializable]
public class VerdictRecord
{
    public string suspectId;
    public VerdictType verdict;
    public int week;
    public int justificationScore;
}

[Serializable]
public class ScheduledConsequence
{
    public string suspectId;
    public string headlineText;
    public int triggerWeek;
}

[Serializable]
public class NoteRecord
{
    public int week;
    public string text;
    public string source; // e.g. "dossier", "contact_linda", "evidence_firereport"
}

[Serializable]
public class SaveData
{
    public int currentWeek = 1;
    public int currentDay = 0;
    public List<DailyChoiceRecord> dailyChoices = new();
    public List<VerdictRecord> verdicts = new();
    public List<ScheduledConsequence> pending = new();
    public List<NoteRecord> notes = new();
    public int currentPressure;
    public bool bluffFailed;
}

public class SaveService
{
    static readonly string SavePath = Path.Combine(
        Application.persistentDataPath, "profile7_save.json");

    public SaveData Data { get; private set; } = new();

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                Data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch
            {
                Data = new SaveData();
            }
        }
    }

    public void Save()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
    }

    public void DeleteSave()
    {
        Data = new SaveData();
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public bool HasSave() => File.Exists(SavePath);
}
