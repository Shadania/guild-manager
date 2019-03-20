using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Controls the player
public class PlayerController : MonoBehaviour
{
    #region Declarations
    [Header("General")]
    public GameObject Camera;
    private float _gravity = 25;
    private float _jumpSpeed = 8;
    private float _speed = 10;
    private Vector3 _moveDirection = Vector3.zero;
    public bool AcceptsInput = true;
    public Guild MyGuild;

    [Header("UI")]
    public GameObject PlayerHUD;
    public Button QuestsButton;
    public Button InventoryButton;
    public Button HelpButton;
    public GameObject QuestsMenu;
    public GameObject InventoryMenu;
    public GameObject HelpMenu;
    public GameObject DeathScreen;
    public Button RespawnButton;
    public Text CauseOfDeathText;
    public GameObject MainMenu;
    public Button ContinueButton;
    public Button RestartButton;
    public Button QuitButton;

    [Header("Gear + Gear UI")]
    public Button GearButton;
    public GameObject GearMenu;
    public GameObject WeaponSlot;
    public GameObject ArmorSlot;
    public GameObject GearMenuInvItemHolder;
    public GameObject VisibleArmor;
    public GameObject VisibleWeapon;
    
    private Vector3 _spawnPoint;

    private enum MenuType
    {
        Quests,
        Inventory,
        Help,
        Gear,
        Main
    }
    public bool AnyMenuUp = false;
    private MenuType _whichMenuUp;

    private float _attackCoolDown;
    private const float _maxAttackCoolDown = 0.3f;

    private const float _hitConeAngle = 60.0f;
    private const float _hitConeRadius = 6.0f;
    private bool _hasPressedUICancelOnce = false;
    #endregion Declarations


    private void Start ()
    {
        // Add button listeners
        QuestsButton.onClick.AddListener(QuestsButtonClicked);
        InventoryButton.onClick.AddListener(InventoryButtonClicked);
        HelpButton.onClick.AddListener(HelpButtonClicked);
        GearButton.onClick.AddListener(GearButtonClicked);
        RespawnButton.onClick.AddListener(RespawnButtonClicked);

        ContinueButton.onClick.AddListener(ContinueButtonClicked);
        RestartButton.onClick.AddListener(RestartButtonClicked);
        QuitButton.onClick.AddListener(QuitButtonClicked);

        // Save spawnpoint
        _spawnPoint = transform.position;

        PlayerHUD.SetActive(true);
        MyGuild = transform.parent.GetComponent<Guild>();
        if (!MyGuild)
            Debug.Log("Player was not part of a guild on game start");

        Health myHealth = GetComponent<Health>();
        if (myHealth)
        {
            myHealth.onTakeDamage.AddListener(HandleTakeDamage);
            myHealth.onDeath.AddListener(HandleDeath);
        }
    }

    #region Combat
    private void HandleTakeDamage(GameObject source)
    {
        // play sound + summon guild members
        List<GuildMemberController> membersInRange = MyGuild.GetGuildMembersInRange(transform.position, 30);
        for (int i = 0; i < membersInRange.Count; ++i)
        {
            membersInRange[i].GoAttack(source);
        }

        GetComponent<AudioSource>().clip = GameManager.Instance.HurtSoundEffect;
        GetComponent<AudioSource>().Play();
    }
    private void HandleDeath(GameObject source)
    {
        AcceptsInput = false;
        transform.position = _spawnPoint;
        DeathScreen.SetActive(true);

        if (source.GetComponent<EnemyBehaviour>())
            CauseOfDeathText.text = "You were killed by an enemy";
    }
    private void RespawnButtonClicked()
    {
        AcceptsInput = true;
        GetComponent<Health>().CurrHealth = GetComponent<Health>().MaxHealth;
        GetComponent<Health>().DealDamage(0.0f, gameObject);
        DeathScreen.SetActive(false);
    }
    #endregion Combat

