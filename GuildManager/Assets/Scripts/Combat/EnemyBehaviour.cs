using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Behaviour of all enemies
public class EnemyBehaviour : MonoBehaviour
{
    #region Declarations
    [Header("UI")]
    public GameObject OverHeadInfo;
    public GameObject Target;
    
    public float PlayerDetectRange = 100.0f;
    
    public GameObject VisibleArmor;
    public GameObject VisibleWeapon;
    
    private float _damage;
    private NavMeshAgent _myNavMeshAgent;
    private Health _myHealth;

    private Material _origMat;
    private int _influenceGivenOnDeath = 5;

    private const float _maxAttackChargeup = 0.3f;
    private float _accuAttackChargeup;
    private const float _maxAttackCooldown = 0.5f;
    private float _accuAttackCooldown;
    private const float _minIdleTime = 1.0f;
    private const float _maxIdleTime = 5.0f;
    private float _currIdleTimeLeft;
    private Vector3 wanderTarget = new Vector3();
    private const float _attackRange = 5.0f;


    private static List<EnemyBehaviour> _enemyList = new List<EnemyBehaviour>();

    private enum State
    {
        Idle,
        Wandering,
        Chasing,
        Attacking,
        AttackCooldown
    }
    private State _state;
    #endregion Declarations




    private void Start()
    {
        _enemyList.Add(this);

        _origMat = GetComponent<MeshRenderer>().material;


        _myNavMeshAgent = GetComponent<NavMeshAgent>();
        if (!_myNavMeshAgent)
            Debug.Log("Forgot to attach a navmesh agent to enemy");
        _myHealth = GetComponent<Health>();
        if (!_myHealth)
            Debug.Log("Forgot to attach health to enemy");

        _myHealth.onTakeDamage.AddListener(HandleTookDamage);
        _myHealth.onDeath.AddListener(HandleDeath);

        InitArmorAndWeapon();
    }
    private void Update()
    {
        // billboarding
        OverHeadInfo.transform.rotation = Camera.main.transform.rotation;

        HandleState();
    }

    private void InitArmorAndWeapon()
    {
        float armorChance = Random.Range(0.0f, 100.0f);
        if (armorChance < 30.0f)
        {
            VisibleArmor.SetActive(false);
            _myHealth.IncomingDamageMultiplier = 1.0f;
        }
        else if (armorChance < 60.0f)
        {
            VisibleArmor.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialWood;
            _myHealth.IncomingDamageMultiplier = 0.8f;
        }
        else if (armorChance < 80.0f)
        {
            VisibleArmor.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialIron;
            _myHealth.IncomingDamageMultiplier = 0.65f;
        }
        else if (armorChance < 95.0f)
        {
            VisibleArmor.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialSteel;
            _myHealth.IncomingDamageMultiplier = 0.5f;
        }
        else
        {
            VisibleArmor.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialMithril;
            _myHealth.IncomingDamageMultiplier = 0.35f;
        }


        float weaponChance = Random.Range(0.0f, 100.0f);
        if (weaponChance < 30.0f)
        {
            VisibleWeapon.SetActive(false);
            _damage = 10.0f;
        }
        else if (weaponChance < 60.0f)
        {
            VisibleWeapon.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialWood;
            _damage = 20.0f;
        }
        else if (weaponChance < 80.0f)
        {
            VisibleWeapon.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialIron;
            _damage = 30.0f;
        }
        else if (weaponChance < 95.0f)
        {
            VisibleWeapon.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialSteel;
            _damage = 40.0f;
        }
        else
        {
            VisibleWeapon.GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialMithril;
            _damage = 50.0f;
        }

    }


    #region Combat
    private void HandleDeath(GameObject source)
    {
        LootCrate.SpawnResourcesOnly(transform);


        // give influence to village
        Village toInfluenceVill = Village.GetClosestVillageTo(transform.position);

        GuildMemberController gmContr = source.GetComponent<GuildMemberController>();
        PlayerController pContr;
        if (gmContr)
        {
            gmContr.NPCsGuild.AddInfluenceFor(toInfluenceVill.gameObject, _influenceGivenOnDeath);
        }
        else if (pContr = source.GetComponent<PlayerController>())
        {
            pContr.MyGuild.AddInfluenceFor(toInfluenceVill.gameObject, _influenceGivenOnDeath);
        }


        Destroy(gameObject);
    }
    private void HandleTookDamage(GameObject source)
    {
        GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialHurt;
        Invoke("ResetMaterial", 0.1f);
        Target = source;
        _state = State.Chasing;

        GetComponent<AudioSource>().clip = GameManager.Instance.HurtSoundEffect;
        GetComponent<AudioSource>().Play();
    }
    private void ResetMaterial()
    {
        GetComponent<MeshRenderer>().material = _origMat;
    }
    #endregion Combat

