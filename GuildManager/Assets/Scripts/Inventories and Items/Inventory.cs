using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class InventoryEvent : UnityEvent<int>
{}

public class Inventory : MonoBehaviour
{
    #region Declarations
    public InventoryEvent StoneGot = new InventoryEvent();
    public InventoryEvent WoodGot = new InventoryEvent();
    public InventoryEvent FoodGot = new InventoryEvent();
    public InventoryEvent GoldGot = new InventoryEvent();

    public Text WoodAmt;
    public Text StoneAmt;
    public Text FoodAmt;
    public Text GoldAmt;

    protected int _amtStone;
    protected int _amtWood;
    protected int _amtFood;
    protected int _amtGold;

    public GameObject ItemHolder;

    public enum ResourceType
    {
        Gold,
        Wood,
        Stone,
        Food
    }
    #endregion Declarations


    protected void Start()
    {
        WoodAmt.text = "Amount of wood: 0";
        StoneAmt.text = "Amount of stone: 0";
        FoodAmt.text = "Amount of food: 0";
        GoldAmt.text = "Gold: 0";
    }

    public void AddResource(ResourceType type, int amt)
    {
        switch (type)
        {
            case ResourceType.Gold:
                _amtGold += amt;
                GoldAmt.text = "Gold: " + _amtGold.ToString();
                GoldGot.Invoke(amt);

                GetComponent<AudioSource>().clip = GameManager.Instance.GoldSoundEffect;
                GetComponent<AudioSource>().Play();

                break;

            case ResourceType.Wood:
                _amtWood += amt;
                WoodAmt.text = "Amount of wood: " + _amtWood.ToString();
                WoodGot.Invoke(amt);
                break;

            case ResourceType.Stone:
                _amtStone += amt;
                StoneAmt.text = "Amount of stone: " + _amtStone.ToString();
                StoneGot.Invoke(amt);
                break;

            case ResourceType.Food:
                _amtFood += amt;
                FoodAmt.text = "Amount of food: " + _amtFood.ToString();
                FoodGot.Invoke(amt);
                break;
        }
    }

    public int GetAmtResource(ResourceType type)
    {
        switch(type)
        {
            case ResourceType.Gold:
                return _amtGold;
                
            case ResourceType.Wood:
                return _amtWood;

            case ResourceType.Stone:
                return _amtStone;

            case ResourceType.Food:
                return _amtFood;
        }

        return 0;
    }

    // ONLY USE WHEN YOU ARE SURE THAT THE AMT TAKEN IS <= AMT YOU HAVE
    public int RemoveResources(ResourceType type, int amt)
    {
        switch (type)
        {
            case ResourceType.Gold:
                _amtGold -= amt;
                GoldAmt.text = "Gold: " + _amtGold.ToString();

                GetComponent<AudioSource>().clip = GameManager.Instance.GoldSoundEffect;
                GetComponent<AudioSource>().Play();

                break;

            case ResourceType.Wood:
                _amtWood -= amt;
                WoodAmt.text = "Amount of wood: " + _amtWood.ToString();
                break;

            case ResourceType.Stone:
                _amtStone -= amt;
                StoneAmt.text = "Amount of stone: " + _amtStone.ToString();
                break;

            case ResourceType.Food:
                _amtFood -= amt;
                FoodAmt.text = "Amount of food: " + _amtFood.ToString();
                break;
        }

        return 0;
    }

    // Clarification: The item in the params is the item being given TO THIS INV
    public void GiveItem(InvItem item)
    {
        item.transform.SetParent(ItemHolder.transform);
        item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localRotation = new Quaternion(0, 0, 0, 0);
    }
}
