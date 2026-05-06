using System.Collections;
using UnityEngine;

public class Animator_Generic : MonoBehaviour
{
    [Header("Setting Time For Animation")]
    [SerializeField] protected float timeToRestorPosRot = 0.25f;
    [SerializeField] protected float transitionDuration = 0.15f;
    [SerializeField] protected float smoothAnimation = 0.1f;

    [Header("Debug")]
    [SerializeField] private AnimationClip quickClip;
    [SerializeField] private int layerClip;

    protected Animator animator;
    protected FeedBack_Entity feedback;
    protected bool useFirstGenericSlot = true;

    public Animator Animator => animator;

    public virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        feedback = GetComponentInParent<FeedBack_Entity>();

        if (quickClip) PlaySingleAction(quickClip, false, layerClip);
    }

    #region AnimationMoving
    public virtual void AnimationMoving(float currentSpeed, float maxSpeed, Rigidbody rb)
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

        float vertical = localVelocity.z;
        float horizontal = localVelocity.x;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);

        vertical = Mathf.Clamp(vertical, -1f, 1f);
        float speedVertical = normalizedSpeed * vertical;
        animator.SetFloat(Parameters.ParameterFloatSpeed, speedVertical, smoothAnimation, Time.deltaTime);

        horizontal = Mathf.Clamp(horizontal, -1f, 1f);
        float speedHorizontal = normalizedSpeed * horizontal;
        animator.SetFloat(Parameters.ParameterFloatDirection, speedHorizontal, smoothAnimation, Time.deltaTime);
    }

    private float lastFootstepTime;
    public void PlayFootSound()
    {
        if (Time.time - lastFootstepTime < 0.25f) return;
        lastFootstepTime = Time.time;
        feedback.PlayFootstepSound();
    }
    #endregion

    #region Logic OverRideAnimation
    public virtual void OnAllOverRideAnimation(AnimatorOverrideController animOver)
    {
        animator.runtimeAnimatorController = animOver;
        SelectAnimation(false, Parameters.ParameterTriggerOnGenericAction);
    }

    public virtual void PlaySingleAction(AnimationClip actionClipToPlay, bool isSmooth = true,int layer = 0)
    {
        if (!actionClipToPlay) return;

        AnimatorOverrideController currentOverride = animator.runtimeAnimatorController as AnimatorOverrideController;

        if (currentOverride == null)
        {
            currentOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = currentOverride;
        }

        string targetStateName;

        if (useFirstGenericSlot)
        {
            currentOverride[Parameters.ParameterClipName1] = actionClipToPlay;
            targetStateName = "GenericAction";
        }
        else
        {
            currentOverride[Parameters.ParameterClipName2] = actionClipToPlay;
            targetStateName = "GenericAction2"; 
        }

        useFirstGenericSlot = !useFirstGenericSlot;
        SelectAnimation(isSmooth, targetStateName, layer);

    }

    #endregion


    #region Setting General

    [ContextMenu("QuickSingleAnimation")]
    public void QuickSingleAnimation() => PlaySingleAction(quickClip, false, layerClip);

    public virtual void SelectAnimation(bool isSmooth, string nameAnimation, int layer = 0, float startAnimation = 0f)
    {
        if (isSmooth) animator.CrossFadeInFixedTime(nameAnimation, transitionDuration, layer, startAnimation);
        else animator.Play(nameAnimation, layer, startAnimation);
    }

    public virtual void ResetAnimatios(bool layer2 = true, bool layer1 = true, bool layer0 = false)
    {
        if (layer2) SelectAnimation(false, "HumanF@Idle01", 2);
        if (layer1) SelectAnimation(false, "HumanF@Idle01", 1);
        if (layer0) SelectAnimation(false, "HumanF@Idle01", 0);
    }
    #endregion
}
