using UnityEngine;

[CreateAssetMenu(fileName = "WorkstationData", menuName = "Workstation/WorkstationData")]
public class WorkstationData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "interagire";
    [SerializeField] private string emptyHandPrompt = "Prendi qualcosa prima";
    [SerializeField] private string invalidItemPrompt = "Oggetto non valido";
    [SerializeField] private string blockedPrompt = "";

    public string Id => id;
    public string DisplayName => displayName;
    
    public string InteractionPrompt => interactionPrompt;
    public string EmptyHandPrompt => emptyHandPrompt;
    public string InvalidItemPrompt => invalidItemPrompt;
    public string BlockedPrompt => blockedPrompt;



}
