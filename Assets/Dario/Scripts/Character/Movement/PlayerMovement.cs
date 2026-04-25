using UnityEngine;
using KinematicCharacterController;
using System.Collections.Generic;

namespace Character
{
    public class PlayerMovement : MonoBehaviour, ICharacterController
    {
        [Header("References")]
        public KinematicCharacterMotor Motor;
        public SprintStaminaSystem StaminaSystem;
        public Transform MeshRoot;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;

        [Header("Speed Multipliers")]
        [Range(0f, 1f)] public float CrouchSpeedMultiplier = 0.5f;
        [Range(1f, 3f)] public float SprintSpeedMultiplier = 1.5f;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float Drag = 0.1f;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0.1f;
        public float JumpPostGroundingGraceTime = 0.1f;

        [Header("Crouch ")]
        public float CrouchedCapsuleHeight;
        public float CrouchedCapsuleRadius;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);

        // State
        private CharacterState _currentState = CharacterState.Default;
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;

        // Jumping
        private bool _jumpRequested;
        private bool _jumpConsumed;
        private bool _jumpedThisFrame;
        private float _timeSinceJumpRequested;
        private float _timeSinceLastAbleToJump;

        // Crouch
        private bool _shouldBeCrouching;
        private bool _isCrouching;
        private float _defaultCapsuleHeight;
        private float _defaultCapsuleRadius;
        private Vector3 _originalMeshScale;

        // Sprint
        private bool _sprinting;

        // Misc
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private Collider[] _probedColliders = new Collider[8];

        private void Awake()
        {
            Motor.CharacterController = this;

            // Store default dimensions
            _defaultCapsuleHeight = Motor.Capsule.height;
            _defaultCapsuleRadius = Motor.Capsule.radius;
            CrouchedCapsuleHeight = Motor.capsuleHeight / 2;
            CrouchedCapsuleRadius = Motor.capsuleRadius;

            if (MeshRoot != null)
                _originalMeshScale = MeshRoot.localScale;
        }

        public void OnStateChanged(CharacterState newState)
        {
            _currentState = newState;
        }

        /// <summary>
        /// This is called every frame by Player in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);


            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            _moveInputVector = cameraPlanarRotation * moveInputVector;
            switch (_currentState)
            {
                case CharacterState.Default:
                    {
                        switch (OrientationMethod)
                        {
                            case OrientationMethod.TowardsCamera:
                                _lookInputVector = cameraPlanarDirection;
                                break;
                            case OrientationMethod.TowardsMovement:
                                _lookInputVector = _moveInputVector.normalized;
                                break;
                        }

                        // Jump input
                        if (inputs.JumpDown)
                        {
                            if (StaminaSystem == null || StaminaSystem.CanJump)
                            {
                                _timeSinceJumpRequested = 0f;
                                _jumpRequested = true;
                            }
                            else
                            {
                                Debug.Log("Not enough stamina to jump!");
                            }
                        }

                        // Crouch input
                        if (inputs.CrouchDown)
                        {
                            _shouldBeCrouching = true;

                            if (!_isCrouching)
                            {
                                _isCrouching = true;
                                Motor.SetCapsuleDimensions(CrouchedCapsuleRadius, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);

                                if (MeshRoot != null)
                                    MeshRoot.localScale = new Vector3(_originalMeshScale.x, _originalMeshScale.y * 0.5f, _originalMeshScale.z);
                            }
                        }
                        else if (inputs.CrouchUp)
                        {
                            _shouldBeCrouching = false;
                        }

                        // Sprint input
                        bool canSprint = !_isCrouching
                                       && Motor.GroundingStatus.IsStableOnGround
                                       && !_jumpRequested
                                       && _moveInputVector.sqrMagnitude > 0.1f;

                        if (inputs.SprintHeld && canSprint && StaminaSystem != null && StaminaSystem.CanSprint)
                        {
                            _sprinting = true;
                            StaminaSystem.StartDrain();
                        }
                        else
                        {
                            _sprinting = false;
                            if (StaminaSystem != null)
                                StaminaSystem.StopDrain();
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (_currentState)
            {
                case CharacterState.Default:
                    {
                        if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                        {
                            Vector3 smoothedLookDirection = Vector3.Slerp(
                                Motor.CharacterForward,
                                _lookInputVector,
                                1 - Mathf.Exp(-OrientationSharpness * deltaTime)
                            ).normalized;

                            currentRotation = Quaternion.LookRotation(smoothedLookDirection, Motor.CharacterUp);
                        }
                        Vector3 currentUp = (currentRotation * Vector3.up);
                        if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                        {
                            // Rotate from current up to invert gravity
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                        else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                        {
                            if (Motor.GroundingStatus.IsStableOnGround)
                            {
                                Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.capsuleRadius);

                                Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                                // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                                Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.capsuleRadius));
                            }
                            else
                            {
                                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                            }
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (_currentState)
            {
                case CharacterState.Default:
                    HandleDefaultMovement(ref currentVelocity, deltaTime);
                    break;
            }
        }

        private void HandleDefaultMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                _timeSinceLastAbleToJump = 0f;
                _jumpConsumed = false;

                // Calculate target velocity with speed modifiers
                float currentMaxSpeed = MaxStableMoveSpeed;

                if (_isCrouching)
                    currentMaxSpeed *= CrouchSpeedMultiplier;
                else if (_sprinting)
                    currentMaxSpeed *= SprintSpeedMultiplier;

                Vector3 targetMovementVelocity = _moveInputVector * currentMaxSpeed;
                currentVelocity = Vector3.Lerp(
                    currentVelocity,
                    targetMovementVelocity,
                    1 - Mathf.Exp(-StableMovementSharpness * deltaTime)
                );
            }
            else
            {
                // Air movement
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    Vector3 addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime;
                    Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                    // Limit air velocity from inputs
                    if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                    {
                        // clamp addedVel to make total vel not exceed max vel on inputs plane
                        Vector3 newTotal = Vector3.ClampMagnitude(
                            currentVelocityOnInputsPlane + addedVelocity,
                            MaxAirMoveSpeed
                        );

                        addedVelocity = newTotal - currentVelocityOnInputsPlane;
                    }
                    else
                    {
                        // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                        if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                        {
                            addedVelocity = Vector3.ProjectOnPlane(
                                addedVelocity,
                                currentVelocityOnInputsPlane.normalized
                            );
                        }
                    }
                    // Prevent air-climbing sloped walls
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                        {
                            Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                        }
                    }

                    currentVelocity += addedVelocity;
                }

                // Gravity
                currentVelocity += Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }

            HandleJumping(ref currentVelocity, deltaTime);

            // Additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }
        }

