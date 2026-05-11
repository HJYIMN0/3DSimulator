using UnityEngine;

public class TrasiitionsBase_SO : ScriptableObject
{
    public StateEntity_SO NextState;
    public float CheckInterval = -1f;

    public virtual void OnEnterTransition(Controller_Entity ce) { }
    public virtual void OnTransition(Controller_Entity ce) { }
    public virtual void OnExitTransition(Controller_Entity ce) { }
}
