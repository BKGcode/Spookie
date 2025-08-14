using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public int numberOfItems = 10;
    public float spawnRadius = 10f;
    public float spawnHeightOffset = 0.5f;
    public Transform[] allowedSpawnAreas;
    List<Transform> usedSpawnAreas = new List<Transform>();
    private Terrain terrain;

    public float spawnHeightAdjustment = 0f;

    private int numSpawnedItems = 0;
    public BookCollector bookCollector;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        bookCollector = FindObjectOfType<BookCollector>();

        for (int i = 0; i < numberOfItems; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            Quaternion spawnRotation = Quaternion.identity; 
            if (usedSpawnAreas.Count > 0)
            {
                spawnRotation = usedSpawnAreas[usedSpawnAreas.Count - 1].rotation;
                spawnPosition.y = usedSpawnAreas[usedSpawnAreas.Count - 1].position.y; 
            }

            spawnPosition.y += spawnHeightAdjustment;
            Instantiate(itemPrefab, spawnPosition, spawnRotation);
            numSpawnedItems++;
        }
    }

    Vector3 GetRandomSpawnPosition()
    {

        List<Transform> unusedAreas = new List<Transform>(allowedSpawnAreas.Length);
        foreach (Transform spawnArea in allowedSpawnAreas)
        {
            if (!usedSpawnAreas.Contains(spawnArea))
            {
                unusedAreas.Add(spawnArea);
            }
        }
        if (unusedAreas.Count == 0)
        {
            Debug.LogWarning("No unused spawn areas available.");
            return transform.position;
        }
        Transform area = unusedAreas[Random.Range(0, unusedAreas.Count)];
        usedSpawnAreas.Add(area);


        Vector3 spawnPosition = area.position + Random.insideUnitSphere * spawnRadius;


        if (terrain != null)
        {
            float terrainHeight = terrain.SampleHeight(spawnPosition);

            spawnPosition.y = terrainHeight + spawnHeightOffset;
        }

        return spawnPosition;
    }

    void CollectItem(GameObject item)
    {

        Destroy(item);


        numSpawnedItems--;
        bookCollector.UpdateTextDisplay();
    }


    void OnDrawGizmosSelected()
    {
        if (allowedSpawnAreas != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform area in allowedSpawnAreas)
            {
                Gizmos.DrawWireSphere(area.position, spawnRadius);
            }
        }
    }
}