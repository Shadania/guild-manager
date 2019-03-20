using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Source & initial holder of Quest Parchments
public class QuestSource : MonoBehaviour
{
    public List<GameObject> QuestSpawnPoints = new List<GameObject>();
    public float QuestSpawnInterval;
    public GameObject QuestParchmentPrefab;
    public Village MyVillage;

    private float _questSpawnElapsedSec;
    private bool _playerInTrigger;

    private void Start()
    {
        MyVillage = transform.parent.parent.GetComponent<Village>();
        if (!MyVillage)
            Debug.Log("No village found for QuestSource");
    }
    private void Update()
    {
        _questSpawnElapsedSec += Time.deltaTime;
        if (_questSpawnElapsedSec > QuestSpawnInterval)
        {
            _questSpawnElapsedSec -= QuestSpawnInterval;

            SpawnQuest();
        }
    }


    private void SpawnQuest()
    {
        for(int i = 0; i < QuestSpawnPoints.Count; ++i)
        {
            if (QuestSpawnPoints[i].transform.childCount == 0)
            {
                GameObject spawnedQuest = Instantiate(QuestParchmentPrefab, QuestSpawnPoints[i].transform);

                QuestParchment qInfo = spawnedQuest.GetComponent<QuestParchment>();
                if (qInfo)
                {
                    qInfo.QuestType = (Quest.Type)Mathf.RoundToInt(Random.Range(0, 4));
                    qInfo.Quota = Mathf.RoundToInt(Random.Range(7, 20));
                    qInfo.RewardGold = Mathf.RoundToInt(Random.Range(0.5f, 2.0f) * qInfo.Quota);
                    qInfo.RewardInfluence = Mathf.RoundToInt(Random.Range(0.7f, 1.5f) * qInfo.Quota);
                    qInfo.SourceVillage = MyVillage;
                }

                spawnedQuest.transform.GetChild(0).rotation = new Quaternion(0,0,0,0);
                spawnedQuest.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
                spawnedQuest.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);

                spawnedQuest.transform.parent = QuestSpawnPoints[i].transform;

                return;
            }
        }
    }
    // used by the questparchments
    public bool IsPlayerInTrigger()
    {
        return _playerInTrigger;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _playerInTrigger = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _playerInTrigger = false;
        }
    }

    public GameObject GetRandomQuest()
    {
        GameObject result = null;

        for (int i = 0; i < QuestSpawnPoints.Count; ++i)
        {
            if (QuestSpawnPoints[i].transform.childCount > 0)
            {
                result = QuestSpawnPoints[i].transform.GetChild(0).gameObject;
                break;
            }
        }

        return result;
    }
}
