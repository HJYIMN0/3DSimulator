using UnityEngine;

public class IK_Position : MonoBehaviour
{
    [Header("Setting Position")]
    public GameObject EnterExitPosition;
    public GameObject OnTransitionPosition;
    public GameObject OnLoopPosition;
    public Transform LookPosition;

    [Header("IK Targets")]
    public Transform RightHandTarget;
    public Transform LeftHandTarget;
    public Transform RightFootTarget;
    public Transform LeftFootTarget;
    public Transform LookTarget;

    [Header("Animatios")]
    public AnimationClip AnimationEnter;
    public AnimationClip AnimationLoop;
    public AnimationClip AnimationExit;

    private void Start()
    {
        EnterExitPosition.gameObject.SetActive(false);
        OnTransitionPosition.gameObject.SetActive(false);
        OnLoopPosition.gameObject.SetActive(false);
    }
}