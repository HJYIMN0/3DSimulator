using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Controller_Entity : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] protected Entity_SO entity;
    [SerializeField] protected StateEntity_SO currentState;
    public float TimeOnCurrentState;

    [Space(2)]
    [Header("Animation OverRide")]
    public AnimatorOverrideController OverrideAnimationAll;
    [Space(2)]
    [Header("Agent Settings")]
    [SerializeField] private Transform[] pointsRoaming;
    [SerializeField] private Transform target;
    [SerializeField] private Transform targetIK;
    [SerializeField] private Transform rotateToTargetOnIdle;
    [SerializeField] private Transform rotateToTargetOnMoving;

    [SerializeField] protected float avoidancePriority = 1.0f;
    [SerializeField] protected bool isRandomizeAvoidancePriority = true;

    [Space(2)]
    [Header("Agent Settings Rotations")]
    [SerializeField] protected float rotationSpeedOnMoving = 3.5f;
    [SerializeField] protected float rotationSpeedOnIdle = 2f;

    protected Vector3 lastPositionTarget;
    protected float targetSpeed;
    protected Coroutine coroutineSetAgentDestination;
    protected Dictionary<TrasiitionsBase_SO, float> transitionTimers = new Dictionary<TrasiitionsBase_SO, float>();

    public Entity_SO Entity => entity;


    // Components
    public Rigidbody RbEntity { get; private set; }
    public Animator_Generic Animator_Generic { get; private set; }
    public IKAnimator IKAnimator { get; private set; }
    public FeedBack_Entity Feedback { get; private set; }
    public Generic_RagDoll Generic_RagDoll { get; private set; }
    public GroundDetector GroundDetector { get; private set; }

    public GameObject Model { get; private set; }

    public Transform MainCamera { get; private set; }
    public Transform HeadPosition { get; private set; }

    // Agent Settings
    public NavMeshAgent Agent { get; private set; }
    public NavMeshObstacle Obstacle { get; private set; }

    public Transform Target => target;
    public Transform TargetIK => target;
    public Transform RotateToTargetOnIdle => rotateToTargetOnIdle;
    public Transform RotateToTargetOnMoving => rotateToTargetOnMoving;


    public Vector3 AnchorPosition { get; set; }
    public float HelperWaitingTime { get; set; }
    public bool IsReturning { get; set; }

    public bool IsImpacted { get; set; }
    public bool IsRunning { get; set; }
    public bool IsOnGround => GroundDetector.IsGrounded;
    public bool CheckOnGround { get; set; }


    private void Awake()
    {
        // Components
        RbEntity = GetComponent<Rigidbody>();
        Feedback = GetComponent<FeedBack_Entity>();

        GroundDetector = GetComponentInChildren<GroundDetector>();
        MainCamera = Camera.main.transform;

        Vector3 headPosition = new Vector3(transform.position.x, transform.position.y + entity.Height, transform.position.z);
        HeadPosition = transform;
        HeadPosition.position = headPosition;

        CheckOnGround = IsOnGround;

        // Agent and Obstacle
        Agent = GetComponent<NavMeshAgent>();
        Obstacle = GetComponent<NavMeshObstacle>();

        Obstacle.enabled = false;
        Obstacle.carving = true;

        Agent.updatePosition = false;
        Agent.updateRotation = false;

        if (isRandomizeAvoidancePriority) Agent.avoidancePriority = Random.Range(1, 99);

        // Model
        foreach (Transform t in transform.Find("Visual").transform) t.gameObject.SetActive(false);
        Model = Instantiate(entity.PrefabEntity, Vector3.zero, Quaternion.identity, transform.Find("Visual").transform);
        Model.transform.localPosition = Vector3.zero;
        Model.transform.localRotation = Quaternion.identity;

        Animator_Generic = GetComponentInChildren<Animator_Generic>();
        IKAnimator = GetComponentInChildren<IKAnimator>();

        Generic_RagDoll = GetComponentInChildren<Generic_RagDoll>();
        if (Generic_RagDoll) Generic_RagDoll.SetUp(this);
    }

    public virtual void OnEnable()
    {
        Starting();
    }

    private void Starting()
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        if (Animator_Generic && OverrideAnimationAll) Animator_Generic.OnAllOverRideAnimation(OverrideAnimationAll);
        ChangeState(Entity.InitialState);
    }

    public virtual void ChangeState(StateEntity_SO newState)
    {
        if (newState == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Tentativo di cambiare a uno stato null. Operazione ignorata.");
            return;
        }

        if (currentState)
        {
            currentState.Exit(this);
            if (currentState.PossibleCoState != null) foreach (StateEntityCo_SO statesCo in currentState.PossibleCoState) if (statesCo) statesCo.Exit(this);
        }

        currentState = newState;
        currentState.Enter(this);
        TimeOnCurrentState = 0f;

        if (currentState.PossibleCoState != null) foreach (StateEntityCo_SO statesCo in currentState.PossibleCoState) if (statesCo) statesCo.Enter(this);
    }

    public virtual void Update()
    {
        CheckGroundAndSetMoving();

        if (!currentState) return;
        TimeOnCurrentState += Time.deltaTime;

        currentState.StateUpdate(this);
        if (currentState.PossibleCoState != null) foreach (StateEntityCo_SO statesCo in currentState.PossibleCoState) if (statesCo) statesCo.StateUpdate(this);


        if (currentState.PossibleTransitions == null) return;

        StateEntity_SO startingStateThisFrame = currentState;
        foreach (TrasiitionsBase_SO transition in currentState.PossibleTransitions)
        {
            if (!transition) continue;

            if (transition.CheckInterval <= 0f)
            {
                transition.OnTransition(this);
                return;
            }

            transitionTimers.TryGetValue(transition, out float nextCheckTime);
            if (Time.time < nextCheckTime) return;

            transition.OnTransition(this);
            transitionTimers[transition] = Time.time + transition.CheckInterval;

            if (currentState != startingStateThisFrame) break;
        }
    }

    public void FixedUpdate()
    {
        if (currentState)
        {
            currentState.StateFixedUpdate(this);
            if (currentState.PossibleCoState != null) foreach (StateEntityCo_SO statesCo in currentState.PossibleCoState) if (statesCo) statesCo.StateFixedUpdate(this);

        }
    }

    #region Moving&RotationAgent
    public virtual void MovingAgent()
    {
        Vector3 desiredVelocity = Agent.desiredVelocity;
        Agent.nextPosition = transform.position;

        if (desiredVelocity.sqrMagnitude < 0.01f) return;

        Vector3 moveDirection = desiredVelocity.normalized;
        moveDirection = Vector3.ProjectOnPlane(moveDirection, GroundDetector.GroundNormal).normalized; 

        float velocityForSlope = GroundDetector.SlopeAngle > 0 ? (GroundDetector.SlopeAngle / 10) : 0;
        Vector3 targetVelocity = moveDirection * (Agent.speed + velocityForSlope);

        RbEntity.AddForce(targetVelocity, ForceMode.Acceleration);

        Vector3 lookDir;
        if (rotateToTargetOnMoving) lookDir = rotateToTargetOnMoving.position - transform.position;
        else lookDir = desiredVelocity;

        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.fixedDeltaTime * rotationSpeedOnMoving);
        }

        if (Animator_Generic) Animator_Generic.AnimationMoving(Agent.speed, entity.RunSpeed, RbEntity);
    }

    public virtual void SetAgentDestinationTimer(float time, bool isDynamicTarget = true)
    {
        if (!Target || !Agent.isActiveAndEnabled || !Agent.isOnNavMesh) return;

        StopAgentDestination();
        Agent.isStopped = false;

        if (coroutineSetAgentDestination != null) StopCoroutine(coroutineSetAgentDestination);
        coroutineSetAgentDestination = StartCoroutine(SetAgentDestinationRoutine(time,isDynamicTarget));
    }


    protected IEnumerator SetAgentDestinationRoutine(float time, bool loop)
    {
        yield return new WaitForSeconds(time);
        if (NavMesh.SamplePosition(Target.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            lastPositionTarget = Target.position;
            Agent.SetDestination(hit.position);
        }

        while (loop)
        {
            yield return new WaitForSeconds(time);
            if (Agent.enabled && Agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(Target.position, out NavMeshHit hitSecond, 2f, NavMesh.AllAreas))
                {
                    if (lastPositionTarget != Target.position)
                    {
                        lastPositionTarget = Target.position;
                        Agent.SetDestination(hitSecond.position);
                    }
                }
            }
        }

    }

    public virtual void StopAgentDestination()
    {
        if (coroutineSetAgentDestination != null) StopCoroutine(coroutineSetAgentDestination);
        Agent.isStopped = true;
        Agent.ResetPath();
    }

    public virtual void IdleRotateTowardsTarget()
    {
        if (GroundDetector.SlopeAngle > 0)
        {
            Vector3 gravityAlongSlope = Vector3.ProjectOnPlane(Physics.gravity, GroundDetector.GroundNormal);
            RbEntity.AddForce(-gravityAlongSlope, ForceMode.Acceleration);
        }

        Agent.nextPosition = transform.position;
        if (Animator_Generic) Animator_Generic.AnimationMoving(RbEntity.linearVelocity.magnitude * 100, entity.RunSpeed, RbEntity);

        if (rotateToTargetOnIdle == null) rotateToTargetOnIdle = Target;
        if (rotateToTargetOnIdle == null) return;

        Vector3 dir = (rotateToTargetOnIdle.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            float angleDifference = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(dir), Time.fixedDeltaTime * rotationSpeedOnIdle);
            // Rotatiotion Body

            if (Mathf.Abs(angleDifference) > 2f)
            {
                float turnValue = Mathf.Clamp(angleDifference / 45f, -1f, 1f);

                if (Animator_Generic) Animator_Generic.Animator.SetFloat(Parameters.ParameterFloatTurning, turnValue, 0.1f, Time.deltaTime);
            }
            else
            {
                if (Animator_Generic) Animator_Generic.Animator.SetFloat(Parameters.ParameterFloatTurning, 0f, 0.1f, Time.deltaTime);
            }
        }
    }

    public virtual void SetDestinationTarget(Transform target) => this.target = target;
    public virtual void SetTargetIK(Transform target) => targetIK = target;
    public virtual void SetRotateToTargetOnIdle(Transform target) => rotateToTargetOnIdle = target;
    public virtual void SetRotateToTargetOnMoving(Transform target) => rotateToTargetOnMoving = target;


    public virtual void ClearDestinationTarget() => target = null;
    public virtual void ClearTargetIK() => targetIK = null;
    public virtual void ClearRotateToTargetOnIdle() => rotateToTargetOnIdle = null;
    public virtual void ClearRotateToTargetOnMoving() => rotateToTargetOnMoving = null;

    #endregion

    public virtual void CheckGroundAndSetMoving()
    {
        if (RbEntity.isKinematic) return;

        if (CheckOnGround != GroundDetector.IsGrounded)
        {
            SetMoving(GroundDetector.IsGrounded);
            CheckOnGround = GroundDetector.IsGrounded;

            SelectMovingAnimation();
        }
    }

    public virtual void SelectMovingAnimation()
    {
        if (Animator_Generic)
        {
            if (GroundDetector.IsGrounded) Animator_Generic.SelectAnimation(true, Parameters.ParameterTriggerOnMovingGround);
            else Animator_Generic.SelectAnimation(true, Parameters.ParameterTriggerOnMovingAir);
        }
    }

    public virtual void SetMoving(bool isOnGround)
    {
        if (isOnGround) SetMovingOnGround();
        else SetMovingOnAir();

        SelectMovingAnimation();
        if (targetIK) IKAnimator.SetTarget(targetIK);
    }

    protected void SetMovingOnGround()
    {
        targetSpeed = IsRunning ? entity.RunSpeed : entity.WalkSpeed;
        Agent.speed = targetSpeed;
        RbEntity.linearDamping = entity.GroundDrag;
    }

    protected void SetMovingOnAir()
    {
        targetSpeed = IsRunning ? entity.RunAirSpeed : entity.WalkAirSpeed;
        Agent.speed = targetSpeed;
        RbEntity.linearDamping = entity.AirDrag;
    }

    [ContextMenu("ActiveRagdoll")]
    public virtual void ActiveRagdoll() => BigDamage(Vector3.zero, transform.position);
    public virtual void BigDamage(Vector3 direction, Vector3 hitPoint)
    {
        if (Generic_RagDoll) Generic_RagDoll.TriggerRagdoll(direction, hitPoint);

        ChangeState(Entity.RagdollState);
        IsImpacted = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impactVelocityXZ = entity.ImpactThresholdXZ;
        float impactVelocityY = entity.ImpactThresholdY;

        if (Mathf.Abs(collision.relativeVelocity.x) > impactVelocityXZ || Mathf.Abs(collision.relativeVelocity.z) > impactVelocityXZ || Mathf.Abs(collision.relativeVelocity.y) > impactVelocityY)
        {
            BigDamage(Vector3.up * 10f, transform.position);
        }
    }
}
