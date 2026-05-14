using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    [Header("Impostazioni Movimento")]
    [Tooltip("Velocità di movimento normale")]
    public float baseSpeed = 10f;
    [Tooltip("Moltiplicatore di velocità quando tieni premuto Shift")]
    public float sprintMultiplier = 3f;

    [Header("Impostazioni Visuale")]
    [Tooltip("Sensibilità del mouse")]
    public float lookSensitivity = 2f;

    // Variabili interne per tenere traccia della rotazione della telecamera
    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Blocca il cursore al centro dello schermo e lo nasconde
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inizializza la rotazione in base a come hai posizionato la camera nell'editor
        Vector3 angles = transform.localEulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
    }

    void Update()
    {
        // 1. SBLOCCO DEL CURSORE (Per l'editor)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Se il cursore non è bloccato, ignoriamo gli input per non muovere la visuale
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0)) // Clicca col sinistro per ri-bloccare
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            return;
        }

        // 2. ROTAZIONE (Mouse)
        rotationX += Input.GetAxis("Mouse X") * lookSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * lookSensitivity;

        // Blocca la rotazione verticale per evitare di fare capriole all'indietro
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        // Applica la rotazione
        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0);

        // 3. MOVIMENTO (Tastiera)
        float currentSpeed = baseSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        // WASD per muoversi avanti/indietro/destra/sinistra
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Q ed E (oppure Spazio/Ctrl) per salire e scendere verticalmente
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))
        {
            moveDirection.y += 1f;
        }
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl))
        {
            moveDirection.y -= 1f;
        }

        // Applica il movimento relativo alla direzione in cui stiamo guardando (Space.Self)
        // Normalizziamo il vettore per evitare di andare più veloci muovendoci in diagonale
        transform.Translate(moveDirection.normalized * currentSpeed * Time.deltaTime, Space.Self);
    }
}