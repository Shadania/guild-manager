using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


// everything you need in a guild member
// lots of UI and AI
public class GuildMemberController : MonoBehaviour
{
    #region Declarations
    [Header("General")]
    public GameObject Target;
    private NavMeshAgent _attachedNavMeshAgent;
    public Guild NPCsGuild;

    public TextMesh NameMesh;
    public GameObject EnemyOverheadInfo;
    private static GameObject _camera;

    [Header("Giving orders/Interaction Menu")]
    public GameObject MainMenu;
    public GameObject NewNameMenu;
    public GameObject InventoryMenu;
    public GameObject ChangeStatusMenu;
    public InputField NewNameInputField;
    public Text MemberMenuName; // set by Guild.cs
    public Button ChangeStatusButton;
    public Button ChangeNameButton;
    public Button InventoryButton;
    public Button SetHomeButton;
    private bool _isMenuOpen;
    public float PlayerInteractRange = 5.0f;
    public const float MaxActivityInterval = 1.0f;

    [Header("Status handling")]
    public Text CurrentStatusText;
    public Button StatusIdleButton;
    public Button StatusFollowButton;
    public Button StatusQuestButton;
    public Button StatusGoHomeButton;
    public Button StatusFindAndDoQuestsButton;


    [Header("Gear")]
    public GameObject GearMenu;
    public Button GearButton;
    public GameObject WeaponSlot;
    public GameObject ArmorSlot;
    public GameObject VisibleArmor;
    public GameObject VisibleWeapon;
    public GameObject NPCItemHolder;
    public GameObject PlayerItemHolder;


    [Header("Quests")]
    public GameObject QuestMenu;
    public GameObject GiveQuestsMenu;
    public Button QuestsButton;
    public Button GiveQuestsButton;
    public GameObject PlayerQuestWrapper;
    public GameObject PlayerQuestsHolder;
    


    [Header("Autonomous doing quests")]
    private Quest _currentQ;
    private enum QuestActivity
    {
        cannotDo,
        onMyWay,
        doingActivity,
        needNewTarget,
        gettingNewQuest
    }
    private QuestActivity _currQuestActivity = QuestActivity.cannotDo;
    
    private enum MenuType // keep track of the open menu
    {
        Main,
        Quest,
        GiveQuests,
        Name,
        Inventory,
        Status,
        Gear,
        EnemyStrategy,
        PickVillageToDefend
    }
    private MenuType _whichMenuActive;

    private enum Status
    {
        Idle,
        Follow,
        Quest,
        ReturningHome
    }
    private Status _status = Status.Idle;

    private Transform _homePos;


    [Header("Enemy handling")]
    public Button StrategyIgnoreButton;
    public Button StrategyGuardButton;
    public Button StrategyAggressiveButton;
    public Button StrategyDefensiveButton;
    public Text StrategyText;
    public GameObject TargetedEnemy;
    public GameObject EnemyStrategyMenu;
    public Button EnemyStrategyButton;
    public GameObject VillageToDefendCardPrefab;
    public GameObject VillageToDefendMenu;
    public GameObject VillageToDefendMenuItemHolder;
    private Village _villToDefend;
    private float _damage = 10.0f;

    private enum EnemyAlertStatus
    {
        Ignore,
        Guard,
        Aggressive,
        Defensive
    }
    private EnemyAlertStatus _currEnemyAlertStatus;

    private enum CombatStatus
    {
        OutOfCombat,
        Chasing,
        Attacking,
        Cooldown
    }
    private CombatStatus _currCombatStatus = CombatStatus.OutOfCombat;

    private bool _fullyAutonomousQuesting = false;
    #endregion Declarations

