using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Town building where you can buy and sell resources for gold
public class Market : MonoBehaviour
{
    #region Declarations
    [Header("General (UI)")]
    public GameObject MarketMenu;
    public float PlayerInteractRange = 100.0f; // not a typo, it really has to be this large
    public float TradingFee = 0.3f; // basically kind of trading taxes to make it less profitable

    [Header("UI Wood")]
    public InputField WoodInputField;
    public Button BuyWoodButton;
    public Button SellWoodButton;
    public Text WoodBuySellRate;
    private const float _woodToGoldRateOriginal = 1.2f;
    private float _woodToGoldRate = 1.2f;

    [Header("UI Stone")]
    public InputField StoneInputField;
    public Button BuyStoneButton;
    public Button SellStoneButton;
    public Text StoneBuySellRate;
    private const float _stoneToGoldRateOriginal = 1.2f;
    private float _stoneToGoldRate = 1.2f;

    [Header("UI Food")]
    public InputField FoodInputField;
    public Button BuyFoodButton;
    public Button SellFoodButton;
    public Text FoodBuySellRate;
    private const float _foodToGoldRateOriginal = 1.5f;
    private float _foodToGoldRate = 1.5f;

    private bool _menuUp = false;
    private const float _maxTickDuration = 3.0f;
    private float _currTickDuration;
    #endregion Declarations

    private void Start()
    {
        BuyWoodButton.onClick.AddListener(BuyWoodButtonClicked);
        SellWoodButton.onClick.AddListener(SellWoodButtonClicked);

        BuyStoneButton.onClick.AddListener(BuyStoneButtonClicked);
        SellStoneButton.onClick.AddListener(SellStoneButtonClicked);

        BuyFoodButton.onClick.AddListener(BuyFoodButtonClicked);
        SellFoodButton.onClick.AddListener(SellFoodButtonClicked);


        WoodBuySellRate.text = "Wood to gold ratio: " + _woodToGoldRate.ToString();
        StoneBuySellRate.text = "Stone to gold ratio: " + _stoneToGoldRate.ToString();
        FoodBuySellRate.text = "Food to gold ratio: " + _foodToGoldRate.ToString();

        Interactable interac = GetComponent<Interactable>();
        if (interac)
            interac.OnPlayerInteract.AddListener(HandlePlayerInteract);
    }
    
    public void HandlePlayerInteract()
    {
        if ((GameManager.Instance.PlayerAvatar.transform.position - transform.position).sqrMagnitude < PlayerInteractRange)
        {
            MarketMenu.SetActive(true);
            _menuUp = true;
            GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = false;
        }
    }
    
    private void Update()
    {
        _currTickDuration += Time.deltaTime;
        if (_currTickDuration >= _maxTickDuration)
        {
            _currTickDuration -= _maxTickDuration;


            // even out the rates to their originals over time

            if (Mathf.Abs(_woodToGoldRate - _woodToGoldRateOriginal) > 0.01f)
            {
                if ((_woodToGoldRate - _woodToGoldRateOriginal) > 0) // the first one is bigger, we must go down
                {
                    _woodToGoldRate -= 0.01f;
                }
                else
                {
                    _woodToGoldRate += 0.01f;
                }
                WoodBuySellRate.text = "Wood to gold ratio: " + _woodToGoldRate.ToString("F2");
            }

            if (Mathf.Abs(_stoneToGoldRate - _stoneToGoldRateOriginal) > 0.01f)
            {
                if ((_stoneToGoldRate - _stoneToGoldRateOriginal) > 0) // the first one is bigger, we must go down
                {
                    _stoneToGoldRate -= 0.01f;
                }
                else
                {
                    _stoneToGoldRate += 0.01f;
                }
                StoneBuySellRate.text = "Stone to gold ratio: " + _stoneToGoldRate.ToString("F2");
            }

            if (Mathf.Abs(_foodToGoldRate - _foodToGoldRateOriginal) > 0.01f)
            {
                if ((_foodToGoldRate - _foodToGoldRateOriginal) > 0) // the first one is bigger, we must go down
                {
                    _foodToGoldRate -= 0.01f;
                }
                else
                {
                    _foodToGoldRate += 0.01f;
                }
                FoodBuySellRate.text = "Food to gold ratio: " + _foodToGoldRate.ToString("F2");
            }
        }

        if (_menuUp && Input.GetButtonDown("UICancel"))
        {
            _menuUp = false;
            MarketMenu.SetActive(false);
            GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().AcceptsInput = true;
        }
    }

