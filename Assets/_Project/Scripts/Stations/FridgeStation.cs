using UnityEngine;

public class FridgeStation : Workstation
{
    [SerializeField] private MeatData meatToSpawn;
    [SerializeField] private Transform spawnPoint;

    protected override void HandleInteraction(Interactor interactor)
    {
        if (interactor.CarryController == null)
            return;

        if (interactor.CarryController.HasItem)
        {
            Debug.Log("Cannot pick up meat, already carrying something.");
            return;
        }

        if (meatToSpawn == null)
        {
            Debug.LogError("Meat prefab is not assigned in the inspector.");
            return;
        }

        if(meatToSpawn.Prefab == null)
        {
            Debug.LogError("Meat prefab reference is missing in the MeatData.");
            return;
        }

        CarryableItem newMeat = Instantiate(meatToSpawn.Prefab, spawnPoint != null ? spawnPoint.position : transform.position + transform.forward,
            Quaternion.identity
        );

        interactor.CarryController.TryPickUp(newMeat);
    }

}
