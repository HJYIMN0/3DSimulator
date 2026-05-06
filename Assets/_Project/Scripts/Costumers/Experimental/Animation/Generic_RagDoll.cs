using System.Collections.Generic;
using UnityEngine;

public class BoneTransform
{
    public Vector3 Position { get; set; }

    public Quaternion Rotation { get; set; }
}

public class Generic_RagDoll : MonoBehaviour
{
    private Controller_Entity ce;

    private List<Rigidbody> ragdollRbs = new List<Rigidbody>();
    private List<Collider> ragdollCollider = new List<Collider>();

    private Transform hipsBone;

    private Transform[] bones;
    private BoneTransform[] standUpBoneTransform;
    private BoneTransform[] standBackBoneTransform;

    private BoneTransform[] ragdollBoneTransform;
    private bool isFacingUp;

    public bool IsStandingUp { get; set; }
    public float TimerForWakeUp { get; set; }
    public float ElepsedResetBones { get; set; }
    public float TimerForTryToStandUp { get; set; }

    public Transform HipsBone => hipsBone;
    public bool IsFacingUp => isFacingUp;

    public void SetUp(Controller_Entity controllerEntity)
    {
        ce = controllerEntity;

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            if (rb == ce.RbEntity) continue;
            ragdollRbs.Add(rb);
            ragdollCollider.Add(rb.GetComponent<Collider>());
        }

        hipsBone = ce.Animator_Generic.Animator.GetBoneTransform(HumanBodyBones.Hips);

        bones = hipsBone.GetComponentsInChildren<Transform>();
        standUpBoneTransform = new BoneTransform[bones.Length];
        standBackBoneTransform = new BoneTransform[bones.Length];
        ragdollBoneTransform = new BoneTransform[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            standUpBoneTransform[i] = new BoneTransform();
            standBackBoneTransform[i] = new BoneTransform();
            ragdollBoneTransform[i] = new BoneTransform();
        }

        PopulationAnimationStartBoneTransform(Parameters.ParameterTriggerStandUpFront, standUpBoneTransform);
        PopulationAnimationStartBoneTransform(Parameters.ParameterTriggerStandUpBack, standBackBoneTransform);

        DisableRagdoll();
    }

    #region Ragdoll 
    public virtual void TriggerRagdoll(Vector3 direction, Vector3 hitPoint)
    {
        EnableRagdoll();

        Rigidbody _hitRb = FindClosest<Rigidbody>(hitPoint);
        _hitRb.AddForceAtPosition(direction * direction.magnitude, hitPoint, ForceMode.Impulse);
    }

    public static T FindClosest<T>(Vector3 point) where T : Component
    {
        T[] components = UnityEngine.Object.FindObjectsByType<T>();

        T closest = null;
        float closestSqrDist = float.MaxValue;

        foreach (T comp in components)
        {
            float sqrDist = (comp.transform.position - point).sqrMagnitude;

            if (sqrDist < closestSqrDist || closest == null)
            {
                closestSqrDist = sqrDist;
                closest = comp;
            }
        }

        return closest;
    }

    public virtual void EnableRagdoll()
    {
        ce.RbEntity.isKinematic = true;
        ce.RbEntity.GetComponent<Collider>().enabled = false;

        foreach (Rigidbody rb in ragdollRbs) rb.isKinematic = false;
        foreach (Collider collider in ragdollCollider) collider.enabled = true;

        ce.Animator_Generic.Animator.enabled = false;
    }

    public virtual void DisableRagdoll()
    {
        ce.RbEntity.isKinematic = false;
        ce.RbEntity.GetComponent<Collider>().enabled = true;

        foreach (Rigidbody rb in ragdollRbs) rb.isKinematic = true;
        foreach (Collider collider in ragdollCollider) collider.enabled = false;

        ce.Animator_Generic.Animator.enabled = true;
    }

    public virtual void DisableCollisionRagdoll()
    {
        ce.RbEntity.isKinematic = false;
        ce.RbEntity.GetComponent<Collider>().enabled = true;

        foreach (Rigidbody rb in ragdollRbs) rb.isKinematic = true;
        foreach (Collider collider in ragdollCollider) collider.enabled = false;
    }

    #endregion

    #region AllingPositionToHips
    public void AllingPositionToHips()
    {
        isFacingUp = hipsBone.forward.y > 0;
        AllingRotationToHips();

        Vector3 originalHipsPosition = hipsBone.position;
        transform.position = originalHipsPosition;

        Vector3 positionOffeset = GetStandUpBoneTransform()[0].Position;
        positionOffeset.y = 0f;
        positionOffeset = transform.rotation * positionOffeset;
        transform.position -= positionOffeset;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, hitInfo.point.z);
        }

        hipsBone.position = originalHipsPosition;

        PopulationBoneTransform(ragdollBoneTransform);
    }

    private void AllingRotationToHips()
    {
        Vector3 originalHipsPosition = hipsBone.position;
        Quaternion originalRotation = hipsBone.rotation;

        Vector3 desiredDirection = isFacingUp ? hipsBone.up * -1f : hipsBone.up;
        desiredDirection.y = 0;
        desiredDirection.Normalize();

        Quaternion fromToRotation = Quaternion.FromToRotation(transform.forward, desiredDirection);
        transform.rotation *= fromToRotation;

        hipsBone.position = originalHipsPosition;
        hipsBone.rotation = originalRotation;
    }

    #endregion

    #region PopulationBone
    private void PopulationBoneTransform(BoneTransform[] boneTransform)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            boneTransform[i].Position = bones[i].localPosition;
            boneTransform[i].Rotation = bones[i].localRotation;
        }
    }

    private void PopulationAnimationStartBoneTransform(string clipName, BoneTransform[] boneTransform)
    {
        Animator anim = ce.Animator_Generic.Animator;
        Vector3 positionBeforeSampling = transform.position;
        Quaternion rotationBeforeSampling = transform.rotation;

        Vector3 childLocalPosBefore = anim.transform.localPosition;
        Quaternion childLocalRotBefore = anim.transform.localRotation;

        foreach (AnimationClip clip in ce.Animator_Generic.Animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                clip.SampleAnimation(anim.gameObject, 0f);
                PopulationBoneTransform(boneTransform);
                break;
            }
        }

        transform.position = positionBeforeSampling;
        transform.rotation = rotationBeforeSampling;

        anim.transform.localPosition = childLocalPosBefore;
        anim.transform.localRotation = childLocalRotBefore;
    }


    public void GetUp()
    {
        if (isFacingUp) ce.Animator_Generic.SelectAnimation(false, Parameters.ParameterTriggerStandUpFront);
        else ce.Animator_Generic.SelectAnimation(false, Parameters.ParameterTriggerStandUpBack);
    }

    public BoneTransform[] GetStandUpBoneTransform() => isFacingUp ? standUpBoneTransform : standBackBoneTransform;

    public void ResetBones(float percent)
    {
        BoneTransform[] getUpBoneTransform = GetStandUpBoneTransform();
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == hipsBone)
            {
                bones[i].localPosition = Vector3.Lerp(ragdollBoneTransform[i].Position, getUpBoneTransform[i].Position, percent);
            }
            bones[i].localRotation = Quaternion.Lerp(ragdollBoneTransform[i].Rotation, getUpBoneTransform[i].Rotation, percent);
        }
    }

    #endregion
}
