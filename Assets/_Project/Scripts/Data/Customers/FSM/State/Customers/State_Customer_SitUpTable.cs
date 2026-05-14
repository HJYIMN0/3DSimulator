using UnityEngine;

[CreateAssetMenu(fileName = "State_Customer_SitUpTable", menuName = "States/Customer/SitUpTable")]
public class State_Customer_SitUpTable : StateEntity_SO
{
    [Header("Setting")]
    public float IkFadeDuration = 0.5f;
    public StateEntity_SO NextState;
    public StateEntity_SO SaveState;

    public override void Enter(Controller_Entity ce)
    {   
        IK_Position iKPosition = ce.MyChair.IK_Position;

        ce.CurrentItem.Consume();
        if (!iKPosition)
        {
            ce.ChangeState(SaveState);
            return;
        }

        ce.SetMoving(false);
        ce.Agent.enabled = false;
        ce.RbEntity.isKinematic = true;
        if (ce.Animator_Generic != null && iKPosition.AnimationExit != null) ce.Animator_Generic.PlaySingleAction(iKPosition.AnimationExit, false, 0);
    }

    public override void StateUpdate(Controller_Entity ce)
    {
        IK_Position iKPosition = ce.Target.GetComponent<IK_Position>();
        if (!iKPosition) return;

        float time = ce.TimeOnCurrentState;

        float sitProgress = time / iKPosition.AnimationExit.length;

        ce.transform.position = Vector3.Lerp(iKPosition.OnLoopPosition.transform.position, iKPosition.EnterExitPosition.transform.position, sitProgress);
        ce.transform.rotation = Quaternion.Slerp(iKPosition.OnLoopPosition.transform.rotation, iKPosition.EnterExitPosition.transform.rotation, sitProgress);

        float currentIkWeight = 1f - Mathf.Clamp01(time / IkFadeDuration);

        ce.IKAnimator.SetHandRightWeight(currentIkWeight);
        ce.IKAnimator.SetHandLeftWeight(currentIkWeight);
        ce.IKAnimator.SetFootRightWeight(currentIkWeight);
        ce.IKAnimator.SetFootLeftWeight(currentIkWeight);
        ce.IKAnimator.SetHeadWeight(currentIkWeight);

        if (time >= iKPosition.AnimationExit.length)
        {
            ce.IKAnimator.ClearAll();
            ce.ChangeState(NextState);
        }
    }

    public override void Exit(Controller_Entity ce)
    {  
        ce.ClearMyChair();
        ce.RbEntity.isKinematic = false;
        ce.Agent.enabled = true;
    }
}
