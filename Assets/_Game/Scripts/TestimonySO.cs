using UnityEngine;

[CreateAssetMenu(menuName = "Profile7/Testimony")]
public class TestimonySO : ScriptableObject
{
    public string witnessName;
    [TextArea(3, 10)] public string baseTestimony;
    [TextArea(3, 10)] public string clarification;
}
