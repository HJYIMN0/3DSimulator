using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Messaggio opzionale mostrato quando il player può interagire.")]
    public string promptMessage = "Premi [E] per interagire";

    // Metodo base chiamato quando il player interagisce con l’oggetto
    public abstract void Interact();
    
}
