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
    public ContactSO[] contacts;

    [Header("Tuesday - Evidence")]
    public EvidenceSO[] evidence;

    [Header("Wednesday - Testimonies")]
    public TestimonySO[] testimonies;

    [Header("Thursday - Interrogation")]
    public InterrogationQA[] standardQuestions;
    public ConditionalInterrogationQA[] conditionalQuestions;
    public FollowUpSO[] followUps;

    [Header("Consequences")]
    [TextArea(3, 10)] public string consequenceGuilty;
    [TextArea(3, 10)] public string consequenceNotGuilty;
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
