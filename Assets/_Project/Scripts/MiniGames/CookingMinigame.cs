using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CookingMinigame : MonoBehaviour, IProcessingMinigame
{
    [Header("Timing")]
    [SerializeField] private float successStartTime = 4f;
    [SerializeField] private float successEndTime = 5f;

    private float elapsedTime;
    private bool isRunning;
    private Action<bool> onComplete;

    public bool IsRunning => isRunning;
    public float ElapsedTime => elapsedTime;
    public float SuccessStartTime => successStartTime;
    public float SuccessEndTime => successEndTime;

    public event Action OnMinigameStart;
    public event Action OnMinigameCompleted;

    private void Update()
    {
        if (!isRunning)
            return;

        elapsedTime += Time.deltaTime;

        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            ResolveCooking();
            return;
        }

        if (elapsedTime > successEndTime)
        {
            ResolveCooking();
        }
    }

    public void StartMinigame(Action<bool> onCompleteCallback)
    {
        if (isRunning)
            return;

        elapsedTime = 0f;
        isRunning = true;
        onComplete = onCompleteCallback;

        OnMinigameStart?.Invoke();

        Debug.Log("Cooking started. Press X between 4 and 5 seconds.");
    }

    private void ResolveCooking()
    {
        bool success = elapsedTime >= successStartTime && elapsedTime <= successEndTime;

        isRunning = false;

        Action<bool> completedCallback = onComplete;
        onComplete = null;

        Debug.Log(success ? "Cooking successful." : "Cooking failed.");

        completedCallback?.Invoke(success);

        OnMinigameCompleted?.Invoke();
    }
}