    private void Start()
    {
        if (NPCsGuild)
        {
            GameObject homeLoc = new GameObject("HomeLocation");
            homeLoc.transform.parent = NPCsGuild.HomeLocations.transform;
            homeLoc.transform.position = NPCsGuild.MemberSpawnPoint.position;
            _homePos = homeLoc.transform;
        }

        _attachedNavMeshAgent = GetComponent<NavMeshAgent>();
        if (_attachedNavMeshAgent)
        {
            _attachedNavMeshAgent.stoppingDistance = 2.0f;
            _attachedNavMeshAgent.autoBraking = true;
        }

        if (!_camera)
        {
            _camera = Camera.main.gameObject;
        }

        // main menu buttons
        ChangeStatusButton.onClick.AddListener(ChangeStatusButtonClicked);
        ChangeNameButton.onClick.AddListener(ChangeNameButtonClicked);
        QuestsButton.onClick.AddListener(QuestButtonClicked);
        GiveQuestsButton.onClick.AddListener(GiveQuestsButtonClicked);
        InventoryButton.onClick.AddListener(InventoryButtonClicked);
        GearButton.onClick.AddListener(GearButtonClicked);
        SetHomeButton.onClick.AddListener(SetHomeButtonClicked);
        EnemyStrategyButton.onClick.AddListener(EnemyStrategyButtonClicked);
        NewNameInputField.onEndEdit.AddListener(NameChanged);

        // Status menu buttons
        StatusIdleButton.onClick.AddListener(SetStatusIdle);
        StatusFollowButton.onClick.AddListener(SetStatusFollow);
        StatusQuestButton.onClick.AddListener(SetStatusQuest);
        StatusGoHomeButton.onClick.AddListener(SetStatusReturn);
        StatusFindAndDoQuestsButton.onClick.AddListener(SetStatusAutonomousQuests);

        // Strategy Buttons
        StrategyIgnoreButton.onClick.AddListener(StrategyIgnoreButtonClicked);
        StrategyGuardButton.onClick.AddListener(StrategyGuardButtonClicked);
        StrategyAggressiveButton.onClick.AddListener(StrategyAggressiveButtonClicked);
        StrategyDefensiveButton.onClick.AddListener(StrategyDefensiveButtonClicked);

        // set original material
        _originalMaterial = GetComponent<MeshRenderer>().material;

        // interactable
        Interactable interac = GetComponent<Interactable>();
        if (interac)
        {
            interac.OnPlayerInteract.AddListener(HandleOpenInteractMenu);
        }

        // quest recheck
        QuestHolderNPC qHolder = GetComponent<QuestHolderNPC>();
        if (qHolder)
        {
            qHolder.RecheckQuests.AddListener(RecheckQuestsFired);
        }

        // setting some text
        GetComponent<InventoryNPC>().GiveItemsText.text = "Give items to " + NameMesh.text;

        Health myHealth = GetComponent<Health>();
        if (myHealth)
        {
            myHealth.onTakeDamage.AddListener(TookDamageFrom);
            myHealth.onDeath.AddListener(KilledBy);
        }

        _allGuildies.Add(gameObject);
    }
    private void Update()
    {
        if (_isMenuOpen)
            if (Input.GetButtonDown("UICancel"))
                PressedUICancelWithMenuOpen();

        // billboarding overhead info
        EnemyOverheadInfo.transform.rotation = _camera.transform.rotation;

        // if ((_currEnemyAlertStatus == EnemyAlertStatus.Ignore) && !TargetedEnemy)
        // {
        //     HandleStatus();
        // }
        if ((_currCombatStatus == CombatStatus.OutOfCombat) || !TargetedEnemy)
        {
            HandleStatus();
        }

        HandleEnemyStrategy();

        HandleCombatStatus();
    }
    

    #region Status button presses
    private void SetStatusIdle()
    {
        SetStatus(Status.Idle);
    }
    private void SetStatusFollow()
    {
        SetStatus(Status.Follow);
    }
    private void SetStatusQuest()
    {
        _fullyAutonomousQuesting = false;
        SetStatus(Status.Quest);
    }
    private void SetStatusReturn()
    {
        SetStatus(Status.ReturningHome);
    }
    private void SetStatusAutonomousQuests()
    {
        _fullyAutonomousQuesting = true;
        SetStatus(Status.Quest);
    }
    #endregion Status button presses

    #region Strategy button presses
    private void StrategyIgnoreButtonClicked()
    {
        _currEnemyAlertStatus = EnemyAlertStatus.Ignore;
        StrategyText.text = "Current strategy: Ignore";
    }
    private void StrategyGuardButtonClicked()
    {
        // bring up menu & ask for shit
        EnemyStrategyMenu.SetActive(false);
        VillageToDefendMenu.SetActive(true);
        _whichMenuActive = MenuType.PickVillageToDefend;

        for (int i = 0; i < VillageToDefendMenuItemHolder.transform.childCount; ++i)
        {
            Destroy(VillageToDefendMenuItemHolder.transform.GetChild(i).gameObject);
        }
        
        foreach (var villStats in NPCsGuild.Influences)
        {
            GameObject card = Instantiate(VillageToDefendCardPrefab, VillageToDefendMenuItemHolder.transform);
            VillageToDefendScreenCard cardScript = card.GetComponent<VillageToDefendScreenCard>();

            cardScript.MyVill = villStats.Key.GetComponent<Village>();
            cardScript.CardTitle.text = "Village: " + cardScript.MyVill.VillageName;
            cardScript.CardInfluence.text = "Influence: " + villStats.Value.ToString();
            cardScript.WasChosen.AddListener(VillageDefenseCardPicked);
        }
    }
    private void VillageDefenseCardPicked(Village vill)
    {
        VillageToDefendMenu.SetActive(false);
        EnemyStrategyMenu.SetActive(true);
        _whichMenuActive = MenuType.EnemyStrategy;
        _villToDefend = vill;
        _currEnemyAlertStatus = EnemyAlertStatus.Guard;
        StrategyText.text = "Current strategy: Guarding ";
        StrategyText.text += vill.VillageName;
        _currPatrolPointIdx = vill.GetComponent<PatrolRoute>().GetClosestPointTo(transform.position);
    }
    private void StrategyAggressiveButtonClicked()
    {
        _currEnemyAlertStatus = EnemyAlertStatus.Aggressive;
        StrategyText.text = "Current strategy: Aggressive";
    }
    private void StrategyDefensiveButtonClicked()
    {
        _currEnemyAlertStatus = EnemyAlertStatus.Defensive;
        StrategyText.text = "Current strategy: Defensive";
    }



