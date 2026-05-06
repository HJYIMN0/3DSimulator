using UnityEngine;

/// <summary>
/// Simple test script to verify interaction system is working.
/// Logs to console when interacted.
/// </summary>
public class InteractionTest : Interactable
{
    [Header("Test Info")]
    [Tooltip("Custom name for this test object")]
    public string objectName = "Test Object";
    
    [Tooltip("Show detailed logs")]
    public bool verboseLogging = true;

    private int interactionCount = 0;

    private void Awake()
    {
        // Set default prompt if empty
        if (string.IsNullOrEmpty(promptMessage))
            promptMessage = $"Press [E] to test {objectName}";
            
        if (verboseLogging)
            Debug.Log($"[{objectName}] Interaction test initialized");
    }

    public override void Interact()
    {
        interactionCount++;
        
        // Main log
        Debug.Log($"✅ INTERACTED WITH: '{objectName}' (Count: {interactionCount})");
        
        // Verbose info
        if (verboseLogging)
        {
            Debug.Log($"  - GameObject: {gameObject.name}");
            Debug.Log($"  - Position: {transform.position}");
            Debug.Log($"  - Layer: {LayerMask.LayerToName(gameObject.layer)}");
            Debug.Log($"  - Time: {Time.time:F2}s");
        }
    }

    private void OnDestroy()
    {
        if (verboseLogging && interactionCount > 0)
        {
            Debug.Log($"[{objectName}] Total interactions: {interactionCount}");
        }
    }
}
