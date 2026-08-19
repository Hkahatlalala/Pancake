using UnityEngine;
using System.Collections;

public class PancakeLogic : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Material mat;
    private Vector3 originalScale;

    public float bounceForce = 35f;
    public GameObject blinkEffectPrefab;

    private float currentWiggle = 0f;
    private bool isFirstLand = false;
    private bool isDropped = false;
    private bool hasPlayedHitSound = false;

    // Khởi tạo các component cơ bản và thiết lập vật lý ban đầu
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        mat = sr.material;

        if (rb != null)
        {
            rb.centerOfMass = new Vector2(0f, -0.8f);
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (sr != null) sr.enabled = false;
    }

    // Thiết lập kích thước chuẩn
    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        if (sr != null) sr.enabled = true;
        StartCoroutine(SpawnPopAnimation());
    }

    // Hoạt ảnh phóng to EaseOutBack
    IEnumerator SpawnPopAnimation()
    {
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeValue = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            if (!isDropped)
            {
                transform.localScale = originalScale * easeValue;
            }
            yield return null;
        }

        if (!isDropped) transform.localScale = originalScale;
    }

    // Cập nhật liên tục mỗi frame: truyền dữ liệu độ rung cho Shader, và Sorting Order
    void Update()
    {
        if (mat != null)
        {
            float targetAmplitude = isDropped ? rb.linearVelocity.magnitude * 0.02f : 0f;
            currentWiggle = Mathf.Lerp(currentWiggle, targetAmplitude, Time.deltaTime * 5f);
            mat.SetFloat("_WiggleAmplitude", Mathf.Clamp(currentWiggle, 0.1f, 0.4f));
        }

        if (isDropped)
        {
            if (Mathf.Abs(transform.localScale.y - originalScale.y) > 0.005f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 10f);
            }
            else
            {
                transform.localScale = originalScale;
            }

            sr.sortingOrder = Mathf.RoundToInt(transform.position.y * 10f) + 1000;
        }
        else
        {
            if (sr != null) sr.sortingOrder = 30000;
        }
    }

    // Khóa góc xoay của bánh 
    void LateUpdate()
    {
        if (!isDropped)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    // Tách bánh khỏi xẻng, kích hoạt hệ thống vật lý Dynamic để bánh rơi tự do
    public void DropPancake()
    {
        isDropped = true;
        isFirstLand = true;

        transform.SetParent(null);
        originalScale = transform.localScale;
        transform.rotation = Quaternion.identity;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = false;

        rb.gravityScale = 3f;
        rb.mass = 1.2f;
    }

    // Xử lý logic khi va chạm, tính toán lực nảy lan truyền xuống các bánh bên dưới và cộng điểm
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasPlayedHitSound)
        {
            if (GameManager.instance != null) GameManager.instance.PlayDropSound();
            hasPlayedHitSound = true;
        }

        if (isDropped && isFirstLand)
        {
            //
            if (collision.gameObject.CompareTag("Pancake") || collision.gameObject.name.Contains("plate"))
            {
                Transform targetParent = collision.transform;
                if (collision.gameObject.CompareTag("Pancake"))
                {
                    targetParent = collision.transform.parent;
                }
                transform.SetParent(targetParent, true);
                originalScale = transform.localScale;
            }

            if (collision.contacts.Length > 0 && collision.contacts[0].point.y < transform.position.y)
            {
                float impactForce = Mathf.Abs(collision.relativeVelocity.y);
                if (impactForce > 0.5f)
                {
                    ApplyVisualSquash(1f);

                    if (blinkEffectPrefab != null)
                    {
                        GameObject fx = Instantiate(blinkEffectPrefab, transform.position, Quaternion.identity);
                        Destroy(fx, 1.5f);
                    }

                    rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
                    rb.interpolation = RigidbodyInterpolation2D.None;

                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

                    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 2.5f);
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider != null && hit.collider.CompareTag("Pancake") && hit.collider.gameObject != gameObject)
                        {
                            PancakeLogic lowerLogic = hit.collider.GetComponent<PancakeLogic>();
                            Rigidbody2D lowerRb = hit.collider.GetComponent<Rigidbody2D>();

                            if (lowerLogic != null && lowerRb != null)
                            {
                                float forceFactor = Mathf.Max(0.8f, 1f - (hit.distance / 12f));

                                lowerLogic.ApplyVisualSquash(forceFactor);

                                lowerRb.linearVelocity = new Vector2(lowerRb.linearVelocity.x, 0f);
                                lowerRb.AddForce(Vector2.up * bounceForce * forceFactor, ForceMode2D.Impulse);
                            }
                        }
                    }

                    if (GameManager.instance != null) GameManager.instance.AddScore();

                    isFirstLand = false;
                }
            }
        }
    }

    // Scale ép bánh theo trục Y và trục X
    public void ApplyVisualSquash(float factor)
    {
        float squash = Mathf.Clamp(1f - (factor * 0.1f), 0.7f, 1f);
        float stretch = 1f + (1f - squash) * 0.4f;
        transform.localScale = new Vector3(originalScale.x * stretch, originalScale.y * squash, originalScale.z);
        currentWiggle += factor * 0.1f;
    }
}