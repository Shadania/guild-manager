using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Village : MonoBehaviour
{
    #region Declarations
    private bool _initDone = false;

    public int MaxAmtVillagers;
    public float TimeBetweenVillagerSpawns = 20.0f;
    private float _accuTimeUntilNextVill;
    public float VillageRadius = 30.0f;
    public string VillageName = "Kortrijk";

    public GameObject HousesParentObj;
    public GameObject VillagerSpawnPoint;
    public GameObject VillagerPrefab;
    public List<GameObject> Villagers = new List<GameObject>();

    public List<GameObject> Council = new List<GameObject>();
    public GameObject King;

    public UnityEvent PlayerBecameKing = new UnityEvent();

    private bool _canSpawn = false;
    private bool _playerKing = false;

    [Header("Utility Buildings")]
    public GameObject TownHall;
    public GameObject QuestBoard;
    
    private const float _taxesGenerationInterval = 10.0f;
    private const float _taxPerVillager = 1.0f;
    private float _taxElapsedSec;
    private int _currTaxesAmt;

    private static List<Village> _allVillages = new List<Village>();
    #endregion Declarations

    public bool IsCouncilMember(GameObject person)
    {
        return Council.Contains(person);
    }
    public bool IsKing(GameObject person)
    {
        return King == person;
    }

    private void Awake()
    {
        // at the start, a village has only as many villagers as half the available houses
        MaxAmtVillagers = HousesParentObj.transform.childCount;
        for (int i = 0; i < MaxAmtVillagers / 2; ++i)
        {
            SpawnVillager();

            if (!King)
            {
                King = Villagers[i];
            }
            else if (Council.Count < 5)
            {
                Council.Add(Villagers[i]);
            }
        }

        _initDone = true;

        _allVillages.Add(this);

        PlayerBecameKing.AddListener(SetPlayerKing);
    }
    private void Update()
    {
        if (_canSpawn)
        {
            if (VillagerSpawnPoint.transform.childCount < MaxAmtVillagers)
            {
                _accuTimeUntilNextVill += Time.deltaTime;
                if (_accuTimeUntilNextVill >= TimeBetweenVillagerSpawns)
                {
                    _accuTimeUntilNextVill -= TimeBetweenVillagerSpawns;
                    SpawnVillager();
                }
            }
            else
                _accuTimeUntilNextVill = 0;
        }
        
        if (_playerKing)
        {
            HandleTaxes();
        }
    }

    private void SetPlayerKing()
    {
        _playerKing = true;
    }
    private void SpawnVillager()
    {
        GameObject newVill = Instantiate(VillagerPrefab, VillagerSpawnPoint.transform);
        Villagers.Add(newVill);



        VillagerBehaviour vBehav = newVill.GetComponent<VillagerBehaviour>();
        if (vBehav)
        {
            vBehav.MyVillage = this;

            NavMeshPath navPath = new NavMeshPath();

            NavMeshAgent navAgent = vBehav.gameObject.GetComponent<NavMeshAgent>();

            if (!navAgent)
                return;

            Vector3 newVillPos;

            bool pathExists = false;

            do {
                newVillPos.x = VillagerSpawnPoint.transform.position.x + Mathf.Cos(Random.Range(0, 360)) * Random.Range(0, VillageRadius);
                newVillPos.y = VillagerSpawnPoint.transform.position.y;
                newVillPos.z = VillagerSpawnPoint.transform.position.z + Mathf.Sin(Random.Range(0, 360)) * Random.Range(0, VillageRadius);

                newVillPos.y += 100.0f;
                Ray topDownRay = new Ray(newVillPos, new Vector3(0, -1, 0));
                RaycastHit hitInfo;
                if (Physics.Raycast(topDownRay, out hitInfo, LayerMask.GetMask("Ground")))
                {
                    newVillPos.y = hitInfo.point.y;
                }

                navAgent.CalculatePath(newVillPos, navPath);

                if (navPath.status == NavMeshPathStatus.PathComplete)
                {
                    pathExists = true;
                    navAgent.SetPath(navPath);
                }

            } while (!pathExists);

            newVill.transform.position = newVillPos;
        }
        else
            Debug.Log("Your Villager prefab doesn't have a VillagerBehaviour on it!");

        if (_initDone)
            TownHall.GetComponent<Townhall>().HandleVillagerAmtChanged();
    }

    #region Taxes
    private void HandleTaxes()
    {
        _taxElapsedSec += Time.deltaTime;
        if (_taxElapsedSec >= _taxesGenerationInterval)
        {
            _taxElapsedSec -= _taxesGenerationInterval;

            int amtVillagers = 0;
            for (int i = 0; i < Villagers.Count; ++i)
            {
                if (Villagers[i])
                    amtVillagers++;
                else
                    Villagers.Remove(Villagers[i]);
            }

            _currTaxesAmt += Mathf.RoundToInt(_taxPerVillager * amtVillagers);
            TownHall.GetComponent<Townhall>().UpdateTaxesAmt(_currTaxesAmt);
        }
    }
    public int ClaimTaxes()
    {
        int result = _currTaxesAmt;
        _currTaxesAmt = 0;
        return result;
    }
    #endregion Taxes

    #region Statics
    public static Village GetClosestVillageTo(Vector3 pos)
    {
        Village closestVillage = null;
        float closestDistSqr = 999999.0f;

        for (int i = 0; i < _allVillages.Count; ++i)
        {
            float distSqr = (_allVillages[i].transform.position - pos).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestVillage = _allVillages[i];
            }
        }

        return closestVillage;
    }
    public static List<Village> GetVillages()
    {
        return _allVillages;
    }
    public static void StartSpawningVillagers()
    {
        for (int i = 0; i < _allVillages.Count; ++i)
        {
            _allVillages[i]._canSpawn = true;
        }
    }
    #endregion Statics
}
