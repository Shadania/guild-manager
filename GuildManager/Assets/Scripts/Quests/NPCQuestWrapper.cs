using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// The wrapper used in the NPC's Quest List.
public class NPCQuestWrapper : MonoBehaviour
{
    public Button TakeQuestButton;
    public GameObject QuestObject;
    public UnityEvent QuestTakenAway;

    private void Start()
    {
        TakeQuestButton.onClick.AddListener(PlayerTakeQuest);
    }

    public void SetQuest(GameObject newQuest)
    {
        QuestObject = newQuest;
        QuestObject.GetComponent<Quest>().QuestAbandoned.AddListener(QuestAbandonedClicked);
        QuestObject.GetComponent<Quest>().MoneyOnQuestComplete.AddListener(QuestCompleted);
        QuestObject.GetComponent<Quest>().InfluenceOnQuestComplete.AddListener(QuestCompletedInfluence);
    }

    private void PlayerTakeQuest()
    {
        PlayerController pContr = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>();
        if (pContr)
        {
            QuestHolder pQuestHolder = GameManager.Instance.PlayerAvatar.GetComponent<QuestHolder>();
            pQuestHolder.AddQuest(QuestObject);
            QuestTakenAway.Invoke();
            Destroy(gameObject);
        }
    }


    void QuestAbandonedClicked()
    {
        Destroy(gameObject);
    }
    void QuestCompleted(int amtGold)
    {
        Destroy(gameObject);
    }
    void QuestCompletedInfluence(Village vill, int amtInfluence)
    {
        GuildMemberController gmContr = GetComponent<GuildMemberController>();

        if (gmContr)
        {
            gmContr.NPCsGuild.AddInfluenceFor(vill.gameObject, amtInfluence);
        }
    }
}
