using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Wrapper that the player's quests are put in when giving NPCs quests
public class PlayerQuestWrapper : MonoBehaviour
{
    public GameObject PlayerQuest = null;
    public Button GiveQuestButton;
    public QuestHolderNPC TargetNPC;

    private void Start()
    {
        GiveQuestButton.onClick.AddListener(GiveQuestButtonClicked);
    }

    public void SetPlayerQuest(GameObject newQuest)
    {
        PlayerQuest = newQuest;
        PlayerQuest.GetComponent<Quest>().QuestAbandoned.AddListener(DeleteWrapper);
        PlayerQuest.GetComponent<Quest>().MoneyOnQuestComplete.AddListener(DeleteWrapper);
        PlayerQuest.transform.position = new Vector3(0, 0, 0);
    }

    void GiveQuestButtonClicked()
    {
        TargetNPC.AddQuest(PlayerQuest);
        Destroy(gameObject);
    }

    void DeleteWrapper()
    {
        Destroy(gameObject);
    }
    void DeleteWrapper(int i)
    {
        DeleteWrapper();
    }

    public void GiveMyQuestTo(QuestHolder target)
    {
        target.AddQuest(PlayerQuest);
        Destroy(gameObject);
    }
}