    #region Buttons
    private void BuyWoodButtonClicked()
    {
        if (WoodInputField.text.Length > 0)
        {
            Inventory targetInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
            int amtWood = int.Parse(WoodInputField.text);
            int price = Mathf.RoundToInt((amtWood * _woodToGoldRate) * (1.0f + TradingFee));

            if (targetInv.GetAmtResource(Inventory.ResourceType.Gold) >= price)
            {
                targetInv.RemoveResources(Inventory.ResourceType.Gold, price);
                targetInv.AddResource(Inventory.ResourceType.Wood, amtWood);
                _woodToGoldRate += amtWood * 0.02f;
            }

            if (_woodToGoldRate > 3.0f)
                _woodToGoldRate = 3.0f;

            WoodBuySellRate.text = "Wood to gold ratio: " + _woodToGoldRate.ToString("F2");
        }
        
    }
    private void SellWoodButtonClicked()
    {
        if (WoodInputField.text.Length > 0)
        {
            Inventory sourceInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
            int amtWood = int.Parse(WoodInputField.text);

            if (sourceInv.GetAmtResource(Inventory.ResourceType.Wood) >= amtWood)
            {
                int price = Mathf.RoundToInt((amtWood * _woodToGoldRate) * (1.0f - TradingFee));
                sourceInv.RemoveResources(Inventory.ResourceType.Wood, amtWood);
                sourceInv.AddResource(Inventory.ResourceType.Gold, price);
                _woodToGoldRate -= amtWood * 0.02f;
            }

            if (_woodToGoldRate < 0.3f)
                _woodToGoldRate = 0.3f;

            WoodBuySellRate.text = "Wood to gold ratio: " + _woodToGoldRate.ToString("F2");
        }
    }
    private void BuyStoneButtonClicked()
    {
        if (StoneInputField.text.Length > 0)
        {
            Inventory targetInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
            int amtStone = int.Parse(StoneInputField.text);
            int price = Mathf.RoundToInt((amtStone * _stoneToGoldRate) * (1.0f + TradingFee));

            if (targetInv.GetAmtResource(Inventory.ResourceType.Gold) >= price)
            {
                targetInv.RemoveResources(Inventory.ResourceType.Gold, price);
                targetInv.AddResource(Inventory.ResourceType.Stone, amtStone);
                _stoneToGoldRate += amtStone * 0.02f;
            }

            if (_stoneToGoldRate > 3.0f)
                _stoneToGoldRate = 3.0f;

            StoneBuySellRate.text = "Stone to gold ratio: " + _stoneToGoldRate.ToString("F2");
        }
    }
    private void SellStoneButtonClicked()
    {
        if (StoneInputField.text.Length > 0)
        {
            Inventory sourceInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
            int amtStone = int.Parse(StoneInputField.text);

            if (sourceInv.GetAmtResource(Inventory.ResourceType.Stone) >= amtStone)
            {
                int price = Mathf.RoundToInt((amtStone * _stoneToGoldRate) * (1.0f - TradingFee));
                sourceInv.RemoveResources(Inventory.ResourceType.Stone, amtStone);
                sourceInv.AddResource(Inventory.ResourceType.Gold, price);
                _stoneToGoldRate -= amtStone * 0.02f;
            }

            if (_stoneToGoldRate < 0.3f)
                _stoneToGoldRate = 0.3f;

            StoneBuySellRate.text = "Stone to gold ratio: " + _stoneToGoldRate.ToString("F2");
        }
    }
    private void BuyFoodButtonClicked()
    {
        if (FoodInputField.text.Length > 0)
        {
            Inventory targetInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
            int amtFood = int.Parse(FoodInputField.text);
            int price = Mathf.RoundToInt((amtFood * _foodToGoldRate) * (1.0f + TradingFee));

            if (targetInv.GetAmtResource(Inventory.ResourceType.Gold) >= price)
            {
                targetInv.RemoveResources(Inventory.ResourceType.Gold, price);
                targetInv.AddResource(Inventory.ResourceType.Food, amtFood);
                _foodToGoldRate += amtFood * 0.02f;
            }

            if (_foodToGoldRate > 3.0f)
                _foodToGoldRate = 3.0f;

            FoodBuySellRate.text = "Food to gold ratio: " + _foodToGoldRate.ToString("F2");
        }
    }
    private void SellFoodButtonClicked()
    {
        if (FoodInputField.text.Length > 0)
        {
            Inventory sourceInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
            int amtFood = int.Parse(FoodInputField.text);

            if (sourceInv.GetAmtResource(Inventory.ResourceType.Food) >= amtFood)
            {
                int price = Mathf.RoundToInt((amtFood * _foodToGoldRate) * (1.0f - TradingFee));
                sourceInv.RemoveResources(Inventory.ResourceType.Food, amtFood);
                sourceInv.AddResource(Inventory.ResourceType.Gold, price);
                _foodToGoldRate -= amtFood * 0.02f;
            }

            if (_foodToGoldRate < 0.3f)
                _foodToGoldRate = 0.3f;

            FoodBuySellRate.text = "Food to gold ratio: " + _foodToGoldRate.ToString("F2");
        }
    }
    #endregion Buttons
}
