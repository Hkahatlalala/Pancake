using UnityEngine;

public class PancakeSpawner : MonoBehaviour
{
    public GameObject pancakePrefab;
    public Transform spawnPoint;

    [HideInInspector]
    public GameObject currentPancake;

    void Start()
    {
        SpawnNewPancake();
    }

    void Update()
    {
        if (currentPancake != null)
        {
            currentPancake.transform.position = spawnPoint.position;
        }
    }

    public void SpawnNewPancake()
    {
        currentPancake = Instantiate(pancakePrefab, spawnPoint.position, Quaternion.identity);
    }

    public void ReleasePancake()
    {
        currentPancake = null;
    }
}