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
    private bool hasPlayedHitSound = false; // Thêm lại cờ âm thanh

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        mat = sr.material;

        if (rb != null)
        {
            rb.centerOfMass = new Vector2(0f, -0.8f);
        }
        // TÀNG HÌNH: Tránh chớp hình 1 frame lúc đẻ
        if (sr != null) sr.enabled = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Start()
    {
        // Nhớ kích thước chuẩn sau khi Unity tính toán xong Parent
        originalScale = transform.localScale;

        // Bóp bánh nhỏ lại chuẩn bị diễn ảo thuật
        transform.localScale = Vector3.zero;

        // HIỆN HÌNH lại
        if (sr != null) sr.enabled = true;

        // Khởi động Coroutine nảy tưng tưng
        StartCoroutine(SpawnPopAnimation());
    }

    // TUYỆT KỸ EASE OUT BACK CHO BÁNH VỪA ĐẺ
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

            // Chỉ thay đổi scale nếu bánh CHƯA rớt
            // Để tránh xung đột với hiệu ứng bẹp nảy lúc đáp xuống tháp
            if (!isDropped)
            {
                transform.localScale = originalScale * easeValue;
            }

            yield return null;
        }

        // Chốt đơn kích thước gốc
        if (!isDropped) transform.localScale = originalScale;
    }

    void Update()
    {
        if (mat != null)
        {
            float targetAmplitude = isDropped ? rb.linearVelocity.magnitude * 0.02f : 0f;
            currentWiggle = Mathf.Lerp(currentWiggle, targetAmplitude, Time.deltaTime * 5f);
            mat.SetFloat("_WiggleAmplitude", Mathf.Clamp(currentWiggle, 0f, 0.4f));
        }

        if (isDropped)
        {
            // FIX BỆNH MẤT NGỦ: Chỉ Lerp khi chưa đạt size chuẩn. Về form rồi là KHÓA CỨNG luôn!
            if (Mathf.Abs(transform.localScale.y - originalScale.y) > 0.005f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 10f);
            }
            else
            {
                transform.localScale = originalScale;
            }

            // FIX BỆNH NHÁY HÌNH
            sr.sortingOrder = Mathf.RoundToInt(transform.position.y * 10f) + 1000;
        }
    }

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. ÂM THANH "BẸP" (Chỉ kêu 1 lần)
        if (!hasPlayedHitSound)
        {
            if (GameManager.instance != null) GameManager.instance.PlayDropSound();
            hasPlayedHitSound = true;
        }

        // 2. HIỆU ỨNG SÓNG CHẤN ĐỘNG CỦA MÀI
        if (isDropped && isFirstLand)
        {
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

    public void ApplyVisualSquash(float factor)
    {
        float squash = Mathf.Clamp(1f - (factor * 0.1f), 0.7f, 1f);
        float stretch = 1f + (1f - squash) * 0.4f;
        transform.localScale = new Vector3(originalScale.x * stretch, originalScale.y * squash, originalScale.z);
        currentWiggle += factor * 0.1f;
    }
}