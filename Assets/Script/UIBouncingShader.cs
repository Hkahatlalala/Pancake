using UnityEngine;
using UnityEngine.UI;

public class UIBouncingShader : MonoBehaviour
{
    private Material mat;

    void Start()
    {
        // Trộm cái Material đang gắn trên cái ảnh UI
        mat = GetComponent<Image>().material;
    }

    void Update()
    {
        if (mat != null)
        {
            // Liên tục bơm máu (unscaledTime) vào cái lỗ _UnscaledTime trong Shader
            mat.SetFloat("_UnscaledTime", Time.unscaledTime);
        }
    }
}