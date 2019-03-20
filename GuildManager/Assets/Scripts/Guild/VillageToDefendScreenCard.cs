using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class VillageToDefendScreenCardEvent : UnityEvent<Village> { };

// In the GuildMember menu, when you tell them to defend a village, these pop up with which villages you can go defend
public class VillageToDefendScreenCard : MonoBehaviour
{
    public Village MyVill;
    public Text CardTitle;
    public Text CardInfluence;
    public Button Defend;
    public VillageToDefendScreenCardEvent WasChosen = new VillageToDefendScreenCardEvent();

    private void Start()
    {
        Defend.onClick.AddListener(DefendButtonClicked);
    }

    private void DefendButtonClicked()
    {
        WasChosen.Invoke(MyVill);
    }
}
