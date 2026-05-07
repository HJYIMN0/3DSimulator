using UnityEngine;

[CreateAssetMenu(fileName = "WorkstationData", menuName = "Workstation/WorkstationData")]
public class WorkstationData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Interaction")]
    [SerializeField] private string interactionPrompt;

    public string Id => id;
    public string DisplayName => displayName;
    
    public string InteractionPrompt => interactionPrompt;



}
