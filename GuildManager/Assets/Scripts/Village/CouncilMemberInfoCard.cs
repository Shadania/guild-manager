using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class CouncilMemberInfoCardEvent : UnityEvent<CouncilMemberInfoCard> { };

// For the five council members in townhalls
public class CouncilMemberInfoCard : MonoBehaviour
{
    public Button RemoveButton;
    public Text NameText;
    public CouncilMemberInfoCardEvent onRemove = new CouncilMemberInfoCardEvent();

    public GameObject CouncilMember;

    private void Start()
    {
        RemoveButton.onClick.AddListener(FireEvent);
    }

    private void FireEvent()
    {
        onRemove.Invoke(this);
    }

    public void SetCouncilMemberAndName(GameObject newMember)
    {
        CouncilMember = newMember;
        GuildMemberController gmContr = CouncilMember.GetComponent<GuildMemberController>();
        VillagerBehaviour vBehav;
        if (gmContr)
        {
            NameText.text = gmContr.NameMesh.text + " (Member of" + gmContr.NPCsGuild.GuildName + ')';
        }
        else if (vBehav = CouncilMember.GetComponent<VillagerBehaviour>())
        {
            NameText.text = vBehav.VillagerName + " (Villager)";
        }
        else if (CouncilMember.GetComponent<PlayerController>())
        {
            NameText.text = "You";
        }
    }
}
