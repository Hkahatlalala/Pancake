using UnityEngine;

public class UIBounce : MonoBehaviour
{
    public float speed = 5f;       // Tốc độ
    public float amplitude = 15f;  // Độ cao
    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.unscaledTime * speed) * amplitude;
        rectTransform.anchoredPosition = new Vector2(startPos.x, newY);
    }
}