    #endregion Strategy button presses

    #region Updating
    public void SetTarget(Transform newTarget)
    {
        if (!_attachedNavMeshAgent)
        {
            Debug.Log("You forgot to attach a NavMeshAgent to your GuildMember!");
            return;
        }
        _attachedNavMeshAgent.isStopped = false;
        _attachedNavMeshAgent.SetDestination(newTarget.transform.position);
    }
    public void SetGuild(Guild guild)
    {
        NPCsGuild = guild;
    }
    private void SetStatus(Status newStatus)
    {
        _status = newStatus;

        switch (_status)
        {
            case Status.Idle:
                // SetTarget(transform);
                // _attachedNavMeshAgent.isStopped = true;
                Target = null;
                CurrentStatusText.text = "Current status: idling.";
                break;
            case Status.Follow:
                _attachedNavMeshAgent.isStopped = false;
                Target = NPCsGuild.GuildLeader;
                _attachedNavMeshAgent.stoppingDistance = 2.0f;
                CurrentStatusText.text = "Current status: following you.";
                break;
            case Status.Quest:
                _attachedNavMeshAgent.isStopped = false;
                _attachedNavMeshAgent.stoppingDistance = 0.01f;
                CurrentStatusText.text = "Current status: doing quests.";
                FindNewQuest();
                break;
            case Status.ReturningHome:
                _attachedNavMeshAgent.isStopped = false;
                CurrentStatusText.text = "Current status: returning to home position.";
                SetTarget(_homePos);
                break;
        }

    }

    private const float _enemyDetectRange = 20.0f;

    private int _currPatrolPointIdx;
    private float _accuPatrolSeconds;
    private const float _maxPatrolSeconds = 30.0f;
    private void HandleEnemyStrategy()
    {
        switch (_currEnemyAlertStatus)
        {
            case EnemyAlertStatus.Aggressive:
                if (!TargetedEnemy)
                {
                    GameObject closestEnemy = EnemyBehaviour.GetClosestEnemy(transform.position);
                    if (closestEnemy)
                    {
                        if ((closestEnemy.transform.position - transform.position).sqrMagnitude < (_enemyDetectRange * _enemyDetectRange))
                        {
                            TargetedEnemy = closestEnemy;
                            _currCombatStatus = CombatStatus.Chasing;
                        }
                    }
                }

                if (TargetedEnemy)
                {
                    if ((TargetedEnemy.transform.position - transform.position).sqrMagnitude > (30.0f * 30.0f))
                    {
                        TargetedEnemy = null;
                        _currCombatStatus = CombatStatus.OutOfCombat;
                    }
                    else
                        SetTarget(TargetedEnemy.transform);
                }
                else if (Target)
                    SetTarget(Target.transform);

                break;

            case EnemyAlertStatus.Defensive:

                if (TargetedEnemy)
                {
                    if ((TargetedEnemy.transform.position - transform.position).sqrMagnitude > (30.0f * 30.0f))
                    {
                        TargetedEnemy = null;
                        _currCombatStatus = CombatStatus.OutOfCombat;
                    }
                    else
                    {
                        _attachedNavMeshAgent.isStopped = false;
                        SetTarget(TargetedEnemy.transform);
                    }
                }


                break;

            case EnemyAlertStatus.Ignore: // literally nothing
                break;

            case EnemyAlertStatus.Guard:

                _accuPatrolSeconds += Time.deltaTime;
                if (_accuPatrolSeconds >= _maxPatrolSeconds)
                {
                    _accuPatrolSeconds -= _maxPatrolSeconds;
                    NPCsGuild.AddInfluenceFor(_villToDefend.gameObject, 1);
                }

                if (!TargetedEnemy)
                {
                    // Handle patrolling
                    float acceptableDistance = 5.0f;
                    GameObject currPatrolPoint = _villToDefend.GetComponent<PatrolRoute>().RoutePoints[_currPatrolPointIdx];
                    if ((currPatrolPoint.transform.position - transform.position).sqrMagnitude < acceptableDistance)
                    {
                        _currPatrolPointIdx = (_currPatrolPointIdx + 1) % _villToDefend.GetComponent<PatrolRoute>().RoutePoints.Count;
                    }
                    Target = _villToDefend.GetComponent<PatrolRoute>().RoutePoints[_currPatrolPointIdx];

                    // Search for an enemy
                    GameObject closestEnemy = EnemyBehaviour.GetClosestEnemy(transform.position);
                    if (closestEnemy)
                    {
                        if ((closestEnemy.transform.position - transform.position).sqrMagnitude < (_enemyDetectRange * _enemyDetectRange))
                        {
                            TargetedEnemy = closestEnemy;
                            _currCombatStatus = CombatStatus.Chasing;
                        }
                    }
                }

                if (!TargetedEnemy)
                {
                    _attachedNavMeshAgent.SetDestination(Target.transform.position);
                }
                else
                    _attachedNavMeshAgent.SetDestination(TargetedEnemy.transform.position);


                break;
        }
    }

