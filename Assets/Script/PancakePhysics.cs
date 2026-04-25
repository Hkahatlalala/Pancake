using UnityEngine;

public class PancakePhysics : MonoBehaviour
{
    private Vector3 originalScale;
    private float springVelocity = 0f;
    private float currentYScale;

    public float stiffness = 10f; // Độ cứng lò xo
    public float damping = 10f;    // Độ hãm (càng cao càng nhanh dừng nhún)

    void Start()
    {
        originalScale = transform.localScale;
        currentYScale = originalScale.y;
    }

    void Update()
    {
        // Công thức toán học lò xo (Spring Physics)
        float force = -stiffness * (currentYScale - originalScale.y);
        springVelocity += force * Time.deltaTime;
        springVelocity -= damping * springVelocity * Time.deltaTime;
        currentYScale += springVelocity * Time.deltaTime;

        // Áp dụng scale mới (Nén Y thì phình X để giữ khối lượng
    }

    // Hàm để vật khác (như cái bánh đang kéo) gọi vào để ép xuống
    public void ApplyCompressForce(float amount)
    {
        springVelocity -= amount;
    }

    // Khi có bánh khác rơi trúng hoặc đè lên
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Lấy lực va chạm để tạo độ nhún tự nhiên
        float force = collision.relativeVelocity.y * 0.1f;
        ApplyCompressForce(force);
    }
}