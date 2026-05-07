using UnityEngine;

public class UIBounce : MonoBehaviour
{
    public float speed = 5f;       // Tốc độ nhún nảy
    public float amplitude = 15f;  // Độ cao nhún nảy

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // Nhớ ngay vị trí ban đầu của cái bánh
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Dùng unscaledTime kết hợp hàm Sine để tạo nhịp đập lên xuống
        // Bất tử với việc Time.timeScale = 0 luôn nha!
        float newY = startPos.y + Mathf.Sin(Time.unscaledTime * speed) * amplitude;

        // Gắn tọa độ mới cho bánh
        rectTransform.anchoredPosition = new Vector2(startPos.x, newY);
    }
}