    private const float _maxAttackChargeTime = 0.3f;
    private float _currAttackChargeTime;
    private const float _maxCooldownTime = 0.5f;
    private float _currCooldownTime;

    private const float _enemyAttackRange = 5.0f;
    private void HandleCombatStatus()
    {
        switch(_currCombatStatus)
        {
            case CombatStatus.OutOfCombat:
                break;

            case CombatStatus.Chasing:
                // if enemy is in range, start attacking
                if (TargetedEnemy)
                {
                    if ((TargetedEnemy.transform.position - transform.position).sqrMagnitude < ((_enemyAttackRange / 2) * (_enemyAttackRange / 2)))
                    {
                        _currCombatStatus = CombatStatus.Attacking;
                        _currAttackChargeTime = 0.0f;
                        _currCooldownTime = 0.0f;
                    }
                }
                
            
                break;
            case CombatStatus.Attacking:
                _currAttackChargeTime += Time.deltaTime;
                if (_currAttackChargeTime >= _maxAttackChargeTime)
                {
                    _currAttackChargeTime = 0.0f;

                    // try to deal damage
                    if (!TargetedEnemy)
                    {
                        _currCombatStatus = CombatStatus.OutOfCombat;
                        break;
                    }
                    if ((TargetedEnemy.transform.position - transform.position).sqrMagnitude < (_enemyAttackRange * _enemyAttackRange))
                    {
                        TargetedEnemy.GetComponent<Health>().DealDamage(_damage, gameObject);
                    }
                    _currCombatStatus = CombatStatus.Cooldown;
                }
                break;
            case CombatStatus.Cooldown:
                _currCooldownTime += Time.deltaTime;
                if (_currCooldownTime >= _maxCooldownTime)
                {
                    _currCooldownTime = 0.0f;
                    _currCombatStatus = CombatStatus.Chasing;
                }
                break;
        }
    }



    private float _acceptableHomeRange = 5.0f;
    private void HandleStatus()
    {
        switch(_status)
        {
            case Status.Idle: // this should never even be getting called
                break;

            case Status.Follow:
                Target = NPCsGuild.GuildLeader;
                SetTarget(Target.transform);
                break;

            case Status.Quest:
                HandleQuestStatus();
                break;

            case Status.ReturningHome:
                if (_attachedNavMeshAgent.velocity.sqrMagnitude < 0.01f)
                {
                    if ((transform.position - _homePos.position).sqrMagnitude < _acceptableHomeRange)
                    {
                        _attachedNavMeshAgent.isStopped = true;
                        _status = Status.Idle;
                        CurrentStatusText.text = "Current status: idling.";
                    }
                    else
                        SetTarget(_homePos);
                }
                break;
        }
    }
    #endregion Updating

