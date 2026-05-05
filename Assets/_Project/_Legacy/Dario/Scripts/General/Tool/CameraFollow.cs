using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Sensibilità")]
    [SerializeField][Range(0.1f, 10f)] private float sensX = 1f;
    [SerializeField][Range(0.1f, 10f)] private float sensY = 1f;

    [Header("Riferimenti")]
    [SerializeField] private Transform orientation; // il corpo/player

    [Header("Input System")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Limiti di Rotazione")]
    [Tooltip("Limite minimo (verso l'alto) e massimo (verso il basso) della rotazione verticale.")]
    [SerializeField][Range(-90f, -0.1f)] private float minVerticalAngle = -80f;
    [SerializeField][Range(0.1f, 90f)] private float maxVerticalAngle = 80f;

    private float xRotation;
    private float yRotation;

    private void OnEnable()
    {
        lookAction.action.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        lookAction.action.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        float mouseX = lookInput.x * sensX;
        float mouseY = lookInput.y * sensY;

        // Aggiorna rotazioni cumulative
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        // Ruota solo il corpo sul piano orizzontale (asse Y)
        orientation.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        // Ruota solo la camera in locale sull'asse X
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
