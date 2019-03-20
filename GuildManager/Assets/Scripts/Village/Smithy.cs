using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Buy, sell & upgrade weaponry and armor in this village building
public class Smithy : MonoBehaviour
{
    #region Declarations
    [Header("Main UI")]
    public GameObject MainMenu;
    public float PlayerInteractRange = 10.0f;

    [Header("Buy UI")]
    public Button Buy;
    public GameObject BuyMenu;
    public GameObject BuyMenuItemHolder; // parent of the items
    public GameObject BuyMenuItemPrefab; // item prefab
    public float TimeBetweenItemSpawns = 20.0f;
    private float _accuTimeBetweenItemSpawns;
    public int AmtItemsMax = 20;

    [Header("Sell UI")]
    public Button Sell;
    public GameObject SellMenu;
    public GameObject SellMenuItemHolder;

    [Header("Upgrade UI")]
    public Button Upgrade;
    public GameObject UpgradeMenu;
    public GameObject UpgradeMenuItemHolder;

    private enum MenuType
    {
        Main,
        Buy,
        Sell,
        Upgrade
    }
    private MenuType _whichMenuUp;
    private bool _menuUp;
    private Transform _originalParent = null;

    #endregion Declarations

    #region Main things
    private void Start()
    {
        Buy.onClick.AddListener(BuyClicked);
        Sell.onClick.AddListener(SellClicked);
        Upgrade.onClick.AddListener(UpgradeClicked);

        Interactable interac = GetComponent<Interactable>();
        if (interac)
        {
            interac.OnPlayerInteract.AddListener(HandlePlayerInteract);
        }
    }
    private void Update()
    {
        if (Input.GetButtonDown("UICancel") && _menuUp)
        {
            UICancelButtonPressed();
        }

        _accuTimeBetweenItemSpawns += Time.deltaTime;
        if (_accuTimeBetweenItemSpawns >= TimeBetweenItemSpawns)
        {
            _accuTimeBetweenItemSpawns -= TimeBetweenItemSpawns;

            if (BuyMenuItemHolder.transform.childCount < AmtItemsMax)
            {
                SpawnItem();
            }
        }
    }
    private void HandlePlayerInteract()
    {
        if ((GameManager.Instance.PlayerAvatar.transform.position - transform.position).sqrMagnitude < PlayerInteractRange)
        {
            MainMenu.SetActive(true);
            _whichMenuUp = MenuType.Main;
            _menuUp = true;
            GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = false;
        }
    }
    #endregion Main things

    private void SpawnItem()
    {
        GameObject NewItem = Instantiate(BuyMenuItemPrefab, BuyMenuItemHolder.transform);
        NewItem.transform.localRotation = new Quaternion(0, 0, 0, 0);
        NewItem.transform.localScale = new Vector3(1, 1, 1);
        int itemId = Mathf.RoundToInt(Random.Range(0, 7));
        NewItem.GetComponent<InvItem>().ItemType = (InvItem.Type)itemId;
        NewItem.GetComponent<InvItem>().SetTexture();
        NewItem.GetComponent<InvItem>().PlayerClicked.AddListener(TrySellItemToPlayer);
    }

