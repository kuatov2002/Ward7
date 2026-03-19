using UnityEngine;

[CreateAssetMenu(menuName = "Profile7/Evidence")]
public class EvidenceSO : ScriptableObject
{
    public string evidenceId;
    public string title;
    [TextArea(3, 10)] public string baseDescription;
    [TextArea(3, 10)] public string expertDescription;
}
