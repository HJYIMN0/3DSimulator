using UnityEngine;

public class StateBase_SO : ScriptableObject
{
    public virtual void Enter(Controller_Entity ce) { }
    public virtual void StateUpdate(Controller_Entity ce) { }
    public virtual void StateFixedUpdate(Controller_Entity ce) { }
    public virtual void Exit(Controller_Entity ce) { }
}
