using UnityEngine;

/// <summary>
/// Comprehensive interaction test - creates multiple test scenarios
/// </summary>
public class InteractionTestSuite : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("Run tests on Start")]
    public bool runTestsOnStart = true;
    
    [Header("Test Results")]
    public int testsRun = 0;
    public int testsPassed = 0;
    public int testsFailed = 0;

    private void Start()
    {
        if (runTestsOnStart)
        {
            Debug.Log("========================================");
            Debug.Log(" INTERACTION SYSTEM TEST SUITE");
            Debug.Log("========================================");
            
            RunAllTests();
            
            Debug.Log("========================================");
            Debug.Log($" RESULTS: {testsPassed}/{testsRun} passed, {testsFailed} failed");
            Debug.Log("========================================");
        }
    }

    private void RunAllTests()
    {
        TestPlayerInteractionExists();
        TestInputManagerExists();
        TestCameraExists();
        TestInteractableObjectsExist();
    }

    private void TestPlayerInteractionExists()
    {
        testsRun++;
        var interaction = FindFirstObjectByType<Character.PlayerInteraction>();
        
        if (interaction != null)
        {
            testsPassed++;
            Debug.Log(" TEST 1: PlayerInteraction component found");
        }
        else
        {
            testsFailed++;
            Debug.LogError(" TEST 1 FAILED: PlayerInteraction component NOT found!");
        }
    }

    private void TestInputManagerExists()
    {
        testsRun++;
        var inputManager = FindFirstObjectByType<Character.InputManager>();
        
        if (inputManager != null)
        {
            testsPassed++;
            Debug.Log(" TEST 2: InputManager component found");
            
            // Additional check
            if (inputManager.PlayerManager != null)
            {
                Debug.Log("   PlayerManager reference: OK");
            }
            else
            {
                Debug.LogWarning("   PlayerManager reference: MISSING");
            }
        }
        else
        {
            testsFailed++;
            Debug.LogError(" TEST 2 FAILED: InputManager component NOT found!");
        }
    }

    private void TestCameraExists()
    {
        testsRun++;
        
        if (Camera.main != null)
        {
            testsPassed++;
            Debug.Log(" TEST 3: Main Camera found");
            Debug.Log($"   Camera tag: {Camera.main.tag}");
        }
        else
        {
            testsFailed++;
            Debug.LogError(" TEST 3 FAILED: Main Camera NOT found!");
        }
    }

    private void TestInteractableObjectsExist()
    {
        testsRun++;

        var interactables = FindObjectsByType<Interactable>(FindObjectsSortMode.None);

        if (interactables.Length > 0)
        {
            testsPassed++;
            Debug.Log($" TEST 4: Found {interactables.Length} interactable object(s)");

            foreach (var obj in interactables)
            {
                Debug.Log($"   {obj.gameObject.name} (Layer: {LayerMask.LayerToName(obj.gameObject.layer)})");
            }
        }
        else
        {
            testsFailed++;
            Debug.LogError(" TEST 4 FAILED: NO interactable objects in scene!");
            Debug.LogError("   Add objects with InteractionTest or Door component");
        }
    }
}
