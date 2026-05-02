using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;

namespace Character
{
    /// <summary>
    /// Manages all player input using NEW Input System.
    /// Refactored from old Input.GetAxis to InputSystem.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("References")]
        public PlayerManager PlayerManager;
        public CameraController CharacterCamera;

        [Header("Settings")]
        public bool fixedDistance = false;

        [Header("Inventory Settings")]
        [Tooltip("Use scroll wheel for inventory instead of camera zoom")]
        public bool scrollForInventory = true;

        [Tooltip("Minimum hold time to activate throw (seconds)")]
        public float throwHoldThreshold = 0.15f;

        [Tooltip("Maximum charge time for throw (seconds)")]
        public float maxChargeTime = 2f;
        [Header("Debug")]
        public bool showInputDebug = false;

        // Input references 
        private PlayerInputActions inputActions;

        // Input values
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float scrollInput;
        private bool jumpPressed;
        private bool crouchPressed;
        private bool crouchReleased;
        private bool sprintHeld;
        private bool interactPressed;

        // Drop/Throw tracking
        private bool dropHeld = false;
        private float dropHoldTime = 0f;

        // Inventory inputs
        private bool item1Pressed;
        private bool item2Pressed;
        private bool item3Pressed;

        // UI charge bar reference (assigned when UI created)
        public ThrowChargeUI throwChargeUI;

        private void Awake()
        {
            inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            inputActions.Enable();

            // Movement inputs
            inputActions.Player.Jump.performed += ctx => jumpPressed = true;
            inputActions.Player.Crouch.performed += ctx => crouchPressed = true;
            inputActions.Player.Crouch.canceled += ctx => crouchReleased = true;

            // Interaction input
            inputActions.Player.Interact.performed += ctx => {
                interactPressed = true;
                if (showInputDebug) Debug.Log("INPUT: Interact");
            };

            // Drop/Throw input - track press and release
            inputActions.Player.Drop.started += ctx => {
                dropHeld = true;
                dropHoldTime = 0f;

                if (showInputDebug) Debug.Log("INPUT: Drop started");
            };

            inputActions.Player.Drop.canceled += ctx => {
                if (dropHeld)
                {
                    HandleDropRelease();
                    dropHeld = false;
                }
            };

            // Inventory slot inputs
            inputActions.Player.Item1.performed += ctx => {
                item1Pressed = true;
                if (showInputDebug) Debug.Log("INPUT: Item1");
            };

            inputActions.Player.Item2.performed += ctx => {
                item2Pressed = true;
                if (showInputDebug) Debug.Log("INPUT: Item2");
            };

            inputActions.Player.Item3.performed += ctx => {
                item3Pressed = true;
                if (showInputDebug) Debug.Log("INPUT: Item3");
            };
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Setup player manager reference
            if (PlayerManager == null)
                PlayerManager = FindFirstObjectByType<PlayerManager>(); 

            // Tell camera to follow transform
            if (CharacterCamera != null && PlayerManager != null)
            {
                CharacterCamera.SetFollowTransform(PlayerManager.CameraFollowPoint);

                // Ignore the character's collider(s) for camera obstruction checks
                CharacterCamera.IgnoredColliders.Clear();
                CharacterCamera.IgnoredColliders.AddRange(PlayerManager.GetComponentsInChildren<Collider>());
            }
        }

