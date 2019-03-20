using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script will spawn objects of type HarvestableToSpawnPrefab 
// on its childless children in hierarchy
public class HarvestableGroup : MonoBehaviour
{
    public GameObject HarvestableToSpawnPrefab;
    public float SpawnInterval = 40.0f;
    public int InitialSpawnAmt = 10;
    private float _timeUntilNextSpawn;


    private void Awake()
    {
        // remove all the white boxes at the start, if it's demo scene
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).childCount > 0)
            {
                GameObject toDestroy = transform.GetChild(i).GetChild(0).gameObject;
                toDestroy.transform.parent = null;
                Destroy(toDestroy);
            }
        }
    }
    private void Start()
    {
        for (int i = 0; i < InitialSpawnAmt; ++i)
        {
            TrySpawnHarvestable();
        }
    }
    private void Update()
    {
        _timeUntilNextSpawn -= Time.deltaTime;
        if (_timeUntilNextSpawn < 0.0f)
        {
            _timeUntilNextSpawn = Random.Range(SpawnInterval, SpawnInterval * 3);
            TrySpawnHarvestable();
        }
    }

    private void TrySpawnHarvestable()
    {
        List<int> validIds = new List<int>();

        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).childCount == 0)
                validIds.Add(i);
        }

        if (validIds.Count != 0)
        {
            int randId = Mathf.RoundToInt(Random.Range(0, validIds.Count));
            Instantiate(HarvestableToSpawnPrefab, transform.GetChild(validIds[randId]));
        }
    }
}
