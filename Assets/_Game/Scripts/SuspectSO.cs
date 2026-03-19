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
}
