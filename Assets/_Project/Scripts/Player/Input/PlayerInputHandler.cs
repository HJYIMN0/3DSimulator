using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    /// <summary>
    /// Manages all player input using NEW Input System.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("References")]
        public PlayerManager PlayerManager;
        public CameraController CharacterCamera;

        [Header("interaction")]
        [SerializeField] private Interactor interactor;

        [Header("Settings")]
        public bool fixedDistance = false;

        [Header("Carry")]
        [SerializeField] private PlayerCarryController carryController;

        [Header("Carry Drop/Throw")]
        [SerializeField] private float throwHoldThreshold = 0.2f;

        private bool dropHeld;
        private float dropHoldTime;

        [Header("Debug")]
        public bool showInputDebug = false;

        // Input references 
        private PlayerInputActions inputActions;

        // Input values
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool jumpPressed;
        private bool crouchPressed;
        private bool crouchReleased;
        private bool sprintHeld;
        private bool interactPressed;

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
            inputActions.Player.Interact.performed += ctx =>
            {
                interactPressed = true;
                if (showInputDebug) Debug.Log("INPUT: Interact");
            };

            inputActions.Player.Drop.started += ctx =>
            {
                dropHeld = true;
                dropHoldTime = 0f;

                if (showInputDebug)
                    Debug.Log("INPUT: Drop/Throw started");
            };

            inputActions.Player.Drop.canceled += ctx =>
            {
                if (!dropHeld)
                    return;

                if (dropHoldTime >= throwHoldThreshold)
                {
                    if (showInputDebug)
                        Debug.Log("INPUT: Throw carried item");

                    carryController?.ThrowCarriedItem();
                }
                else
                {
                    if (showInputDebug)
                        Debug.Log("INPUT: Drop carried item");

                    carryController?.DropCarriedItem();
                }

                dropHeld = false;
                dropHoldTime = 0f;
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
                PlayerManager = FindAnyObjectByType<PlayerManager>();

            if (CharacterCamera == null)
                CharacterCamera = FindAnyObjectByType<CameraController>();

            if (interactor == null && PlayerManager != null)
                interactor = PlayerManager.GetComponentInChildren<Interactor>();

            if (interactor == null)
                interactor = FindAnyObjectByType<Interactor>();

            if (carryController == null && PlayerManager != null)
                carryController = PlayerManager.GetComponent<PlayerCarryController>();

            if (carryController == null)
                carryController = FindAnyObjectByType<PlayerCarryController>();

            // Tell camera to follow transform questa cosa non deve stare qui!!!!!!!!!!!!!!!!!!
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
            sprintHeld = inputActions.Player.Sprint.IsPressed();

            // Re-lock cursor if clicked
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (dropHeld)
            {
                dropHoldTime += Time.deltaTime;
            }

            HandleCharacterInput();

        }

        private void LateUpdate()
        {
            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            if (CharacterCamera == null)
                return;

            Vector3 lookInputVector = new Vector3(lookInput.x, lookInput.y, 0f);

            if (Cursor.lockState != CursorLockMode.Locked)
                lookInputVector = Vector3.zero;

            float finalScrollInput = 0f;

            CharacterCamera.UpdateWithInput(Time.deltaTime, finalScrollInput, lookInputVector);

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f)
                    ? CharacterCamera.DefaultDistance
                    : 0f;
            }
        }

        private void HandleCharacterInput()
        {
            if (PlayerManager == null || PlayerManager.Movement == null) return;
            if (CharacterCamera == null) return;
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

            if (interactPressed && interactor != null)
            {
                if (showInputDebug) Debug.Log("INPUT: Calling Interactor.TryInteract()");
                interactor.TryInteract();
            }
            // Reset one-frame inputs
            jumpPressed = false;
            crouchPressed = false;
            crouchReleased = false;
            interactPressed = false;
        }

    }
}