    #region MainMenuClicks
    private void ChangeStatusButtonClicked()
    {
        MainMenu.SetActive(false);
        ChangeStatusMenu.SetActive(true);
        _whichMenuActive = MenuType.Status;
    }
    private void ChangeNameButtonClicked()
    {
        MainMenu.SetActive(false);
        NewNameMenu.SetActive(true);
        _whichMenuActive = MenuType.Name;
    }
    private void QuestButtonClicked()
    {
        MainMenu.SetActive(false);
        QuestMenu.SetActive(true);
        _whichMenuActive = MenuType.Quest;
    }
    private void GiveQuestsButtonClicked()
    {
        // copy all the player's quests into a new menu, wrapping every quest into a new
        // panel with an extra button

        // first: get the player's quest holder
        QuestHolder playerQuests = null;

        GameObject player = NPCsGuild.GuildLeader;
        if (player)
        {
            playerQuests = player.GetComponent<QuestHolder>();
        }

        // then put all the quests in the wrapper
        if (playerQuests)
        {
            Transform pQuestParent = playerQuests.QuestsHolder.transform;
            
            while (pQuestParent.childCount != 0)
            {
                GameObject qWrap = Instantiate(PlayerQuestWrapper);
                qWrap.transform.SetParent(PlayerQuestsHolder.transform);
                qWrap.transform.localScale = new Vector3(1, 1, 1);
                qWrap.GetComponent<PlayerQuestWrapper>().SetPlayerQuest(pQuestParent.GetChild(0).gameObject);
                pQuestParent.GetChild(0).SetParent(qWrap.transform);
                qWrap.GetComponent<PlayerQuestWrapper>().PlayerQuest.transform.localScale = new Vector3(1, 1, 1);
                qWrap.GetComponent<PlayerQuestWrapper>().PlayerQuest.transform.rotation = new Quaternion(0,0,0,0);
                qWrap.GetComponent<PlayerQuestWrapper>().TargetNPC = GetComponent<QuestHolderNPC>();
            }
        }


        // then change the menus
        QuestMenu.SetActive(false);
        GiveQuestsMenu.SetActive(true);
        _whichMenuActive = MenuType.GiveQuests;
    }
    private void InventoryButtonClicked()
    {
        MainMenu.SetActive(false);
        InventoryMenu.SetActive(true);
        _whichMenuActive = MenuType.Inventory;
    }
    private void NameChanged(string newName)
    {
        if (newName.Length > 0)
        {
            NameMesh.text = newName;
            MemberMenuName.text = newName;
            NewNameMenu.SetActive(false);
            MainMenu.SetActive(true);
            _whichMenuActive = MenuType.Main;
        }
    }
    private void HandleStatusChanged(int newStatus)
    {
        Status newState = (Status)newStatus;
        switch(newState)
        {
            case Status.Idle:
                // SetTarget(transform);
                _attachedNavMeshAgent.isStopped = true;
                CurrentStatusText.text = "Current status: idling.";
                break;
            case Status.Follow:
                _attachedNavMeshAgent.isStopped = false;
                SetTarget(NPCsGuild.GuildLeader.transform);
                _attachedNavMeshAgent.stoppingDistance = 2.0f;
                CurrentStatusText.text = "Current status: following you.";
                break;
            case Status.Quest:
                _attachedNavMeshAgent.isStopped = false;
                _attachedNavMeshAgent.stoppingDistance = 0.01f;
                CurrentStatusText.text = "Current status: doing quests.";
                FindNewQuest();
                break;
            case Status.ReturningHome:
                _attachedNavMeshAgent.isStopped = false;
                CurrentStatusText.text = "Current status: returning to home position.";
                SetTarget(_homePos);
                break;
        }

        _status = newState;
    }
    private void SetHomeButtonClicked()
    {
        _homePos.position = transform.position;
    }

    private Transform _originalNPCItemParent;
    private Transform _originalPlayerItemParent;
    private void GearButtonClicked()
    {
        MainMenu.SetActive(false);
        GearMenu.SetActive(true);
        _whichMenuActive = MenuType.Gear;

        // transfer items from NPC's inventory to gear menu's npc inv
        _originalNPCItemParent = GetComponent<InventoryNPC>().ItemHolder.transform.parent;
        _originalNPCItemParent.GetChild(0).SetParent(NPCItemHolder.transform);
        _originalNPCItemParent.GetComponent<ScrollRect>().content = null;
        NPCItemHolder.GetComponent<ScrollRect>().content =
            NPCItemHolder.transform.GetChild(0).GetComponent<Image>().rectTransform;
        NPCItemHolder.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        NPCItemHolder.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        NPCItemHolder.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);


