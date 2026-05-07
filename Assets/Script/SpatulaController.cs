using UnityEngine;
using System.Collections;

public class SpatulaController : MonoBehaviour
{
    public GameObject pancakePrefab;
    public GameObject smokeParticlePrefab;
    public GameObject clickEffectPrefab; // ĐÂY NÈ: Ổ cắm cho hiệu ứng click của mài
    public Transform spawnPoint;
    public float dragThreshold = 0.5f;

    [HideInInspector] public bool isTilting = false;
    [HideInInspector] public bool isDragging = false;
    [HideInInspector] public Vector3 defaultPosition;

    private GameObject currentPancake;
    private bool isValidDrag = false;
    private Vector3 offset;
    private Vector3 dragStartMousePos;

    void Start()
    {
        defaultPosition = transform.position;
        SpawnNewPancake();
    }

    void Update()
    {
        if (isTilting) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMouseWorldPos();

            // --- TUYỆT KỸ ĐẺ HIỆU ỨNG CLICK DƯỚI NGÓN TAY ---
            if (clickEffectPrefab != null)
            {
                // Đẻ hiệu ứng ra và đưa tọa độ Z về mức chuẩn để Camera thấy được
                Vector3 fxPos = new Vector3(mousePos.x, mousePos.y, 0f);
                GameObject fx = Instantiate(clickEffectPrefab, fxPos, Quaternion.identity);
                Destroy(fx, 1f); // Cho sống 1 giây diễn trò rồi phi tang xác cho nhẹ RAM
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

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = GetMouseWorldPos();
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);

            if (Vector2.Distance(dragStartMousePos, mousePos) > dragThreshold)
            {
                isValidDrag = true;
            }
        }

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

    IEnumerator TiltAndDrop()
    {
        isTilting = true;

        Vector3 dropPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;
        float tiltDuration = 0.2f;
        float currentAngle = 0f;
        float targetAngle = -45f;

        // 1. NGHIÊNG XẺNG
        while (elapsed < tiltDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / tiltDuration));
            float targetAngleThisFrame = targetAngle * percent;
            float deltaAngle = targetAngleThisFrame - currentAngle;

            transform.RotateAround(spawnPoint.position, Vector3.forward, deltaAngle);

            currentAngle = targetAngleThisFrame;
            yield return null;
        }

        // 2. THẢ BÁNH VÀ PHÁT ÂM THANH
        if (currentPancake != null)
        {
            currentPancake.GetComponent<PancakeLogic>().DropPancake();
            currentPancake = null;

            // ---> CẮM LỆNH PHÁT TIẾNG RỚT BÁNH Ở ĐÂY <---
            if (GameManager.instance != null) GameManager.instance.PlayDropSound();
        }

        yield return new WaitForSeconds(0.05f);

        // 3. NGẨNG XẺNG
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

        // 4. BAY LƯỚT VỀ NHÀ
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

    void SpawnNewPancake()
    {
        if (smokeParticlePrefab != null)
        {
            GameObject smoke = Instantiate(smokeParticlePrefab, spawnPoint.position, Quaternion.identity);
            Destroy(smoke, 1f);
        }

        currentPancake = Instantiate(pancakePrefab, spawnPoint.position, Quaternion.identity);
        currentPancake.transform.SetParent(transform);

        // ---> CẮM LỆNH PHÁT TIẾNG ĐẺ BÁNH Ở ĐÂY <---
        if (GameManager.instance != null) GameManager.instance.PlaySpawnSound();
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        if (float.IsInfinity(screenPos.x) || float.IsInfinity(screenPos.y)) return transform.position;
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}