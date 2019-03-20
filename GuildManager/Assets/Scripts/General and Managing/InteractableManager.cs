using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages all interactables & interaction with them
public class InteractableManager : MonoBehaviour
{
    #region singleton
    private static InteractableManager _instance = null;
    public static InteractableManager Instance
    {
        get
        {
            if (_instance == null && !_applicationIsQuiting)
            {
                _instance = FindObjectOfType<InteractableManager>();
                if (_instance == null)
                {
                    GameObject newObject = new GameObject("Singleton_InteractableManager");
                    _instance = newObject.AddComponent<InteractableManager>();
                }
            }

            return _instance;
        }
    }
    private static bool _applicationIsQuiting = false;

    private void OnApplicationQuit()
    {
        _applicationIsQuiting = true;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion singleton


    private List<GameObject> _interactables = new List<GameObject>();
    private List<GameObject> _harvestables = new List<GameObject>();

    public void RegisterObject(GameObject obj)
    {
        if (!_interactables.Contains(obj))
            _interactables.Add(obj);
    }
    public void RegisterHarvestable(GameObject obj)
    {
        if (!_harvestables.Contains(obj))
            _harvestables.Add(obj);
        RegisterObject(obj);
    }

    public void UnRegisterObject(GameObject obj)
    {
        _interactables.Remove(obj);
        _harvestables.Remove(obj);
    }



    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hitInfo;

            // players have to mouse over which thing they want to interact with
            if (Physics.Raycast(mouseRay, out hitInfo, 1000.0f, LayerMask.GetMask("Interactable")))
            {
                Interactable interac = hitInfo.transform.GetComponent<Interactable>();
                if (interac && _interactables.Contains(hitInfo.transform.gameObject))
                {
                    interac.PlayerInteracted();
                }
            }
        }
    }


    public GameObject FindClosestHarvestableOfType(Inventory.ResourceType targetType, Transform target)
    {
        if (target == null)
            return null;

        GameObject closestHarvestable = null;
        float closestDistanceSQ = 100000.0f;

        for (int i = 0; i < _harvestables.Count; ++i)
        {
            if (_harvestables[i] == null)
                continue;

            if (_harvestables[i].GetComponent<Harvestable>().HarvestableType != targetType)
                continue;

            float distanceSQ = (_harvestables[i].transform.position - target.position).sqrMagnitude;

            if (distanceSQ < closestDistanceSQ)
            {
                closestDistanceSQ = distanceSQ;
                closestHarvestable = _harvestables[i];
            }
        }

        return closestHarvestable;
    }

    public void DeleteAllLists()
    {
        _interactables.Clear();
        _harvestables.Clear();
    }
}
