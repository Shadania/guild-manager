using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Villagers to wander around and make villages seem lively
public class VillagerBehaviour : MonoBehaviour
{
    #region Declarations
    public Village MyVillage;

    private NavMeshAgent _myNavMeshAgent;
    private GameObject _target;

    public TextMesh NameMesh;

    public GameObject OverheadInfo;

    public string VillagerName;

    private enum VillagerState
    {
        moving,
        idle
    }
    private VillagerState _vState = VillagerState.idle;

    private const float _maxIdleTime = 5.0f;
    private const float _minIdleTime = 1.0f;
    private float _idleTimeToGo;
    #endregion Declarations


    private void Awake()
    {
        _myNavMeshAgent = GetComponent<NavMeshAgent>();
        if (!_myNavMeshAgent)
            Debug.Log("Forgot to attach NavMeshAgent component to a villager");

        _target = new GameObject("thisVillagerTargetPos");
        _target.transform.parent = transform;

        _idleTimeToGo += Random.Range(_minIdleTime, _maxIdleTime);

        VillagerName = GameManager.Instance.NameGen.GenerateName();

        NameMesh.text = VillagerName;


    }
    private void Update()
    {
        if (MyVillage)
        {
            switch (_vState)
            {
                case VillagerState.idle:
                    _idleTimeToGo -= Time.deltaTime;

                    if (_idleTimeToGo < 0)
                    {
                        _idleTimeToGo += Random.Range(_minIdleTime, _maxIdleTime);
                        StartMoving();
                    }
                    break;

                case VillagerState.moving:
                    if ((_myNavMeshAgent.velocity.sqrMagnitude < 0.005f) && _myNavMeshAgent.remainingDistance < .5f)
                    {
                        _vState = VillagerState.idle;
                        _myNavMeshAgent.isStopped = true;
                    }
                    break;
            }
        }

        // billboarding
        OverheadInfo.transform.rotation = Camera.main.transform.rotation;
    }

    private void StartMoving()
    {
        _myNavMeshAgent.isStopped = false;

        Vector3 newTargetPos;

        NavMeshPath path = new NavMeshPath();

        bool pathExists = false;

        do
        {
            newTargetPos.x = transform.parent.position.x + Mathf.Cos(Random.Range(0, 360)) * Random.Range(0, MyVillage.VillageRadius);
            newTargetPos.y = MyVillage.gameObject.transform.position.y;
            newTargetPos.z = transform.parent.position.z + Mathf.Sin(Random.Range(0, 360)) * Random.Range(0, MyVillage.VillageRadius);

            newTargetPos.y += 100.0f;
            Ray topDownRay = new Ray(newTargetPos, new Vector3(0, -1, 0));
            RaycastHit hitInfo;
            if (Physics.Raycast(topDownRay, out hitInfo, LayerMask.GetMask("Ground")))
            {
                newTargetPos.y = hitInfo.point.y;
            }
            
            _myNavMeshAgent.CalculatePath(newTargetPos, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                pathExists = true;
                _myNavMeshAgent.SetPath(path);
            }

        } while (!pathExists);
        
        _target.transform.position = newTargetPos;

        _vState = VillagerState.moving;
    }
}
