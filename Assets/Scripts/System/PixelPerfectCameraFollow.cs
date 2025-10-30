using UnityEngine;
using System.Collections;

public class PixelPerfectCameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.1f;
    public bool usePixelPerfect = false;
    public int referencePPU = 16;
    public int zoomLevel = 1;

    [Header("Camera Offset")]
    public Vector2 cameraOffset = new Vector2(0f, -1f);

    [Header("Camera Boundaries")]
    public bool useCameraBounds = true;
    public Vector2 minBounds = new Vector2(-10f, -5f);
    public Vector2 maxBounds = new Vector2(10f, 5f);
    public bool showGizmos = true;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    private int lastScreenHeight;
    private float originalOrthoSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        originalOrthoSize = cam.orthographicSize;

        if (usePixelPerfect)
        {
            UpdatePixelPerfect();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Добавляем смещение к позиции игрока
        Vector3 targetPosition = target.position + new Vector3(cameraOffset.x, cameraOffset.y, 0);
        targetPosition.z = transform.position.z;

        // Ограничиваем позицию камеры границами
        if (useCameraBounds)
        {
            targetPosition = ClampCameraPosition(targetPosition);
        }

        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothSpeed
        );

        transform.position = smoothedPosition;

        if (usePixelPerfect && Screen.height != lastScreenHeight)
        {
            UpdatePixelPerfect();
            lastScreenHeight = Screen.height;
        }
    }

    private Vector3 ClampCameraPosition(Vector3 targetPosition)
    {
        if (cam == null) return targetPosition;

        // Рассчитываем половину размера видимой области камеры
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        // Ограничиваем позицию камеры с учетом ее размера
        float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + cameraWidth, maxBounds.x - cameraWidth);
        float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + cameraHeight, maxBounds.y - cameraHeight);

        return new Vector3(clampedX, clampedY, targetPosition.z);
    }

    private void UpdatePixelPerfect()
    {
        if (cam == null || !usePixelPerfect) return;

        cam.orthographic = true;
        float pixelPerfectSize = Screen.height / (referencePPU * zoomLevel * 2f);
        cam.orthographicSize = pixelPerfectSize;
    }

    // Включить/выключить пиксель-перфект во время игры
    public void SetPixelPerfect(bool enabled)
    {
        usePixelPerfect = enabled;
        if (enabled)
        {
            UpdatePixelPerfect();
        }
        else
        {
            cam.orthographicSize = originalOrthoSize;
        }
    }

    // Метод для установки границ камеры
    public void SetCameraBounds(Vector2 newMinBounds, Vector2 newMaxBounds)
    {
        minBounds = newMinBounds;
        maxBounds = newMaxBounds;
        useCameraBounds = true;
    }

    // Метод для включения/выключения границ
    public void SetCameraBoundsEnabled(bool enabled)
    {
        useCameraBounds = enabled;
    }

    // Метод для динамического изменения смещения
    public void SetCameraOffset(Vector2 newOffset)
    {
        cameraOffset = newOffset;
    }

    // Метод для плавного изменения смещения
    public void SetCameraOffsetSmooth(Vector2 newOffset, float duration)
    {
        StartCoroutine(SmoothOffsetChange(newOffset, duration));
    }

    private IEnumerator SmoothOffsetChange(Vector2 targetOffset, float duration)
    {
        Vector2 startOffset = cameraOffset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cameraOffset = Vector2.Lerp(startOffset, targetOffset, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraOffset = targetOffset;
    }

    // Визуализация границ в редакторе
    private void OnDrawGizmos()
    {
        if (!showGizmos || !useCameraBounds) return;

        Gizmos.color = Color.yellow;

        // Рассчитываем центр и размер области границ
        Vector3 center = new Vector3(
            (minBounds.x + maxBounds.x) * 0.5f,
            (minBounds.y + maxBounds.y) * 0.5f,
            transform.position.z
        );

        Vector3 size = new Vector3(
            maxBounds.x - minBounds.x,
            maxBounds.y - minBounds.y,
            0.1f
        );

        Gizmos.DrawWireCube(center, size);

        // Отображаем текущие видимые границы камеры
        if (cam != null && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            float cameraHeight = cam.orthographicSize;
            float cameraWidth = cameraHeight * cam.aspect;

            Vector3 cameraBoundsSize = new Vector3(cameraWidth * 2, cameraHeight * 2, 0.1f);
            Gizmos.DrawWireCube(transform.position, cameraBoundsSize);
        }
    }

    void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null && Application.isPlaying)
        {
            if (usePixelPerfect)
            {
                UpdatePixelPerfect();
            }
        }

        // Проверяем что границы валидны
        if (minBounds.x > maxBounds.x) minBounds.x = maxBounds.x - 1f;
        if (minBounds.y > maxBounds.y) minBounds.y = maxBounds.y - 1f;
    }
}