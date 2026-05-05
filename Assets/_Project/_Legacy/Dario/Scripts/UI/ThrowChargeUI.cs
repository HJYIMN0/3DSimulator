using UnityEngine;
using UnityEngine.UI;

namespace Character
{
    /// <summary>
    /// UI element showing throw charge percentage.
    /// Appears when holding Q for throw.
    /// </summary>
    public class ThrowChargeUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Image component that fills based on charge")]
        public Image chargeBar;
        
        [Tooltip("Optional: Background panel")]
        public GameObject chargePanel;

        [Header("Visual Settings")]
        [Tooltip("Color at 0% charge")]
        public Color minChargeColor = Color.yellow;
        
        [Tooltip("Color at 100% charge")]
        public Color maxChargeColor = Color.red;
        
        [Tooltip("Optional: Pulse effect when at max")]
        public bool pulseAtMax = true;
        
        [Tooltip("Pulse speed")]
        public float pulseSpeed = 5f;

        private bool isVisible = false;
        private float pulseTimer = 0f;

        private void Awake()
        {
            // Start hidden
            Hide();
        }

        private void Update()
        {
            // Pulse effect when at max charge
            if (isVisible && pulseAtMax && chargeBar.fillAmount >= 0.99f)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // 0-1 oscillation
                
                chargeBar.color = Color.Lerp(maxChargeColor * 0.7f, maxChargeColor, pulse);
            }
        }

        /// <summary>
        /// Update charge bar fill and color
        /// </summary>
        public void UpdateCharge(float percent)
        {
            if (chargeBar == null) return;

            percent = Mathf.Clamp01(percent);
            
            chargeBar.fillAmount = percent;
            
            if (!pulseAtMax || percent < 0.99f)
            {
                chargeBar.color = Color.Lerp(minChargeColor, maxChargeColor, percent);
            }
        }

        /// <summary>
        /// Show charge UI
        /// </summary>
        public void Show()
        {
            isVisible = true;
            pulseTimer = 0f;
            
            if (chargePanel != null)
                chargePanel.SetActive(true);
            else
                gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide charge UI
        /// </summary>
        public void Hide()
        {
            isVisible = false;
            
            if (chargePanel != null)
                chargePanel.SetActive(false);
            else
                gameObject.SetActive(false);
        }
    }
}
