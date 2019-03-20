using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


// The point from where you manage your guild
// Mainly UI
public class GuildManagementDesk : MonoBehaviour
{
    #region Declarations
    [Header("General")]
    private bool _isMenuActive = false;
    private bool _playerInTrigger = false;
    public Guild DeskGuild;
    public bool CanSpawnRecruitCards = true;

    public UnityEvent RecruitedSomeone = new UnityEvent();


    [Header("Main menu")]
    public GameObject MainMenu;
    public Button CheckGuildResources;
    public Button CheckGuildRecruits;
    public Button CheckGuildInfluence;
    public Button MoveDeskButton;
    public Button ChangeGuildNameButton;
    private bool _isBeingMoved;
    public GameObject BankMenu;
    public GameObject ChangeGuildNameMenu;
    public InputField ChangeGuildNameInputfield;
    public UnityEvent GuildNameChangedEvent = new UnityEvent();


    [Header("Recruits")]
    public GameObject RecruitsMenu;
    public float NewRecruitCardInterval = 10.0f;
    public int MaxRecruitCards = 5;
    private float _elapsedSecondsSinceLastRecruitCard;
    public GameObject RecruitCardList;
    public GameObject RecruitCardPrefab;

    [Header("Influence")]
    public GameObject InfluenceMenu;
    public GameObject InfluenceCardPrefab;
    public GameObject InfluenceCardHolder;

    private enum MenuType
    {
        Main,
        Recruits,
        Influence,
        Bank,
        ChangeGuildName
    }
    private MenuType _whichMenuActive = MenuType.Main;

    #endregion Declarations

    private void Awake()
    {
        DeskGuild = transform.parent.GetComponent<Guild>();
        if (DeskGuild)
        {
            DeskGuild.Desk = this;
        }

        // Buttons binding
        CheckGuildResources.onClick.AddListener(CheckGuildResourcesButtonClicked);
        CheckGuildRecruits.onClick.AddListener(CheckGuildRecruitsButtonClicked);
        CheckGuildInfluence.onClick.AddListener(CheckGuildInfluenceButtonClicked);
        MoveDeskButton.onClick.AddListener(MoveDeskButtonClicked);
        ChangeGuildNameButton.onClick.AddListener(ChangeGuildNameButtonClicked);
        ChangeGuildNameInputfield.onEndEdit.AddListener(GuildNameChanged);

        Interactable interac = GetComponent<Interactable>();
        if (interac)
        {
            interac.OnPlayerInteract.AddListener(HandleOpenInteractMenu);
        }

        GetComponent<InventoryNPC>().GiveItemsText.text = "Deposit Items in Guild Bank";
    }
    private void HandleOpenInteractMenu()
    {
        if (_playerInTrigger && !_isMenuActive)
        {
            PlayerController pContr = DeskGuild.GuildLeader.GetComponent<PlayerController>();
            if (pContr)
            {
                _isMenuActive = true;
                MainMenu.SetActive(_isMenuActive);
                pContr.AcceptsInput = !_isMenuActive;
            }
        }
    }


    #region Button Handling
    private void CheckGuildResourcesButtonClicked()
    {
        // Debug.Log("Clicked resources");
        MainMenu.SetActive(false);
        BankMenu.SetActive(true);
        _whichMenuActive = MenuType.Bank;
    }
    private void CheckGuildRecruitsButtonClicked()
    {
        _whichMenuActive = MenuType.Recruits;
        MainMenu.SetActive(false);
        RecruitsMenu.SetActive(true);
    }
    private void CheckGuildInfluenceButtonClicked()
    {
        _whichMenuActive = MenuType.Influence;
        MainMenu.SetActive(false);
        InfluenceMenu.SetActive(true);
    }
    private void MoveDeskButtonClicked()
    {
        _isBeingMoved = true;
        MainMenu.SetActive(false);
        
        PlayerController pContr = DeskGuild.GuildLeader.GetComponent<PlayerController>();
        if (pContr)
        {
            pContr.AcceptsInput = true;
            transform.parent = DeskGuild.GuildLeader.transform;
        }
    }
    private void ChangeGuildNameButtonClicked()
    {
        _whichMenuActive = MenuType.ChangeGuildName;
        MainMenu.SetActive(false);
        ChangeGuildNameMenu.SetActive(true);
    }
    private void GuildNameChanged(string newName)
    {
        if (newName.Length > 0)
        {
            DeskGuild.GuildName = newName;
            GuildNameChangedEvent.Invoke();
        }
    }
    #endregion Button Handling



