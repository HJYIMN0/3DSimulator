using UnityEngine;

namespace Character
{
    /// <summary>
    /// Rigidbody-based character controller
    /// Sostituisce KinematicCharacterMotor con fisica Unity nativa
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerRigidbodyController : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody Rb;
        public CapsuleCollider Capsule;

        [Header("Ground Detection")]
        [Tooltip("Distanza raycast per ground detection")]
        public float groundCheckDistance = 0.2f;

        [Tooltip("Offset sfera per ground check")]
        public float groundCheckRadius = 0.3f;

        [Tooltip("Layer del terreno")]
        public LayerMask groundLayers = -1;

        [Header("Slope Settings")]
        [Tooltip("Angolo massimo slope stabile")]
        [Range(0f, 89f)]
        public float maxSlopeAngle = 45f;

        [Header("Step Settings")]
        [Tooltip("Altezza massima step climbable")]
        public float maxStepHeight = 0.3f;

        [Tooltip("Distanza forward per step detection")]
        public float stepCheckDistance = 0.5f;

        // Grounding state
        public bool IsGrounded { get; private set; }
        public bool IsStableOnGround { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        public Vector3 GroundPoint { get; private set; }
        public float GroundDistance { get; private set; }

        // Cached
        private Vector3 _lastGroundNormal;
        private float _capsuleHeight;
        private float _capsuleRadius;

        private void Awake()
        {
            if (Rb == null)
                Rb = GetComponent<Rigidbody>();

            if (Capsule == null)
                Capsule = GetComponent<CapsuleCollider>();

            // Configura Rigidbody
            Rb.isKinematic = false;
            Rb.useGravity = true;
            Rb.constraints = RigidbodyConstraints.FreezeRotation; // Blocca rotazione fisica
            Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;

            _capsuleHeight = Capsule.height;
            _capsuleRadius = Capsule.radius;

            GroundNormal = Vector3.up;
            _lastGroundNormal = Vector3.up;
        }

        private void FixedUpdate()
        {
            CheckGrounding();
        }

        /// <summary>
        /// Ground detection con spherecast
        /// </summary>
        private void CheckGrounding()
        {
            Vector3 sphereCenter = transform.position + Vector3.up * (_capsuleRadius + 0.01f);
            float checkDistance = groundCheckDistance + _capsuleRadius;

            if (Physics.SphereCast(
                sphereCenter,
                groundCheckRadius,
                Vector3.down,
                out RaycastHit hit,
                checkDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore))
            {
                IsGrounded = true;
                GroundNormal = hit.normal;
                GroundPoint = hit.point;
                GroundDistance = hit.distance - _capsuleRadius;

                // Check slope angle
                float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                IsStableOnGround = slopeAngle <= maxSlopeAngle;

                _lastGroundNormal = hit.normal;
            }
            else
            {
                IsGrounded = false;
                IsStableOnGround = false;
                GroundNormal = Vector3.up;
                GroundPoint = Vector3.zero;
                GroundDistance = checkDistance;
            }
        }

        /// <summary>
        /// Applica velocità al rigidbody
        /// </summary>
        public void SetVelocity(Vector3 velocity)
        {
            Rb.linearVelocity = velocity;
        }

        /// <summary>
        /// Aggiungi velocità (per jump, additive forces)
        /// </summary>
        public void AddVelocity(Vector3 velocity)
        {
            Rb.linearVelocity += velocity;
        }

        /// <summary>
        /// Proietta direzione su slope
        /// </summary>
        public Vector3 ProjectOnSlope(Vector3 direction)
        {
            if (!IsStableOnGround)
                return direction;

            return Vector3.ProjectOnPlane(direction, GroundNormal).normalized;
        }

        /// <summary>
        /// Check se c'è step davanti
        /// </summary>
        public bool CheckStep(Vector3 moveDirection, out Vector3 stepUpPosition)
        {
            stepUpPosition = Vector3.zero;

            if (!IsGrounded)
                return false;

            // Raycast forward a metà altezza
            Vector3 rayOrigin = transform.position + Vector3.up * (maxStepHeight * 0.5f);

            if (Physics.Raycast(
                rayOrigin,
                moveDirection,
                out RaycastHit forwardHit,
                stepCheckDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore))
            {
                // Raycast down da sopra obstacle
                Vector3 stepCheckOrigin = forwardHit.point + Vector3.up * (maxStepHeight + 0.1f);

                if (Physics.Raycast(
                    stepCheckOrigin,
                    Vector3.down,
                    out RaycastHit stepHit,
                    maxStepHeight + 0.2f,
                    groundLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    float stepHeightDifference = stepHit.point.y - transform.position.y;

                    // Valida step height
                    if (stepHeightDifference > 0.05f && stepHeightDifference <= maxStepHeight)
                    {
                        stepUpPosition = stepHit.point + Vector3.up * 0.1f;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Forza unground (per jump)
        /// </summary>
        public void ForceUnground()
        {
            IsGrounded = false;
            IsStableOnGround = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            // Ground check sphere
            Vector3 sphereCenter = transform.position + Vector3.up * (_capsuleRadius + 0.01f);
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(sphereCenter, groundCheckRadius);

            // Ground normal
            if (IsGrounded)
            {
                Gizmos.color = IsStableOnGround ? Color.green : Color.yellow;
                Gizmos.DrawRay(GroundPoint, GroundNormal * 2f);
            }
        }
    }
}