        // transfer items from player's inv to gear menu's player inv
        _originalPlayerItemParent = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().ItemHolder.transform.parent;
        _originalPlayerItemParent.GetChild(0).SetParent(PlayerItemHolder.transform);
        _originalPlayerItemParent.GetComponent<ScrollRect>().content = null;
        PlayerItemHolder.GetComponent<ScrollRect>().content =
            PlayerItemHolder.transform.GetChild(0).GetComponent<Image>().rectTransform;
        PlayerItemHolder.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        PlayerItemHolder.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        PlayerItemHolder.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);

        // bind listeners
        for (int i = 0; i < NPCItemHolder.transform.GetChild(0).childCount; ++i)
        {
            NPCItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.AddListener(EquipItem);
        }

        for (int i = 0; i < PlayerItemHolder.transform.GetChild(0).childCount; ++i)
        {
            PlayerItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.AddListener(EquipItem);
        }
    }
    private void EnemyStrategyButtonClicked()
    {
        MainMenu.SetActive(false);
        EnemyStrategyMenu.SetActive(true);
        _whichMenuActive = MenuType.EnemyStrategy;
    }
    #endregion MainMenuClicks

    #region Gear
    private void EquipItem(InvItem item)
    {
        item.PlayerClicked.RemoveAllListeners();
        item.PlayerClicked.AddListener(UnequipItem);

        Material neededMat = GameManager.Instance.MaterialWood;
        switch ((int)item.ItemType % 4)
        {
            case 1:
                neededMat = GameManager.Instance.MaterialIron;
                break;
            case 2:
                neededMat = GameManager.Instance.MaterialSteel;
                break;
            case 3:
                neededMat = GameManager.Instance.MaterialMithril;
                break;
        }

        if ((int)item.ItemType <= 3)
        {
            if (ArmorSlot.transform.childCount > 0)
                GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().GiveItem(ArmorSlot.transform.GetChild(0).GetComponent<InvItem>());
            else
                VisibleArmor.SetActive(true);

            VisibleArmor.GetComponent<MeshRenderer>().material = neededMat;

            item.transform.SetParent(ArmorSlot.transform);
        }
        else
        {
            if (WeaponSlot.transform.childCount > 0)
                GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().GiveItem(WeaponSlot.transform.GetChild(0).GetComponent<InvItem>());
            else
                VisibleWeapon.SetActive(true);

            VisibleWeapon.GetComponent<MeshRenderer>().material = neededMat;
            
            item.transform.SetParent(WeaponSlot.transform);
        }

        item.transform.localPosition = new Vector3(0, 0, 0);
        item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localRotation = new Quaternion(0, 0, 0, 0);

        switch ((int)item.ItemType)
        {
            case 0: // armor
                GetComponent<Health>().IncomingDamageMultiplier = 0.8f;
                break;
            case 1:
                GetComponent<Health>().IncomingDamageMultiplier = 0.65f;
                break;
            case 2:
                GetComponent<Health>().IncomingDamageMultiplier = 0.5f;
                break;
            case 3:
                GetComponent<Health>().IncomingDamageMultiplier = 0.35f;
                break;
            case 4: // weapon
                _damage = 20.0f;
                break;
            case 5:
                _damage = 30.0f;
                break;
            case 6:
                _damage = 40.0f;
                break;
            case 7:
                _damage = 50.0f;
                break;
        }
    }
    private void UnequipItem(InvItem item)
    {
        if ((int)item.ItemType <= 3)
            VisibleArmor.SetActive(false);
        else
            VisibleWeapon.SetActive(false);

        GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().GiveItem(item);

        if ((int)item.ItemType <= 3)
            GetComponent<Health>().IncomingDamageMultiplier = 1.0f;
        else
            _damage = 10.0f;
    }
    #endregion Gear

    #region UI
    void HandleOpenInteractMenu()
    {
        if ((GameManager.Instance.PlayerAvatar.transform.position - transform.position).sqrMagnitude <= PlayerInteractRange)
        {
            if (!_isMenuOpen)
            {
                _isMenuOpen = true;
                MainMenu.SetActive(_isMenuOpen);

                _whichMenuActive = MenuType.Main;
                
                GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = false;
            }
        }

    }
    void PressedUICancelWithMenuOpen()
    {
        // close things depending on where in the menu we are

        switch (_whichMenuActive)
        {
            case MenuType.Main:
                // close things and give control back to the player
                _isMenuOpen = false;
                MainMenu.SetActive(_isMenuOpen);
                GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = true;
                break;

            case MenuType.Name:
                NewNameMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuActive = MenuType.Main;

                break;

            case MenuType.Quest:
                QuestMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuActive = MenuType.Main;
                break;

            case MenuType.GiveQuests:

                // give the quests back to the player
                QuestHolder playerQuests = null;

                GameObject player = NPCsGuild.GuildLeader;
                if (player)
                {
                    playerQuests = player.GetComponent<QuestHolder>();
                }

                // get all the quests back out of the wrapper and give them back to the player
                if (playerQuests)
                {
                    Transform pQuestParent = playerQuests.QuestsHolder.transform;
                    // Transform thisQuestParent = PlayerQuestsHolder.transform;

                    int test = PlayerQuestsHolder.transform.childCount;
                    while (test > 0)
                    {
                        GameObject pQuestWrapper = PlayerQuestsHolder.transform.GetChild(test-1).gameObject;
                        pQuestWrapper.GetComponent<PlayerQuestWrapper>().GiveMyQuestTo(player.GetComponent<QuestHolder>());

                        --test;
                    }
                }

                // set menus
                GiveQuestsMenu.SetActive(false);
                QuestMenu.SetActive(true);
                _whichMenuActive = MenuType.Quest;
                break;

            case MenuType.Inventory:

                InventoryNPC inv = GetComponent<InventoryNPC>();
                if (inv)
                {
                    if (inv.GiveItemsMenuUp) // the inv handles this itself
                        break;
                }

                InventoryMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuActive = MenuType.Main;
                break;

            case MenuType.Status:
                ChangeStatusMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuActive = MenuType.Main;
                break;

            case MenuType.Gear:

                // remove all listeners
                for (int i = 0; i < NPCItemHolder.transform.GetChild(0).childCount; ++i)
                {
                    NPCItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.RemoveAllListeners();
                }
                for (int i = 0; i < PlayerItemHolder.transform.GetChild(0).childCount; ++i)
                {
                    PlayerItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.RemoveAllListeners();
                }

                // give back the items
                NPCItemHolder.transform.GetChild(0).SetParent(_originalNPCItemParent);
                NPCItemHolder.GetComponent<ScrollRect>().content = null;
                _originalNPCItemParent.GetComponent<ScrollRect>().content =
                    _originalNPCItemParent.GetChild(0).GetComponent<Image>().rectTransform;
                _originalNPCItemParent.GetChild(0).localPosition = new Vector3(0, 0, 0);
                _originalNPCItemParent.GetChild(0).localScale = new Vector3(1, 1, 1);
                _originalNPCItemParent.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);

                

                GearMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuActive = MenuType.Main;
                break;

            case MenuType.EnemyStrategy:
                EnemyStrategyMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuActive = MenuType.Main;
                break;

            case MenuType.PickVillageToDefend:

                VillageToDefendMenu.SetActive(false);
                EnemyStrategyMenu.SetActive(true);
                _whichMenuActive = MenuType.EnemyStrategy;

                break;
        }
    }
    #endregion UI
    
    #region Autonomous Questing
    private void RecheckQuestsFired()
    {
        Invoke("FindNewQuest", 0.5f);
    }

    private void FindNewQuest()
    {
        QuestHolderNPC myQuests = GetComponent<QuestHolderNPC>();

        if (myQuests)
        {
            if (myQuests.QuestsHolder.transform.childCount != 0) // there are quests
            {
                for (int i = 0; i < myQuests.QuestsHolder.transform.childCount; ++i)
                {
                    Transform thisQuest = myQuests.QuestsHolder.transform.GetChild(i);
                    if (thisQuest.GetComponent<NPCQuestWrapper>().QuestObject)
                        if (thisQuest.GetComponent<NPCQuestWrapper>().QuestObject.GetComponent<Quest>().QuestType != Quest.Type.GatherGold)
                        {
                            StartQuest(myQuests.QuestsHolder.transform.GetChild(i).GetComponent<NPCQuestWrapper>().QuestObject.GetComponent<Quest>());
                            return;
                        }
                }
            }

            // there were no (valid) quests

            if (!_fullyAutonomousQuesting)
            {
                SetStatus(Status.ReturningHome);
                _currQuestActivity = QuestActivity.needNewTarget;
                _currentQ = null;
                _attachedNavMeshAgent.isStopped = false;
            }
            else
            {
                _villToGetQuestsFrom = Village.GetClosestVillageTo(transform.position);
                _currQuestSource = _villToGetQuestsFrom.QuestBoard.GetComponent<QuestSource>();
                SetTarget(_currQuestSource.transform);
                _currQuestActivity = QuestActivity.gettingNewQuest;
            }
        }
    }
    private Village _villToGetQuestsFrom;
    private QuestSource _currQuestSource;

    private void StartQuest(Quest q)
    {
        _currentQ = q;
        _currentQ.QuestDone.AddListener(QuestComplete);

        GetNewQuestTarget();
    }

    private void GetNewQuestTarget()
    {
        GameObject newTarget;

        Inventory.ResourceType targetType = Inventory.ResourceType.Wood;

        // get target type for quest
        if (_currentQ)
        {
            switch (_currentQ.QuestType)
            {
                case Quest.Type.GatherWood:
                    targetType = Inventory.ResourceType.Wood;
                    break;
                case Quest.Type.GatherStone:
                    targetType = Inventory.ResourceType.Stone;
                    break;
                case Quest.Type.GatherFood:
                    targetType = Inventory.ResourceType.Food;
                    break;
            }
        }

        newTarget = InteractableManager.Instance.FindClosestHarvestableOfType(targetType, transform);
        if (newTarget)
        {
            Target = newTarget;
            SetTarget(newTarget.transform);
            _currQuestActivity = QuestActivity.onMyWay;
        }
        else
        {
            _currQuestActivity = QuestActivity.cannotDo;
        }
    }

    private void HandleQuestStatus()
    {
        switch (_currQuestActivity)
        {
            case QuestActivity.cannotDo:
                if (_currentQ)
                {
                    _currentQ.AbandonQuestButtonClicked();
                }
                FindNewQuest();
                break;

            case QuestActivity.doingActivity:
                if (!_attachedNavMeshAgent.isStopped)
                    _attachedNavMeshAgent.isStopped = true;

                if (!Target)
                {
                    _currQuestActivity = QuestActivity.needNewTarget;
                    _attachedNavMeshAgent.isStopped = false;
                }
                else
                    DoActivity();
                break;

            case QuestActivity.needNewTarget:
                GetNewQuestTarget();
                break;

            case QuestActivity.onMyWay:
                TryStartActivity();
                break;

            case QuestActivity.gettingNewQuest:
                // if in range of the target
                if ((transform.position - _currQuestSource.transform.position).sqrMagnitude < 5.0f)
                {
                    // get a quest
                    
                    GameObject qObj = _currQuestSource.GetRandomQuest();
                    if (qObj)
                    {
                        // and go do it
                        QuestParchment qParch = qObj.GetComponent<QuestParchment>();
                        if (qParch)
                        {
                            qParch.AcceptQuestNPC(GetComponent<QuestHolderNPC>());
                            FindNewQuest();
                        }
                    }
                }

                break;
        }
    }

    private void TryStartActivity()
    {
        if (Target)
        {
            if (Target.GetComponent<Harvestable>().IsNPCInRange(transform))
            {
                _currQuestActivity = QuestActivity.doingActivity;
            }
            else
            {
                SetTarget(Target.transform);
            }
        }
        else
        {
            GetNewQuestTarget();
        }
    }

    private void QuestComplete()
    {
        if (_currentQ)
        {
            _currentQ.CompleteQuestButtonClicked();
            _currentQ.gameObject.transform.SetParent(null);
            _currentQ = null;
        }
        
        _currQuestActivity = QuestActivity.cannotDo;
    }

    private float _currActivityInterval;
    private void DoActivity()
    {
        _currActivityInterval += Time.deltaTime;
        if (_currActivityInterval >= MaxActivityInterval)
        {
            _currActivityInterval -= MaxActivityInterval;

            Harvestable obj = Target.GetComponent<Harvestable>();
            if (obj)
            {
                obj.HandleNPCInteract(this);
            }
            if (!Target)
            {
                _currQuestActivity = QuestActivity.needNewTarget;
            }
        }
    }

    #endregion Autonomous Questing

    #region Combat
    Material _originalMaterial;
    public void GoAttack(GameObject target)
    {
        if (_currEnemyAlertStatus != EnemyAlertStatus.Ignore)
        {
            TargetedEnemy = target;
            _currCombatStatus = CombatStatus.Chasing;
        }
    }
    private void TookDamageFrom(GameObject source)
    {
        if (_currEnemyAlertStatus == EnemyAlertStatus.Defensive)
            if (!TargetedEnemy)
                TargetedEnemy = source;

        GetComponent<MeshRenderer>().material = GameManager.Instance.MaterialHurt;
        Invoke("ResetMaterial", 0.1f);

        GetComponent<AudioSource>().clip = GameManager.Instance.HurtSoundEffect;
        GetComponent<AudioSource>().Play();
    }
    private void ResetMaterial()
    {
        GetComponent<MeshRenderer>().material = _originalMaterial;
    }
    private void KilledBy(GameObject source)
    {
        InventoryNPC myInv = GetComponent<InventoryNPC>();

        if (WeaponSlot.transform.childCount > 0)
        {
            InvItem weapon = WeaponSlot.transform.GetChild(0).GetComponent<InvItem>();
            if (weapon)
                myInv.GiveItem(weapon);
        }
        
        if (ArmorSlot.transform.childCount > 0)
        {
            InvItem armor = ArmorSlot.transform.GetChild(0).GetComponent<InvItem>();
            if (armor)
                myInv.GiveItem(armor);
        }
        

        LootCrate.SpawnLootcrateOn(transform);

        Destroy(gameObject);
    }
    #endregion Combat

    #region Statics
    private static List<GameObject> _allGuildies = new List<GameObject>();
    public static GameObject GetClosestGuildMemberTo(Vector3 pos, Guild requiredGuild = null )
    {
        GameObject result = null;
        float closestDistanceSqr = 999999.0f;

        for (int i = 0; i < _allGuildies.Count; ++i)
        {
            if (!_allGuildies[i])
            {
                _allGuildies.Remove(_allGuildies[i]);
                continue;
            }

            if (requiredGuild)
            {
                if (_allGuildies[i].GetComponent<GuildMemberController>().NPCsGuild != requiredGuild)
                    continue;
            }

            float dist = (_allGuildies[i].transform.position - pos).sqrMagnitude;
            if (dist < closestDistanceSqr)
            {
                closestDistanceSqr = dist;
                result = _allGuildies[i];
            }
        }
        return result;
    }
    #endregion Statics

    private void OnDestroy()
    {
        _allGuildies.Remove(gameObject);
    }
}
