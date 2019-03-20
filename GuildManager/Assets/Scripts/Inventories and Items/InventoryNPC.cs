using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// NPC's Inventory
// "Take" is going from NPC to Player
// "Give" is going from Player to NPC

public class InventoryNPC : Inventory
{
    #region Declarations
    public InputField GoldInput;
    public InputField WoodInput;
    public InputField StoneInput;
    public InputField FoodInput;

    public Button GoldTakeButton;
    public Button GoldGiveButton;
    public Button WoodTakeButton;
    public Button WoodGiveButton;
    public Button StoneTakeButton;
    public Button StoneGiveButton;
    public Button FoodTakeButton;
    public Button FoodGiveButton;

    public Button GiveItemsButton;
    public Text GiveItemsText;
    public GameObject GiveItemsMenu;
    public GameObject GiveItemsMenuItemHolder;
    public bool GiveItemsMenuUp = false;

    private Transform _originalParent;
    #endregion Declarations

    new private void Start()
    {
        GoldTakeButton.onClick.AddListener(GoldTake);
        GoldGiveButton.onClick.AddListener(GoldGive);
        WoodTakeButton.onClick.AddListener(WoodTake);
        WoodGiveButton.onClick.AddListener(WoodGive);
        StoneTakeButton.onClick.AddListener(StoneTake);
        StoneGiveButton.onClick.AddListener(StoneGive);
        FoodTakeButton.onClick.AddListener(FoodTake);
        FoodGiveButton.onClick.AddListener(FoodGive);

        GiveItemsButton.onClick.AddListener(GiveItemsButtonClicked);

        WoodAmt.text = "Amount of wood: 0";
        StoneAmt.text = "Amount of stone: 0";
        FoodAmt.text = "Amount of food: 0";
        GoldAmt.text = "Gold: 0";
    }
    private void Update()
    {
        if (Input.GetButtonDown("UICancel") && GiveItemsMenuUp)
        {
            HandleUICancelButtonPressed();
        }
    }

    #region Handle Buttons
    private void GoldTake()
    {
        TakeResource(ResourceType.Gold);
    }
    private void GoldGive()
    {
        GiveResource(ResourceType.Gold);
    }
    private void WoodTake()
    {
        TakeResource(ResourceType.Wood);
    }
    private void WoodGive()
    {
        GiveResource(ResourceType.Wood);
    }
    private void StoneTake()
    {
        TakeResource(ResourceType.Stone);
    }
    private void StoneGive()
    {
        GiveResource(ResourceType.Stone);
    }
    private void FoodTake()
    {
        TakeResource(ResourceType.Food);
    }
    private void FoodGive()
    {
        GiveResource(ResourceType.Food);
    }
    private void GiveItemsButtonClicked()
    {
        GiveItemsMenuUp = true;
        GiveItemsMenu.SetActive(true);
        Inventory sourceInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
        if (sourceInv)
        {
            // save original parent
            _originalParent = sourceInv.ItemHolder.transform.parent;
            // set parent of the holder of the items to the holder of the thing
            sourceInv.ItemHolder.transform.SetParent(GiveItemsMenuItemHolder.transform);
            // set the scrollrect's content
            GiveItemsMenuItemHolder.GetComponent<ScrollRect>().content =
                GiveItemsMenuItemHolder.transform.GetChild(0).GetComponent<Image>().rectTransform;
            // set content of original holder to zero
            sourceInv.ItemHolder.transform.parent.GetComponent<ScrollRect>().content = null;
            // set scale & rotation & position
            GiveItemsMenuItemHolder.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
            GiveItemsMenuItemHolder.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);
            GiveItemsMenuItemHolder.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);

