using UnityEngine;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        mat = sr.material;

        originalScale = transform.localScale;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (mat != null)
        {
            float targetAmplitude = isDropped ? rb.linearVelocity.magnitude * 0.02f : 0f;
            currentWiggle = Mathf.Lerp(currentWiggle, targetAmplitude, Time.deltaTime * 10f);
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

            // FIX BỆNH NHÁY HÌNH: Nhân 10 thôi để nó không bị nhảy layer liên tục vì sai số siêu nhỏ
            sr.sortingOrder = Mathf.RoundToInt(transform.position.y * 10f) + 1000;
        }
    }

    public void DropPancake()
    {
        isDropped = true;
        isFirstLand = true;

        transform.SetParent(null);
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
        if (isDropped && isFirstLand)
        {
            if (collision.contacts.Length > 0 && collision.contacts[0].point.y < transform.position.y)
            {
                float impactForce = Mathf.Abs(collision.relativeVelocity.y);
                if (impactForce > 0.5f)
                {
                    // Bản thân bẹp dí ảo giác
                    ApplyVisualSquash(1f);

                    if (blinkEffectPrefab != null)
                    {
                        GameObject fx = Instantiate(blinkEffectPrefab, transform.position, Quaternion.identity);
                        Destroy(fx, 1.5f);
                    }

                    // Tắt Continuous để tháp được ngủ ngoan, không check va chạm vô tội vạ nữa
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
                    rb.interpolation = RigidbodyInterpolation2D.None;

                    // Hất tung y chang Pancake Drag
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

                    // Quét các bánh dưới để truyền lực
                    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 10f);
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider != null && hit.collider.CompareTag("Pancake") && hit.collider.gameObject != gameObject)
                        {
                            PancakeLogic lowerLogic = hit.collider.GetComponent<PancakeLogic>();
                            Rigidbody2D lowerRb = hit.collider.GetComponent<Rigidbody2D>();

                            if (lowerLogic != null && lowerRb != null)
                            {
                                float forceFactor = Mathf.Max(0.8f, 1f - (hit.distance / 12f));

                                // CHỈ làm bẹp hình ảnh thôi, TUYỆT ĐỐI KHÔNG ÉP XUỐNG BẰNG VẬT LÝ NỮA!
                                lowerLogic.ApplyVisualSquash(forceFactor);

                                // Búng lên
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

    // Hàm gọi ép bẹp hình ảnh đơn thuần
    public void ApplyVisualSquash(float factor)
    {
        float squash = Mathf.Clamp(1f - (factor * 0.1f), 0.7f, 1f);
        float stretch = 1f + (1f - squash) * 0.4f;
        transform.localScale = new Vector3(originalScale.x * stretch, originalScale.y * squash, originalScale.z);
        currentWiggle += factor * 0.1f;
    }
}