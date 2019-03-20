using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class AddGuildCouncilMemberCardEvent : UnityEvent<AddGuildCouncilMemberCard> { };

// Script for in the Townhall menu "Manage Council"
public class AddGuildCouncilMemberCard : MonoBehaviour
{
    public Text NameText;
    public Button AddButton;
    public GameObject GuildMember;

    public AddGuildCouncilMemberCardEvent onAdd = new AddGuildCouncilMemberCardEvent();

    private void Start()
    {
        AddButton.onClick.AddListener(FireEvent);
    }

    private void FireEvent()
    {
        onAdd.Invoke(this);
    }
}
