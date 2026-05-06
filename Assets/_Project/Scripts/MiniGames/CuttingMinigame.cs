using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CuttingMinigame : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float indicatorSpeed = 1.5f;
    [SerializeField] private float successMin = 0.4f;
    [SerializeField] private float successMax = 0.6f;

    private float indicatorPosition = 0f;
    private int direction = 1;
    private bool isRunning = false;

    private Action onSuccess;
    private Action onFail;

    public bool IsRunning => isRunning;
    private void Update()
    {
        if(!isRunning)
            return;

        indicatorPosition += direction * indicatorSpeed * Time.deltaTime;

        if (indicatorPosition >= 1f)
        {
            indicatorPosition = 1f;
            direction = -1;
        }
        else if (indicatorPosition <= 0f)
        {
            indicatorPosition = 0f;
            direction = 1;
        }

        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            ResolveCut();
        }

    }

    public void StartMinigame(Action successCallback, Action failCallback)
    {
        if (isRunning)
            return;

        isRunning = true;
        indicatorPosition = 0f;
        direction = 1;

        onSuccess = successCallback;
        onFail = failCallback;

        Debug.Log("Cutting minigame started! Press X to cut when the indicator is in the green zone.");
    }

    private void ResolveCut()
    {
        bool success = indicatorPosition >= successMin && indicatorPosition <= successMax;

        isRunning = false;

        Action successCallback = onSuccess;
        Action failCallback = onFail;

        onSuccess = null;
        onFail = null;

        if (success)
        {
            Debug.Log("Cut successful!");
            successCallback?.Invoke();
        }
        else
        {
            Debug.Log("Cut failed!");
            failCallback?.Invoke();
        }
    }

}
