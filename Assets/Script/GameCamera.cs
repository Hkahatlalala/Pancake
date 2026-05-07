using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    public Transform spawner;
    public float zoomStep = 0.3f;

    private Camera cam;
    private float targetSize;
    private Vector3 targetCamPos;
    private Vector3 targetSpawnerPos;
    private SpatulaController spatula;

    private Vector3 shakeOffset = Vector3.zero;

    // Tui thêm cái neo này để giữ tọa độ gốc của Camera, không bị dính chùm với hiệu ứng rung
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

    void Update()
    {
        // 1. DÙNG unscaledDeltaTime ĐỂ BẤT TỬ VỚI TIMESCALE = 0
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.unscaledDeltaTime * 3f);

        // 2. Tính toán điểm neo thực tế của Camera
        realCamPos = Vector3.Lerp(realCamPos, targetCamPos, Time.unscaledDeltaTime * 3f);

        // 3. Tọa độ xuất ra màn hình = Điểm neo thực tế + Lực Rung (Bao giật)
        transform.position = realCamPos + shakeOffset;

        if (spatula != null)
        {
            spatula.defaultPosition = Vector3.Lerp(spatula.defaultPosition, targetSpawnerPos, Time.unscaledDeltaTime * 3f);

            if (!spatula.isDragging && !spatula.isTilting)
            {
                spawner.position = spatula.defaultPosition;
            }
        }
    }

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