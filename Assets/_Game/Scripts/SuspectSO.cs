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

    [Header("Thursday - Interrogation")]
    public InterrogationQA[] standardQuestions;
    public ConditionalInterrogationQA[] conditionalQuestions;
    public FollowUpData[] followUps;

    [Header("Consequences")]
    [TextArea(3, 10)] public string consequenceGuilty;
    [TextArea(3, 10)] public string consequenceNotGuilty;
}

// ─── Inline data classes ───

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
}

[System.Serializable]
public class ConditionalInterrogationQA
{
    public ChoiceType requiredChoiceType;
    public string requiredChoiceId;
    [TextArea(1, 3)] public string question;
    [TextArea(2, 10)] public string answer;
}
