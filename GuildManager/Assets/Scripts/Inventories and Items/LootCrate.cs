using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootCrate : MonoBehaviour
{
    #region Declarations
    public GameObject Menu;
    public GameObject ItemHolder;
    public Button TakeResourcesButton;

    public int AmtWood;
    public int AmtGold;
    public int AmtStone;
    public int AmtFood;    

    public Text WoodText;
    public Text GoldText;
    public Text StoneText;
    public Text FoodText;

    private bool _menuUp = false;
    private bool _resourcesTaken = false;
    #endregion Declarations

    private void Start()
    {
        TakeResourcesButton.onClick.AddListener(TakeResourcesButtonClicked);

        Interactable interac = GetComponent<Interactable>();
        if (interac)
        {
            interac.OnPlayerInteract.AddListener(HandlePlayerInteract);
        }
    }
    private void Update()
    {
        if (_menuUp && Input.GetButtonDown("UICancel"))
        {
            Menu.SetActive(false);
            _menuUp = false;
        }
    }

    private void HandlePlayerInteract()
    {
        if (!_menuUp)
        {
            Menu.SetActive(true);
            _menuUp = true;
        }
    }
    private void TakeResourcesButtonClicked()
    {
        _resourcesTaken = true;

        Inventory targetInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();

        targetInv.AddResource(Inventory.ResourceType.Gold, AmtGold);
        targetInv.AddResource(Inventory.ResourceType.Wood, AmtWood);
        targetInv.AddResource(Inventory.ResourceType.Stone, AmtStone);
        targetInv.AddResource(Inventory.ResourceType.Food, AmtFood);

        CheckIfShouldDestroy();
    }
    // called by the thing that spawns the lootcrate (the static function)
    public void Init()
    {
        // Add listeners to items
        if (ItemHolder.transform.childCount > 0)
        {
            for (int i = 0; i < ItemHolder.transform.GetChild(0).childCount; ++i)
            {
                ItemHolder.transform.GetChild(0).GetChild(i).GetComponent<InvItem>().PlayerClicked.AddListener(ItemClicked);
            }
        }

        // Init text
        WoodText.text = "Wood: " + AmtWood.ToString();
        GoldText.text = "Gold: " + AmtGold.ToString();
        StoneText.text = "Stone: " + AmtStone.ToString();
        FoodText.text = "Food: " + AmtFood.ToString();

        if ((AmtWood + AmtStone + AmtGold + AmtFood) == 0)
            _resourcesTaken = true;

        CheckIfShouldDestroy();
    }

    private void ItemClicked(InvItem item)
    {
        GameManager.Instance.PlayerAvatar.GetComponent<Inventory>().GiveItem(item);
        
        CheckIfShouldDestroy();
    }



    public static void SpawnLootcrateOn(Transform target)
    {
        GameObject LootCrateObj = Instantiate(GameManager.Instance.LootCratePrefab);
        LootCrateObj.transform.position = new Vector3(target.transform.position.x, LootCrateObj.transform.position.y, target.transform.position.z);
        LootCrate crate = LootCrateObj.GetComponent<LootCrate>();
        Inventory myInv = target.GetComponent<Inventory>();

        crate.AmtGold = myInv.GetAmtResource(Inventory.ResourceType.Gold);
        crate.AmtWood = myInv.GetAmtResource(Inventory.ResourceType.Wood);
        crate.AmtStone = myInv.GetAmtResource(Inventory.ResourceType.Stone);
        crate.AmtFood = myInv.GetAmtResource(Inventory.ResourceType.Food);

        myInv.ItemHolder.transform.SetParent(crate.ItemHolder.transform);
        crate.ItemHolder.transform.GetChild(0).localRotation = new Quaternion(0, 0, 0, 0);
        crate.ItemHolder.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        crate.ItemHolder.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        crate.ItemHolder.GetComponent<ScrollRect>().content =
            crate.ItemHolder.transform.GetChild(0).GetComponent<Image>().rectTransform;

        crate.Init();
    }
    public static void SpawnResourcesOnly(Transform target)
    {
        GameObject LootCrateObj = Instantiate(GameManager.Instance.LootCratePrefab);
        // LootCrateObj.transform.position = new Vector3(target.transform.position.x, LootCrateObj.transform.position.y, target.transform.position.z);
        LootCrateObj.transform.position = target.transform.position;
        LootCrate crate = LootCrateObj.GetComponent<LootCrate>();

        crate.AmtGold = Random.Range(0,9);
        crate.AmtWood = Random.Range(0, 9);
        crate.AmtStone = Random.Range(0, 9);
        crate.AmtFood = Random.Range(0, 9);

        crate.Init();
    }

    private void CheckIfShouldDestroy()
    {
        if (ItemHolder.transform.childCount > 0)
        {
            if (ItemHolder.transform.GetChild(0).childCount == 0 && _resourcesTaken)
            {
                Destroy(gameObject);
            }
        }
        else if (_resourcesTaken)
        {
            Destroy(gameObject);
        }
    }
}
