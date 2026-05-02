using UnityEngine;
namespace Character
{
    public class EquipableItem : MonoBehaviour
    {
        [SerializeField]private Rigidbody rb;
        private Collider col;
        private AudioSource audioSrc;

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    Debug.LogWarning($"EquipableItem '{name}' has no Rigidbody component! Please add one for proper physics handling.");
                    return;
                }
            }
            col = GetComponent<Collider>();
            audioSrc = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Chiamata quando l’oggetto viene equipaggiato (disattiva fisica)
        /// </summary>
        public void OnEquipped()
        {
            if(rb == null)
            {
                Debug.LogWarning($"EquipableItem '{name}' has no Rigidbody component! Cannot disable physics on equip.");
                return;
            }

                rb.isKinematic = true;
                rb.detectCollisions = false;

            //if (col != null)
            //    col.enabled = false;

            if (audioSrc != null)
                audioSrc.enabled = false;
        }

        /// <summary>
        /// Chiamata quando l’oggetto viene droppato (riattiva fisica)
        /// </summary>
        public void OnDropped()
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            //if (col != null)
            //   col.enabled = true;

            if (audioSrc != null)
                audioSrc.enabled = true;
        }
    }
}