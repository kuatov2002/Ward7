using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SuspectSO")]
public class SuspectSO : ScriptableObject
{
    public string displayName;
    public bool isGulity;
    public string profileText;
    public EvidenceSO[] evidences;
}
