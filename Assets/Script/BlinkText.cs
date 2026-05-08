using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    public TextMeshProUGUI textToBlink;
    public float blinkSpeed = 3f; // Tốc độ

    void Start()
    {
        if (textToBlink == null)
        {
            textToBlink = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (textToBlink != null)
        {
            Color c = textToBlink.color;

            // Alpha dao động từ 0.2 đến 1
            c.a = Mathf.Lerp(0.2f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f));
            textToBlink.color = c;
        }
    }
}