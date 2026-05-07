using System;
using UnityEngine;

public interface IProcessingMinigame
{
    bool IsRunning { get; }
    void StartMinigame(Action<bool> onComplete);
}
