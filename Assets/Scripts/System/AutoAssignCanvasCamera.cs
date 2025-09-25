using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AutoAssignCanvasCamera : MonoBehaviour
{
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera ||
            canvas.renderMode == RenderMode.WorldSpace)
        {

            if (Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
            }
            else
            {

                Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (cameras.Length > 0)
                {
                    canvas.worldCamera = cameras[0];
                }
            }
        }
    }

    void Update()
    {
        if (canvas.worldCamera == null && Camera.main != null)
        {
            canvas.worldCamera = Camera.main;
        }
    }
}