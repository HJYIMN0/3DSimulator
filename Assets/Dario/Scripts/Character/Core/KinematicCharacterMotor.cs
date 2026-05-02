using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace KinematicCharacterController
{
    public enum RigidbodyInteractionType
    {
        None,
        Kinematic,
        SimulatedDynamic
    }

    public enum StepHandlingMethod
    {
        None,
        Standard,
        Extra
    }

    public enum MovementSweepState
    {
        Initial,
        AfterFirstHit,
        FoundBlockingCrease,
        FoundBlockingCorner,
    }

    [System.Serializable]
    public struct KinematicCharacterMotorState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 BaseVelocity;
        public bool MustUnground;
        public float MustUngroundTime;
        public bool LastMovementIterationFoundAnyGround;
        public CharacterTransientGroundingReport GroundingStatus;
        public Rigidbody AttachedRigidbody;
        public Vector3 AttachedRigidbodyVelocity;
    }

    public struct OverlapResult
    {
        public Vector3 Normal;
        public Collider Collider;

        public OverlapResult(Vector3 normal, Collider collider)
        {
            Normal = normal;
            Collider = collider;
        }
    }

    public struct CharacterGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;
        public Collider GroundCollider;
        public Vector3 GroundPoint;

        public void CopyFrom(CharacterTransientGroundingReport transientGroundingReport)
        {
            FoundAnyGround = transientGroundingReport.FoundAnyGround;
            IsStableOnGround = transientGroundingReport.IsStableOnGround;
            SnappingPrevented = transientGroundingReport.SnappingPrevented;
            GroundNormal = transientGroundingReport.GroundNormal;
            InnerGroundNormal = transientGroundingReport.InnerGroundNormal;
            OuterGroundNormal = transientGroundingReport.OuterGroundNormal;
            GroundCollider = null;
            GroundPoint = Vector3.zero;
        }
    }

    public struct CharacterTransientGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public void CopyFrom(CharacterGroundingReport groundingReport)
        {
            FoundAnyGround = groundingReport.FoundAnyGround;
            IsStableOnGround = groundingReport.IsStableOnGround;
            SnappingPrevented = groundingReport.SnappingPrevented;
            GroundNormal = groundingReport.GroundNormal;
            InnerGroundNormal = groundingReport.InnerGroundNormal;
            OuterGroundNormal = groundingReport.OuterGroundNormal;
        }
    }

    public struct HitStabilityReport
    {
        public bool IsStable;
        public bool FoundInnerNormal;
        public Vector3 InnerNormal;
        public bool FoundOuterNormal;
        public Vector3 OuterNormal;
        public bool ValidStepDetected;
        public Collider SteppedCollider;
        public bool LedgeDetected;
        public bool IsOnEmptySideOfLedge;
        public float DistanceFromLedge;
        public bool IsMovingTowardsEmptySideOfLedge;
        public Vector3 LedgeGroundNormal;
        public Vector3 LedgeRightDirection;
        public Vector3 LedgeFacingDirection;
    }

    public struct RigidbodyProjectionHit
    {
        public Rigidbody Rigidbody;
        public Vector3 HitPoint;
        public Vector3 EffectiveHitNormal;
        public Vector3 HitVelocity;
        public bool StableOnHit;
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class KinematicCharacterMotor : MonoBehaviour
    {
#pragma warning disable 0414
        [Header("Components")]
        [ReadOnly]
        public CapsuleCollider Capsule;

        [Header("Capsule Settings")]
        [SerializeField]
        [Tooltip("Radius of the Character Capsule")]
        private float CapsuleRadius = 0.5f;
        public float capsuleRadius => CapsuleRadius;

        [SerializeField]
        [Tooltip("Height of the Character Capsule")]
        private float CapsuleHeight = 2f;
        public float capsuleHeight => CapsuleHeight;

        [SerializeField]
        [Tooltip("Local Y offset of capsule center")]
        private float CapsuleYOffset = 1f;

        [SerializeField]
        [Tooltip("Physics material of the Character Capsule")]
        private PhysicsMaterial CapsulePhysicsMaterial;

        [Header("Grounding settings")]
        [Tooltip("Increases the range of ground detection")]
        public float GroundDetectionExtraDistance = 0f;

        [Range(0f, 89f)]
        [Tooltip("Maximum slope angle on which the character can be stable")]
        public float MaxStableSlopeAngle = 60f;

        [Tooltip("Which layers can the character be considered stable on")]
        public LayerMask StableGroundLayers = -1;

        [Tooltip("Notifies the Character Controller when discrete collisions are detected")]
        public bool DiscreteCollisionEvents = false;

        [Header("Step settings")]
        [Tooltip("Handles properly detecting grounding status on steps")]
        public StepHandlingMethod StepHandling = StepHandlingMethod.Standard;

        [Tooltip("Maximum height of a step which the character can climb")]
        public float MaxStepHeight = 0.5f;

        [Tooltip("Can the character step up obstacles even if it is not currently stable?")]
        public bool AllowSteppingWithoutStableGrounding = false;

        [Tooltip("Minimum length of a step that the character can step on")]
        public float MinRequiredStepDepth = 0.1f;

        [Header("Ledge settings")]
        [Tooltip("Handles properly detecting ledge information and grounding status")]
        public bool LedgeAndDenivelationHandling = true;

        [Tooltip("The distance from the capsule central axis at which the character can stand on a ledge")]
        public float MaxStableDistanceFromLedge = 0.5f;

        [Tooltip("Prevents snapping to ground on ledges beyond a certain velocity")]
        public float MaxVelocityForLedgeSnap = 0f;

        [Tooltip("The maximum downward slope angle change that the character can be subjected to and still be snapping to the ground")]
        [Range(1f, 180f)]
        public float MaxStableDenivelationAngle = 180f;

        [Header("Rigidbody interaction settings")]
        [Tooltip("Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies")]
        public bool InteractiveRigidbodyHandling = true;

        [Tooltip("How the character interacts with non-kinematic rigidbodies")]
        public RigidbodyInteractionType RigidbodyInteractionType;

        [Tooltip("Mass used for pushing bodies")]
        public float SimulatedCharacterMass = 70f;

        [Tooltip("Determines if the character preserves moving platform velocities when de-grounding from them")]
        public bool PreserveAttachedRigidbodyMomentum = true;

        [Header("Constraints settings")]
        [Tooltip("Determines if the character's movement uses the planar constraint")]
        public bool HasPlanarConstraint = false;

        [Tooltip("Defines the plane that the character's movement is constrained on")]
        public Vector3 PlanarConstraintAxis = Vector3.forward;

        [Header("Other settings")]
        [Tooltip("INUTILE IN MODALITA RIGIDBODY - era per sweep kinematico")]
        public int MaxMovementIterations = 5;

        [Tooltip("INUTILE IN MODALITA RIGIDBODY - era per decollision kinematico")]
        public int MaxDecollisionIterations = 1;

        [Tooltip("INUTILE IN MODALITA RIGIDBODY - era per overlap detection pre-sweep")]
        public bool CheckMovementInitialOverlaps = true;

        [Tooltip("INUTILE IN MODALITA RIGIDBODY")]
        public bool KillVelocityWhenExceedMaxMovementIterations = true;

        [Tooltip("INUTILE IN MODALITA RIGIDBODY")]
        public bool KillRemainingMovementWhenExceedMaxMovementIterations = true;

        [System.NonSerialized]
        public CharacterGroundingReport GroundingStatus = new CharacterGroundingReport();

        [System.NonSerialized]
        public CharacterTransientGroundingReport LastGroundingStatus = new CharacterTransientGroundingReport();

        [System.NonSerialized]
        public LayerMask CollidableLayers = -1;

        public Transform Transform { get { return _transform; } }
        public Vector3 TransientPosition { get { return _transientPosition; } }
        public Quaternion TransientRotation { get { return _transientRotation; } }
        public Vector3 CharacterUp { get { return _characterUp; } }
        public Vector3 CharacterForward { get { return _characterForward; } }
        public Vector3 CharacterRight { get { return _characterRight; } }
        public Vector3 InitialSimulationPosition { get { return _initialSimulationPosition; } }
        public Quaternion InitialSimulationRotation { get { return _initialSimulationRotation; } }
        public Rigidbody AttachedRigidbody { get { return _attachedRigidbody; } }
        public Vector3 CharacterTransformToCapsuleCenter { get { return _characterTransformToCapsuleCenter; } }
        public Vector3 CharacterTransformToCapsuleBottom { get { return _characterTransformToCapsuleBottom; } }
        public Vector3 CharacterTransformToCapsuleTop { get { return _characterTransformToCapsuleTop; } }
        public Vector3 CharacterTransformToCapsuleBottomHemi { get { return _characterTransformToCapsuleBottomHemi; } }
        public Vector3 CharacterTransformToCapsuleTopHemi { get { return _characterTransformToCapsuleTopHemi; } }
        public Vector3 AttachedRigidbodyVelocity { get { return _attachedRigidbodyVelocity; } }
        public int OverlapsCount { get { return _overlapsCount; } }
        public OverlapResult[] Overlaps { get { return _overlaps; } }
        public Vector3 Velocity { get { return BaseVelocity + _attachedRigidbodyVelocity; } }

        [NonSerialized]
        public ICharacterController CharacterController;

        [NonSerialized]
        public bool LastMovementIterationFoundAnyGround;

        [NonSerialized]
        public int IndexInCharacterSystem;

        [NonSerialized]
        public Vector3 InitialTickPosition;

        [NonSerialized]
        public Quaternion InitialTickRotation;

        [NonSerialized]
        public Rigidbody AttachedRigidbodyOverride;

        [NonSerialized]
        public Vector3 BaseVelocity;

        private Rigidbody Rb;
        private Transform _transform;
        private Vector3 _transientPosition;
        private Quaternion _transientRotation;
        private Vector3 _characterUp;
        private Vector3 _characterForward;
        private Vector3 _characterRight;
        private Vector3 _initialSimulationPosition;
        private Quaternion _initialSimulationRotation;
        private Rigidbody _attachedRigidbody;
        private Rigidbody _lastAttachedRigidbody;
        private Vector3 _attachedRigidbodyVelocity;
        private Vector3 _characterTransformToCapsuleCenter;
        private Vector3 _characterTransformToCapsuleBottom;
        private Vector3 _characterTransformToCapsuleTop;
        private Vector3 _characterTransformToCapsuleBottomHemi;
        private Vector3 _characterTransformToCapsuleTopHemi;
        private int _overlapsCount;
        private OverlapResult[] _overlaps = new OverlapResult[16];
        private bool _mustUnground = false;
        private float _mustUngroundTimeCounter = 0f;
        private RaycastHit[] _internalCharacterHits = new RaycastHit[16];
        private Collider[] _internalProbedColliders = new Collider[16];
        private List<Rigidbody> _rigidbodiesPushedThisMove = new List<Rigidbody>(16);
        private RigidbodyProjectionHit[] _internalRigidbodyProjectionHits = new RigidbodyProjectionHit[16];
        private int _rigidbodyProjectionHitCount = 0;
        private Vector3 _cachedWorldUp = Vector3.up;
        private Vector3 _cachedWorldForward = Vector3.forward;
        private Vector3 _cachedWorldRight = Vector3.right;
        private Vector3 _cachedZeroVector = Vector3.zero;
        private bool _solveGrounding = true;

        private const float CollisionOffset = 0.01f;
        private const float GroundProbeReboundDistance = 0.02f;
        private const float MinimumGroundProbingDistance = 0.005f;
        private const float SecondaryProbesVertical = 0.02f;
        private const float SecondaryProbesHorizontal = 0.001f;
        private const float SteppingForwardDistance = 0.03f;
        private const float MinVelocityMagnitude = 0.01f;
        private const float MinDistanceForLedge = 0.05f;
        private const float GroundProbingBackstepDistance = 0.1f;
#pragma warning restore 0414

        private void OnEnable()
        {
            KinematicCharacterSystem.EnsureCreation();
            KinematicCharacterSystem.RegisterCharacterMotor(this);
        }

        private void OnDisable()
        {
            KinematicCharacterSystem.UnregisterCharacterMotor(this);
        }

        private void Reset()
        {
            ValidateData();
        }

        private void OnValidate()
        {
            ValidateData();
        }

        [ContextMenu("Remove Component")]
        private void HandleRemoveComponent()
        {
            CapsuleCollider tmpCapsule = gameObject.GetComponent<CapsuleCollider>();
            Rigidbody tmpRb = gameObject.GetComponent<Rigidbody>();
            DestroyImmediate(this);
            DestroyImmediate(tmpCapsule);
            DestroyImmediate(tmpRb);
        }

        public void ValidateData()
        {
            Capsule = GetComponent<CapsuleCollider>();
            CapsuleRadius = Mathf.Clamp(CapsuleRadius, 0f, CapsuleHeight * 0.5f);
            Capsule.direction = 1;
            Capsule.sharedMaterial = CapsulePhysicsMaterial;
            SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);

            MaxStepHeight = Mathf.Clamp(MaxStepHeight, 0f, Mathf.Infinity);
            MinRequiredStepDepth = Mathf.Clamp(MinRequiredStepDepth, 0f, CapsuleRadius);
            MaxStableDistanceFromLedge = Mathf.Clamp(MaxStableDistanceFromLedge, 0f, CapsuleRadius);

            transform.localScale = Vector3.one;

#if UNITY_EDITOR
            Capsule.hideFlags = HideFlags.NotEditable;
            if (!Mathf.Approximately(transform.lossyScale.x, 1f) || !Mathf.Approximately(transform.lossyScale.y, 1f) || !Mathf.Approximately(transform.lossyScale.z, 1f))
            {
                Debug.LogError("Character's lossy scale is not (1,1,1). This is not allowed. Make sure the character's transform and all of its parents have a (1,1,1) scale.", this.gameObject);
            }
#endif
        }

        public void SetCapsuleDimensions(float radius, float height, float yOffset)
        {
            height = Mathf.Max(height, (radius * 2f) + 0.01f);

            CapsuleRadius = radius;
            CapsuleHeight = height;
            CapsuleYOffset = yOffset;

            Capsule.radius = CapsuleRadius;
            Capsule.height = Mathf.Clamp(CapsuleHeight, CapsuleRadius * 2f, CapsuleHeight);
            Capsule.center = new Vector3(0f, CapsuleYOffset, 0f);

            _characterTransformToCapsuleCenter = Capsule.center;
            _characterTransformToCapsuleBottom = Capsule.center + (-_cachedWorldUp * (Capsule.height * 0.5f));
            _characterTransformToCapsuleTop = Capsule.center + (_cachedWorldUp * (Capsule.height * 0.5f));
            _characterTransformToCapsuleBottomHemi = Capsule.center + (-_cachedWorldUp * (Capsule.height * 0.5f)) + (_cachedWorldUp * Capsule.radius);
            _characterTransformToCapsuleTopHemi = Capsule.center + (_cachedWorldUp * (Capsule.height * 0.5f)) + (-_cachedWorldUp * Capsule.radius);
        }

        private void Awake()
        {
            _transform = transform;

            Rb = GetComponent<Rigidbody>();
            if (Rb == null)
            {
                Rb = gameObject.AddComponent<Rigidbody>();
            }

            Rb.isKinematic = false;
            Rb.useGravity = true; //test
            Rb.constraints = RigidbodyConstraints.FreezeRotationX |
                 RigidbodyConstraints.FreezeRotationY |
                 RigidbodyConstraints.FreezeRotationZ;//test
            Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;
            Rb.mass = SimulatedCharacterMass;

            ValidateData();

            _transientPosition = _transform.position;
            _transientRotation = _transform.rotation;
            _initialSimulationPosition = _transientPosition;
            _initialSimulationRotation = _transientRotation;
            InitialTickPosition = _transientPosition;
            InitialTickRotation = _transientRotation;

            _characterUp = _transientRotation * _cachedWorldUp;
            _characterForward = _transientRotation * _cachedWorldForward;
            _characterRight = _transientRotation * _cachedWorldRight;

            CollidableLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
                {
                    CollidableLayers |= (1 << i);
                }
            }

            SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);
        }

        public void UpdatePhase1(float deltaTime)
        {
            print("=== PHASE 1 START ===");

            if (float.IsNaN(BaseVelocity.x) || float.IsNaN(BaseVelocity.y) || float.IsNaN(BaseVelocity.z))
            {
                print("fase 1 base velocity");//controlla
                BaseVelocity = Vector3.zero;
            }
            if (float.IsNaN(_attachedRigidbodyVelocity.x) || float.IsNaN(_attachedRigidbodyVelocity.y) || float.IsNaN(_attachedRigidbodyVelocity.z))
            {
                print("fase 1 rb velocity");//controlla
                _attachedRigidbodyVelocity = Vector3.zero;
            }

#if UNITY_EDITOR
            if (!Mathf.Approximately(_transform.lossyScale.x, 1f) || !Mathf.Approximately(_transform.lossyScale.y, 1f) || !Mathf.Approximately(_transform.lossyScale.z, 1f))
            {
                Debug.LogError("Character's lossy scale is not (1,1,1). This is not allowed.", this.gameObject);
            }
#endif

            _rigidbodiesPushedThisMove.Clear();

            CharacterController.BeforeCharacterUpdate(deltaTime);

            _transientPosition = _transform.position;
            _transientRotation = _transform.rotation;
            _initialSimulationPosition = _transientPosition;
            _initialSimulationRotation = _transientRotation;
            _rigidbodyProjectionHitCount = 0;
            _overlapsCount = 0;

            _characterUp = _transientRotation * _cachedWorldUp;
            _characterForward = _transientRotation * _cachedWorldForward;
            _characterRight = _transientRotation * _cachedWorldRight;

            LastGroundingStatus.CopyFrom(GroundingStatus);
            GroundingStatus = new CharacterGroundingReport();
            GroundingStatus.GroundNormal = _characterUp;
            print($"_solveGrounding: {_solveGrounding}");//test
            if (_solveGrounding)
            {
                print($"MustUnground: {MustUnground()}");//test
                if (MustUnground())
                {
                    _transientPosition += _characterUp * (MinimumGroundProbingDistance * 1.5f);
                }
                else
                {
                    float selectedGroundProbingDistance = MinimumGroundProbingDistance*1.5f;
                    print($"LastGroundingStatus.IsStableOnGround: {LastGroundingStatus.IsStableOnGround}");
                    print($"LastMovementIterationFoundAnyGround: {LastMovementIterationFoundAnyGround}");//test


                    if (!LastGroundingStatus.SnappingPrevented && (LastGroundingStatus.IsStableOnGround || LastMovementIterationFoundAnyGround))
                    {
                        if (StepHandling != StepHandlingMethod.None)
                        {
                            selectedGroundProbingDistance = Mathf.Max(CapsuleRadius, MaxStepHeight);
                        }
                        else
                        {
                            selectedGroundProbingDistance = CapsuleRadius;
                        }

                        selectedGroundProbingDistance += GroundDetectionExtraDistance;
                    }
                    print($"selectedGroundProbingDistance: {selectedGroundProbingDistance}");
                    print("CALLING ProbeGround...");//test
                    ProbeGround(ref _transientPosition, _transientRotation, selectedGroundProbingDistance, ref GroundingStatus);
                    print($"AFTER ProbeGround - FoundAnyGround: {GroundingStatus.FoundAnyGround}, IsStable: {GroundingStatus.IsStableOnGround}");//test
                    if (!LastGroundingStatus.IsStableOnGround && GroundingStatus.IsStableOnGround)
                    {
                        BaseVelocity = Vector3.ProjectOnPlane(BaseVelocity, CharacterUp);
                        BaseVelocity = GetDirectionTangentToSurface(BaseVelocity, GroundingStatus.GroundNormal) * BaseVelocity.magnitude;
                    }
                }
            }

            LastMovementIterationFoundAnyGround = false;

            if (_mustUngroundTimeCounter > 0f)
            {
                _mustUngroundTimeCounter -= deltaTime;
            }
            _mustUnground = false;

            if (_solveGrounding)
            {
                CharacterController.PostGroundingUpdate(deltaTime);
            }

            if (InteractiveRigidbodyHandling)
            {
                _lastAttachedRigidbody = _attachedRigidbody;
                if (AttachedRigidbodyOverride)
                {
                    _attachedRigidbody = AttachedRigidbodyOverride;
                }
                else
                {
                    if (GroundingStatus.IsStableOnGround && GroundingStatus.GroundCollider && GroundingStatus.GroundCollider.attachedRigidbody)
                    {
                        Rigidbody interactiveRigidbody = GetInteractiveRigidbody(GroundingStatus.GroundCollider);
                        if (interactiveRigidbody)
                        {
                            _attachedRigidbody = interactiveRigidbody;
                        }
                    }
                    else
                    {
                        _attachedRigidbody = null;
                    }
                }

                Vector3 tmpVelocityFromCurrentAttachedRigidbody = Vector3.zero;
                Vector3 tmpAngularVelocityFromCurrentAttachedRigidbody = Vector3.zero;
                if (_attachedRigidbody)
                {
                    GetVelocityFromRigidbodyMovement(_attachedRigidbody, _transientPosition, deltaTime, out tmpVelocityFromCurrentAttachedRigidbody, out tmpAngularVelocityFromCurrentAttachedRigidbody);
                }

                if (PreserveAttachedRigidbodyMomentum && _lastAttachedRigidbody != null && _attachedRigidbody != _lastAttachedRigidbody)
                {
                    BaseVelocity += _attachedRigidbodyVelocity;
                    BaseVelocity -= tmpVelocityFromCurrentAttachedRigidbody;
                }

                _attachedRigidbodyVelocity = _cachedZeroVector;
                if (_attachedRigidbody)
                {
                    _attachedRigidbodyVelocity = tmpVelocityFromCurrentAttachedRigidbody;

                    Vector3 newForward = Vector3.ProjectOnPlane(Quaternion.Euler(Mathf.Rad2Deg * tmpAngularVelocityFromCurrentAttachedRigidbody * deltaTime) * _characterForward, _characterUp).normalized;
                    _transientRotation = Quaternion.LookRotation(newForward, _characterUp);
                }

                if (GroundingStatus.GroundCollider &&
                    GroundingStatus.GroundCollider.attachedRigidbody &&
                    GroundingStatus.GroundCollider.attachedRigidbody == _attachedRigidbody &&
                    _attachedRigidbody != null &&
                    _lastAttachedRigidbody == null)
                {
                    BaseVelocity -= Vector3.ProjectOnPlane(_attachedRigidbodyVelocity, _characterUp);
                }
            }
            print("=== PHASE 1 END ===");//test
        }

        public void UpdatePhase2(float deltaTime)
        {
            print("=== PHASE 2 START ===");//test
            CharacterController.UpdateRotation(ref _transientRotation, deltaTime);
            _characterUp = _transientRotation * _cachedWorldUp;
            _characterForward = _transientRotation * _cachedWorldForward;
            _characterRight = _transientRotation * _cachedWorldRight;
            print($"BaseVelocity BEFORE UpdateVelocity: {BaseVelocity}");//test
            CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);
            print($"BaseVelocity AFTER UpdateVelocity: {BaseVelocity}");//test
            if (BaseVelocity.magnitude < MinVelocityMagnitude)
            {
                BaseVelocity = Vector3.zero;
            }

            if (HasPlanarConstraint)
            {
                BaseVelocity = Vector3.ProjectOnPlane(BaseVelocity, PlanarConstraintAxis.normalized);
            }

            Vector3 targetVelocity = BaseVelocity + _attachedRigidbodyVelocity;
            print($"Setting Rb.linearVelocity to: {targetVelocity}");//test
            Rb.linearVelocity = targetVelocity;
            print($"Rb.linearVelocity AFTER assignment: {Rb.linearVelocity}");//test
            print($"Rb.constraints: {Rb.constraints}");//test
            print($"Rb.isKinematic: {Rb.isKinematic}");//test
            print($"Capsule.isTrigger: {Capsule.isTrigger}");//test
            print($"Rb.mass: {Rb.mass}");//test
            print($"Transform.position.y: {transform.position.y}");//test

            _transform.rotation = _transientRotation;
            

            if (HasPlanarConstraint)
            {
                _transientPosition = _initialSimulationPosition + Vector3.ProjectOnPlane(_transientPosition - _initialSimulationPosition, PlanarConstraintAxis.normalized);
            }

            if (DiscreteCollisionEvents)
            {
                int nbOverlaps = CharacterCollisionsOverlap(_transientPosition, _transientRotation, _internalProbedColliders, CollisionOffset * 2f);
                for (int i = 0; i < nbOverlaps; i++)
                {
                    CharacterController.OnDiscreteCollisionDetected(_internalProbedColliders[i]);
                }
            }

            CharacterController.AfterCharacterUpdate(deltaTime);
            print("=== PHASE 2 END ===");//test
        }

        private void OnCollisionStay(Collision collision)
        {
            print($"COLLISION WITH: {collision.collider.name}");//test
            if (!InteractiveRigidbodyHandling) return;

            Rigidbody hitRigidbody = collision.rigidbody;
            if (hitRigidbody && !hitRigidbody.isKinematic && _rigidbodyProjectionHitCount < _internalRigidbodyProjectionHits.Length)
            {
                if (hitRigidbody != _attachedRigidbody && !_rigidbodiesPushedThisMove.Contains(hitRigidbody))
                {
                    RigidbodyProjectionHit rph = new RigidbodyProjectionHit();
                    rph.Rigidbody = hitRigidbody;

                    ContactPoint contact = collision.GetContact(0);
                    rph.HitPoint = contact.point;
                    rph.EffectiveHitNormal = contact.normal;
                    rph.HitVelocity = Rb.linearVelocity;
                    rph.StableOnHit = IsStableOnNormal(contact.normal);

                    _internalRigidbodyProjectionHits[_rigidbodyProjectionHitCount] = rph;
                    _rigidbodyProjectionHitCount++;

                    _rigidbodiesPushedThisMove.Add(hitRigidbody);

                    float characterMass = SimulatedCharacterMass;
                    float hitBodyMass = hitRigidbody.mass;
                    Vector3 characterVelocity = Rb.linearVelocity;
                    Vector3 hitBodyVelocity = hitRigidbody.linearVelocity;

                    float characterToBodyMassRatio = characterMass / (characterMass + hitBodyMass);

                    if (RigidbodyInteractionType == RigidbodyInteractionType.Kinematic)
                    {
                        characterToBodyMassRatio = 1f;
                    }

                    ComputeCollisionResolutionForHitBody(
                        contact.normal,
                        characterVelocity,
                        hitBodyVelocity,
                        characterToBodyMassRatio,
                        out Vector3 velocityChangeOnCharacter,
                        out Vector3 velocityChangeOnBody);

                    if (velocityChangeOnBody.sqrMagnitude > 0.001f)
                    {
                        hitRigidbody.AddForceAtPosition(velocityChangeOnBody, contact.point, ForceMode.VelocityChange);
                    }
                }
            }
        }

        public void ProbeGround(ref Vector3 probingPosition, Quaternion atRotation, float probingDistance, ref CharacterGroundingReport groundingReport)
        {
            print($"ProbeGround START - probingDistance: {probingDistance}");//test
            if (probingDistance < MinimumGroundProbingDistance)
            {
                probingDistance = MinimumGroundProbingDistance;
            }

            Vector3 probeStart = probingPosition + (atRotation * _characterTransformToCapsuleBottomHemi);
            print($"probeStart: {probeStart}, CapsuleRadius: {CapsuleRadius}, _characterUp: {_characterUp}");
            print($"CollidableLayers: {CollidableLayers}, StableGroundLayers: {StableGroundLayers}");
            print($"Combined layers: {CollidableLayers & StableGroundLayers}");//test
            bool hitSomething = Physics.SphereCast(
        probeStart,
        CapsuleRadius * 0.9f,
        -_characterUp,
        out RaycastHit groundSweepHit,
        probingDistance,
        CollidableLayers & StableGroundLayers,
        QueryTriggerInteraction.Ignore);

            print($"SphereCast result: {hitSomething}");//test

            if (hitSomething)
            {
                print($"Hit collider: {groundSweepHit.collider.name}, distance: {groundSweepHit.distance}, normal: {groundSweepHit.normal}");//test

                Vector3 targetPosition = probingPosition + (-_characterUp * groundSweepHit.distance);
                HitStabilityReport groundHitStabilityReport = new HitStabilityReport();
                EvaluateHitStability(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, targetPosition, _transientRotation, BaseVelocity, ref groundHitStabilityReport);

                groundingReport.FoundAnyGround = true;
                groundingReport.GroundNormal = groundSweepHit.normal;
                groundingReport.InnerGroundNormal = groundHitStabilityReport.InnerNormal;
                groundingReport.OuterGroundNormal = groundHitStabilityReport.OuterNormal;
                groundingReport.GroundCollider = groundSweepHit.collider;
                groundingReport.GroundPoint = groundSweepHit.point;
                groundingReport.SnappingPrevented = false;

                if (groundHitStabilityReport.IsStable)
                {
                    groundingReport.SnappingPrevented = !IsStableWithSpecialCases(ref groundHitStabilityReport, BaseVelocity);

                    // AGGIUNTA: Snap solo se abbastanza vicino
                    float maxSnapDistance = Mathf.Max(CapsuleRadius, MaxStepHeight);
                    if (groundSweepHit.distance <= maxSnapDistance)
                    {
                        groundingReport.IsStableOnGround = true;

                        if (!groundingReport.SnappingPrevented)
                        {
                            probingPosition = targetPosition;

                            Vector3 snapDelta = probingPosition - _transform.position;
                            if (snapDelta.sqrMagnitude > 0.0001f)
                            {
                                _transform.position = probingPosition;
                                Rb.position = probingPosition;
                            }
                        }

                        CharacterController?.OnGroundHit(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, ref groundHitStabilityReport);
                    }
                    else
                    {
                        // Trovato ground ma troppo lontano - consideralo in aria
                        print($"Ground too far ({groundSweepHit.distance}m) - staying airborne");
                        groundingReport.IsStableOnGround = false;

                        CharacterController.OnGroundHit(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, ref groundHitStabilityReport);
                    }
                }
            }
            else { print("SphereCast HIT NOTHING!"); }//test
        }

        public void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, Vector3 withCharacterVelocity, ref HitStabilityReport stabilityReport)
        {
            if (!_solveGrounding)
            {
                stabilityReport.IsStable = false;
                return;
            }

            Vector3 atCharacterUp = atCharacterRotation * _cachedWorldUp;
            Vector3 innerHitDirection = Vector3.ProjectOnPlane(hitNormal, atCharacterUp).normalized;

            stabilityReport.IsStable = IsStableOnNormal(hitNormal);
            stabilityReport.FoundInnerNormal = false;
            stabilityReport.FoundOuterNormal = false;
            stabilityReport.InnerNormal = hitNormal;
            stabilityReport.OuterNormal = hitNormal;

            if (LedgeAndDenivelationHandling)
            {
                float ledgeCheckHeight = MinDistanceForLedge;
                if (StepHandling != StepHandlingMethod.None)
                {
                    ledgeCheckHeight = MaxStepHeight;
                }

                bool isStableLedgeInner = false;
                bool isStableLedgeOuter = false;

                if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * SecondaryProbesVertical) + (innerHitDirection * SecondaryProbesHorizontal),
                    -atCharacterUp,
                    ledgeCheckHeight + SecondaryProbesVertical,
                    out RaycastHit innerLedgeHit,
                    _internalCharacterHits) > 0)
                {
                    Vector3 innerLedgeNormal = innerLedgeHit.normal;
                    stabilityReport.InnerNormal = innerLedgeNormal;
                    stabilityReport.FoundInnerNormal = true;
                    isStableLedgeInner = IsStableOnNormal(innerLedgeNormal);
                }

                if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * SecondaryProbesVertical) + (-innerHitDirection * SecondaryProbesHorizontal),
                    -atCharacterUp,
                    ledgeCheckHeight + SecondaryProbesVertical,
                    out RaycastHit outerLedgeHit,
                    _internalCharacterHits) > 0)
                {
                    Vector3 outerLedgeNormal = outerLedgeHit.normal;
                    stabilityReport.OuterNormal = outerLedgeNormal;
                    stabilityReport.FoundOuterNormal = true;
                    isStableLedgeOuter = IsStableOnNormal(outerLedgeNormal);
                }

                stabilityReport.LedgeDetected = (isStableLedgeInner != isStableLedgeOuter);
                if (stabilityReport.LedgeDetected)
                {
                    stabilityReport.IsOnEmptySideOfLedge = isStableLedgeOuter && !isStableLedgeInner;
                    stabilityReport.LedgeGroundNormal = isStableLedgeOuter ? stabilityReport.OuterNormal : stabilityReport.InnerNormal;
                    stabilityReport.LedgeRightDirection = Vector3.Cross(hitNormal, stabilityReport.LedgeGroundNormal).normalized;
                    stabilityReport.LedgeFacingDirection = Vector3.ProjectOnPlane(Vector3.Cross(stabilityReport.LedgeGroundNormal, stabilityReport.LedgeRightDirection), CharacterUp).normalized;
                    stabilityReport.DistanceFromLedge = Vector3.ProjectOnPlane((hitPoint - (atCharacterPosition + (atCharacterRotation * _characterTransformToCapsuleBottom))), atCharacterUp).magnitude;
                    stabilityReport.IsMovingTowardsEmptySideOfLedge = Vector3.Dot(withCharacterVelocity.normalized, stabilityReport.LedgeFacingDirection) > 0f;
                }

                if (stabilityReport.IsStable)
                {
                    stabilityReport.IsStable = IsStableWithSpecialCases(ref stabilityReport, withCharacterVelocity);
                }
            }

            if (StepHandling != StepHandlingMethod.None && !stabilityReport.IsStable)
            {
                Rigidbody hitRigidbody = hitCollider.attachedRigidbody;
                if (!(hitRigidbody && !hitRigidbody.isKinematic))
                {
                    DetectSteps(atCharacterPosition, atCharacterRotation, hitPoint, innerHitDirection, ref stabilityReport);
                    if (stabilityReport.ValidStepDetected)
                    {
                        stabilityReport.IsStable = true;
                    }
                }
            }

            CharacterController.ProcessHitStabilityReport(hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref stabilityReport);
        }

        private void DetectSteps(Vector3 characterPosition, Quaternion characterRotation, Vector3 hitPoint, Vector3 innerHitDirection, ref HitStabilityReport stabilityReport)
        {
            Vector3 characterUp = characterRotation * _cachedWorldUp;
            Vector3 verticalCharToHit = Vector3.Project((hitPoint - characterPosition), characterUp);
            Vector3 horizontalCharToHitDirection = Vector3.ProjectOnPlane((hitPoint - characterPosition), characterUp).normalized;
            Vector3 stepCheckStartPos = (hitPoint - verticalCharToHit) + (characterUp * MaxStepHeight) + (horizontalCharToHitDirection * CollisionOffset * 3f);

            int nbStepHits = CharacterCollisionsSweep(
                stepCheckStartPos,
                characterRotation,
                -characterUp,
                MaxStepHeight + CollisionOffset,
                out RaycastHit outerStepHit,
                _internalCharacterHits,
                0f,
                true);

            if (CheckStepValidity(nbStepHits, characterPosition, characterRotation, innerHitDirection, stepCheckStartPos, out Collider tmpCollider))
            {
                stabilityReport.ValidStepDetected = true;
                stabilityReport.SteppedCollider = tmpCollider;
            }
        }

        private bool CheckStepValidity(int nbStepHits, Vector3 characterPosition, Quaternion characterRotation, Vector3 innerHitDirection, Vector3 stepCheckStartPos, out Collider hitCollider)
        {
            hitCollider = null;
            Vector3 characterUp = characterRotation * Vector3.up;

            bool foundValidStepPosition = false;

            while (nbStepHits > 0 && !foundValidStepPosition)
            {
                RaycastHit farthestHit = new RaycastHit();
                float farthestDistance = 0f;
                int farthestIndex = 0;
                for (int i = 0; i < nbStepHits; i++)
                {
                    float hitDistance = _internalCharacterHits[i].distance;
                    if (hitDistance > farthestDistance)
                    {
                        farthestDistance = hitDistance;
                        farthestHit = _internalCharacterHits[i];
                        farthestIndex = i;
                    }
                }

                Vector3 characterPositionAtHit = stepCheckStartPos + (-characterUp * (farthestHit.distance - CollisionOffset));

                int atStepOverlaps = CharacterCollisionsOverlap(characterPositionAtHit, characterRotation, _internalProbedColliders);
                if (atStepOverlaps <= 0)
                {
                    if (CharacterCollisionsRaycast(
                        farthestHit.point + (characterUp * SecondaryProbesVertical) + (-innerHitDirection * SecondaryProbesHorizontal),
                        -characterUp,
                        MaxStepHeight + SecondaryProbesVertical,
                        out RaycastHit outerSlopeHit,
                        _internalCharacterHits,
                        true) > 0)
                    {
                        if (IsStableOnNormal(outerSlopeHit.normal))
                        {
                            if (CharacterCollisionsSweep(
                                characterPosition,
                                characterRotation,
                                characterUp,
                                MaxStepHeight - farthestHit.distance,
                                out RaycastHit tmpUpObstructionHit,
                                _internalCharacterHits) <= 0)
                            {
                                bool innerStepValid = false;
                                RaycastHit innerStepHit;

                                if (AllowSteppingWithoutStableGrounding)
                                {
                                    innerStepValid = true;
                                }
                                else
                                {
                                    if (CharacterCollisionsRaycast(
                                        characterPosition + Vector3.Project((characterPositionAtHit - characterPosition), characterUp),
                                        -characterUp,
                                        MaxStepHeight,
                                        out innerStepHit,
                                        _internalCharacterHits,
                                        true) > 0)
                                    {
                                        if (IsStableOnNormal(innerStepHit.normal))
                                        {
                                            innerStepValid = true;
                                        }
                                    }
                                }

                                if (!innerStepValid)
                                {
                                    if (CharacterCollisionsRaycast(
                                        farthestHit.point + (innerHitDirection * SecondaryProbesHorizontal),
                                        -characterUp,
                                        MaxStepHeight,
                                        out innerStepHit,
                                        _internalCharacterHits,
                                        true) > 0)
                                    {
                                        if (IsStableOnNormal(innerStepHit.normal))
                                        {
                                            innerStepValid = true;
                                        }
                                    }
                                }

                                if (innerStepValid)
                                {
                                    hitCollider = farthestHit.collider;
                                    foundValidStepPosition = true;
                                    return true;
                                }
                            }
                        }
                    }
                }

                if (!foundValidStepPosition)
                {
                    nbStepHits--;
                    if (farthestIndex < nbStepHits)
                    {
                        _internalCharacterHits[farthestIndex] = _internalCharacterHits[nbStepHits];
                    }
                }
            }

            return false;
        }

        private bool IsStableOnNormal(Vector3 normal)
        {
            return Vector3.Angle(_characterUp, normal) <= MaxStableSlopeAngle;
        }

        private bool IsStableWithSpecialCases(ref HitStabilityReport stabilityReport, Vector3 velocity)
        {
            if (LedgeAndDenivelationHandling)
            {
                if (stabilityReport.LedgeDetected)
                {
                    if (stabilityReport.IsMovingTowardsEmptySideOfLedge)
                    {
                        Vector3 velocityOnLedgeNormal = Vector3.Project(velocity, stabilityReport.LedgeFacingDirection);
                        if (velocityOnLedgeNormal.magnitude >= MaxVelocityForLedgeSnap)
                        {
                            return false;
                        }
                    }

                    if (stabilityReport.IsOnEmptySideOfLedge && stabilityReport.DistanceFromLedge > MaxStableDistanceFromLedge)
                    {
                        return false;
                    }
                }

                if (LastGroundingStatus.FoundAnyGround && stabilityReport.InnerNormal.sqrMagnitude != 0f && stabilityReport.OuterNormal.sqrMagnitude != 0f)
                {
                    float denivelationAngle = Vector3.Angle(stabilityReport.InnerNormal, stabilityReport.OuterNormal);
                    if (denivelationAngle > MaxStableDenivelationAngle)
                    {
                        return false;
                    }
                    else
                    {
                        denivelationAngle = Vector3.Angle(LastGroundingStatus.InnerGroundNormal, stabilityReport.OuterNormal);
                        if (denivelationAngle > MaxStableDenivelationAngle)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, _characterUp);
            return Vector3.Cross(surfaceNormal, directionRight).normalized;
        }

        public void ForceUnground(float time = 0.1f)
        {
            _mustUnground = true;
            _mustUngroundTimeCounter = time;
        }

        public bool MustUnground()
        {
            return _mustUnground || _mustUngroundTimeCounter > 0f;
        }

        public void ComputeCollisionResolutionForHitBody(
            Vector3 hitNormal,
            Vector3 characterVelocity,
            Vector3 bodyVelocity,
            float characterToBodyMassRatio,
            out Vector3 velocityChangeOnCharacter,
            out Vector3 velocityChangeOnBody)
        {
            velocityChangeOnCharacter = default;
            velocityChangeOnBody = default;

            float bodyToCharacterMassRatio = 1f - characterToBodyMassRatio;
            float characterVelocityMagnitudeOnHitNormal = Vector3.Dot(characterVelocity, hitNormal);
            float bodyVelocityMagnitudeOnHitNormal = Vector3.Dot(bodyVelocity, hitNormal);

            if (characterVelocityMagnitudeOnHitNormal < 0f)
            {
                Vector3 restoredCharacterVelocity = hitNormal * characterVelocityMagnitudeOnHitNormal;
                velocityChangeOnCharacter += restoredCharacterVelocity;
            }

            if (bodyVelocityMagnitudeOnHitNormal > characterVelocityMagnitudeOnHitNormal)
            {
                Vector3 relativeImpactVelocity = hitNormal * (bodyVelocityMagnitudeOnHitNormal - characterVelocityMagnitudeOnHitNormal);
                velocityChangeOnCharacter += relativeImpactVelocity * bodyToCharacterMassRatio;
                velocityChangeOnBody += -relativeImpactVelocity * characterToBodyMassRatio;
            }
        }

        public void GetVelocityFromRigidbodyMovement(Rigidbody interactiveRigidbody, Vector3 atPoint, float deltaTime, out Vector3 linearVelocity, out Vector3 angularVelocity)
        {
            if (deltaTime > 0f)
            {
                linearVelocity = interactiveRigidbody.linearVelocity;
                angularVelocity = interactiveRigidbody.angularVelocity;

                if (interactiveRigidbody.isKinematic)
                {
                    PhysicsMover physicsMover = interactiveRigidbody.GetComponent<PhysicsMover>();
                    if (physicsMover)
                    {
                        linearVelocity = physicsMover.Velocity;
                        angularVelocity = physicsMover.AngularVelocity;
                    }
                }

                if (angularVelocity != Vector3.zero)
                {
                    Vector3 centerOfRotation = interactiveRigidbody.transform.TransformPoint(interactiveRigidbody.centerOfMass);
                    Vector3 centerOfRotationToPoint = atPoint - centerOfRotation;
                    Quaternion rotationFromInteractiveRigidbody = Quaternion.Euler(Mathf.Rad2Deg * angularVelocity * deltaTime);
                    Vector3 finalPointPosition = centerOfRotation + (rotationFromInteractiveRigidbody * centerOfRotationToPoint);
                    linearVelocity += (finalPointPosition - atPoint) / deltaTime;
                }
            }
            else
            {
                linearVelocity = default;
                angularVelocity = default;
            }
        }

        private Rigidbody GetInteractiveRigidbody(Collider onCollider)
        {
            Rigidbody colliderAttachedRigidbody = onCollider.attachedRigidbody;
            if (colliderAttachedRigidbody)
            {
                if (colliderAttachedRigidbody.gameObject.GetComponent<PhysicsMover>())
                {
                    return colliderAttachedRigidbody;
                }

                if (!colliderAttachedRigidbody.isKinematic)
                {
                    return colliderAttachedRigidbody;
                }
            }
            return null;
        }

        public Vector3 GetVelocityForMovePosition(Vector3 fromPosition, Vector3 toPosition, float deltaTime)
        {
            return GetVelocityFromMovement(toPosition - fromPosition, deltaTime);
        }

        public Vector3 GetVelocityFromMovement(Vector3 movement, float deltaTime)
        {
            if (deltaTime <= 0f)
                return Vector3.zero;

            return movement / deltaTime;
        }

        public void SetGroundSolvingActivation(bool stabilitySolvingActive)
        {
            _solveGrounding = stabilitySolvingActive;
        }

        public void SetTransientPosition(Vector3 newPos)
        {
            _transientPosition = newPos;
        }

        public void SetPosition(Vector3 position, bool bypassInterpolation = true)
        {
            _transform.position = position;
            _initialSimulationPosition = position;
            _transientPosition = position;
            Rb.position = position;

            if (bypassInterpolation)
            {
                InitialTickPosition = position;
            }
        }

        public void SetRotation(Quaternion rotation, bool bypassInterpolation = true)
        {
            _transform.rotation = rotation;
            _initialSimulationRotation = rotation;
            _transientRotation = rotation;
            _characterUp = _transientRotation * _cachedWorldUp;
            _characterForward = _transientRotation * _cachedWorldForward;
            _characterRight = _transientRotation * _cachedWorldRight;
            Rb.rotation = rotation;

            if (bypassInterpolation)
            {
                InitialTickRotation = rotation;
            }
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true)
        {
            _transform.SetPositionAndRotation(position, rotation);
            _initialSimulationPosition = position;
            _initialSimulationRotation = rotation;
            _transientPosition = position;
            _transientRotation = rotation;
            _characterUp = _transientRotation * _cachedWorldUp;
            _characterForward = _transientRotation * _cachedWorldForward;
            _characterRight = _transientRotation * _cachedWorldRight;
            Rb.position = position;
            Rb.rotation = rotation;

            if (bypassInterpolation)
            {
                InitialTickPosition = position;
                InitialTickRotation = rotation;
            }
        }

        public KinematicCharacterMotorState GetState()
        {
            KinematicCharacterMotorState state = new KinematicCharacterMotorState();
            state.Position = _transientPosition;
            state.Rotation = _transientRotation;
            state.BaseVelocity = BaseVelocity;
            state.AttachedRigidbodyVelocity = _attachedRigidbodyVelocity;
            state.MustUnground = _mustUnground;
            state.MustUngroundTime = _mustUngroundTimeCounter;
            state.LastMovementIterationFoundAnyGround = LastMovementIterationFoundAnyGround;
            state.GroundingStatus.CopyFrom(GroundingStatus);
            state.AttachedRigidbody = _attachedRigidbody;
            return state;
        }

        public void ApplyState(KinematicCharacterMotorState state, bool bypassInterpolation = true)
        {
            SetPositionAndRotation(state.Position, state.Rotation, bypassInterpolation);
            BaseVelocity = state.BaseVelocity;
            _attachedRigidbodyVelocity = state.AttachedRigidbodyVelocity;
            _mustUnground = state.MustUnground;
            _mustUngroundTimeCounter = state.MustUngroundTime;
            LastMovementIterationFoundAnyGround = state.LastMovementIterationFoundAnyGround;
            GroundingStatus.CopyFrom(state.GroundingStatus);
            _attachedRigidbody = state.AttachedRigidbody;
        }

        public int CharacterCollisionsOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {
            int queryLayers = CollidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                queryLayers = CollidableLayers & StableGroundLayers;
            }

            Vector3 bottom = position + (rotation * _characterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * _characterTransformToCapsuleTopHemi);
            if (inflate != 0f)
            {
                bottom += (rotation * Vector3.down * inflate);
                top += (rotation * Vector3.up * inflate);
            }

            int nbHits = 0;
            int nbUnfilteredHits = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                Capsule.radius + inflate,
                overlappedColliders,
                queryLayers,
                QueryTriggerInteraction.Ignore);

            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                if (overlappedColliders[i] == Capsule)
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        overlappedColliders[i] = overlappedColliders[nbHits];
                    }
                }
            }

            return nbHits;
        }

        public int CharacterOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, LayerMask layers, QueryTriggerInteraction triggerInteraction, float inflate = 0f)
        {
            Vector3 bottom = position + (rotation * _characterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * _characterTransformToCapsuleTopHemi);
            if (inflate != 0f)
            {
                bottom += (rotation * Vector3.down * inflate);
                top += (rotation * Vector3.up * inflate);
            }

            int nbHits = 0;
            int nbUnfilteredHits = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                Capsule.radius + inflate,
                overlappedColliders,
                layers,
                triggerInteraction);

            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                if (overlappedColliders[i] == Capsule)
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        overlappedColliders[i] = overlappedColliders[nbHits];
                    }
                }
            }

            return nbHits;
        }

        public int CharacterCollisionsSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {
            int queryLayers = CollidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                queryLayers = CollidableLayers & StableGroundLayers;
            }

            Vector3 bottom = position + (rotation * _characterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * _characterTransformToCapsuleTopHemi);
            if (inflate != 0f)
            {
                bottom += (rotation * Vector3.down * inflate);
                top += (rotation * Vector3.up * inflate);
            }

            int nbHits = 0;
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                bottom,
                top,
                Capsule.radius + inflate,
                direction,
                hits,
                distance,
                queryLayers,
                QueryTriggerInteraction.Ignore);

            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                RaycastHit hit = hits[i];
                float hitDistance = hit.distance;

                if (hitDistance <= 0f || hit.collider == Capsule)
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        hits[i] = hits[nbHits];
                    }
                }
                else
                {
                    if (hitDistance < closestDistance)
                    {
                        closestHit = hit;
                        closestDistance = hitDistance;
                    }
                }
            }

            return nbHits;
        }

        public int CharacterCollisionsRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, bool acceptOnlyStableGroundLayer = false)
        {
            int queryLayers = CollidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                queryLayers = CollidableLayers & StableGroundLayers;
            }

            int nbHits = 0;
            int nbUnfilteredHits = Physics.RaycastNonAlloc(
                position,
                direction,
                hits,
                distance,
                queryLayers,
                QueryTriggerInteraction.Ignore);

            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                RaycastHit hit = hits[i];
                float hitDistance = hit.distance;

                if (hitDistance <= 0f || hit.collider == Capsule)
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        hits[i] = hits[nbHits];
                    }
                }
                else
                {
                    if (hitDistance < closestDistance)
                    {
                        closestHit = hit;
                        closestDistance = hitDistance;
                    }
                }
            }

            return nbHits;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Disegna la posizione del probe
            Vector3 probeStart = _transform.position + (_transform.rotation * _characterTransformToCapsuleBottomHemi);
            Gizmos.color = Color.red;//controllami
            Gizmos.DrawWireSphere(probeStart, CapsuleRadius * 0.9f);

            // Disegna il ray del ground check
            if (GroundingStatus.FoundAnyGround)
            {
                Gizmos.color = GroundingStatus.IsStableOnGround ? Color.green : Color.red;
                Gizmos.DrawLine(probeStart, GroundingStatus.GroundPoint);
                Gizmos.DrawWireSphere(GroundingStatus.GroundPoint, 0.1f);
            }
            else
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(probeStart, probeStart + Vector3.down * 2f);
            }

            // Stato grounding
            UnityEditor.Handles.Label(_transform.position + Vector3.up * 2f,
                $"Grounded: {GroundingStatus.IsStableOnGround}\n" +
                $"Found: {GroundingStatus.FoundAnyGround}\n" +
                $"Velocity: {Rb.linearVelocity.magnitude:F2}");
        }
    }
}