using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// QuestHolder for an NPC, to wrap a quest in the NPC wrapper
// before adding it to itself
public class QuestHolderNPC : QuestHolder
{
    public GameObject NPCQuestWrapper;
    public UnityEvent RecheckQuests;
    
    new private void Start()
    {
        InventoryNPC inv = GetComponent<InventoryNPC>();
        inv.WoodGot.AddListener(HandleWoodGot);
        inv.StoneGot.AddListener(HandleStoneGot);
        inv.FoodGot.AddListener(HandleFoodGot);
        inv.GoldGot.AddListener(HandleGoldGot);
    }
    new public void AddQuest(GameObject q)
    {
        GameObject newQ = Instantiate(NPCQuestWrapper);
        newQ.transform.SetParent(QuestsHolder.transform);
        q.transform.SetParent(newQ.transform);

        q.GetComponent<Quest>().QuestDone.AddListener(QuestDone);
        q.GetComponent<Quest>().MoneyOnQuestComplete.RemoveAllListeners();
        q.GetComponent<Quest>().MoneyOnQuestComplete.AddListener(QuestCompleted);
        q.GetComponent<Quest>().InfluenceOnQuestComplete.RemoveAllListeners();
        q.GetComponent<Quest>().InfluenceOnQuestComplete.AddListener(HandleInfluenceGot);

        q.transform.localScale = new Vector3(1, 1, 1);
        q.transform.localRotation = new Quaternion(0, 0, 0, 0);
        q.transform.localPosition = new Vector3(0, 0, 0);

        newQ.transform.localScale = new Vector3(1, 1, 1);
        newQ.transform.localRotation = new Quaternion(0, 0, 0, 0);
        newQ.transform.localPosition = new Vector3(0, 0, 0);

        newQ.GetComponent<NPCQuestWrapper>().SetQuest(q);
        newQ.GetComponent<NPCQuestWrapper>().QuestTakenAway.AddListener(FireRecheckQuests);
    }

    private void QuestDone()
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            Quest currQ = QuestsHolder.transform.GetChild(i).GetComponent<NPCQuestWrapper>().QuestObject.GetComponent<Quest>();
            if (currQ)
            {
                if (currQ.IsQuestComplete)
                {
                    currQ.CompleteQuestButtonClicked();
                }
            }
        }
    }
    private void FireRecheckQuests()
    {
        RecheckQuests.Invoke();
    }


    #region Resource handling
    new private void HandleWoodGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            NPCQuestWrapper qWrap = QuestsHolder.transform.GetChild(i).GetComponent<NPCQuestWrapper>();

            if (qWrap)
            {
                if (!qWrap.QuestObject)
                {
                    Destroy(gameObject);
                    return;
                }

                Quest currQ = qWrap.QuestObject.GetComponent<Quest>();

                if (currQ)
                {

                    if (currQ.QuestType == Quest.Type.GatherWood)
                    {
                        currQ.AddDoneAmt(amt);
                        return;
                    }
                }
            }
        }
    }
    new private void HandleStoneGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            NPCQuestWrapper qWrap = QuestsHolder.transform.GetChild(i).GetComponent<NPCQuestWrapper>();

            if (qWrap)
            {
                if (!qWrap.QuestObject)
                {
                    Destroy(gameObject);
                    return;
                }

                Quest currQ = qWrap.QuestObject.GetComponent<Quest>();

                if (currQ)
                {
                    if (currQ.QuestType == Quest.Type.GatherStone)
                    {
                        currQ.AddDoneAmt(amt);
                        return;
                    }
                }
            }
        }
    }
    new private void HandleFoodGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            NPCQuestWrapper qWrap = QuestsHolder.transform.GetChild(i).GetComponent<NPCQuestWrapper>();
            
            if (qWrap)
            {
                if (!qWrap.QuestObject)
                {
                    Destroy(gameObject);
                    return;
                }


                Quest currQ = qWrap.QuestObject.GetComponent<Quest>();

                if (currQ)
                {
                    if (currQ.QuestType == Quest.Type.GatherFood)
                    {
                        currQ.AddDoneAmt(amt);
                        return;
                    }
                }
            }
        }

    }
    new private void HandleGoldGot(int amt)
    {
        for (int i = 0; i < QuestsHolder.transform.childCount; ++i)
        {
            NPCQuestWrapper qWrap = QuestsHolder.transform.GetChild(i).GetComponent<NPCQuestWrapper>();

            if (!qWrap.QuestObject)
            {
                Destroy(qWrap.gameObject);
                continue;
            }

            Quest currQ = qWrap.QuestObject.GetComponent<Quest>();

            if (currQ)
            {
                if (currQ.QuestType == Quest.Type.GatherGold)
                {
                    currQ.AddDoneAmt(amt);
                    return;
                }
            }
        }
    }
    #endregion Resource handling
}
