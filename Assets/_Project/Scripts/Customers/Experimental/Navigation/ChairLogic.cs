using UnityEngine;

public class ChairLogic : MonoBehaviour
{
    public IK_Position IK_Position {  get; private set; }
    public bool IsTaken {  get; private set; }

    private void Awake()
    {
        IK_Position = GetComponent<IK_Position>();
    }

    public void SetIsTaken(bool value) => IsTaken = value;
    public void ClearChair() => IsTaken = false;
}
