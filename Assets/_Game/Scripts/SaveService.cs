using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum VerdictType { None, Guilty, NotGuilty }

[Serializable]
public class ActionRecord
{
    public ActionType actionType;
    public string     targetId;
    public int        caseNumber;
}

[Serializable]
public class CaseResultRecord
{
    public string    caseId;
    public CaseResult result;
    public string    accusedPersonId;
    public int       caseNumber;
}

[Serializable]
public class ScheduledConsequence
{
    public string caseId;
    public string headlineText;
    public string detailText;
    public int    triggerCase;
}

[Serializable]
public class SaveData
{
    public int currentCase    = 1;
    public int movesRemaining = 8;
    public int pressPenalty;

    public List<ActionRecord> actions = new();

    // ── Fragments ──
    // All revealed fragments (physical + confirmed testimony)
    public List<string> revealedFragments = new();
    // Physical-only: from locations and database (always trustworthy)
    public List<string> physicalFragments = new();

    // ── Old deduction chain slots (kept for save compatibility, no longer used by UI) ──
    public string chainMotive     = "";
    public string chainOpportunity = "";
    public string chainEvidence   = "";
    public string chainSuspect    = "";

    // ── New free-form accusation ──
    public string       accusedPersonId      = "";
    public List<string> accusationFragments  = new(); // exactly 3, chosen by player

    // ── Case history ──
    public List<CaseResultRecord>   caseResults      = new();
    public List<string>             escapedCriminals = new();
    public List<ScheduledConsequence> pending         = new();

    // ── Action tracking ──
    public List<string> askedQuestions     = new();
    public List<string> inspectedZones     = new();
    public List<string> madeQueries        = new();
    public List<string> doneConfrontations = new();

    // ── Contradiction system ──
    // Lies that have been confronted and resolved
    public List<string> resolvedContradictions = new(); // "personId:questionIndex"
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
            try   { Data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)); }
            catch { Data = new SaveData(); }
        }
    }

    public void Save()      => File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
    public void DeleteSave() { Data = new SaveData(); if (File.Exists(SavePath)) File.Delete(SavePath); }
    public bool HasSave()   => File.Exists(SavePath);
}