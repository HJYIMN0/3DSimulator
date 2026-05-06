using UnityEngine;

[CreateAssetMenu(fileName = "Entity")]
public class Entity_SO : ScriptableObject
{
    [Header("Entity Identity")]
    public string EntityName = "Nome Generico n2";
    public float Height = 1.8f;
    public GameObject PrefabEntity;
    public string EntityDescription;

    [Space(2)]
    [Header("Entity State")]
    public StateEntity_SO InitialState;
    [Space(1)]
    public StateEntity_SO DeathState;
    public StateEntity_SO RagdollState;

    [Space(2)]
    [Header("Entity Impact")]
    public float ImpactThresholdXZ = 25f;
    public float ImpactThresholdY = 10f;

    [Space(2)]
    [Header("Movement Ground")]
    public float WalkSpeed = 10f;
    public float RunSpeed = 20f;
    public float GroundDrag = 5f;
    public float SlopeLimit = 35f;

    [Space(2)]
    [Header("Movement Air & AdvanceDialogue")]
    public float WalkAirSpeed = 1.5f;
    public float RunAirSpeed = 2.5f;
    public float AirDrag = 0f;

    public float JumpForce = 7f;
}
