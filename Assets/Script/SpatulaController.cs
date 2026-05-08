using UnityEngine;
using System.Collections;

public class SpatulaController : MonoBehaviour
{
    public GameObject pancakePrefab;
    public GameObject smokeParticlePrefab;
    public GameObject clickEffectPrefab;
    public Transform spawnPoint;

    public float dragThreshold = 0.2f; // Kéo tối thiểu 
    public float minYLimit = -1.5f;    

    [HideInInspector] public bool isTilting = false;
    [HideInInspector] public bool isDragging = false;
    [HideInInspector] public Vector3 defaultPosition;

    private GameObject currentPancake;
    private bool isValidDrag = false;
    private Vector3 offset;
    private Vector3 dragStartMousePos;

    // Lưu vị trí gốc và tạo bánh đầu tiên
    void Start()
    {
        defaultPosition = transform.position;
        SpawnNewPancake();
    }

    // Xử lý thao tác chạm, kéo thả xẻng và giới hạn vùng di chuyển
    void Update()
    {
        if (isTilting) return;

        // Tạo hiệu ứng click và ghi nhận vị trí
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMouseWorldPos();

            if (clickEffectPrefab != null)
            {
                Vector3 fxPos = new Vector3(mousePos.x, mousePos.y, 0f);
                GameObject fx = Instantiate(clickEffectPrefab, fxPos, Quaternion.identity);
                Destroy(fx, 1f);
            }

            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.gameObject == gameObject)
            {
                isDragging = true;
                isValidDrag = false;
                dragStartMousePos = mousePos;
                offset = transform.position - mousePos;
                offset.z = 0f;
            }
        }

        // Đang giữ và kéo: Di chuyển xẻng theo chuột, chốt chặn không cho rớt dưới minYLimit
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = GetMouseWorldPos();

            float newX = mousePos.x + offset.x;
            float newY = mousePos.y + offset.y;
            newY = Mathf.Max(newY, minYLimit);

            transform.position = new Vector3(newX, newY, transform.position.z);

            if (Vector2.Distance(dragStartMousePos, mousePos) > dragThreshold)
            {
                isValidDrag = true;
            }
        }

        // Thả ngón tay: Quyết định đổ bánh hay thu xẻng về
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if (isValidDrag)
            {
                StartCoroutine(TiltAndDrop());
            }
            else
            {
                StartCoroutine(ReturnToDefault());
            }
        }
    }

    // Nghiêng xẻng -> Thả bánh -> Ngẩng xẻng -> Bay về vị trí cũ
    IEnumerator TiltAndDrop()
    {
        isTilting = true;

        Vector3 dropPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;
        float tiltDuration = 0.2f;
        float currentAngle = 0f;
        float targetAngle = -45f;
        bool hasDropped = false;

        // Giai đoạn 1: Nghiêng xẻng
        while (elapsed < tiltDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / tiltDuration));
            float targetAngleThisFrame = targetAngle * percent;
            float deltaAngle = targetAngleThisFrame - currentAngle;

            transform.RotateAround(spawnPoint.position, Vector3.forward, deltaAngle);
            currentAngle = targetAngleThisFrame;

            // Thả bánh ngay khi xẻng nghiêng được 30%
            if (percent >= 0.3f && !hasDropped)
            {
                if (currentPancake != null)
                {
                    currentPancake.GetComponent<PancakeLogic>().DropPancake();
                    currentPancake = null;
                    if (GameManager.instance != null) GameManager.instance.PlayDropSound();
                }
                hasDropped = true;
            }

            yield return null;
        }

        // Đảm bảo bánh chắc chắn được thả nếu bị rớt frame
        if (!hasDropped && currentPancake != null)
        {
            currentPancake.GetComponent<PancakeLogic>().DropPancake();
            currentPancake = null;
            if (GameManager.instance != null) GameManager.instance.PlayDropSound();
        }

        yield return new WaitForSeconds(0.05f);

        // Giai đoạn 2: Ngẩng xẻng 
        elapsed = 0f;
        currentAngle = 0f;
        float reverseAngle = 45f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.15f));
            float targetAngleThisFrame = reverseAngle * percent;
            float deltaAngle = targetAngleThisFrame - currentAngle;

            transform.RotateAround(spawnPoint.position, Vector3.forward, deltaAngle);

            currentAngle = targetAngleThisFrame;
            yield return null;
        }
        transform.position = dropPos;
        transform.rotation = startRot;

        // Giai đoạn 3: Về vị trí chờ
        elapsed = 0f;
        float flyDuration = 0.25f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / flyDuration));
            transform.position = Vector3.Lerp(dropPos, defaultPosition, percent);
            yield return null;
        }
        transform.position = defaultPosition;

        isTilting = false;
        SpawnNewPancake();
    }

    // Về vị trí chờ nếu chưa kéo đủ xa
    IEnumerator ReturnToDefault()
    {
        isTilting = true;
        Vector3 currentPos = transform.position;

        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(currentPos, defaultPosition, percent);
            yield return null;
        }
        transform.position = defaultPosition;
        isTilting = false;
    }

    // Tạo bánh mới, gán vào xẻng
    void SpawnNewPancake()
    {
        if (smokeParticlePrefab != null)
        {
            GameObject smoke = Instantiate(smokeParticlePrefab, spawnPoint.position, Quaternion.identity);
            Destroy(smoke, 1f);
        }

        currentPancake = Instantiate(pancakePrefab, spawnPoint.position, Quaternion.identity);
        currentPancake.transform.SetParent(transform);

        if (GameManager.instance != null) GameManager.instance.PlaySpawnSound();
    }

    // Chuyển đổi tọa độ chuột từ màn hình sang không gian 2D của Game
    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        if (float.IsInfinity(screenPos.x) || float.IsInfinity(screenPos.y)) return transform.position;
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}