    #region Recruit Handling
    void HandleNewRecruitCards()
    {
        if (!CanSpawnRecruitCards)
            return;



        _elapsedSecondsSinceLastRecruitCard += Time.deltaTime;

        // time interval done
        if (_elapsedSecondsSinceLastRecruitCard > NewRecruitCardInterval)
        {

            // reset timer
            _elapsedSecondsSinceLastRecruitCard -= NewRecruitCardInterval;


            // add recruit if list is not yet at capacity
            if (RecruitCardList.transform.childCount < MaxRecruitCards)
                AddNewRecruitCard();
        }

    }
    void AddNewRecruitCard()
    {
        GameObject newRecruit = Instantiate(RecruitCardPrefab);
        newRecruit.transform.SetParent(RecruitCardList.transform);
        RecruitCardBehaviour recruitCard = newRecruit.GetComponent<RecruitCardBehaviour>();
        recruitCard.GotRecruited.AddListener(RecruitWasRecruited);
        recruitCard.ApplyingGuild = transform.parent.GetComponent<Guild>();
        recruitCard.NameText.text = GameManager.Instance.NameGen.GenerateName();
        recruitCard.transform.rotation = new Quaternion(0, 0, 0, 0); 
        recruitCard.transform.localScale = new Vector3(1, 1, 1);
    }
    private void RecruitWasRecruited()
    {
        RecruitedSomeone.Invoke();
    }
    #endregion Recruit Handling

    private void Update()
    {
        if (Input.GetButtonDown("UICancel") && _isMenuActive)
        {
            HandleUICancel();
        }

        HandleNewRecruitCards();

        if (_isBeingMoved)
        {
            if (Input.GetButtonDown("Interact"))
            {
                _isBeingMoved = false;
                MainMenu.SetActive(true);
                transform.parent = DeskGuild.transform;
                PlayerController pContr = DeskGuild.GuildLeader.GetComponent<PlayerController>();
                if (pContr)
                {
                    pContr.AcceptsInput = false;
                }
            }
        }
    }

    // To check if player is in range to interact with it
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
            _playerInTrigger = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
            _playerInTrigger = false;
    }

    
    // general updating
    private void HandleUICancel()
    {
        switch (_whichMenuActive)
        {
            case MenuType.Main:
                _isMenuActive = false;
                MainMenu.SetActive(false);
                GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = true;
                break;

            case MenuType.Recruits:
                _whichMenuActive = MenuType.Main;
                RecruitsMenu.SetActive(false);
                MainMenu.SetActive(true);
                break;
                
            case MenuType.Bank:
                _whichMenuActive = MenuType.Main;
                BankMenu.SetActive(false);
                MainMenu.SetActive(true);
                break;

            case MenuType.Influence:
                _whichMenuActive = MenuType.Main;
                InfluenceMenu.SetActive(false);
                MainMenu.SetActive(true);
                break;

            case MenuType.ChangeGuildName:
                _whichMenuActive = MenuType.Main;
                ChangeGuildNameMenu.SetActive(false);
                MainMenu.SetActive(true);
                break;
        }


    }
    public void UpdateInfluenceMenu()
    {
        for (int i = 0; i < InfluenceCardHolder.transform.childCount; ++i)
        {
            Destroy(InfluenceCardHolder.transform.GetChild(i).gameObject);
        }

        Dictionary<GameObject, int> dic = DeskGuild.Influences;

        foreach(var elem in dic)
        {
            GameObject newCard = Instantiate(InfluenceCardPrefab, InfluenceCardHolder.transform);
            newCard.transform.localScale = new Vector3( 1, 1, 1 );
            newCard.transform.localRotation = new Quaternion(0, 0, 0, 0);
            InfluenceInfoCard infoCardComponent = newCard.GetComponent<InfluenceInfoCard>();

            if (infoCardComponent)
            {
                infoCardComponent.SetInfluence(elem.Value);
                Village vill = elem.Key.GetComponent<Village>();
                if (vill)
                {
                    infoCardComponent.SetVillageName(vill.VillageName);
                }
            }
        }
    }
}
