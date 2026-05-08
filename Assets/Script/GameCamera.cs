using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    public Transform spawner;
    public float zoomStep = 0.3f; // Tỉ lệ Scale

    private Camera cam;
    private float targetSize;
    private Vector3 targetCamPos;
    private Vector3 targetSpawnerPos;
    private SpatulaController spatula;

    private Vector3 shakeOffset = Vector3.zero;
    private Vector3 realCamPos;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetSize = cam.orthographicSize;

        realCamPos = transform.position;
        targetCamPos = realCamPos;

        if (spawner != null)
        {
            targetSpawnerPos = spawner.position;
            spatula = spawner.GetComponent<SpatulaController>();
        }
    }

    // Nội suy 
    void Update()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.unscaledDeltaTime * 3f);
        realCamPos = Vector3.Lerp(realCamPos, targetCamPos, Time.unscaledDeltaTime * 3f);
        transform.position = realCamPos + shakeOffset;

        // Kéo vị trí gốc của Xẻng di chuyển tịnh tiến theo Scale Camera
        if (spatula != null)
        {
            spatula.defaultPosition = Vector3.Lerp(spatula.defaultPosition, targetSpawnerPos, Time.unscaledDeltaTime * 3f);

            if (!spatula.isDragging && !spatula.isTilting)
            {
                spawner.position = spatula.defaultPosition;
            }
        }
    }

    // Scale
    public void ZoomOut()
    {
        targetSize += zoomStep;
        targetCamPos.y += zoomStep;
        if (spawner != null)
        {
            targetSpawnerPos.y += (zoomStep * 2f);
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    // Tạo ra các tọa độ chênh lệch ngẫu nhiên liên tục để làm rung màn hình
    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }
}