using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Townhall : MonoBehaviour
{
    #region Declarations
    [Header("UI")]
    public GameObject MainMenu;
    public GameObject InfoMenu;
    public GameObject CouncilManagementMenu;
    public GameObject CouncilDecisionsMenu;
    public GameObject KingDecisionsMenu;

    public Text MenuHeader;

    public Button BuyCouncilPosition;
    public Button BuyKingPosition;
    public Button BuyInfluence;
    public Button DisplayInfo;
    public Button ManageCouncilMembersButton;
    public Button CouncilDecisionsButton;
    public Button KingDecisionsButton;

    public Button RecruitRandomVillagerButton;

    public GameObject ManageCouncilMenuGuildMembersHolder;
    public GameObject AddCouncilMemberOptionCardPrefab;
    public List<GameObject> CouncilMemberInfoCards;

    public Text CurrentAvailableTaxesText;
    public Button CollectTaxesButton;

    [Header("Townhall Infoscreen UI")]
    public Text VillageNameText;
    public Text HousesVillagersText;
    public Text CurrentKingText;
    public Text CouncilMembersText;
    public Text MyGuildsInfluenceText;
    public GameObject InterVillageStandingHolder;
    public GameObject VillageStandingInfoCardPrefab;

    [Header("Townhall Rename Village")]
    public InputField RenameVillageInputField;
    public Button RenameVillageButton;



    [Header("General")]
    public float PlayerInteractRange = 5.0f;
    public int KingPrice = 500;
    public int CouncilPrice = 100;

    private enum MenuType
    {
        Main,
        Info,
        ManageCouncil,
        CouncilAction,
        KingAction
    }
    private MenuType _whichMenuUp;
    private bool _menuUp = false;


    private Village _vill;
    #endregion Declarations


    private void Start()
    {
        Interactable interac = GetComponent<Interactable>();
        if (interac)
        {
            interac.OnPlayerInteract.AddListener(HandlePlayerInteract);
        }
        MenuHeader.text = "Townhall of " + transform.parent.parent.GetComponent<Village>().VillageName;

        // Bind buttons
        BuyCouncilPosition.onClick.AddListener(BuyCouncilPositionClicked);
        BuyKingPosition.onClick.AddListener(BuyKingPositionClicked);
        BuyInfluence.onClick.AddListener(BuyInfluenceClicked);
        DisplayInfo.onClick.AddListener(DisplayInfoClicked);
        ManageCouncilMembersButton.onClick.AddListener(ManageCouncilMembersClicked);
        CouncilDecisionsButton.onClick.AddListener(CouncilDecisionsButtonClicked);
        KingDecisionsButton.onClick.AddListener(KingDecisionsButtonClicked);

        BuyKingPosition.transform.GetChild(0).GetComponent<Text>().text = "Become King (" + KingPrice.ToString() + ')';
        BuyCouncilPosition.transform.GetChild(0).GetComponent<Text>().text = "Buy Council Position (" + CouncilPrice.ToString() + ')';

        RecruitRandomVillagerButton.onClick.AddListener(RecruitRandomVillagerButtomClicked);
        CollectTaxesButton.onClick.AddListener(ClaimTaxes);
        RenameVillageButton.onClick.AddListener(RenameVillage);

        // Set text

        _vill = transform.parent.parent.GetComponent<Village>();

        BuyKingPosition.gameObject.SetActive(false);


        // Setting info screen text
        VillageNameText.text = _vill.VillageName;
        HousesVillagersText.text = "Houses " + _vill.Villagers.Count.ToString() + " Villagers in " + _vill.MaxAmtVillagers.ToString() + " Houses.";

        // separate scope because i want to reuse the names of these variables for less confusion
        {
            GuildMemberController gmContr;
            VillagerBehaviour vBehav;

            CurrentKingText.text = "King: ";

            if (_vill.King.GetComponent<PlayerController>())
            {
                CurrentKingText.text += "You!";
            }
            else if (gmContr = _vill.King.GetComponent<GuildMemberController>())
            {
                CurrentKingText.text += gmContr.NameMesh.text + " (Member of " + gmContr.NPCsGuild.GuildName + ')';
            }
            else if (vBehav = _vill.King.GetComponent<VillagerBehaviour>())
            {
                CurrentKingText.text += vBehav.VillagerName + " (Villager)";
            }
            else
            {
                CurrentKingText.text += "Unknown!";
            }
        }

        CouncilMembersText.text = "Council members: ";
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            GuildMemberController gmContr;
            VillagerBehaviour vBehav;
            if (_vill.Council[i].GetComponent<PlayerController>())
            {
                CouncilMembersText.text += "You";
            }
            else if (gmContr = _vill.Council[i].GetComponent<GuildMemberController>())
            {
                CouncilMembersText.text += gmContr.NameMesh.text;
                CouncilMembersText.text += " (Guild Member of " + gmContr.NPCsGuild.GuildName + ')';
            }
            else if (vBehav = _vill.Council[i].GetComponent<VillagerBehaviour>())
            {
                CouncilMembersText.text += vBehav.NameMesh.text;
                CouncilMembersText.text += " (Villager)";
            }
            else
            {
                CouncilMembersText.text += "Unknown";
            }

            if ((i + 1) != _vill.Council.Count)
                CouncilMembersText.text += ", ";
        }

        // Also set council members in the Manage Council menu
        for (int i = 0; i < CouncilMemberInfoCards.Count; ++i)
        {
            CouncilMemberInfoCard card = CouncilMemberInfoCards[i].GetComponent<CouncilMemberInfoCard>();

            card.CouncilMember = _vill.Council[i];
            // they should all still be villagers by now
            card.NameText.text = _vill.Council[i].GetComponent<VillagerBehaviour>().VillagerName;
            card.RemoveButton.gameObject.SetActive(false);
        } 


    }
    private void Update()
    {
        if (_menuUp)
        {
            if (Input.GetButtonDown("UICancel"))
            {
                HandleUICancelPressed();
            }
        }
    }




    public void HandleVillagerAmtChanged()
    {
        HousesVillagersText.text = "Houses " + _vill.Villagers.Count.ToString() + " Villagers in " + _vill.MaxAmtVillagers.ToString() + " Houses.";
    }
    public void HandleMyInfluenceChanged()
    {
        MyGuildsInfluenceText.text = "My guild's Influence on this Village: " +
            GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild.GetInfluenceFor(_vill.gameObject).ToString();
    }

    #region ButtonClicks & UICancel
    private void HandleUICancelPressed()
    {
        switch (_whichMenuUp)
        {
            case MenuType.Main:

                MainMenu.SetActive(false);
                _menuUp = false;
                GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = true;
                break;

            case MenuType.Info:

                InfoMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuUp = MenuType.Main;
                break;

            case MenuType.CouncilAction:
                CouncilDecisionsMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuUp = MenuType.Main;
                break;

            case MenuType.KingAction:
                KingDecisionsMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuUp = MenuType.Main;
                break;

            case MenuType.ManageCouncil:
                CouncilManagementMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuUp = MenuType.Main;

                for (int i = 0; i < ManageCouncilMenuGuildMembersHolder.transform.childCount; ++i)
                {
                    Destroy(ManageCouncilMenuGuildMembersHolder.transform.GetChild(i).gameObject);
                }

                // council members may have changed since the last time we went to the main menu so change it here
                CouncilMembersText.text = "Council members: ";
                for (int i = 0; i < _vill.Council.Count; ++i)
                {
                    if (_vill.Council[i])
                    {
                        GuildMemberController gmContr;
                        VillagerBehaviour vBehav;
                        if (_vill.Council[i].GetComponent<PlayerController>())
                        {
                            CouncilMembersText.text += "You";
                        }
                        else if (gmContr = _vill.Council[i].GetComponent<GuildMemberController>())
                        {
                            CouncilMembersText.text += gmContr.NameMesh.text;
                            CouncilMembersText.text += " (Guild Member of " + gmContr.NPCsGuild.GuildName + ')';
                        }
                        else if (vBehav = _vill.Council[i].GetComponent<VillagerBehaviour>())
                        {
                            CouncilMembersText.text += vBehav.NameMesh.text;
                            CouncilMembersText.text += " (Villager)";
                        }
                        else
                        {
                            CouncilMembersText.text += "UnknownTypeA";
                            Debug.Log("This should never be called");
                        }
                    }
                    else
                    {
                        CouncilMembersText.text += "No one";
                    }

                    if ((i + 1) != _vill.Council.Count)
                        CouncilMembersText.text += ", ";
                }




                break;
        }
    }
    private void DisplayInfoClicked()
    {
        MainMenu.SetActive(false);
        InfoMenu.SetActive(true);
        _whichMenuUp = MenuType.Info;
        MyGuildsInfluenceText.text = "My guild's Influence on this Village: " +
            GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild.GetInfluenceFor(_vill.gameObject).ToString();
    }
    private void BuyInfluenceClicked()
    {
        Guild pGuild = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild;
        if (!pGuild)
        {
            Debug.Log("PlayerController's MyGuild was null");
            return;
        }

        Inventory sourceInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
        if (!sourceInv)
        {
            Debug.Log("PlayerAvatar did not have an inventory component");
            return;
        }

        int amtGold = sourceInv.GetAmtResource(Inventory.ResourceType.Gold);

        if (amtGold >= 50)
        {
            sourceInv.RemoveResources(Inventory.ResourceType.Gold, 50);
            pGuild.AddInfluenceFor(_vill.gameObject, 10);
        }
    }
    private void BuyKingPositionClicked()
    {
        // check if player has enough influenceGuild pGuild = GameManager.PlayerAvatar.GetComponent<PlayerController>().MyGuild;
        Guild pGuild = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild;
        if (!pGuild)
        {
            Debug.Log("PlayerController's MyGuild was null");
            return;
        }

        if (pGuild.GetInfluenceFor(_vill.gameObject) < KingPrice)
        {
            Debug.Log("Did not have enough influence to buy king position");
            return;
        }

        // deduct influence from player account

        pGuild.RemoveInfluenceFor(_vill.gameObject, KingPrice);

        //  update text

        MyGuildsInfluenceText.text = "Your guild's influence on this village: " + pGuild.GetInfluenceFor(_vill.gameObject).ToString();

        // give player the king position

        _vill.King = GameManager.Instance.PlayerAvatar;

        // update king text

        CurrentKingText.text = "King: You!";

        // enable the KingMenu of the village

        KingDecisionsButton.gameObject.SetActive(true);

        // disable buy king button

        BuyKingPosition.gameObject.SetActive(false);

        // fire the event

        _vill.PlayerBecameKing.Invoke();
    }
    private void BuyCouncilPositionClicked()
    {
        // check if player has a guild
        Guild pGuild = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild;
        if (!pGuild)
        {
            Debug.Log("PlayerController's MyGuild was null");
            return;
        }
        
        // check if player has enough influence
        if (pGuild.GetInfluenceFor(_vill.gameObject) < CouncilPrice)
        {
            Debug.Log("Did not have enough influence to buy council position");
            return;
        }

        // deduct influence from player account
        pGuild.RemoveInfluenceFor(_vill.gameObject, CouncilPrice);

        // give player guild the council position
        pGuild.AddCouncilPositionFor(_vill.gameObject);

        // if this was the guild's fifth council position, remove button
        int amtPos = pGuild.GetAmtCouncilPositionsFor(_vill.gameObject);
        if (amtPos == 5)
        {
            BuyCouncilPosition.gameObject.SetActive(false);
            BuyKingPosition.gameObject.SetActive(true);
        }
        else if (amtPos == 1)
        {
            // if this was their first, enable the menu
            ManageCouncilMembersButton.gameObject.SetActive(true);
        }

        // either way, remove a villager from the council
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            if (_vill.Council[i])
            {
                if (_vill.Council[i].GetComponent<VillagerBehaviour>())
                {
                    // found our victim to be removed from councilhood
                    _vill.Council[i] = null;
                    break;
                }
            }
        }

        // and reset the council text
        CouncilMembersText.text = "Council members: ";
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            if (_vill.Council[i])
            {
                GuildMemberController gmContr;
                VillagerBehaviour vBehav;
                if (_vill.Council[i].GetComponent<PlayerController>())
                {
                    CouncilMembersText.text += "You";
                }
                else if (gmContr = _vill.Council[i].GetComponent<GuildMemberController>())
                {
                    CouncilMembersText.text += gmContr.NameMesh.text;
                    CouncilMembersText.text += " (Guild Member of " + gmContr.NPCsGuild.GuildName + ')';
                }
                else if (vBehav = _vill.Council[i].GetComponent<VillagerBehaviour>())
                {
                    CouncilMembersText.text += vBehav.NameMesh.text;
                    CouncilMembersText.text += " (Villager)";
                }
                else
                {
                    CouncilMembersText.text += "UnknownTypeA";
                    Debug.Log("This should never be called");
                }
            }
            else
            {
                CouncilMembersText.text += "No one";
            }

            if ((i + 1) != _vill.Council.Count)
                CouncilMembersText.text += ", ";
        }
    }
    private void ManageCouncilMembersClicked()
    {
        MainMenu.SetActive(false);
        CouncilManagementMenu.SetActive(true);
        _whichMenuUp = MenuType.ManageCouncil;

        // how to check for free spots in your guild's council?
        Guild pGuild = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild;
        // count how many spots in the council are your guildies
        int councilSpotsOccupied = 0;
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            GameObject thisMember = _vill.Council[i];
            if (thisMember)
            {
                GuildMemberController gmContr = thisMember.GetComponent<GuildMemberController>();
                if (gmContr)
                {
                    if (gmContr.NPCsGuild == pGuild)
                        councilSpotsOccupied++;
                }
            }
        }

        // get amount of spots that are free
        int councilSpotsFree = pGuild.GetAmtCouncilPositionsFor(_vill.gameObject) - councilSpotsOccupied;

        // make the list
        List<GameObject> members = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild.GuildMembers;
        for (int i = 0; i < members.Count; ++i)
        {
            if (_vill.IsCouncilMember(members[i]))
                continue;

            GameObject newCard = Instantiate(AddCouncilMemberOptionCardPrefab, ManageCouncilMenuGuildMembersHolder.transform);
            newCard.transform.localPosition = new Vector3(0, 0, 0);
            newCard.transform.localScale = new Vector3(1, 1, 1);
            newCard.transform.localRotation = new Quaternion(0, 0, 0, 0);
            AddGuildCouncilMemberCard card = newCard.GetComponent<AddGuildCouncilMemberCard>();
            card.GuildMember = members[i];
            card.NameText.text = members[i].GetComponent<GuildMemberController>().NameMesh.text;
            card.onAdd.AddListener(GuildMemberAddedToCouncil);

            if (councilSpotsFree == 0)
            {
                card.AddButton.gameObject.SetActive(false);
            }
        }

        // Check names & types of the current Council Members
        for (int i = 0; i < CouncilMemberInfoCards.Count; ++i)
        {
            CouncilMemberInfoCard card = CouncilMemberInfoCards[i].GetComponent<CouncilMemberInfoCard>();
            GameObject cMember = _vill.Council[i];
            card.CouncilMember = cMember;

            if (cMember)
            {
                VillagerBehaviour vBehav = cMember.GetComponent<VillagerBehaviour>();
                GuildMemberController gmContr;
                if (vBehav)
                {
                    card.NameText.text = vBehav.VillagerName + " (Villager)";
                    card.RemoveButton.gameObject.SetActive(false);
                }
                else if (gmContr = cMember.GetComponent<GuildMemberController>())
                {
                    card.NameText.text = gmContr.NameMesh.text + " (Member of " + gmContr.NPCsGuild.GuildName + ')';
                    if (pGuild == gmContr.NPCsGuild)
                        card.RemoveButton.gameObject.SetActive(true);
                    else
                        card.RemoveButton.gameObject.SetActive(false);
                }
                else if (cMember.GetComponent<PlayerController>())
                {
                    card.NameText.text = "You";
                    card.RemoveButton.gameObject.SetActive(true);
                }
            }
            else
            {
                card.NameText.text = "No one";
                card.RemoveButton.gameObject.SetActive(false);
            }

            card.onRemove.RemoveAllListeners();
            card.onRemove.AddListener(GuildMemberRemovedFromCouncil);
        }
    }
    private void GuildMemberAddedToCouncil(AddGuildCouncilMemberCard card)
    {
        // find a spot to put the member

        // this button being available should mean that there is
        int freeIdx = -1;
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            if (!_vill.Council[i])
            {
                freeIdx = i;
                break;
            }
        }
        if (freeIdx == -1)
        {
            Debug.Log("This shouldn't happen");
            return;
        }

        // spot found! put the member in power
        _vill.Council[freeIdx] = card.GuildMember;

        // adapt the current council overview
        {
            // separate scope for reuse of variable names
            CouncilMemberInfoCard infoCard = CouncilMemberInfoCards[freeIdx].GetComponent<CouncilMemberInfoCard>();
            GuildMemberController gmContr = card.GuildMember.GetComponent<GuildMemberController>();
            infoCard.NameText.text = gmContr.NameMesh.text;
            infoCard.RemoveButton.gameObject.SetActive(true);
            infoCard.onRemove.RemoveAllListeners();
            infoCard.onRemove.AddListener(GuildMemberRemovedFromCouncil);
            infoCard.CouncilMember = card.GuildMember;
        }

        // check if we should remove the "add" buttons from the list

        Guild pGuild = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild;

        // count how many spots in the council are your guildies
        int councilSpotsOccupied = 0;
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            GameObject thisMember = _vill.Council[i];
            if (thisMember)
            {
                GuildMemberController gmContr = thisMember.GetComponent<GuildMemberController>();
                if (gmContr)
                {
                    if (gmContr.NPCsGuild == pGuild)
                        councilSpotsOccupied++;
                }
            }
        }

        // get amount of spots that are free
        int councilSpotsFree = pGuild.GetAmtCouncilPositionsFor(_vill.gameObject) - councilSpotsOccupied;

        if (councilSpotsFree == 0) // yes we should remove the add buttons
        {
            for (int i = 0; i < ManageCouncilMenuGuildMembersHolder.transform.childCount; ++i)
            {
                AddGuildCouncilMemberCard thisCard = ManageCouncilMenuGuildMembersHolder.transform.GetChild(i).GetComponent<AddGuildCouncilMemberCard>();

                thisCard.AddButton.gameObject.SetActive(false);
            }
        }

        // get rid of the card
        Destroy(card.gameObject);

        // activate the button
        CouncilDecisionsButton.gameObject.SetActive(true);
    }
    private void GuildMemberRemovedFromCouncil(CouncilMemberInfoCard card)
    {
        // re-add the guild member to the available list below
        GameObject newCard = Instantiate(AddCouncilMemberOptionCardPrefab, ManageCouncilMenuGuildMembersHolder.transform);
        AddGuildCouncilMemberCard newCardScript = newCard.GetComponent<AddGuildCouncilMemberCard>();
        newCardScript.GuildMember = card.CouncilMember;
        newCardScript.NameText.text = card.NameText.text;


        // remove the guild member from the council
        bool found = false;
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            if (_vill.Council[i] == card.CouncilMember)
            {
                _vill.Council[i] = null;
                card.CouncilMember = null;
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.Log("This should never get called");
            return;
        }

        // update the council member info card
        card.RemoveButton.gameObject.SetActive(false);
        card.onRemove.RemoveAllListeners();
        card.NameText.text = "No one";

        // add the add button again to everything
        for (int i = 0; i < ManageCouncilMenuGuildMembersHolder.transform.childCount; ++i)
        {
            AddGuildCouncilMemberCard thisCard = ManageCouncilMenuGuildMembersHolder.transform.GetChild(i).GetComponent<AddGuildCouncilMemberCard>();

            thisCard.AddButton.gameObject.SetActive(true);
        }

        Guild pGuild = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild;

        // count how many spots in the council are your guildies
        int councilSpotsOccupied = 0;
        for (int i = 0; i < _vill.Council.Count; ++i)
        {
            GameObject thisMember = _vill.Council[i];
            GuildMemberController gmContr = thisMember.GetComponent<GuildMemberController>();
            if (gmContr)
            {
                if (gmContr.NPCsGuild == pGuild)
                    councilSpotsOccupied++;
            }
        }

        if (councilSpotsOccupied == 0)
        {
            // there are no more council spots occupied by your guildies

            // remove the button
            CouncilDecisionsButton.gameObject.SetActive(false);
        }
    }
    private void CouncilDecisionsButtonClicked()
    {
        MainMenu.SetActive(false);
        CouncilDecisionsMenu.SetActive(true);
        _whichMenuUp = MenuType.CouncilAction;
    }
    private void KingDecisionsButtonClicked()
    {
        MainMenu.SetActive(false);
        KingDecisionsMenu.SetActive(true);
        _whichMenuUp = MenuType.KingAction;
    }
    private void RecruitRandomVillagerButtomClicked()
    {
        // get a random villager

        if (_vill.Villagers.Count < 7)
        {
            Debug.Log("Village needs to keep at least six villagers");
            return;
        }

        GameObject target;
        int idx = -1;
        bool canRecruit = false;
        do
        {
            idx = Random.Range(0, _vill.Villagers.Count);
            target = _vill.Villagers[idx];
            if (target)
            {
                if (!_vill.IsCouncilMember(target))
                {
                    canRecruit = true;
                }
            }
        } while (!canRecruit);

        // make the GuildMember that will be the new GuildMember
        GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild.AddMemberFromVillager(target.GetComponent<VillagerBehaviour>());

        // delete it from the list
        _vill.Villagers.Remove(_vill.Villagers[idx]);
        // delete the villager obj
        Destroy(target);
    }
    private void RenameVillage()
    {
        string newName = RenameVillageInputField.text;
        if (newName.Length > 0)
        {
            _vill.VillageName = newName;
            MenuHeader.text = "Townhall of " + newName;
        }
    }
    #endregion ButtonClicks & UICancel

    private void HandlePlayerInteract()
    {
        if (!_menuUp && ((GameManager.Instance.PlayerAvatar.transform.position - transform.position).sqrMagnitude < PlayerInteractRange))
        {
            GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = false;
            MainMenu.SetActive(true);
            _whichMenuUp = MenuType.Main;
            _menuUp = true;

            // Which buttons should NOT be visible?

            PlayerController pContr = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>();
            int amtPos = pContr.MyGuild.GetAmtCouncilPositionsFor(_vill.gameObject);
            bool playerKing = _vill.IsKing(pContr.gameObject);

            // If the player has all five council positions and is not yet king, be able to buy king. Else not.
            if (amtPos == 5 && !_vill.IsKing(pContr.gameObject))
                BuyKingPosition.gameObject.SetActive(true);
            else
                BuyKingPosition.gameObject.SetActive(false);

            // If the player does not have all five council members yet, be able to buy council. Else not.
            if (amtPos < 5)
                BuyCouncilPosition.gameObject.SetActive(true);
            else
                BuyCouncilPosition.gameObject.SetActive(false);

            // If the player does not have any council positions at all, he can not manage them
            if (amtPos == 0)
                ManageCouncilMembersButton.gameObject.SetActive(false);
            else
                ManageCouncilMembersButton.gameObject.SetActive(true);

            // If the player is not king, he is not able to see the king menu option.
            if (playerKing)
                KingDecisionsButton.gameObject.SetActive(true);
            else
                KingDecisionsButton.gameObject.SetActive(false);

            // If there is no guildies in the council, player can not see council decisions
            bool hasGuildies = false;
            for (int i = 0; i < _vill.Council.Count; ++i)
            {
                if (_vill.Council[i])
                {
                    GuildMemberController gmContr = _vill.Council[i].GetComponent<GuildMemberController>();
                    if (gmContr)
                    {
                        if (gmContr.NPCsGuild == pContr.MyGuild)
                        {
                            hasGuildies = true;
                            break;
                        }
                    }
                }
            }

            if (hasGuildies)
                CouncilDecisionsButton.gameObject.SetActive(true);
            else
                CouncilDecisionsButton.gameObject.SetActive(false);
        }
    }
    public void UpdateTaxesAmt(int newAmt)
    {
        CurrentAvailableTaxesText.text = "Stored Taxes: " + newAmt.ToString();
    }
    private void ClaimTaxes()
    {
        _vill.King.GetComponent<Inventory>().AddResource(Inventory.ResourceType.Gold, _vill.ClaimTaxes());
        CurrentAvailableTaxesText.text = "Stored Taxes: 0";
    }
}
