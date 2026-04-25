using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Transform spawner;
    public float zoomStep = 0.3f;

    private Camera cam;
    private float targetSize;
    private Vector3 targetCamPos;
    private Vector3 targetSpawnerPos;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetSize = cam.orthographicSize;
        targetCamPos = transform.position;

        if (spawner != null)
            targetSpawnerPos = spawner.position;
    }

    void Update()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * 3f);
        transform.position = Vector3.Lerp(transform.position, targetCamPos, Time.deltaTime * 3f);

        if (spawner != null)
            spawner.position = Vector3.Lerp(spawner.position, targetSpawnerPos, Time.deltaTime * 3f);
    }

    public void ZoomOut()
    {
        targetSize += zoomStep;
        targetCamPos.y += zoomStep;
        if (spawner != null)
            targetSpawnerPos.y += (zoomStep * 2f);
    }
}