    private void ContinueButtonClicked()
    {
        MainMenu.SetActive(false);
        AcceptsInput = true;
    }
    private void RestartButtonClicked()
    {
        // RESTART GAME
        InteractableManager.Instance.DeleteAllLists();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void QuitButtonClicked()
    {
        Application.Quit();
    }



    private void Update()
    {
        HandleMovement();

        if (_attackCoolDown > 0.0f)
            _attackCoolDown -= Time.deltaTime;

        if (Input.GetButtonDown("Attack") && _attackCoolDown < 0.1f)
        {
            float damage = 10.0f;

            if (WeaponSlot.transform.childCount > 0)
            {
                InvItem myWeapon = WeaponSlot.transform.GetChild(0).GetComponent<InvItem>();
                switch ((int)myWeapon.ItemType)
                {
                    case 4:
                        damage = 20.0f;
                        break;
                    case 5:
                        damage = 30.0f;
                        break;
                    case 6:
                        damage = 40.0f;
                        break;
                    case 7:
                        damage = 50.0f;
                        break;
                }
            }

            EnemyBehaviour.TryHitEnemy(_hitConeRadius, _hitConeAngle, gameObject, damage);
            _attackCoolDown += _maxAttackCoolDown;
        }
        if (Input.GetButtonDown("UICancel"))
            HandleUICancelPressed();
    }

    void HandleUICancelPressed()
    {
        if (AnyMenuUp)
        {
            switch (_whichMenuUp)
            {
                case MenuType.Quests:
                    AnyMenuUp = false;
                    QuestsMenu.SetActive(AnyMenuUp);
                    break;

                case MenuType.Inventory:
                    AnyMenuUp = false;
                    InventoryMenu.SetActive(AnyMenuUp);
                    break;

                case MenuType.Help:
                    AnyMenuUp = false;
                    HelpMenu.SetActive(AnyMenuUp);
                    break;

                case MenuType.Gear:
                    AnyMenuUp = false;
                    GearMenu.SetActive(false);
                    ExitGearMenu();
                    break;

                case MenuType.Main:
                    AnyMenuUp = false;
                    MainMenu.SetActive(false);
                    AcceptsInput = true;
                    break;
            }
        }
        else if (AcceptsInput)
        {
            if (!_hasPressedUICancelOnce)
            {
                _hasPressedUICancelOnce = true;
            }
            else
            {
                MainMenu.SetActive(true);
                AcceptsInput = false;
                AnyMenuUp = true;
                _whichMenuUp = MenuType.Main;
                _hasPressedUICancelOnce = false;
            }
        }
    }

    #region Button handling
    void QuestsButtonClicked()
    {
        if (AcceptsInput)
        {
            if (AnyMenuUp)
            {
                if (_whichMenuUp == MenuType.Quests)
                {
                    QuestsMenu.SetActive(false);
                    AnyMenuUp = false;
                }
                // else do nothing
            }
            else
            {
                // no menu is up
                QuestsMenu.SetActive(true);
                _whichMenuUp = MenuType.Quests;
                AnyMenuUp = true;
            }
        }
    }
    void InventoryButtonClicked()
    {
        if (AcceptsInput)
        {
            if (AnyMenuUp)
            {
                if (_whichMenuUp == MenuType.Inventory)
                {
                    InventoryMenu.SetActive(false);
                    AnyMenuUp = false;
                }
                // else do nothing
            }
            else
            {
                // no menu is up
                InventoryMenu.SetActive(true);
                _whichMenuUp = MenuType.Inventory;
                AnyMenuUp = true;
            }
        }
    }
    void HelpButtonClicked()
    {
        if (AcceptsInput)
        {
            if (AnyMenuUp)
            {
                if (_whichMenuUp == MenuType.Help)
                {
                    HelpMenu.SetActive(false);
                    AnyMenuUp = false;
                }
                // else do nothing
            }
            else
            {
                // no menu is up
                HelpMenu.SetActive(true);
                _whichMenuUp = MenuType.Help;
                AnyMenuUp = true;
            }
        }

    }
    private Transform _originalParent;
    void GearButtonClicked()
    {
        if (AcceptsInput)
        {
            if (AnyMenuUp)
            {
                // turn off menu
                if (_whichMenuUp == MenuType.Gear)
                {
                    GearMenu.SetActive(false);
                    AnyMenuUp = false;

                    ExitGearMenu();
                }
            }
            // turn on menu
            else
            {
                GearMenu.SetActive(true);
                _whichMenuUp = MenuType.Gear;
                AnyMenuUp = true;

                // get all the items in here
                Inventory inv = GetComponent<Inventory>();
                // save original parent
                _originalParent = inv.ItemHolder.transform.parent;
                // set the parent
                inv.ItemHolder.transform.SetParent(GearMenuInvItemHolder.transform);
                // set the content
                GearMenuInvItemHolder.GetComponent<ScrollRect>().content = 
                    GearMenuInvItemHolder.transform.GetChild(0).GetComponent<Image>().rectTransform;
                _originalParent.GetComponent<ScrollRect>().content = null;
                // set the transform
                GearMenuInvItemHolder.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
                GearMenuInvItemHolder.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);
                GearMenuInvItemHolder.transform.GetChild(0).localScale = new Vector3(1, 1, 1);

                // add listeners
                for (int i = 0; i < GearMenuInvItemHolder.transform.GetChild(0).childCount; ++i)
                {
                    GearMenuInvItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.AddListener(EquipItem);
                }
            }
        }
    }
    #endregion Button handling


