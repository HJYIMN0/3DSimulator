using UnityEngine;
using System.Collections.Generic;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RbPlayerController : MonoBehaviour
    {
        [Header("References")]
        public SprintStaminaSystem StaminaSystem;
        public Transform MeshRoot;

        [Header("Movement")]
        public float MoveSpeed = 6f;
        public float SprintMultiplier = 1.5f;
        public float CrouchMultiplier = 0.5f;
        public float AirControl = 0.3f;
        public float GroundDrag = 6f;
        public float AirDrag = 0.5f;

        [Header("Jumping")]
        public float JumpForce = 8f;
        public float JumpCooldown = 0.2f;

        [Header("Ground Check")]
        public float GroundCheckDistance = 0.3f;
        public LayerMask GroundLayers = -1;

        [Header("Slope Handling")]
        public float MaxSlopeAngle = 45f;

        [Header("Capsule")]
        public float CapsuleRadius = 0.3f;
        public float StandingHeight = 2f;
        [Tooltip("Lascia 0 per usare StandingHeight/2 automaticamente")]
        public float CrouchHeight = 0f;

        [Header("Pivot Position")]
        public bool Pivot_centered;

        // Calcolato a runtime
        private float _effectiveCrouchHeight;

        [Header("Orientation")]
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;
        public float RotationSpeed = 10f;

        [Header("Debug")]
        public bool ShowDebug = true;

        // State
        private CharacterState _currentState = CharacterState.Default;
        private bool _isGrounded;
        private bool _isCrouching;
        private bool _isSprinting;
        private float _lastJumpTime;
        private Vector3 _moveInput;
        private Vector3 _lookDirection;
        private RaycastHit _slopeHit;
        private Vector3 _originalMeshScale;

        // Components
        private Rigidbody _rb;
        private CapsuleCollider _capsule;

        // Constants
        private const float GRAVITY = -30f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();

            _rb.mass = 70f;
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0f;
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ;
            _rb.isKinematic = false;

            _effectiveCrouchHeight = CrouchHeight > 0f ? CrouchHeight : StandingHeight / 2f;

            _capsule.direction = 1;
            _capsule.radius = CapsuleRadius;
            _capsule.height = StandingHeight;
            _capsule.center = Vector3.up * (StandingHeight / 2f);

            if (MeshRoot != null)
                _originalMeshScale = MeshRoot.localScale;
            if (ShowDebug) Debug.Log($"[INIT] constraints={_rb.constraints} | isKinematic={_rb.isKinematic} | useGravity={_rb.useGravity}");
        }

        private void FixedUpdate()
        {
            CheckGround();
            ApplyGravity();
            HandleMovement();
            ApplyDrag();
            if (ShowDebug) Debug.Log($"[FRAME] pos.y={transform.position.y:F4} | rb.pos.y={_rb.position.y:F4}");
        }

        private void Update()
        {
            HandleRotation();
        }

        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            Vector3 moveInput = new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward);
            moveInput = Vector3.ClampMagnitude(moveInput, 1f);

            if (ShowDebug && moveInput.sqrMagnitude > 0.01f)
                Debug.Log($"[INPUT] Move raw: ({inputs.MoveAxisRight:F2}, {inputs.MoveAxisForward:F2})");
            if (ShowDebug && inputs.JumpDown)
                Debug.Log("[INPUT] Jump pressed");
            if (ShowDebug && inputs.SprintHeld)
                Debug.Log("[INPUT] Sprint held");

            Vector3 cameraForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Vector3.up).normalized;
            if (cameraForward.sqrMagnitude == 0f)
                cameraForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Vector3.up).normalized;

            Quaternion cameraRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
            _moveInput = cameraRotation * moveInput;

            switch (OrientationMethod)
            {
                case OrientationMethod.TowardsCamera:
                    _lookDirection = cameraForward;
                    break;
                case OrientationMethod.TowardsMovement:
                    if (_moveInput.sqrMagnitude > 0.01f)
                        _lookDirection = _moveInput.normalized;
                    break;
            }

            // Jump
            if (inputs.JumpDown && CanJump())
            {
                Jump();
            }

            // Crouch
            if (inputs.CrouchDown && !_isCrouching)
            {
                StartCrouch();
            }
            else if (inputs.CrouchUp && _isCrouching)
            {
                TryStandUp();
            }

            // Sprint
            bool canSprint = !_isCrouching && _isGrounded && _moveInput.sqrMagnitude > 0.1f;
            if (inputs.SprintHeld && canSprint && StaminaSystem != null && StaminaSystem.CanSprint)
            {
                _isSprinting = true;
                StaminaSystem.StartDrain();
            }
            else
            {
                _isSprinting = false;
                if (StaminaSystem != null)
                    StaminaSystem.StopDrain();
            }
        }

        private void CheckGround()
        {
            if (Time.time - _lastJumpTime < 0.15f)
            {
                _isGrounded = false;
                return;
            }

            Vector3 spherePos = transform.position + Vector3.up * _capsule.center.y;
            float castDistance = (_capsule.height / 2f) - _capsule.radius + GroundCheckDistance;

            _isGrounded = Physics.SphereCast(
                spherePos,
                _capsule.radius * 0.9f,
                Vector3.down,
                out _slopeHit,
                castDistance,
                GroundLayers,
                QueryTriggerInteraction.Ignore
                );

            if (ShowDebug)
                Debug.Log($"[GROUND] isGrounded={_isGrounded} | probeStart=({spherePos.x:F2},{spherePos.y:F2},{spherePos.z:F2}) | dist={GroundCheckDistance} | layers={GroundLayers.value}" + (_isGrounded ? $" | hit={_slopeHit.collider.name}" : ""));
        }

        private void ApplyGravity()
        {
            if (!_isGrounded)
            {
                _rb.AddForce(Vector3.up * GRAVITY, ForceMode.Acceleration);
                if (ShowDebug)
                    Debug.Log($"[GRAVITY] Applied {GRAVITY} | vel.y={_rb.linearVelocity.y:F2}");
            }
        }

        private void HandleMovement()
        {
            float targetSpeed = MoveSpeed;

            if (_isCrouching)
                targetSpeed *= CrouchMultiplier;
            else if (_isSprinting)
                targetSpeed *= SprintMultiplier;

            Vector3 targetVelocity = _moveInput * targetSpeed;

            if (ShowDebug)
                Debug.Log($"[MOVE] pos=({transform.position.x:F2},{transform.position.y:F2},{transform.position.z:F2}) | vel=({_rb.linearVelocity.x:F2},{_rb.linearVelocity.y:F2},{_rb.linearVelocity.z:F2}) | target=({targetVelocity.x:F2},{targetVelocity.z:F2}) | grounded={_isGrounded} | kinematic={_rb.isKinematic}");

            if (_isGrounded)
            {
                if (IsOnSlope())
                {
                    targetVelocity = Vector3.ProjectOnPlane(targetVelocity, _slopeHit.normal).normalized * targetSpeed;
                }

                Vector3 velocityChange = targetVelocity - new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                _rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            else
            {
                Vector3 currentHorizontalVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                Vector3 airVelocity = Vector3.Lerp(currentHorizontalVel, targetVelocity, AirControl * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector3(airVelocity.x, _rb.linearVelocity.y, airVelocity.z);
            }
        }

        private void ApplyDrag()
        {
            float drag = _isGrounded ? GroundDrag : AirDrag;
            Vector3 horizontalVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            Vector3 dragForce = -horizontalVel * drag;
            _rb.AddForce(dragForce, ForceMode.Acceleration);
        }

        private void HandleRotation()
        {
            if (_lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_lookDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
        }

        private bool CanJump()
        {
            if (!_isGrounded) return false;
            if (Time.time - _lastJumpTime < JumpCooldown) return false;
            if (StaminaSystem != null && !StaminaSystem.CanJump) return false;
            return true;
        }

        private void Jump()
        {
            if (StaminaSystem != null && !StaminaSystem.TryConsumeJumpStamina())
                return;

            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
            _lastJumpTime = Time.time;
            _isGrounded = false;
        }

        private void StartCrouch()
        {
            _isCrouching = true;
            _capsule.height = _effectiveCrouchHeight;
            _capsule.center = new Vector3(0f, _effectiveCrouchHeight / 2f, 0f);

            if (MeshRoot != null)
            {
                if (Pivot_centered){
                    float heightDiff = StandingHeight - _effectiveCrouchHeight;
                    MeshRoot.localPosition = new Vector3(MeshRoot.localPosition.x, -heightDiff / 2f, MeshRoot.localPosition.z);
                } else {
                    float ratio = _effectiveCrouchHeight / StandingHeight;
                    MeshRoot.localScale = new Vector3(_originalMeshScale.x, _originalMeshScale.y * ratio, _originalMeshScale.z);
                } 
            }
        }

        private void TryStandUp()
        {
            Vector3 checkPos = transform.position + Vector3.up * (StandingHeight / 2f);
            if (!Physics.CheckCapsule(
                checkPos - Vector3.up * (StandingHeight / 2f - _capsule.radius),
                checkPos + Vector3.up * (StandingHeight / 2f - _capsule.radius),
                _capsule.radius,
                GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                _isCrouching = false;
                _capsule.height = StandingHeight;
                _capsule.center = Vector3.up * (StandingHeight / 2f);

                if (MeshRoot != null)
                {
                    MeshRoot.localScale = _originalMeshScale;
                }

            }
        }

        private bool IsOnSlope()
        {
            if (!_isGrounded) return false;
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle > 0.1f && angle < MaxSlopeAngle;
        }

        public void OnStateChanged(CharacterState newState)
        {
            _currentState = newState;
        }

        public bool IsGrounded => _isGrounded;
        public Vector3 Velocity => _rb.linearVelocity;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Vector3 spherePos = transform.position + Vector3.up * (_capsule.radius - 0.05f);
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(spherePos + Vector3.down * GroundCheckDistance, _capsule.radius * 0.9f);
        }
    }
}