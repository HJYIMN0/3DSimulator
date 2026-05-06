using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("Il transform che fa da perno (hinge/pivot). Se vuoto, usa il transform della porta.")]
    public Transform hinge;

    [Tooltip("Angolo di apertura in gradi (valore positivo).")]
    public float openAngle = 90f;

    [Tooltip("Se attivo, apre verso l'altro lato (angolo negativo).")]
    public bool invertDirection = false;

    [Tooltip("Tempo (in secondi) per aprire/chiudere.")]
    public float animationTime = 0.25f;

    [Tooltip("Abilita/Disabilita rotazioni fluide (lerp).")]
    public bool smooth = true;

    [Header("Lock Settings")]
    [Tooltip("Se assegnato, la porta richiede questo oggetto nell'inventario.")]
    public Character.InventoryItemData requiredKey;

    [Tooltip("Se true, la porta rimane bloccata anche dopo l'apertura finché non hai la chiave.")]
    public bool staysLockedUntilUnlocked = false;

    [SerializeField] private string promptMessage;

    private bool _isOpen = false;
    private bool isLocked = false;
    private Quaternion _closedLocalRot;
    private Quaternion _openLocalRot;
    private Coroutine _anim;

    private void Awake()
    {
        if (hinge == null) hinge = transform;

        // salviamo la rotazione chiusa
        _closedLocalRot = hinge.localRotation;

        // calcoliamo quella aperta (±openAngle sul yaw)
        float sign = invertDirection ? -1f : 1f;
        _openLocalRot = _closedLocalRot * Quaternion.Euler(0f, sign * openAngle, 0f);

        // Se c’è una chiave richiesta, blocchiamo la porta
        if (requiredKey != null)
            isLocked = true;

        // messaggio per la UI (opzionale)
        if (string.IsNullOrEmpty(promptMessage))
            promptMessage = "Premi [E] per aprire/chiudere";
    }

    public  void Interact()
    { }
        /*
        // Se c'è un blocco, controlliamo l'inventario
        if (isLocked)
        {
            InventorySystem inv = FindFirstObjectByType<Character.InventorySystem>();
            if (inv != null && inv.Get(requiredKey) != null)
            {
                Debug.Log($"Chiave '{requiredKey.displayName}' trovata! Porta sbloccata.");
                isLocked = false;
            }
            else
            {
                Debug.Log($"La porta è chiusa a chiave. Serve: {requiredKey.displayName}");
                return;
            }
        }

        SetDoorState(!_isOpen);
    }
    */
    private void SetDoorState(bool open)
    {
        _isOpen = open;
        if (_anim != null) StopCoroutine(_anim);

        if (smooth && animationTime > 0f)
            _anim = StartCoroutine(RotateDoorRoutine(open ? _openLocalRot : _closedLocalRot));
        else
            hinge.localRotation = open ? _openLocalRot : _closedLocalRot;
    }

    private System.Collections.IEnumerator RotateDoorRoutine(Quaternion target)
    {
        Quaternion start = hinge.localRotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animationTime;
            hinge.localRotation = Quaternion.Slerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        hinge.localRotation = target;
        _anim = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform h = hinge != null ? hinge : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(h.position, 0.05f);
        Gizmos.DrawRay(h.position, h.up * 0.3f); // asse di rotazione (Y)
    }
#endif
}
