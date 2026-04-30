using UnityEngine;

public class PancakeDrag : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isOnSpatula = true;
    private bool isDragging = false;
    private SpriteRenderer sr;
    private PancakeSpawner spawner;
    private Vector3 originalScale;
    private Material mat;

    private bool isClicked = false;
    private Vector3 startMousePos;
    public float dragThreshold = 0.5f;

    public float bounceForce = 35f;
    public float followSpeed = 25f;

    public GameObject blinkEffectPrefab;

    public static int globalSortOrder = 10;
    private float currentWiggle = 0f;
    private float defaultGravity;

    // Cờ báo hiệu chỉ cho nổ pháo hoa ở lần chạm đĩa đầu tiên
    private bool isFirstLand = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        spawner = FindFirstObjectByType<PancakeSpawner>();
        originalScale = transform.localScale;
        mat = sr.material;

        defaultGravity = rb.gravityScale;
        rb.bodyType = RigidbodyType2D.Kinematic;

        sr.sortingOrder = globalSortOrder;
        globalSortOrder++;
    }

    void Update()
    {
        if (mat != null)
        {
            float targetAmplitude = 0f;
            if (!isOnSpatula)
            {
                float speed = rb.linearVelocity.magnitude;
                targetAmplitude = speed * 0.02f;
            }

            currentWiggle = Mathf.Lerp(currentWiggle, targetAmplitude, Time.deltaTime * 10f);
            mat.SetFloat("_WiggleAmplitude", Mathf.Clamp(currentWiggle, 0f, 0.4f));
        }

        if (!isOnSpatula && !isDragging)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 10f);
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = GetMouseWorldPos();

            if (isOnSpatula)
            {
                if (Input.GetMouseButtonDown(0) && sr.bounds.Contains(mousePos))
                {
                    isClicked = true;
                    startMousePos = mousePos;
                    transform.localScale = originalScale * 1.1f;
                }

                if (isClicked && Input.GetMouseButton(0))
                {
                    if (Vector3.Distance(startMousePos, mousePos) > dragThreshold)
                    {
                        isClicked = false;
                        isOnSpatula = false;
                        isDragging = true;

                        originalScale = transform.localScale;

                        rb.bodyType = RigidbodyType2D.Dynamic;
                        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                        rb.freezeRotation = true;

                        rb.gravityScale = 0f;

                        if (spawner != null) spawner.ReleasePancake();
                    }
                }

                if (isClicked && Input.GetMouseButtonUp(0))
                {
                    isClicked = false;
                    transform.localScale = originalScale;
                }
            }
            else
            {
                if (isDragging)
                {
                    if (Input.GetMouseButton(0))
                    {
                        Vector2 moveDir = (mousePos - transform.position);
                        rb.linearVelocity = moveDir * followSpeed;
                        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, 40f);
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        isDragging = false;
                        rb.gravityScale = defaultGravity;
                        rb.freezeRotation = false;
                        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
                        rb.interpolation = RigidbodyInterpolation2D.None;

                        rb.linearVelocity = Vector2.zero;

                        // Bật cờ lên khi mài buông tay thả bánh
                        isFirstLand = true;

                        float overDrag = transform.position.y - mousePos.y;

                        bool isTouchingSomething = false;
                        RaycastHit2D[] touchChecks = Physics2D.RaycastAll(transform.position, Vector2.down, 0.8f);
                        foreach (var check in touchChecks)
                        {
                            if (check.collider != null && check.collider.gameObject != gameObject)
                            {
                                isTouchingSomething = true;
                                break;
                            }
                        }

                        if (overDrag > 0.05f && isTouchingSomething)
                        {
                            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

                            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 10f);
                            foreach (RaycastHit2D hit in hits)
                            {
                                if (hit.collider != null && hit.collider.CompareTag("Pancake") && hit.collider.gameObject != gameObject)
                                {
                                    Rigidbody2D lowerRb = hit.collider.GetComponent<Rigidbody2D>();
                                    if (lowerRb != null)
                                    {
                                        float forceFactor = Mathf.Max(0.8f, 1f - (hit.distance / 12f));
                                        lowerRb.linearVelocity = new Vector2(lowerRb.linearVelocity.x, 0f);
                                        lowerRb.AddForce(Vector2.up * bounceForce * forceFactor, ForceMode2D.Impulse);
                                    }
                                }
                            }
                        }

                        if (GameManager.instance != null) GameManager.instance.AddScore();
                        if (spawner != null) spawner.Invoke("SpawnNewPancake", 0.5f);
                    }
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isOnSpatula && !isDragging)
        {
            float impactForce = Mathf.Abs(collision.relativeVelocity.y);
            if (impactForce > 0.5f)
            {
                float squash = Mathf.Clamp(1f - (impactForce * 0.05f), 0.7f, 1f);
                float stretch = 1f + (1f - squash) * 0.4f;
                transform.localScale = new Vector3(originalScale.x * stretch, originalScale.y * squash, originalScale.z);

                currentWiggle += impactForce * 0.08f;

                // CHỈ NỔ NẾU ĐÂY LÀ LẦN RỚT ĐẦU TIÊN TỪ TRÊN TAY XUỐNG
                if (isFirstLand && blinkEffectPrefab != null)
                {
                    GameObject fx = Instantiate(blinkEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(fx, 1.5f);

                    // Dập cờ đi, cấm nổ bậy bạ ở các lần nảy sau
                    isFirstLand = false;
                }
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return worldPos;
    }
}