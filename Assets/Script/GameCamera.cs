using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Transform spawner;
    public float zoomStep = 0.3f;

    private Camera cam;
    private float targetSize;
    private Vector3 targetCamPos;
    private Vector3 targetSpawnerPos;
    private SpatulaController spatula; // Kết nối với script Xẻng

    void Start()
    {
        cam = GetComponent<Camera>();
        targetSize = cam.orthographicSize;
        targetCamPos = transform.position;

        if (spawner != null)
        {
            targetSpawnerPos = spawner.position;
            spatula = spawner.GetComponent<SpatulaController>();
        }
    }

    void Update()
    {
        // 1. Camera tự động zoom và nhích lên cao
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * 3f);
        transform.position = Vector3.Lerp(transform.position, targetCamPos, Time.deltaTime * 3f);

        // 2. Ép Xẻng đi theo Camera cực kỳ mượt mà
        if (spatula != null)
        {
            // Liên tục cập nhật tọa độ "Nhà Mới" lên cao dần theo Camera
            spatula.defaultPosition = Vector3.Lerp(spatula.defaultPosition, targetSpawnerPos, Time.deltaTime * 3f);

            // LUẬT THÉP: Chỉ lôi cái xẻng đi khi nó ĐANG RẢNH RỖI (không bị kéo, không đổ bánh)
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
            targetSpawnerPos.y += (zoomStep * 2f); // Nhích mục tiêu của xẻng lên cao theo tỷ lệ 2 lần zoom
        }
    }
}