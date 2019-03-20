using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawnpoint for enemies
public class EnemyHideout : MonoBehaviour
{
    #region Declarations
    public int EnemyCapacity = 5;

    public float RespawnTimer = 60.0f;
    private float _remainingRespawnTime;

    public List<GameObject> EnemiesPresent = new List<GameObject>();

    public GameObject SpawnPoint;

    private bool _canSpawn = false;

    private static List<EnemyHideout> _allHideouts = new List<EnemyHideout>();
    #endregion Declarations

    private void Start()
    {
        _allHideouts.Add(this);
    }
    private void Update()
    {
        if (_canSpawn)
        {
            if (EnemiesPresent.Count < EnemyCapacity)
            {
                _remainingRespawnTime -= Time.deltaTime;
                if (_remainingRespawnTime < 0.0f)
                {
                    _remainingRespawnTime = Random.Range(RespawnTimer, RespawnTimer * 3);
                    SpawnEnemy();
                }
            }
            else
                _remainingRespawnTime = RespawnTimer*2;
        }
    }

    private void SpawnEnemy()
    {
        GameObject newEnemy = Instantiate(GameManager.Instance.EnemyPrefab, SpawnPoint.transform);

        float angle = Random.Range(0, Mathf.PI * 2);
        Vector3 newPos = newEnemy.transform.position;
        newPos.x += Mathf.Cos(angle) * 3.0f;
        newPos.y += 5.0f;
        newPos.z += Mathf.Sin(angle) * 3.0f;

        newEnemy.transform.rotation = new Quaternion(0, 0, 0, 0);

        newEnemy.transform.position = newPos;

        EnemiesPresent.Add(newEnemy);

        newEnemy.GetComponent<Health>().onDeath.AddListener(EnemyDied);
        Debug.Log("Enemy Spawned!");
    }
    private void EnemyDied(GameObject source)
    {
        for (int i = 0; i < EnemiesPresent.Count; ++i)
        {
            if (!EnemiesPresent[i])
                EnemiesPresent.Remove(EnemiesPresent[i]);
        }
    }

    // Gets called by the DemoSceneManager at the end of the tutorial
    public static void StartSpawning()
    {
        for (int i = 0; i < _allHideouts.Count; ++i)
        {
            _allHideouts[i]._canSpawn = true;
        }
    }
}
