using UnityEngine;

[CreateAssetMenu(menuName = "Profile7/Suspect")]
public class SuspectSO : ScriptableObject
{
    public string suspectId;
    public string displayName;
    public int weekNumber;
    public bool isGuilty;

    [Header("Monday - Dossier")]
    [TextArea(5, 20)] public string dossierText;
    public ContactData[] contacts;

    [Header("Tuesday - Evidence")]
    public EvidenceData[] evidence;

    [Header("Wednesday - Testimonies")]
    public TestimonyData[] testimonies;
    public ContradictionData[] contradictions;

    [Header("Thursday - Interrogation")]
    public InterrogationQA[] standardQuestions;
    public ConditionalInterrogationQA[] conditionalQuestions;
    public BluffQuestionData[] bluffQuestions;
    public FollowUpData[] followUps;
    [Tooltip("Pressure at which suspect shuts down (0 = no pressure system)")]
    public int pressureThreshold = 5;

    [Header("Timeline — Find Contradictions")]
    public TimelineEntryData[] timelineEntries;
    public TimelineContradictionData[] timelineContradictions;
    public int maxContradictionAttempts = 5;

    [Header("Connection Board")]
    public ConnectionCardData[] connectionCards;
    public ConnectionData[] connections;
    public int maxConnectionAttempts = 8;

    [Header("Friday - Verdict Justification")]
    public ArgumentData[] arguments;

    [Header("Consequences")]
    [TextArea(3, 10)] public string consequenceGuilty;
    [TextArea(3, 10)] public string consequenceNotGuilty;
}

// ─── Existing data classes ───

[System.Serializable]
public class ContactData
{
    public string contactId;
    public string displayName;
    [TextArea(3, 10)] public string response;
}

[System.Serializable]
public class EvidenceData
{
    public string evidenceId;
    public string title;
    [TextArea(3, 10)] public string baseDescription;
    [TextArea(3, 10)] public string expertDescription;
}

[System.Serializable]
public class TestimonyData
{
    public string witnessName;
    [TextArea(3, 10)] public string baseTestimony;
    [TextArea(3, 10)] public string clarification;
}

[System.Serializable]
public class FollowUpData
{
    public string followUpId;
    [TextArea(2, 5)] public string question;
    [TextArea(3, 10)] public string answer;
}

[System.Serializable]
public class InterrogationQA
{
    [TextArea(1, 3)] public string question;
    [TextArea(2, 10)] public string answer;
    [Tooltip("+1/+2 aggressive, -1 soft, 0 neutral")]
    public int pressureChange;
}

[System.Serializable]
public class ConditionalInterrogationQA
{
    public ChoiceType requiredChoiceType;
    public string requiredChoiceId;
    [TextArea(1, 3)] public string question;
    [TextArea(2, 10)] public string answer;
    public int pressureChange;
}

// ─── New data classes ───

[System.Serializable]
public class BluffQuestionData
{
    [TextArea(1, 3)] public string question;
    [TextArea(2, 10)] public string answerSuccess;
    [TextArea(2, 10)] public string answerFail;
    public ChoiceType requiredChoiceType;
    public string requiredChoiceId;
    public int pressureChange = 2;
}

[System.Serializable]
public class ContradictionData
{
    public string witnessA;
    public string witnessB;
    [TextArea(2, 5)] public string description;
}

[System.Serializable]
public class ArgumentData
{
    public string argumentId;
    [TextArea(1, 3)] public string text;
    public bool supportsGuilty;
    [Range(1, 3)] public int weight = 1;
    public bool alwaysVisible;
    public ChoiceType requiredChoiceType;
    public string requiredChoiceId;
}

/// <summary>
/// A single entry on the pre-built timeline the player reads.
/// </summary>
[System.Serializable]
public class TimelineEntryData
{
    public string entryId;
    public string time;         // display string: "23:05"
    public string description;  // "Салас вышел со склада"
    public string source;       // "Журнал пропусков" — shown as tag
    public bool alwaysVisible;
    public ChoiceType requiredChoiceType;
    public string requiredChoiceId;
}

/// <summary>
/// A contradiction the player must find — two timeline entries that don't add up.
/// </summary>
[System.Serializable]
public class TimelineContradictionData
{
    public string entryA;       // entryId of first event
    public string entryB;       // entryId of second event
    [TextArea(1, 3)] public string explanation; // why these contradict
}

[System.Serializable]
public class ConnectionCardData
{
    public string cardId;
    public string label;
    public CardType type;
    public enum CardType { Person, Item, Event }
    [Tooltip("Discovery gate: ChoiceType required. Ignored if alwaysVisible.")]
    public ChoiceType requiredChoiceType;
    public string requiredChoiceId;
    [Tooltip("If true, card is always visible (no gate)")]
    public bool alwaysVisible;
}

[System.Serializable]
public class ConnectionData
{
    public string cardA;
    public string cardB;
    [TextArea(1, 3)] public string revealText;
}