    // Updating, idling
    private void HandleState()
    {
        switch (_state)
        {
            case State.Idle:
                if ((GameManager.Instance.PlayerAvatar.transform.position - transform.position).sqrMagnitude < PlayerDetectRange)
                {
                    _state = State.Chasing;
                    Target = GameManager.Instance.PlayerAvatar;
                }
                else
                {
                    _currIdleTimeLeft -= Time.deltaTime;
                    if (_currIdleTimeLeft < 0.0f)
                    {
                        _currIdleTimeLeft = Random.Range(_minIdleTime, _maxIdleTime);
                        _state = State.Wandering;
                        Vector3 targetPos = transform.position;
                        NavMeshPath path = new NavMeshPath();
                        do
                        {
                            // float range = Random.Range(0, 10.0f);
                            float angle = Random.Range(0, Mathf.PI * 2);
                            targetPos.x += Mathf.Sin(angle) * Random.Range(0, 10.0f);
                            targetPos.z += Mathf.Cos(angle) * Random.Range(0, 10.0f);
                            targetPos.y += 100.0f;

                            Ray topDownRay = new Ray(targetPos, new Vector3(0, -1, 0));
                            RaycastHit hitInfo;
                            if (Physics.Raycast(topDownRay, out hitInfo, LayerMask.GetMask("Ground")))
                            {
                                targetPos.y = hitInfo.point.y;
                            }

                            Debug.Log("Trying to calculate wander path");

                            _myNavMeshAgent.CalculatePath(targetPos, path);
                        } while (path.status != NavMeshPathStatus.PathComplete);

                        wanderTarget = targetPos;

                        _myNavMeshAgent.SetPath(path);
                    }
                }
                break;
            case State.Wandering:
                if ((GameManager.Instance.PlayerAvatar.transform.position - transform.position).sqrMagnitude < PlayerDetectRange)
                {
                    _state = State.Chasing;
                    Target = GameManager.Instance.PlayerAvatar;
                }
                else
                {
                    if ((_myNavMeshAgent.velocity.sqrMagnitude < 0.01f) && ((wanderTarget - transform.position).sqrMagnitude < 1.0f))
                    {
                        _state = State.Idle;
                    }
                }
                break;
            case State.Chasing:
                if (Target)
                {
                    if ((Target.transform.position - transform.position).sqrMagnitude > PlayerDetectRange)
                    {
                        // stop following
                        _state = State.Idle;
                        Target = null;
                    }
                    else
                    {
                        // can keep following
                        _myNavMeshAgent.SetDestination(Target.transform.position);
                        if ((Target.transform.position - transform.position).sqrMagnitude < _attackRange / 2)
                        {
                            _state = State.Attacking;
                            _myNavMeshAgent.isStopped = true;
                        }
                    }
                }
                else
                    _state = State.Idle;
                break;

            case State.Attacking:
                _accuAttackChargeup += Time.deltaTime;
                if (_accuAttackChargeup >= _maxAttackChargeup)
                {
                    _accuAttackChargeup = 0.0f;

                    if ((Target.transform.position - transform.position).sqrMagnitude < _attackRange)
                    {
                        Health targetHealth = Target.GetComponent<Health>();
                        if (targetHealth)
                            targetHealth.DealDamage(_damage, gameObject);
                    }
                    _state = State.AttackCooldown;
                }
                break;

            case State.AttackCooldown:
                _accuAttackCooldown += Time.deltaTime;
                if (_accuAttackCooldown >= _maxAttackCooldown)
                {
                    _accuAttackCooldown = 0.0f;
                    _state = State.Chasing;
                    _myNavMeshAgent.isStopped = false;
                }
                
                break;
        }
    }


    #region static functions
    public static void TryHitEnemy(float range, float angle, GameObject attacker, float damage)
    {
        for (int i = 0; i < _enemyList.Count; ++i)
        {
            if (!_enemyList[i])
            {
                _enemyList.Remove(_enemyList[i]);
                continue;
            }

            EnemyBehaviour thisEnemy = _enemyList[i];

            if ((thisEnemy.transform.position - attacker.transform.position).sqrMagnitude > (range * range))
                continue;

            Vector3 attackerToTarget = thisEnemy.transform.position - attacker.transform.position;
            Vector3 newForward = attacker.transform.forward;
            newForward.y = 0;
            if (Mathf.Abs(Vector3.Angle(attackerToTarget, newForward)) > angle)
                continue;

            thisEnemy.GetComponent<Health>().DealDamage(damage, attacker);

            newForward.y = 10.0f;
            thisEnemy.GetComponent<NavMeshAgent>().velocity = newForward;
        }
    }
    public static GameObject GetClosestEnemy(Vector3 pos)
    {
        GameObject closestEnemy = null;
        float closestDistanceSqr = 999999.0f;

        for (int i = 0; i < _enemyList.Count; ++i )
        {
            if (!_enemyList[i])
            {
                _enemyList.Remove(_enemyList[i]);
                continue;
            }

            float dist = (_enemyList[i].transform.position - pos).sqrMagnitude;
            if (dist < closestDistanceSqr)
            {
                closestDistanceSqr = dist;
                closestEnemy = _enemyList[i].gameObject;
            }
        }

        return closestEnemy;
    }

    #endregion static functions

    private void OnDestroy()
    {
        _enemyList.Remove(this);
    }
}
