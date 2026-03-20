using UnityEngine;

[CreateAssetMenu(menuName = "Profile7/Case")]
public class CaseSO : ScriptableObject
{
    public string caseId;
    public string displayName;
    public int caseNumber;
    public int totalMoves = 8;

    [Tooltip("Person ID of the true culprit")]
    public string trueCulpritId;

    [Header("Persons (suspects & witnesses)")]
    public CasePersonData[] persons;

    [Header("Interrogations")]
    public CaseInterrogationData[] interrogations;

    [Header("Locations to inspect")]
    public LocationData[] locations;

    [Header("Database queries")]
    public DatabaseQueryData[] databaseQueries;

    [Header("Confrontations")]
    public ConfrontationData[] confrontations;

    [Header("Deduction fragments")]
    public DeductionFragmentData[] fragments;

    [Header("Correct deduction chain")]
    public string correctMotiveId;
    public string correctOpportunityId;
    public string correctEvidenceId;
    public string correctSuspectId;

    [Header("Case briefing")]
    [TextArea(5, 20)] public string briefingText;

    [Header("Consequences")]
    public CaseConsequenceData consequenceCorrectArrest;
    public CaseConsequenceData consequenceWrongArrest;
    public CaseConsequenceData consequenceUnsolved;
    public CaseConsequenceData consequenceWeakCase;
}

// ─── Enums ───

public enum PersonRole { Suspect, Witness }
public enum HiddenAgenda { Honest, Covering, Deflecting, SelfPreserving }
public enum FragmentType { Motive, Opportunity, Evidence, Suspect }
public enum ActionType { Interrogation, LocationInspect, DatabaseQuery, Confrontation }
public enum CaseResult { CorrectArrest, WrongArrest, Unsolved, WeakCase }

// ─── Data classes ───

[System.Serializable]
public class CasePersonData
{
    public string personId;
    public string displayName;
    public PersonRole role;
    [Tooltip("Hidden from player — determines NPC behavior")]
    public HiddenAgenda hiddenAgenda;
    [TextArea(2, 5)] public string description;
}

[System.Serializable]
public class CaseInterrogationData
{
    public string targetPersonId;
    public InterrogationQuestionData[] questions;
}

[System.Serializable]
public class InterrogationQuestionData
{
    [TextArea(1, 3)] public string questionText;
    [TextArea(2, 10)] public string answerText;
    public bool isLie;
    [TextArea(2, 5)] public string truthText;
    [Tooltip("Fragment revealed when this question is asked")]
    public string revealedFragmentId;
}

[System.Serializable]
public class LocationData
{
    public string locationId;
    public string displayName;
    [TextArea(2, 5)] public string description;
    public LocationZoneData[] zones;
}

[System.Serializable]
public class LocationZoneData
{
    public string zoneName;
    [TextArea(2, 5)] public string description;
    public string revealedFragmentId;
}

[System.Serializable]
public class DatabaseQueryData
{
    public string queryId;
    public string displayName;
    [TextArea(2, 10)] public string resultText;
    public string revealedFragmentId;
}

[System.Serializable]
public class ConfrontationData
{
    public string personA;
    public string personB;
    [TextArea(3, 10)] public string resultText;
    [Tooltip("Person ID of who breaks under pressure")]
    public string whoBreaks;
    public string revealedFragmentId;
}

[System.Serializable]
public class DeductionFragmentData
{
    public string fragmentId;
    [TextArea(1, 3)] public string displayText;
    public FragmentType fragmentType;
    [Tooltip("Is this fragment part of the correct chain?")]
    public bool isTrue;
    public string relatedPersonId;
}

[System.Serializable]
public class CaseConsequenceData
{
    [TextArea(3, 10)] public string headlineText;
    [TextArea(2, 5)] public string detailText;
}
