using UnityEngine;

[CreateAssetMenu(fileName = "Transition_Customer_TakeOrder", menuName = "Transition/Customer/TakeOrder")]
public class Transition_Customer_TakeOrder : TrasiitionsBase_SO
{
    public AnimationClip takeAnimation;
    public float SphereRadius = 2f;

    public override void OnTransition(Controller_Entity ce)
    {
        Collider[] hitColliders = Physics.OverlapSphere(ce.Target.position, SphereRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out CarryableItem item))
            {
                foreach (MeatData data in ce.Entity.PossibleRequiredMeat)
                {
                    if (data == item.MeatData)
                    {
                        item.PickUp(ce.Animator_Generic.Animator.GetBoneTransform(HumanBodyBones.RightHand));
                        item.transform.localPosition = item.PositionOnNpcHand;
                        ce.SetCurrentItem(item);
                        ce.Animator_Generic.PlayLoopAction(takeAnimation, true, 1);
                        ce.CustomersBaseLogic.LeaveLine(ce);
                        ce.ChangeState(NextState);
                        return;
                    }
                }
            }
        }
    }
}
