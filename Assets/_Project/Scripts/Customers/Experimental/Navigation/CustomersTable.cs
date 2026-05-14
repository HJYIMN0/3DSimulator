using System.Collections.Generic;
using UnityEngine;

public class CustomersTable : MonoBehaviour
{
    private List<ChairLogic> chairList = new List<ChairLogic>();

    private void Awake()
    {
        foreach(Transform t in transform) if (t.TryGetComponent(out ChairLogic chair)) chairList.Add(chair);
    }

    public ChairLogic FreePlace()
    {
        foreach (ChairLogic chair in chairList)
        {
            if (!chair.IsTaken) return chair;
        }
        return null; 
    }
}
