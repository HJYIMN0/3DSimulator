using UnityEngine;

public class CuttingMinigameUI : MonoBehaviour
{
    [SerializeField] private CuttingMinigame cuttingMinigame;
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform indicator;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform bar;

    private void Awake()
    {
        if (root == null)
        {
            Debug.LogError("Root GameObject reference is missing in CuttingMinigameUI.");
            return;
        }

        root.SetActive(false);
    }

    private void OnEnable()
    {
        if (cuttingMinigame == null)
        {
            Debug.LogError("CuttingMinigame reference is missing in CuttingMinigameUI.");
            return;
        }

        cuttingMinigame.OnMinigameStart += ShowUI;
        cuttingMinigame.OnMinigameCompleted += HideUI;
    }

    private void OnDisable()
    {
        if (cuttingMinigame == null)
            return;

        cuttingMinigame.OnMinigameStart -= ShowUI;
        cuttingMinigame.OnMinigameCompleted -= HideUI;
    }

    private void Update()
    {
        if (cuttingMinigame == null || !cuttingMinigame.IsRunning)
            return;

        UpdateIndicator();
    }

    private void ShowUI()
    {
        if (root == null || bar == null || successZone == null || indicator == null)
            return;

        root.SetActive(true);

        UpdateSuccessZone();
        UpdateIndicator();
    }

    private void HideUI()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void UpdateSuccessZone()
    {
        successZone.anchorMin = new Vector2(cuttingMinigame.SuccessMin, 0f);
        successZone.anchorMax = new Vector2(cuttingMinigame.SuccessMax, 1f);

        successZone.offsetMin = Vector2.zero;
        successZone.offsetMax = Vector2.zero;
    }

    private void UpdateIndicator()
    {
        float x = cuttingMinigame.IndicatorPosition;

        indicator.anchorMin = new Vector2(x, 0.5f);
        indicator.anchorMax = new Vector2(x, 0.5f);
        indicator.anchoredPosition = Vector2.zero;
    }
}