using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomersBaseLogic : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Controller_Entity baseEntity;
    [SerializeField] private StateEntity_SO stateWaitOnLine;
    [SerializeField] private StateEntity_SO stateGoTakeFood;

    [SerializeField] private List<Entity_SO> possibleEntities;

    [SerializeField] private List<Transform> possibleStations;
    [SerializeField] private List<CustomersTable> possibleCustomerTable;

    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<Transform> roamingPoints;

    [SerializeField] private float timeCheckForSpawnCustomers = 5f;
    [SerializeField] private int maxEntityActive = 1;

    private List<Controller_Entity> entitiesOnScene = new List<Controller_Entity>();

    public List<Transform> SpawnPoints => spawnPoints;
    public List<Transform> RoamingPoints => RoamingPoints;


    private Transform playerEyes;
    private float heightPlayer = 1.8f;

    private void Awake()
    {
        if(!player) player = GameObject.FindWithTag("Player").transform;

        playerEyes = new GameObject("EyeTarget").transform;
        Vector3 newHeight = player.position;
        newHeight.y += heightPlayer;
        playerEyes.position = newHeight;

        StartCoroutine(SpawnCustomersRoutine());
    }

    public void ReturnToList(Controller_Entity ce)
    {
        ce.gameObject.SetActive(false);
    }

    private Controller_Entity GetCustomer()
    {
        Controller_Entity customer = entitiesOnScene.FirstOrDefault(c => !c.gameObject.activeInHierarchy);

        if (customer)
        {
            customer.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            customer.gameObject.SetActive(true);
            return customer;
        }
        else
        {
            Controller_Entity ce = Instantiate(baseEntity, spawnPoints[Random.Range(0, spawnPoints.Count)].position, Quaternion.identity,transform);
            entitiesOnScene.Add(ce);
            ce.SetCustomersBaseLogic(this);
            ce.SetRoamingPoint(roamingPoints);
            return ce;
        }
    }

    private IEnumerator SpawnCustomersRoutine()
    {
        while(true)
        {
            int activeNow = entitiesOnScene.Count(c => c.gameObject.activeInHierarchy);

            if (activeNow < maxEntityActive)
            {
                Controller_Entity newCustomers = GetCustomer();
                newCustomers.SetDestinationTarget(possibleStations[Random.Range(0, possibleStations.Count)]);
                newCustomers.SetTargetIK(playerEyes);
                newCustomers.SetRotateToTargetOnIdle(player);
                newCustomers.ChangeState(newCustomers.Entity.InitialState);
            }

            yield return new WaitForSeconds(timeCheckForSpawnCustomers);
        }
    }

    public ChairLogic RequestChair()
    {
        ChairLogic chair = null;
        foreach(CustomersTable customersTable in possibleCustomerTable)
        {
            chair = customersTable.FreePlace();
            if (chair) return chair;
        }

        return chair;
    }
}
