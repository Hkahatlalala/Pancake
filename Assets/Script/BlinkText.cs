using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    public TextMeshProUGUI textToBlink;
    public float blinkSpeed = 3f; // Tốc độ nhấp nháy, số càng to nháy càng nhanh

    void Start()
    {
        // Tự động tìm cái TextMeshProUGUI nếu mài lười kéo thả
        if (textToBlink == null)
        {
            textToBlink = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (textToBlink != null)
        {
            // Lấy màu hiện tại của chữ
            Color c = textToBlink.color;

            // Ép độ mờ (Alpha) chạy lên chạy xuống từ 0.2 đến 1
            // Dùng unscaledTime để bất tử với việc đóng băng thời gian
            c.a = Mathf.Lerp(0.2f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f));

            // Gắn màu lại cho chữ
            textToBlink.color = c;
        }
    }
}