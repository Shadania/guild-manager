using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The yellowish papers that spawn on the quest boards
public class QuestParchment : MonoBehaviour
{
    #region Declarations
    [Header("Quest stuff")]
    public Quest.Type QuestType = Quest.Type.GatherWood;
    public int Quota = 10;
    public int DoneAmt = 0;
    public int RewardGold = 20;
    public int RewardInfluence = 5;
    public GameObject QuestPrefab;
    public Village SourceVillage;

    [Header("UI")]
    public GameObject QuestParchmentMenu;
    private bool _isMenuOpen;
    public Text QuestName;
    public Text QuestDescription;
    public Button AcceptQuest;
    #endregion Declarations

    
    private void Start()
    {
        Interactable interac = GetComponent<Interactable>();
        if (interac)
        {
            interac.OnPlayerInteract.AddListener(HandlePlayerInteract);
        }

        AcceptQuest.onClick.AddListener(HandleAcceptQuestClickedPlayer);


        switch (QuestType)
        {
            case Quest.Type.GatherWood:
                QuestName.text = "Gather wood";
                QuestDescription.text = "Chop down trees to get wood.\n" +
                    "For this quest you need to gather " + Quota.ToString() + " wood.";
                break;

            case Quest.Type.GatherStone:
                QuestName.text = "Gather stone";
                QuestDescription.text = "Cut some rocks to get stone.\n" +
                    "For this quest you need to gather " + Quota.ToString() + " stone.";
                break;

            case Quest.Type.GatherFood:
                QuestName.text = "Gather food";
                QuestDescription.text = "Gather berries from berry bushes to get food.\n" +
                    "For this quest you need to gather " + Quota.ToString() + " food.";
                break;

            case Quest.Type.GatherGold:
                QuestName.text = "Gather gold";
                QuestDescription.text = "For this quest you need to gather " + Quota.ToString() + " gold.";
                break;
        }

        QuestDescription.text += "\nThe reward money for this quest is " + RewardGold.ToString() + " gold.";
        QuestDescription.text += "\nThe reward influence for your guild for this quest is " + RewardInfluence.ToString() + '.';

    }

    private void HandlePlayerInteract()
    {
        PlayerController pContr = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>();
        if (pContr)
        {
            if (pContr.AcceptsInput)
            {
                QuestSource mySource = transform.parent.parent.GetComponent<QuestSource>();
                if (mySource.IsPlayerInTrigger() && !_isMenuOpen)
                {
                    // display menu
                    _isMenuOpen = true;
                    QuestParchmentMenu.SetActive(_isMenuOpen);

                    pContr.AcceptsInput = false;
                }
            }
        }
    }
    public void AcceptQuestNPC(QuestHolderNPC qHolder)
    {
        GameObject newQobj = Instantiate(QuestPrefab);
        Quest newQuest = newQobj.GetComponent<Quest>();

        newQuest.QuestType = QuestType;
        newQuest.Quota = Quota;
        newQuest.RewardGoldAmt = RewardGold;
        newQuest.SourceVillage = SourceVillage;

        newQobj.transform.localRotation = new Quaternion(0, 0, 0, 0);
        newQobj.transform.localPosition = new Vector3(0, 0, 0);
        newQobj.transform.localScale = new Vector3(1, 1, 1);

        newQuest.QuestInit();

        qHolder.AddQuest(newQobj);

        Destroy(gameObject);
    }
    private void HandleAcceptQuestClickedPlayer()
    {
        GameObject recipient = GameManager.Instance.PlayerAvatar;

        if (recipient)
        {
            QuestHolder questHolder = recipient.GetComponent<QuestHolder>();
            
            GameObject newQobj = Instantiate(QuestPrefab);
            Quest newQuest = newQobj.GetComponent<Quest>();

            newQuest.QuestType = QuestType;
            newQuest.Quota = Quota;
            newQuest.RewardGoldAmt = RewardGold;
            newQuest.RewardInfluence = RewardInfluence;
            newQuest.SourceVillage = SourceVillage;

            newQuest.QuestInit();

            questHolder.AddQuest(newQobj);
            
            PlayerController pContr = recipient.GetComponent<PlayerController>();
            pContr.AcceptsInput = true;

            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (_isMenuOpen)
        {
            if (Input.GetButtonDown("UICancel"))
            {
                _isMenuOpen = false;
                QuestParchmentMenu.SetActive(_isMenuOpen);
                PlayerController pContr = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>();
                if (pContr)
                {
                    pContr.AcceptsInput = true;
                }
            }
        }
    }
}
