using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomersBaseLogic : MonoBehaviour
{
    [Range(0.1f,10)] public float TESTTIMEVELOCITY = 1.0f;
    private float PREVELOCITY;

    [Header("Reference")]
    [SerializeField] private Controller_Entity baseEntity;
    [SerializeField] private StateEntity_SO stateWaitOnLine;
    [SerializeField] private StateEntity_SO stateRoaming;
    [SerializeField] private StateEntity_SO stateGoTakeFood;

    [SerializeField] private List<Entity_SO> possibleEntities;

    [SerializeField] private List<CustomersTable> possibleCustomerTable;

    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<Transform> dynimicLinePoints;
    [SerializeField] private List<Transform> linePoints;
    [SerializeField] private List<Transform> roamingSerchChairPoints;
    [SerializeField] private List<Transform> roamingWaitingForLinePoints;



    [SerializeField] private float delayUniversalMoveLine = 1.75f;
    [SerializeField] private Vector2 delayRangeMoveLine = new Vector2(0.6f, 0.85f);

    [SerializeField] private float timeCheckForSpawnCustomers = 5f;
    [SerializeField] private Vector2 randomRatioSpawnNewCustomers = new Vector2 (1.5f, 4);

    [SerializeField] private int maxEntityActive = 1;

    private Transform player;
    private List<Controller_Entity> entitiesOnScene = new List<Controller_Entity>();
    private List<Controller_Entity> lineWaintig = new List<Controller_Entity>();
    private List<Controller_Entity> roamingWaiting = new List<Controller_Entity>();

    public List<Transform> SpawnPoints => spawnPoints;

    private Transform playerEyes;
    private float heightPlayer = 1.8f;

    private void Awake()
    {
        if (!player)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");

            if (playerObj)
            {
                player = playerObj.transform;

                playerEyes = new GameObject("EyeTarget").transform;
                Vector3 newHeight = player.position;
                newHeight.y += heightPlayer;
                playerEyes.position = newHeight;
            }
            else Debug.LogError("No Player On Scene");
        }

        StartCoroutine(TryCustomersRoutine());
    }

    private void Update()
    {
        if(PREVELOCITY != TESTTIMEVELOCITY)
        {
            PREVELOCITY = TESTTIMEVELOCITY;
            Time.timeScale = PREVELOCITY;
        }
    }

    public void ReturnToList(Controller_Entity ce)
    {
        ce.gameObject.SetActive(false);
    }


    private Controller_Entity GetCustomer() // Prende Cliente
    {
        Controller_Entity customer = entitiesOnScene.FirstOrDefault(c => !c.gameObject.activeInHierarchy);

        if (customer) // Se cliente gia stato spawnato lo riprende 
        {
            customer.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
            customer.gameObject.SetActive(true);
            customer.SetRoamingPoint(roamingSerchChairPoints);
            return customer;
        }
        else // Se no lo spawna e lo setta
        {
            Controller_Entity ce = Instantiate(baseEntity, spawnPoints[Random.Range(0, spawnPoints.Count)].position, Quaternion.identity,transform);
            entitiesOnScene.Add(ce);
            ce.SetCustomersBaseLogic(this);
            ce.SetRoamingPoint(roamingSerchChairPoints);
            return ce;
        }
    }

    private IEnumerator TryCustomersRoutine() //Spawna Customer a go go
    {
        while(true)
        {
            int activeNow = entitiesOnScene.Count(c => c.gameObject.activeInHierarchy);
            int howManyToSpawn = maxEntityActive - activeNow;

            if (howManyToSpawn > 0) yield return StartCoroutine(SpawingCustomerRoutine(howManyToSpawn));

            yield return new WaitForSeconds(timeCheckForSpawnCustomers);
        }
    }

    private IEnumerator SpawingCustomerRoutine(int howMany)
    {
        for (int i = 0; i < howMany; i++)
        {
            Controller_Entity newCustomers = GetCustomer();
            newCustomers.SetTargetIK(playerEyes); // Li dice di guardare il player dritto ma dritto nei occhi
            newCustomers.SetRotateToTargetOnIdle(player); // Li dice di mettere il player a disagio

            JoinLine(newCustomers); // Parte la logica del classico normale cliente

            yield return new WaitForSeconds(Random.Range(randomRatioSpawnNewCustomers.x,randomRatioSpawnNewCustomers.y));
        }
    }

    public ChairLogic RequestChair() // Cerco sedia 
    {
        ChairLogic chair = null;
        foreach(CustomersTable customersTable in possibleCustomerTable)
        {
            chair = customersTable.FreePlace();
            if (chair && !chair.IsTaken)
            {
                chair.SetIsTaken(true);
                return chair;
            }
        }

        return chair;
    }

    #region Logic Line

    public void JoinLine(Controller_Entity ce)
    {
        if(lineWaintig.Count < dynimicLinePoints.Count) // Se i punti di attesa sono di più delle persone che stanno in attesa allora si va in attessa se no ciccia
        {
            lineWaintig.Add(ce);
            UpdateCustomerLinePosition(ce);
            ce.ChangeState(stateWaitOnLine);
        }
        else
        {
            if (!roamingWaiting.Contains(ce)) roamingWaiting.Add(ce);
            ce.SetRoamingPoint(roamingWaitingForLinePoints);
            ce.ChangeState(stateRoaming);
        }    
    }

    public void LeaveLine(Controller_Entity ce) // Qui il cliente dice che è uscito dalla line solo se prende l'ordine oppure si scazza
    {
        if(lineWaintig.Contains(ce)) // Si tolgono solo se erano nella linea 
        {
            lineWaintig.Remove(ce);

            RearrangeQueue();
        }
    }

    private void RearrangeQueue()
    {
        for (int i = 0; i < lineWaintig.Count; i++) // Manda avanti la linea
        {
            StartCoroutine(MoveCustomerWithDelay(lineWaintig[i], (i * Random.Range(delayRangeMoveLine.x, delayRangeMoveLine.y)) + delayUniversalMoveLine));
        }

        if (lineWaintig.Count < dynimicLinePoints.Count && roamingWaiting.Count > 0)
        {
            Controller_Entity nextCustomer = roamingWaiting[0];
            roamingWaiting.RemoveAt(0);
            JoinLine(nextCustomer);
        }
    }

    private IEnumerator MoveCustomerWithDelay(Controller_Entity ce, float delay) // Piccolo ritardo incrementale per andare nella nuova posizione 
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        UpdateCustomerLinePosition(ce);
    }

    private void UpdateCustomerLinePosition(Controller_Entity ce)
    {
        int index = lineWaintig.IndexOf(ce);

        if (index >= 0 && index < dynimicLinePoints.Count)
        {
            Transform areaPointLine = dynimicLinePoints[index]; // Si prende la posizione del cliente nella linea 

            List<Transform> points = new List<Transform>();
            foreach (Transform child in areaPointLine) points.Add(child); // Prende eventuali figli e li rapisce

            Transform pointChoose = areaPointLine;

            if (points.Count > 0) pointChoose = points[Random.Range(0, points.Count)]; // Scelgo il punto dove bisogna andare 

            if (ce.Target != pointChoose)
            {
                ce.SetDestinationTarget(pointChoose);
                ce.AnchorPosition = pointChoose.position;
            }

            if (index == 0) StartCoroutine(WaitBeforeFood(ce));
        }
    }

    //private void UpdateCustomerLinePosition(Controller_Entity ce) // Vecchio
    //{
    //    int index = lineWaintig.IndexOf(ce); // Si prende la posizione del cliente nella linea

    //    if (index >= 0 && index < linePoints.Count)
    //    {
    //        ce.SetDestinationTarget(linePoints[index]);
    //        ce.AnchorPosition = linePoints[index].position;

    //        if (index == 0) StartCoroutine(WaitBeforeFood(ce)); // Se sei 0 è il tuo turno e fai avanzare la fila
    //    }
    //}

    private IEnumerator WaitBeforeFood(Controller_Entity ce)
    {
        yield return new WaitForSeconds(1f);
        ce.SetRoamingPoint(roamingSerchChairPoints);
        ce.ChangeState(stateGoTakeFood);
    }

    #endregion
}
