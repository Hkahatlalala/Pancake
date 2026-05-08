using UnityEngine;
using UnityEngine.UI;

public class UIBouncingShader : MonoBehaviour
{
    private Material mat;

    void Start()
    {
        mat = GetComponent<Image>().material;
    }

    void Update()
    {
        if (mat != null)
        {
            mat.SetFloat("_UnscaledTime", Time.unscaledTime);
        }
    }
}