            // add listeners for all the things
            for (int i = 0; i < GiveItemsMenuItemHolder.transform.GetChild(0).childCount; ++i)
            {
                GameObject currChild = GiveItemsMenuItemHolder.transform.GetChild(0).GetChild(i).gameObject;

                currChild.GetComponent<InvItem>().PlayerClicked.AddListener(GiveItem);
            }
        }
    }
    #endregion Handle Buttons

    private void HandleUICancelButtonPressed()
    {
        // set things to false
        GiveItemsMenuUp = false;
        GiveItemsMenu.SetActive(false);
        // set parent back to original parent
        GiveItemsMenuItemHolder.transform.GetChild(0).SetParent(_originalParent);
        // set content
        _originalParent.GetComponent<ScrollRect>().content = _originalParent.transform.GetChild(0).GetComponent<Image>().rectTransform;
        // remove content
        GiveItemsMenuItemHolder.GetComponent<ScrollRect>().content = null;
        // set scale & rotation
        _originalParent.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        _originalParent.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);
        // remove listeners
        for (int i = 0; i < _originalParent.transform.GetChild(0).childCount; ++i)
        {
            GameObject currChild = _originalParent.transform.GetChild(0).GetChild(i).gameObject;
            currChild.GetComponent<InvItem>().PlayerClicked.RemoveAllListeners();
        }
    }



    private void TakeResource(ResourceType type)
    {
        int amtToTake = 0;
        int maxAmtToTake = 0;

        // set amts based on which type
        switch(type)
        {
            case ResourceType.Gold:
                amtToTake = int.Parse(GoldInput.text);
                maxAmtToTake = _amtGold;
                break;
            case ResourceType.Wood:
                amtToTake = int.Parse(WoodInput.text);
                maxAmtToTake = _amtWood;
                break;
            case ResourceType.Stone:
                amtToTake = int.Parse(StoneInput.text);
                maxAmtToTake = _amtStone;
                break;
            case ResourceType.Food:
                amtToTake = int.Parse(FoodInput.text);
                maxAmtToTake = _amtFood;
                break;
        }

        // is possible to take
        if (amtToTake <= maxAmtToTake)
        {
            Inventory targetInv = null;
            // get the guild leader who probably requested this
            GuildMemberController memberContr = GetComponent<GuildMemberController>();
            
            // get the target inv
            if (memberContr)
            {
                targetInv = memberContr.NPCsGuild.GuildLeader.GetComponent<Inventory>();
            }
            else
            {
                GuildManagementDesk desk = GetComponent<GuildManagementDesk>();
                targetInv = desk.DeskGuild.GuildLeader.GetComponent<Inventory>();
            }

            if (targetInv)
            {
                // give the resources to the target inv
                targetInv.AddResource(type, amtToTake);

                // take the resources away from this inv
                switch(type)
                {
                    case ResourceType.Gold:
                        _amtGold -= amtToTake;
                        GoldAmt.text = "Gold: " + _amtGold.ToString();
                        break;
                    case ResourceType.Wood:
                        _amtWood -= amtToTake;
                        WoodAmt.text = "Amount of wood: " + _amtWood.ToString();
                        break;
                    case ResourceType.Stone:
                        _amtStone -= amtToTake;
                        StoneAmt.text = "Amount of stone: " + _amtStone.ToString();
                        break;
                    case ResourceType.Food:
                        _amtFood -= amtToTake;
                        FoodAmt.text = "Amount of food: " + _amtFood.ToString();
                        break;
                }
            }
        }
    }
    private void GiveResource(ResourceType type)
    {
        int amtToGive = 0;
        int maxAmtToGive = 0;
        Inventory sourceInv = null;
        GuildManagementDesk desk = null;

        // if this inventory is on an NPC
        GuildMemberController memberContr = GetComponent<GuildMemberController>();
        if (memberContr)
        {
            // set the source inv to the guild leader's inventory
            sourceInv = memberContr.NPCsGuild.GuildLeader.GetComponent<Inventory>();
        }
        else
        {
            // this inventory is on a desk
            desk = GetComponent<GuildManagementDesk>();
            if (desk)
            {
                // source inv is the guild leader's inventory
                sourceInv = desk.DeskGuild.GuildLeader.GetComponent<Inventory>();
            }
        }

        maxAmtToGive = sourceInv.GetAmtResource(type);

        // set amts based on which type
        switch (type)
        {
            case ResourceType.Gold:
                amtToGive = int.Parse(GoldInput.text);
                break;
            case ResourceType.Wood:
                amtToGive = int.Parse(WoodInput.text);
                break;
            case ResourceType.Stone:
                amtToGive = int.Parse(StoneInput.text);
                break;
            case ResourceType.Food:
                amtToGive = int.Parse(FoodInput.text);
                break;
        }


        if (amtToGive <= maxAmtToGive)
        {
            if (sourceInv)
            {
                // take the resources from the source inv
                sourceInv.RemoveResources(type, amtToGive);

                // add the resources to this inv
                switch (type)
                {
                    case ResourceType.Gold:
                        _amtGold += amtToGive;
                        GoldAmt.text = "Gold: " + _amtGold.ToString();
                        break;
                    case ResourceType.Wood:
                        _amtWood += amtToGive;
                        WoodAmt.text = "Amount of wood: " + _amtWood.ToString();
                        break;
                    case ResourceType.Stone:
                        _amtStone += amtToGive;
                        StoneAmt.text = "Amount of stone: " + _amtStone.ToString();
                        break;
                    case ResourceType.Food:
                        _amtFood += amtToGive;
                        FoodAmt.text = "Amount of food: " + _amtFood.ToString();
                        break;
                }
            }
        }
    }

    new public void GiveItem(InvItem item)
    {
        item.transform.SetParent(ItemHolder.transform);
        item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localRotation = new Quaternion(0, 0, 0, 0);
        item.PlayerClicked.AddListener(ItemClicked);
    }
    private void ItemClicked(InvItem item)
    {
        bool canTake = false;
        Guild PlayersGuild = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild;

        GuildMemberController gmContr = GetComponent<GuildMemberController>();
        if (gmContr)
        {
            if (gmContr.NPCsGuild == PlayersGuild)
                canTake = true;
        }
        else
        {
            GuildManagementDesk gmDesk = GetComponent<GuildManagementDesk>();
            if (gmDesk)
            {
                if (gmDesk.DeskGuild == PlayersGuild)
                    canTake = true;
            }
        }

        if (!canTake)
        {
            Debug.Log("You can't take items from an inventory which does not belong to your guild");
            return;
        }

        Inventory targetInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();

        targetInv.GiveItem(item);
    }
}
