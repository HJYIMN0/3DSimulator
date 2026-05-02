using UnityEngine;

[CreateAssetMenu(fileName = "State_Agent_IKPositionEnter", menuName = "States/Agent/IKPositionEnter")]
public class State_Agent_IKPositionEnter_SO : StateEntity_SO
{
    [Header("Setting")]
    public float PreEnterRotationTime = 0.5f;
    public float IkFadeDuration = 1f;
    public StateEntity_SO NextState;

    public override void Enter(Controller_Entity ce)
    {
        IK_Position iKPosition = ce.Target.GetComponent<IK_Position>();
        if (!iKPosition)
        {
            ce.ChangeState(NextState);
            return;
        }

        ce.StopAgentDestination();
        ce.SetMoving(false);

        if (ce.Animator_Generic)
        {
            ce.Animator_Generic.SelectAnimation(true, Parameters.ParameterTriggerOnIdle);
            ce.Animator_Generic.Animator.SetFloat(Parameters.ParameterFloatSpeed, 0, 0.1f, Time.deltaTime);
        }

        ce.Agent.enabled = false;
        ce.RbEntity.isKinematic = true;

        ce.SetRotateToTargetOnIdle(iKPosition.LookPosition);
    }

    public override void StateUpdate(Controller_Entity ce)
    {
        IK_Position iKPosition = ce.Target.GetComponent<IK_Position>();
        if (!iKPosition) return;

        float time = ce.TimeOnCurrentState;

        // Preparazione
        if (time < PreEnterRotationTime)
        {
            ce.transform.position = Vector3.Slerp(ce.transform.position, iKPosition.EnterExitPosition.transform.position, Time.deltaTime * 2);
            ce.IdleRotateTowardsTarget();
        }


        // Avanzamento
        else if (time >= PreEnterRotationTime && time < (PreEnterRotationTime + iKPosition.AnimationEnter.length))
        {
            float sitTime = time - PreEnterRotationTime;

            if (sitTime <= Time.deltaTime)
            {
                if (ce.Animator_Generic != null && iKPosition.AnimationEnter != null) ce.Animator_Generic.PlaySingleAction(iKPosition.AnimationEnter, true, 0);
            }

            float sitProgress = sitTime / iKPosition.AnimationEnter.length;

            ce.transform.position = Vector3.Lerp(iKPosition.EnterExitPosition.transform.position, iKPosition.OnTransitionPosition.transform.position, sitProgress);
            ce.transform.rotation = Quaternion.Slerp(iKPosition.EnterExitPosition.transform.rotation, iKPosition.OnTransitionPosition.transform.rotation, sitProgress);
        }

        // Aggiustamento
        else if (time >= (PreEnterRotationTime + iKPosition.AnimationEnter.length))
        {
            float loopTime = time - (PreEnterRotationTime + iKPosition.AnimationEnter.length);

            if (loopTime <= Time.deltaTime)
            {
                ce.transform.position = iKPosition.OnTransitionPosition.transform.position;
                ce.transform.rotation = iKPosition.OnTransitionPosition.transform.rotation;

                if (ce.Animator_Generic != null && iKPosition.AnimationLoop != null) ce.Animator_Generic.PlaySingleAction(iKPosition.AnimationLoop, true, 0);

                ce.IKAnimator.SetRightHandTarget(iKPosition.RightHandTarget);
                ce.IKAnimator.SetLeftHandTarget(iKPosition.LeftHandTarget);
                ce.IKAnimator.SetRightFootTarget(iKPosition.RightFootTarget);
                ce.IKAnimator.SetLeftFootTarget(iKPosition.LeftFootTarget);

                ce.IKAnimator.SetTarget(iKPosition.LookTarget);
                ce.IKAnimator.SetLooking(true);
            }

            float currentIkWeight = Mathf.Clamp01(loopTime / IkFadeDuration);

            ce.IKAnimator.SetHandRightWeight(currentIkWeight);
            ce.IKAnimator.SetHandLeftWeight(currentIkWeight);
            ce.IKAnimator.SetFootRightWeight(currentIkWeight);
            ce.IKAnimator.SetFootLeftWeight(currentIkWeight);

            ce.IKAnimator.SetHeadWeight(currentIkWeight * 0.8f);

            ce.transform.position = Vector3.Lerp(ce.transform.position, iKPosition.OnLoopPosition.transform.position, Time.deltaTime * 2.5f);
            ce.transform.rotation = Quaternion.Slerp(ce.transform.rotation, iKPosition.OnLoopPosition.transform.rotation, Time.deltaTime * 2.5f);

            if (currentIkWeight >= 1f) ce.ChangeState(NextState);
        }
    }

    public override void Exit(Controller_Entity ce)
    {

    }
}
