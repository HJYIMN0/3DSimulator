using UnityEngine;

[CreateAssetMenu(fileName = "Transition_Agent_Wait", menuName = "Transition/Agent/Wait")]

public class Transition_Agent_Wait : TrasiitionsBase_SO
{
    public bool ClearWaitOnExit = true;
    public Vector2 TimeWaitRange = new Vector2(5f, 5f);

    public override void OnTransition(Controller_Entity ce) 
    {
        ce.TimeWait++;
        float t = Random.Range(TimeWaitRange.x,TimeWaitRange.y);

        if (ce.TimeWait >= t)
        {
            ce.TimeWait = 0;
            ce.ChangeState(NextState);
        }
    }

    public override void OnExitTransition(Controller_Entity ce)
    {
        if (ClearWaitOnExit) ce.TimeWait = 0;
    }
}
