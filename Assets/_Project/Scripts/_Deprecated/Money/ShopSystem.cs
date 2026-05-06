using UnityEngine;

/// <summary>
/// Mi immagino che ci sia un trigger in cui una volta entrati, si può interagire con lo shop.
/// O comunque un'azione che permette di aprire un'interfaccia di acquisto.
/// Per ora non ho lo script di Inventario, per cui procedo con del pseudocode.
/// </summary>
public class ShopSystem : MonoBehaviour
{
    [SerializeField] private GameObject interactCanvaPrefab;
    // Mi immagino anche che lo shop sia un sistema di scriplable Objects da comprare.
    // Per cui avremo:
    // [SerializeField] private List<ItemSO> itemsForSale; // Lista di oggetti in vendita, da definire in base a come sarà strutturato l'inventario.

    private GameObject interactCanva;


    private PlayerMoneyManager _playerMoneyManager;
    // private Inventory _inventory; // Interfaccia per gestire l'inventario del giocatore, in base al pull, andrà modificata.

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _playerMoneyManager == null)
        {
            _playerMoneyManager = other.GetComponent<PlayerMoneyManager>();
            // Se dà errore, basterà controllare i parent. Adesso lo aggiungo, ma poi idelamente è da rimuovere,
            // Una volta che so dove si trova il component e se è lo stesso oggetto in cui c'è il rigidbody.

            //_inventory = other.GetComponent<Inventory>(); // Stesso discorso di prima, da modificare in base a come sarà strutturato l'inventario.

            if (_playerMoneyManager == null)
            {
                _playerMoneyManager = other.GetComponentInParent<PlayerMoneyManager>();
                Debug.Log("PlayerMoneyManager trovato nel parent: " + _playerMoneyManager.gameObject.name);
                if (_playerMoneyManager == null)
                {
                    _playerMoneyManager = other.GetComponentInChildren<PlayerMoneyManager>();
                    Debug.LogError("PlayerMoneyManager trovato nei children: " + _playerMoneyManager.gameObject.name);

                    if (_playerMoneyManager == null)
                    {
                        Debug.LogError("PlayerMoneyManager non trovato in Player o nei suoi parent/children.");
                        // A questo punto abbiamo semplicemente dimenticato di assegnare il component
                    }
                }
            }

            // N.B. In realtà saerbbe più comodo avere un canva attaccato al player, che andiamo a modificare in base all'interazione richiesta.
            // Finché non ho il player, va bene così.
            if (interactCanva == null)
            {
                interactCanva = Instantiate(interactCanvaPrefab);
                interactCanva.transform.parent = this.gameObject.transform;
            }
            else  
            {
                interactCanva.SetActive(true);
            }

            //A questo punto servirebbe conoscere come funziona L'interact del player.
            //Suppongo sia una cosa del genere:
            //other.GetComponent<PlayerInteraction>().SetCurrentInteractable(this);

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (interactCanva != null)
            {
                interactCanva.SetActive(false);
            }
            // Altra cosa da fare è resettare l'interazione del player, in modo che non possa più interagire con lo shop.
            // other.GetComponent<PlayerInteraction>().SetCurrentInteractable(null);
        }
    }

    public void BuyItem(ItemSO itemToBuy, PlayerMoneyManager playerMoneyManager)
    {
        if (playerMoneyManager.Money >= itemToBuy.ItemBuyCost)
        {
            playerMoneyManager.RemoveMoney(itemToBuy.ItemBuyCost);
            // Aggiungere l'item all'inventario, in base a come sarà strutturato.
            // _inventory.AddItem(item);
        }
        else
        {
            Debug.Log("Non hai abbastanza soldi per comprare questo oggetto.");
        }
    }

    public void SellItem(ItemSO itemToSell, PlayerMoneyManager playerMoneyManager)
    {
        // Aggiungere l'item all'inventario, in base a come sarà strutturato.
        // if (_inventory.HasItem(item))
        // {
        //     _inventory.RemoveItem(item);
        //     _playerMoneyManager.AddMoney(itemCost);
        // }
        // else
        // {
        //     Debug.Log("Non hai questo oggetto da vendere.");
        // }
    }

}
