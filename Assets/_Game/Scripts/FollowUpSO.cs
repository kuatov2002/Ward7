using UnityEngine;

[CreateAssetMenu(menuName = "Profile7/FollowUp")]
public class FollowUpSO : ScriptableObject
{
    [TextArea(2, 5)] public string question;
    [TextArea(3, 10)] public string answer;
}
