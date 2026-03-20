using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum VerdictType { None, Guilty, NotGuilty }

[Serializable]
public class ActionRecord
{
    public ActionType actionType;
    public string targetId;
    public int caseNumber;
}

[Serializable]
public class CaseResultRecord
{
    public string caseId;
    public CaseResult result;
    public string accusedPersonId;
    public int caseNumber;
}

[Serializable]
public class ScheduledConsequence
{
    public string caseId;
    public string headlineText;
    public string detailText;
    public int triggerCase;
}

[Serializable]
public class SaveData
{
    public int currentCase = 1;
    public int movesRemaining = 8;
    public int pressPenalty;

    public List<ActionRecord> actions = new();
    public List<string> revealedFragments = new();

    // Deduction chain slots
    public string chainMotive = "";
    public string chainOpportunity = "";
    public string chainEvidence = "";
    public string chainSuspect = "";

    public List<CaseResultRecord> caseResults = new();
    public List<string> escapedCriminals = new();
    public List<ScheduledConsequence> pending = new();

    // Track which interrogation questions were asked (caseNumber:personId:questionIndex)
    public List<string> askedQuestions = new();
    // Track which location zones were inspected (caseNumber:locationId:zoneIndex)
    public List<string> inspectedZones = new();
    // Track which database queries were made (caseNumber:queryId)
    public List<string> madeQueries = new();
    // Track which confrontations were done (caseNumber:personA|personB)
    public List<string> doneConfrontations = new();
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
