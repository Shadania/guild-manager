using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// The script that allows anything to hold quests and manages them a little bit
public class QuestHolder : MonoBehaviour
{
    public GameObject QuestsHolder;

    public UnityEvent QuestAcquiredEvent = new UnityEvent();
    public UnityEvent QuestCompletedEvent = new UnityEvent();
    public UnityEvent QuestDoneEvent = new UnityEvent();


    protected void Start()
    {
        Inventory inv = GetComponent<Inventory>();
        if (inv)
        {
            inv.WoodGot.AddListener(HandleWoodGot);
            inv.StoneGot.AddListener(HandleStoneGot);
            inv.FoodGot.AddListener(HandleFoodGot);
            inv.GoldGot.AddListener(HandleGoldGot);
        }
    }

    public void AddQuest(GameObject q)
    {
        q.transform.SetParent(QuestsHolder.transform);
        q.GetComponent<Quest>().MoneyOnQuestComplete.RemoveAllListeners();
        q.GetComponent<Quest>().MoneyOnQuestComplete.AddListener(QuestCompleted);
        q.GetComponent<Quest>().InfluenceOnQuestComplete.RemoveAllListeners();
        q.GetComponent<Quest>().InfluenceOnQuestComplete.AddListener(HandleInfluenceGot); 

        q.transform.localScale = new Vector3(1, 1, 1);
        q.transform.localRotation = new Quaternion(0, 0, 0, 0);
        q.transform.localPosition = new Vector3(0, 0, 0);

        QuestAcquiredEvent.Invoke();
        q.GetComponent<Quest>().QuestDone.AddListener(QuestDoneDemoSceneFunction);
    }

    protected void HandleInfluenceGot(Village vill, int influence)
    {
        PlayerController pContr = GetComponent<PlayerController>();
        if (pContr)
        {
            pContr.MyGuild.AddInfluenceFor(vill.gameObject, influence);
        }
        else
        {
            GuildMemberController gmContr = GetComponent<GuildMemberController>();
            if (gmContr)
            {
                gmContr.NPCsGuild.AddInfluenceFor(vill.gameObject, influence);
            }
        }
    }
    protected void QuestCompleted(int rewardGold)
    {
        Inventory inv = GetComponent<Inventory>();
        if (inv)
        {
            inv.AddResource(Inventory.ResourceType.Gold, rewardGold);
        }

        QuestCompletedEvent.Invoke();
    }

    protected void QuestDoneDemoSceneFunction()
    {
        QuestDoneEvent.Invoke();
    }

    #region handle resource got
    protected void HandleWoodGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            Quest currQ = QuestsHolder.transform.GetChild(i).GetComponent<Quest>();

            if (currQ)
            {
                if (!currQ.IsQuestComplete && currQ.QuestType == Quest.Type.GatherWood)
                {
                    currQ.AddDoneAmt(amt);
                    return;
                }
            }
        }
    }
    protected void HandleStoneGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            Quest currQ = QuestsHolder.transform.GetChild(i).GetComponent<Quest>();

            if (currQ)
            {
                if (!currQ.IsQuestComplete && currQ.QuestType == Quest.Type.GatherStone)
                {
                    currQ.AddDoneAmt(amt);
                    return;
                }
            }
        }

    }
    protected void HandleFoodGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            Quest currQ = QuestsHolder.transform.GetChild(i).GetComponent<Quest>();

            if (currQ)
            {
                if (!currQ.IsQuestComplete && currQ.QuestType == Quest.Type.GatherFood)
                {
                    currQ.AddDoneAmt(amt);
                    return;
                }
            }
        }

    }
    protected void HandleGoldGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            Quest currQ = QuestsHolder.transform.GetChild(i).GetComponent<Quest>();
        
            if (currQ)
            {
                if (!currQ.IsQuestComplete && currQ.QuestType == Quest.Type.GatherGold)
                {
                    currQ.AddDoneAmt(amt);
                    return;
                }
            }
        }
    }

    #endregion handle resource got
}
