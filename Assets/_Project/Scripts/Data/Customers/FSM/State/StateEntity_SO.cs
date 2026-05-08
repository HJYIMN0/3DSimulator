using UnityEngine;

public class StateEntity_SO : StateBase_SO
{
    public StateEntityCo_SO[] PossibleCoState;
    public TrasiitionsBase_SO[] PossibleTransitions;
    public AnimationClip clipGeneric;

    public void PlayAnimationClip(Controller_Entity ce, bool smooth, int layer) {if(clipGeneric) ce.Animator_Generic.PlaySingleAction(clipGeneric, smooth, layer); }

    public void PlayAnimationLoopClip(Controller_Entity ce, bool smooth, int layer) { if (clipGeneric) ce.Animator_Generic.PlayLoopAction(clipGeneric, smooth, layer); }

}
