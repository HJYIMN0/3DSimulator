using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class FixedAspectCanvas : MonoBehaviour
{
    [Tooltip("Proporzione desiderata (16/9 = 1.777...)")]
    public float targetAspect = 16f / 9f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Nessuna camera principale trovata!");
            return;
        }

        UpdateViewport();
    }

    void UpdateViewport()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // Barre nere sopra e sotto (letterbox)
            Rect rect = cam.rect;
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) / 2f;
            cam.rect = rect;
        }
        else
        {
            // Barre nere ai lati (pillarbox)
            float scaleWidth = 1f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}