    private void EquipItem(InvItem item)
    {
        // get the material
        Material neededMat = new Material(GameManager.Instance.MaterialWood);

        switch ((int)item.ItemType % 4)
        {
            // default thus unneeded case
            // case 0:
            //     neededMat = GameManager.Instance.MaterialWood;
            //     break;
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

        // handle UI and ingame appearance
        if ((int)item.ItemType <= 3)
        {
            // is armor
            // if there's already something in the armor slot, give it back
            if (ArmorSlot.transform.childCount > 0)
                GetComponent<Inventory>().GiveItem(ArmorSlot.transform.GetChild(0).GetComponent<InvItem>());
            else // if there wasnt, we still need to turn on the visible armor
                VisibleArmor.SetActive(true);

            // set the parent
            item.transform.SetParent(ArmorSlot.transform);
            
            // set the material
            VisibleArmor.GetComponent<MeshRenderer>().material = neededMat;
        }
        else
        {
            // is weapon
            // if there's already something in the weapon slot, give it back
            if (WeaponSlot.transform.childCount > 0)
                GetComponent<Inventory>().GiveItem(WeaponSlot.transform.GetChild(0).GetComponent<InvItem>());
            else // if there wasnt, we still need to turn on the visible weapon
                VisibleWeapon.SetActive(true);

            // set the parent
            item.transform.SetParent(WeaponSlot.transform);

            // set the material
            VisibleWeapon.GetComponent<MeshRenderer>().material = neededMat;
        }

        item.transform.localRotation = new Quaternion(0, 0, 0, 0);
        item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localPosition = new Vector3(0, 0, 0);

        item.PlayerClicked.AddListener(UnequipItem);

        switch ((int)item.ItemType)
        {
            case 4:
                GetComponent<Health>().IncomingDamageMultiplier = 0.8f;
                break;
            case 5:
                GetComponent<Health>().IncomingDamageMultiplier = 0.65f;
                break;
            case 6:
                GetComponent<Health>().IncomingDamageMultiplier = 0.5f;
                break;
            case 7:
                GetComponent<Health>().IncomingDamageMultiplier = 0.35f;
                break;
        }
    }
    private void UnequipItem(InvItem item)
    {
        item.PlayerClicked.AddListener(EquipItem);
        GetComponent<Inventory>().GiveItem(item);
        if ((int)item.ItemType <= 3)// it armor
        {
            VisibleArmor.SetActive(false);
            GetComponent<Health>().IncomingDamageMultiplier = 1.0f;
        }
        else
            VisibleWeapon.SetActive(false);
    }

    void HandleMovement()
    {
        CharacterController controller = GetComponent<CharacterController>();

        if (AcceptsInput)
        {
            if (controller.isGrounded)
            {
                _moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
                _moveDirection = transform.TransformDirection(_moveDirection);
                _moveDirection = _moveDirection * _speed;

                if (Input.GetButton("Jump"))
                {
                    _moveDirection.y = _jumpSpeed;
                }
            }

            // Apply gravity
            _moveDirection.y = _moveDirection.y - (_gravity * Time.deltaTime);
        }
        else
        {
            // just apply gravity
            _moveDirection = new Vector3(0, _moveDirection.y, 0);
            _moveDirection.y = _moveDirection.y - (_gravity * Time.deltaTime);
        }

        // Move the controller
        controller.Move(_moveDirection * Time.deltaTime);
    }


    private void ExitGearMenu()
    {
        // remove listeners
        for (int i = 0; i < GearMenuInvItemHolder.transform.GetChild(0).childCount; ++i)
        {
            GearMenuInvItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.RemoveAllListeners();
        }

        // set parent
        GearMenuInvItemHolder.transform.GetChild(0).SetParent(_originalParent);
        // set content
        GearMenuInvItemHolder.GetComponent<ScrollRect>().content = null;
        _originalParent.GetComponent<ScrollRect>().content =
            _originalParent.transform.GetChild(0).GetComponent<Image>().rectTransform;
        // set the transform
        _originalParent.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        _originalParent.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);
        _originalParent.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
    }
}