        private void HandleJumping(ref Vector3 currentVelocity, float deltaTime)
        {
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            _timeSinceLastAbleToJump += deltaTime;

            if (_jumpRequested)
            {
                if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
                {
                    bool hasStamina = true;
                    if (StaminaSystem != null)
                    {
                        hasStamina = StaminaSystem.TryConsumeJumpStamina();
                    }

                    if (hasStamina)
                    {
                        // Calculate jump direction before ungrounding
                        Vector3 jumpDirection = Motor.CharacterUp;
                        if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                        {
                            jumpDirection = Motor.GroundingStatus.GroundNormal;
                        }

                        Motor.ForceUnground();

                        // Add to the return velocity and reset jump state
                        currentVelocity += (jumpDirection * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                        currentVelocity += (_moveInputVector * JumpScalableForwardSpeed);

                        _jumpRequested = false;
                        _jumpConsumed = true;
                        _jumpedThisFrame = true;
                    }
                    else
                    {
                        // No stamina = jump request cancelled
                        _jumpRequested = false;
                    }
                }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime) { }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            switch (_currentState)
            {
                case CharacterState.Default:
                    {
                        // Jump grace period
                        if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                            _jumpRequested = false;

                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            if (!_jumpedThisFrame)
                                _jumpConsumed = false;
                            _timeSinceLastAbleToJump = 0f;
                        }

                        // Handle uncrouching
                        if (_isCrouching && !_shouldBeCrouching)
                        {
                            // Check if space above to stand up
                            Motor.SetCapsuleDimensions(_defaultCapsuleRadius, _defaultCapsuleHeight, _defaultCapsuleHeight * 0.5f);

                            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, Motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
                            {
                                // Obstruction detected, stay crouched
                                Motor.SetCapsuleDimensions(CrouchedCapsuleRadius, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                            }
                            else
                            {
                                // No obstruction, stand up
                                if (MeshRoot != null)
                                    MeshRoot.localScale = _originalMeshScale;
                                //MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                                _isCrouching = false;
                            }
                        }
                        break;
                    }
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
                OnLanded(); 

            if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
                OnLeaveStableGround(); 
        }
        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0)
            {
                return true;
            }

            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }

            return true;
        }
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport stabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport stabilityReport) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        protected void OnLanded(){}
        protected void OnLeaveStableGround(){}
        public void OnDiscreteCollisionDetected(Collider hitCollider){}
        public void AddVelocity(Vector3 velocity)
        {
            switch (_currentState)
            {
                case CharacterState.Default:
                    {
                        _internalVelocityAdd += velocity;
                        break;
                    }
            }
        }
    }
}