    #region buttons
    private void UICancelButtonPressed()
    {
        switch (_whichMenuUp)
        {
            case MenuType.Main:
                GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = true;
                _menuUp = false;
                MainMenu.SetActive(false);

                break;
            case MenuType.Buy:
                BuyMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuUp = MenuType.Main;

                break;
            case MenuType.Sell:
                SellMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuUp = MenuType.Main;

                // gives items back to player
                for (int i = 0; i < SellMenuItemHolder.transform.GetChild(0).childCount; ++i)
                {
                    SellMenuItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.RemoveAllListeners();
                }
                ReturnItemsToPlayerFrom(SellMenuItemHolder);

                break;
            case MenuType.Upgrade:
                UpgradeMenu.SetActive(false);
                MainMenu.SetActive(true);
                _whichMenuUp = MenuType.Main;
                
                // give items back to player
                for (int i = 0; i < UpgradeMenuItemHolder.transform.GetChild(0).childCount; ++i)
                {
                    UpgradeMenuItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.RemoveAllListeners();
                }
                ReturnItemsToPlayerFrom(UpgradeMenuItemHolder);

                break;

        }
    }
    private void BuyClicked()
    {
        MainMenu.SetActive(false);
        BuyMenu.SetActive(true);
        _whichMenuUp = MenuType.Buy;
    }
    private void SellClicked()
    {
        MainMenu.SetActive(false);
        SellMenu.SetActive(true);
        _whichMenuUp = MenuType.Sell;
                

        MovePlayerInvInto(SellMenuItemHolder);
        for (int i = 0; i < SellMenuItemHolder.transform.GetChild(0).childCount; ++i)
        {
            GameObject currChild = SellMenuItemHolder.transform.GetChild(0).GetChild(i).gameObject;
            currChild.GetComponent<InvItem>().PlayerClicked.AddListener(TryBuyItemFromPlayer);
        }
    }
    private void UpgradeClicked()
    {
        MainMenu.SetActive(false);
        UpgradeMenu.SetActive(true);
        _whichMenuUp = MenuType.Upgrade;

        MovePlayerInvInto(UpgradeMenuItemHolder);
        for (int i = 0; i < UpgradeMenuItemHolder.transform.GetChild(0).childCount; ++i)
        {
            GameObject currChild = UpgradeMenuItemHolder.transform.GetChild(0).GetChild(i).gameObject;
            currChild.GetComponent<InvItem>().PlayerClicked.AddListener(TryUpgradePlayerItem);
        }
    }
    #endregion buttons

    
    private void MovePlayerInvInto(GameObject target)
    {
        Inventory sourceInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
        if (sourceInv)
        {
            // save original parent
            _originalParent = sourceInv.ItemHolder.transform.parent;
            // set new parent
            sourceInv.ItemHolder.transform.SetParent(target.transform);
            // set scrollrect's content
            target.GetComponent<ScrollRect>().content =
                target.transform.GetChild(0).GetComponent<Image>().rectTransform;
            // set original holder to zero
            _originalParent.GetComponent<ScrollRect>().content = null;
            // set scale, rotation and position
            target.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
            target.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);
            target.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        }
    }
    private void ReturnItemsToPlayerFrom(GameObject source)
    {
        Inventory targetInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
        if (targetInv)
        {
            // set parent back to original parent
            source.transform.GetChild(0).SetParent(_originalParent);
            // set contents
            source.GetComponent<ScrollRect>().content = null;
            _originalParent.GetComponent<ScrollRect>().content =
                _originalParent.transform.GetChild(0).GetComponent<Image>().rectTransform;
            // set transform
            _originalParent.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);
            _originalParent.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
            _originalParent.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        }
    }
    private void TryBuyItemFromPlayer(InvItem item)
    {
        // give the player the money
        int money = 0;

        string debugResult = "Successfully bought ";

        switch (item.ItemType)
        {
            case InvItem.Type.ArmorLeather:
                debugResult += "Leather Armor ";
                money = 30;
                break;
            case InvItem.Type.ArmorIron:
                debugResult += "Iron Armor ";
                money = 70;
                break;
            case InvItem.Type.ArmorSteel:
                debugResult += "Steel Armor ";
                money = 110;
                break;
            case InvItem.Type.ArmorMithril:
                debugResult += "Mithril Armor ";
                money = 150;
                break;
            case InvItem.Type.SwordWood:
                debugResult += "Wooden Sword ";
                money = 20;
                break;
            case InvItem.Type.SwordIron:
                debugResult += "Iron Sword ";
                money = 40;
                break;
            case InvItem.Type.SwordSteel:
                debugResult += "Steel Sword ";
                money = 60;
                break;
            case InvItem.Type.SwordMithril:
                debugResult += "Mithril Sword ";
                money = 100;
                break;
        }

        GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().AddResource(Inventory.ResourceType.Gold, money);

        debugResult += "from player!";
        Debug.Log(debugResult);

        // switch item ownership
        item.transform.SetParent(BuyMenuItemHolder.transform);
        item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localRotation = new Quaternion(0, 0, 0, 0);
        item.PlayerClicked.AddListener(TrySellItemToPlayer);
    }
    private void TrySellItemToPlayer(InvItem item)
    {
        int price = 0;


        string debugResult = "Successfully sold ";

        switch (item.ItemType)
        {
            case InvItem.Type.ArmorLeather:
                debugResult += "Leather Armor ";
                price = 50;
                break;
            case InvItem.Type.ArmorIron:
                debugResult += "Iron Armor ";
                price = 100;
                break;
            case InvItem.Type.ArmorSteel:
                debugResult += "Steel Armor ";
                price = 150;
                break;
            case InvItem.Type.ArmorMithril:
                debugResult += "Mithril Armor ";
                price = 200;
                break;
            case InvItem.Type.SwordWood:
                debugResult += "Wooden Sword ";
                price = 30;
                break;
            case InvItem.Type.SwordIron:
                debugResult += "Iron Sword ";
                price = 60;
                break;
            case InvItem.Type.SwordSteel:
                debugResult += "Steel Sword ";
                price = 100;
                break;
            case InvItem.Type.SwordMithril:
                debugResult += "Mithril Sword ";
                price = 150;
                break;
        }

        Inventory targetInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();

        int budget = targetInv.GetAmtResource(Inventory.ResourceType.Gold);

        if (budget >= price) // can afford
        {
            targetInv.RemoveResources(Inventory.ResourceType.Gold, price);
            item.PlayerClicked.RemoveAllListeners();
            targetInv.GiveItem(item);
            debugResult += "to player!";
            Debug.Log(debugResult);
        }
        else
        {
            Debug.Log("Could not afford this item");
        }
    }
    private void TryUpgradePlayerItem(InvItem item)
    {
        string debugResult = "Successfully upgraded ";
        int price = 0;
        bool canUpgrade = true;

        switch (item.ItemType)
        {
            case InvItem.Type.ArmorLeather:
                debugResult += "Leather Armor ";
                price = 50;
                break;
            case InvItem.Type.ArmorIron:
                debugResult += "Iron Armor ";
                price = 55;
                break;
            case InvItem.Type.ArmorSteel:
                debugResult += "Steel Armor ";
                price = 60;
                break;
            case InvItem.Type.ArmorMithril:
                debugResult = "Could not upgrade item, item is already at max tier!";
                canUpgrade = false;
                break;
            case InvItem.Type.SwordWood:
                debugResult += "Wooden Sword ";
                price = 35;
                break;
            case InvItem.Type.SwordIron:
                debugResult += "Iron Sword ";
                price = 45;
                break;
            case InvItem.Type.SwordSteel:
                debugResult += "Steel Sword ";
                price = 55;
                break;
            case InvItem.Type.SwordMithril:
                debugResult = "Could not upgrade item, item is already at max tier!";
                canUpgrade = false;
                break;
        }

        if (canUpgrade)
        {
            
            // if player can pay
            if (GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().GetAmtResource(Inventory.ResourceType.Gold) >= price)
            {
                debugResult += "for player!";

                GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().RemoveResources(Inventory.ResourceType.Gold, price);
                item.ItemType++;
                item.SetTexture();
            }
            else
            {
                debugResult = "Could not afford that upgrade";
            }
        }
        
        Debug.Log(debugResult);
    }
}
