using UnityEngine;

public class TrasiitionsBase_SO : ScriptableObject
{
    public StateEntity_SO NextState;
    public float CheckInterval = -1f;

    public virtual void OnTransition(Controller_Entity ce) { }
}