        private void Update()
        {
            // Read input values 
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            lookInput = inputActions.Player.Look.ReadValue<Vector2>();
            scrollInput = inputActions.Player.Scroll.ReadValue<float>();
            sprintHeld = inputActions.Player.Sprint.IsPressed();

            // Track drop hold time
            if (dropHeld)
            {
                dropHoldTime += Time.deltaTime;

                // Clamp to max charge time
                if (dropHoldTime > maxChargeTime)
                    dropHoldTime = maxChargeTime;

                // Update UI charge bar
                UpdateThrowChargeUI();
            }

            // Re-lock cursor if clicked
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            HandleCharacterInput();
            HandleInventoryInput();
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            if (CharacterCamera.RotateWithPhysicsMover &&
                PlayerManager.Movement.Motor.AttachedRigidbody != null)
            {
                PhysicsMover mover = PlayerManager.Movement.Motor.AttachedRigidbody.GetComponent<PhysicsMover>();
                if (mover != null)
                {
                    CharacterCamera.PlanarDirection = mover.RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
                    CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, PlayerManager.Movement.Motor.CharacterUp).normalized;
                }
            }

            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            Vector3 lookInputVector = new Vector3(lookInput.x, lookInput.y, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Scroll behavior: inventory OR camera zoom
            float finalScrollInput = 0f;

            if (!scrollForInventory)
            {
                // Use scroll for camera zoom
                finalScrollInput = fixedDistance ? 0 : -scrollInput;
            }
            // else: scroll used for inventory (handled in HandleInventoryInput)

            CharacterCamera.UpdateWithInput(Time.deltaTime, finalScrollInput, lookInputVector);

            // Handle toggling zoom level (right click)
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ?
                    CharacterCamera.DefaultDistance : 0f;
            }
        }

        private void HandleCharacterInput()
        {
            if (PlayerManager == null || PlayerManager.Movement == null) return;

            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Camera
            characterInputs.CameraRotation = CharacterCamera.Transform.rotation;

            // Movement 
            characterInputs.MoveAxisForward = moveInput.y;
            characterInputs.MoveAxisRight = moveInput.x;

            // Actions
            characterInputs.JumpDown = jumpPressed;
            characterInputs.CrouchDown = crouchPressed;
            characterInputs.CrouchUp = crouchReleased;
            characterInputs.SprintHeld = sprintHeld;
            characterInputs.Interact = interactPressed;

            // Apply inputs to character
            PlayerManager.Movement.SetInputs(ref characterInputs);

            if (interactPressed && PlayerManager.Interaction != null)
            {
                if (showInputDebug) Debug.Log(" INPUT: Calling TryInteract()"); //  Debug
                PlayerManager.Interaction.TryInteract();
            }
            // Reset one-frame inputs
            jumpPressed = false;
            crouchPressed = false;
            crouchReleased = false;
            interactPressed = false;
        }

        private void HandleInventoryInput()
        {
            if (PlayerManager == null || PlayerManager.Inventory == null) return;

            // === SCROLL WHEEL: Cycle through slots ===
            if (scrollForInventory && Mathf.Abs(scrollInput) > 0.1f)
            {
                int direction = scrollInput > 0 ? 1 : -1; // Up = next, Down = previous
                PlayerManager.Inventory.CycleEquippedItem(direction);

                if (showInputDebug)
                    Debug.Log($"Scroll inventory: {direction}");
            }

            // === DIRECT SLOT SELECT (1/2/3) ===
            if (item1Pressed)
            {
                PlayerManager.Inventory.EquipItem(0);
                item1Pressed = false;
            }

            if (item2Pressed)
            {
                PlayerManager.Inventory.EquipItem(1);
                item2Pressed = false;
            }

            if (item3Pressed)
            {
                PlayerManager.Inventory.EquipItem(2);
                item3Pressed = false;
            }
        }


        /// <summary>
        /// Handle drop button release - determine if drop or throw
        /// </summary>
        private void HandleDropRelease()
        {
            if (PlayerManager == null || PlayerManager.Inventory == null) return;

            bool isThrow = dropHoldTime >= throwHoldThreshold;
            float chargePercent = Mathf.Clamp01(dropHoldTime / maxChargeTime);

            // Get throw direction from camera
            Vector3 throwDirection = CharacterCamera.Transform.forward;

            if (showInputDebug)
            {
                Debug.Log($"Drop released: isThrow={isThrow}, charge={chargePercent:P0}, holdTime={dropHoldTime:F2}s");
            }

            // Execute drop or throw
            PlayerManager.Inventory.DropItem(isThrow, chargePercent, throwDirection);

            // Hide UI charge bar
            HideThrowChargeUI();

            dropHoldTime = 0f;
        }

        /// <summary>
        /// Update throw charge UI while holding Q
        /// </summary>
        private void UpdateThrowChargeUI()
        {
            if (throwChargeUI == null) return;

            if (dropHoldTime >= throwHoldThreshold)
            {
                // Show charge bar
                if (!throwChargeUI.gameObject.activeSelf)
                    throwChargeUI.Show();

                float chargePercent = Mathf.Clamp01(dropHoldTime / maxChargeTime);
                throwChargeUI.UpdateCharge(chargePercent);
            }
        }

        /// <summary>
        /// Hide throw charge UI
        /// </summary>
        private void HideThrowChargeUI()
        {
            if (throwChargeUI != null)
                throwChargeUI.Hide();
        }
    }
}
