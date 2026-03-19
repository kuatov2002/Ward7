using UnityEngine;

[CreateAssetMenu(menuName = "Profile7/Contact")]
public class ContactSO : ScriptableObject
{
    public string contactId;
    public string displayName;
    [TextArea(3, 10)] public string response;
}
