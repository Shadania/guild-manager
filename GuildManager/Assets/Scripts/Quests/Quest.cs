using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class QuestEvent : UnityEvent<int>
{ };

[System.Serializable]
public class QuestInfluenceEvent : UnityEvent<Village, int>
{ };

public class Quest : MonoBehaviour
{
    #region Declarations
    public QuestEvent MoneyOnQuestComplete = new QuestEvent();
    public QuestInfluenceEvent InfluenceOnQuestComplete = new QuestInfluenceEvent();
    public UnityEvent QuestDone = new UnityEvent();
    public UnityEvent QuestAbandoned = new UnityEvent();

    [Header("General")]

    public Type QuestType = Type.GatherWood;
    public int Quota = 10;
    public int DoneAmt = 0;
    public int RewardGoldAmt = 20;
    public int RewardInfluence = 5;
    public bool IsQuestComplete = false;
    public enum Type
    {
        GatherWood,
        GatherStone,
        GatherFood,
        GatherGold
    };
    public Village SourceVillage;



    [Header("UI")]
    public Text QuestName;
    public Text QuestDescription;
    public Text QuestProgress;
    public Text QuestReward;
    public Button CompleteQuestButton;
    public Button AbandonQuestButton;

    public GameObject CompleteQuestButtonObject; // game object because it needs to hide at first

    private bool _questCompleteButtonAlreadyClicked = false;
    #endregion Declarations

    private void Start()
    {
        CompleteQuestButton.onClick.AddListener(CompleteQuestButtonClicked);
        AbandonQuestButton.onClick.AddListener(AbandonQuestButtonClicked);
    }

    // quest init of questname, questdesc and questprog
    public void QuestInit()
    {
        switch (QuestType)
        {
            case Type.GatherWood:
                QuestName.text = "Gather wood";
                QuestDescription.text = "Chop down some trees to get wood";
                QuestProgress.text = "Wood gathered from trees: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                break;

            case Type.GatherStone:
                QuestName.text = "Gather stone";
                QuestDescription.text = "Cut some rocks to get stone";
                QuestProgress.text = "Stone gathered from rocks: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                break;

            case Type.GatherFood:
                QuestName.text = "Gather food";
                QuestDescription.text = "Pick some berries from bushes to get food";
                QuestProgress.text = "Food gathered from bushes: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                break;

            case Type.GatherGold:
                QuestName.text = "Gather gold";
                QuestDescription.text = "Get gold from various sources";
                QuestProgress.text = "Gold got: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                break;
        }

        QuestReward.text = "Reward gold for completing this quest: " + RewardGoldAmt.ToString();
        QuestReward.text += "\nReward influence for completing this quest: " + RewardInfluence.ToString();
    }
    
    public void CompleteQuestButtonClicked()
    {
        if (!_questCompleteButtonAlreadyClicked) // prevents NPC spamming the button
        {
            _questCompleteButtonAlreadyClicked = true;

            if (SourceVillage)
            {
                // give reward influence
                InfluenceOnQuestComplete.Invoke(SourceVillage, RewardInfluence);
            }

            // give reward money
            MoneyOnQuestComplete.Invoke(RewardGoldAmt);

            Destroy(gameObject);
        }
    }
    public void AbandonQuestButtonClicked()
    {
        QuestAbandoned.Invoke();
        Destroy(gameObject);
    }

    public void AddDoneAmt(int amt)
    {
        if (!IsQuestComplete)
        {
            DoneAmt += amt;

            switch (QuestType)
            {
                case Type.GatherWood:
                    QuestProgress.text = "Wood gathered from trees: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                    break;

                case Type.GatherStone:
                    QuestProgress.text = "Stone gathered from rocks: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                    break;

                case Type.GatherFood:
                    QuestProgress.text = "Food gathered from bushes: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                    break;

                case Type.GatherGold:
                    QuestProgress.text = "Gold got: " + DoneAmt.ToString() + " out of " + Quota.ToString();
                    break;
            }

            if (DoneAmt >= Quota)
            {
                IsQuestComplete = true;
                CompleteQuestButtonObject.SetActive(IsQuestComplete);
                QuestDone.Invoke();
            }
        }
        // else
        // {
        //     QuestDone.Invoke();
        // }
    }
}
