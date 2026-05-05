using UnityEngine;
using UnityEngine.UI;

public class SprintStaminaSystem : MonoBehaviour
{
    [Header("Sprint Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 10f;
    public float regenDelay = 2f;

    [Header("Jump Settings")]
    public float jumpStaminaCost = 15f; 
    public float minStaminaToJump = 10f;

    [Header("UI Settings")]
    public Slider staminaBar;
    public Color fullColor = Color.green;
    public Color emptyColor = Color.red;
    public Image fillImage;

    private float currentStamina;
    private float regenTimer;
    private bool draining = false;

    public bool CanSprint => currentStamina > 0.1f;
    public bool CanJump => currentStamina >= minStaminaToJump;
    public float CurrentStamina => currentStamina;


    private void Start()
    {
        currentStamina = maxStamina;
        if (staminaBar != null)
            staminaBar.maxValue = maxStamina;
    }

    private void Update()
    {
        if (draining)
        {
            DrainStamina();
            regenTimer = 0f;
        }
        else
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenDelay)
                RegenerateStamina();
        }

        UpdateUI();
    }

    public void StartDrain()
    {
        if (!CanSprint) return;
        draining = true;
    }

    public void StopDrain()
    {
        draining = false;
    }
    public bool TryConsumeJumpStamina()
    {
        if (!CanJump)
            return false;

        currentStamina -= jumpStaminaCost;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // Reset regen timer (delay before regen starts)
        regenTimer = 0f;

        return true;
    }
    private void DrainStamina()
    {
        currentStamina -= staminaDrainRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    private void RegenerateStamina()
    {
        currentStamina += staminaRegenRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    private void UpdateUI()
    {
        if (staminaBar == null) return;
        staminaBar.value = currentStamina;

        if (fillImage != null)
            fillImage.color = Color.Lerp(emptyColor, fullColor, currentStamina / maxStamina);
    }
}
