using UnityEngine;

public class IKAnimator : MonoBehaviour
{
    [Header("Setting Look IK")]
    [SerializeField] Transform lookTarget;
    [SerializeField, Range(0, 1)] private float totalWeight = 1f;
    [SerializeField, Range(0, 1)] private float headWeight = 0.75f;
    [SerializeField, Range(0, 1)] private float bodyWeight = 0.25f;

    [Header("Hands IK Settings")]
    [SerializeField] private Transform rightHandTarget;
    [SerializeField, Range(0, 1)] private float rightHandWeight = 1f;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField, Range(0, 1)] private float leftHandWeight = 1f;

    [Header("Feet IK Settings")]
    [SerializeField] private Transform rightFootTarget;
    [SerializeField, Range(0, 1)] private float rightFootWeight = 1f;
    [SerializeField] private Transform leftFootTarget;
    [SerializeField, Range(0, 1)] private float leftFootWeight = 1f;

    [Header("Distance Limits")]
    [SerializeField] private float maxDistanceBody = 5f;
    [SerializeField] private float maxDistanceHead = 10f;

    [SerializeField] private float maxDistanceHands= 10f;
    [SerializeField] private float maxDistanceFeet = 10f;

    [Header("Angle & Smoothing")]
    [SerializeField] private float maxAngle = 90f;
    [SerializeField] private float smoothSpeed = 2f;

    private Animator animator;
    private bool isLooking = true;

    private float currentSmoothWeight = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetTarget(Transform target) => this.lookTarget = target;
    public void SetHeadWeight(float weight) => this.headWeight = weight;
    public void SetBodyWeight(float weight) => this.bodyWeight = weight;
    public void SetLooking(bool isLooking) => this.isLooking = isLooking;
    public void ClearLookTarget() => lookTarget = null;


    public void SetRightHandTarget(Transform target) => this.rightHandTarget = target;
    public void SetLeftHandTarget(Transform target) => this.leftHandTarget = target;
    public void SetHandRightWeight(float weight) => this.rightHandWeight = weight;
    public void SetHandLeftWeight(float weight) => this.leftHandWeight = weight;
    public void ClearRightHandTarget() => rightHandTarget = null;
    public void ClearHandLeftTarget() => leftHandTarget = null;

    public void ClearAllHands()
    {
        rightHandTarget = null;
        leftHandTarget = null;
    }


    public void SetRightFootTarget(Transform target) => this.rightFootTarget = target;
    public void SetLeftFootTarget(Transform target) => this.leftFootTarget = target;
    public void SetFootRightWeight(float weight) => this.rightFootWeight = weight;
    public void SetFootLeftWeight(float weight) => this.leftFootWeight = weight;
    public void ClearFootRightTarget() => rightFootTarget = null;
    public void ClearLeftFootTarget() => leftFootTarget = null;

    public void ClearAllFoots()
    {
        rightFootTarget = null;
        leftFootTarget = null;
    }

    public void ClearAllLimbs()
    {
        ClearAllHands();
        ClearAllFoots();
    }

    public void ClearAll()
    {
        ClearLookTarget();
        ClearAllLimbs();
    }


    private void OnAnimatorIK(int layerIndex)
    {
        LookAtIk();
        HandsIk();
        FeetIk();
    }

    private void LookAtIk()
    {
        float targetOverallWeight = 0f;

        if (isLooking && lookTarget && totalWeight > 0)
        {
            float distanceToTarget = Vector3.Distance(transform.position, lookTarget.position);

            Vector3 dirToTarget = lookTarget.position - transform.position;
            dirToTarget.y = 0;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (distanceToTarget <= maxDistanceHead && angle <= maxAngle) targetOverallWeight = totalWeight;
        }

        currentSmoothWeight = Mathf.Lerp(currentSmoothWeight, targetOverallWeight, Time.deltaTime * smoothSpeed);
        if (currentSmoothWeight <= 0.1f)
        {
            animator.SetLookAtWeight(0);
            return;
        }

        float currentDistance = GetDistanceToTarget(lookTarget);
        animator.SetLookAtWeight(currentSmoothWeight, GetWeight(maxDistanceBody, currentDistance, bodyWeight), GetWeight(maxDistanceHands, currentDistance, headWeight));
        animator.SetLookAtPosition(lookTarget.position);
    }

    private void HandsIk()
    {
        ApplyLimbIK(rightHandTarget, AvatarIKGoal.RightHand, rightHandWeight, maxDistanceHands);
        ApplyLimbIK(leftHandTarget, AvatarIKGoal.LeftHand, leftHandWeight, maxDistanceHands);
    }

    private void FeetIk()
    {
        ApplyLimbIK(rightFootTarget, AvatarIKGoal.RightFoot, rightFootWeight, maxDistanceFeet);
        ApplyLimbIK(leftFootTarget, AvatarIKGoal.LeftFoot, leftFootWeight, maxDistanceFeet);
    }

    private void ApplyLimbIK(Transform targetLimb,AvatarIKGoal limbGoal, float weight,float maxDistance)
    {
        if (targetLimb)
        {
            animator.SetIKPositionWeight(limbGoal, GetWeight(maxDistance, GetDistanceToTarget(targetLimb),weight));
            animator.SetIKRotationWeight(limbGoal, GetWeight(maxDistance, GetDistanceToTarget(targetLimb),weight));
            animator.SetIKPosition(limbGoal, targetLimb.position);
            animator.SetIKRotation(limbGoal, targetLimb.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(limbGoal, 0);
            animator.SetIKRotationWeight(limbGoal, 0);
        }
    }

    private float GetDistanceToTarget(Transform target) => Vector3.Distance(transform.position, target.position);
    private float GetWeight(float maxDistance,float currentDistance,float normalWeight)
    {
        float Multiplier = 1f - Mathf.Clamp01(currentDistance / maxDistance);
        return normalWeight * Multiplier;
    }
}