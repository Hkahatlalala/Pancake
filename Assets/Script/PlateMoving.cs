using UnityEngine;

public class PlateMoving : MonoBehaviour
{
    public float speed = 2f;      
    public float distance = 2.5f; 

    private float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        float newX = startX + Mathf.Sin(Time.time * speed) * distance;

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}