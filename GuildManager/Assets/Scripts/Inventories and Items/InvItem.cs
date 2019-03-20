using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public class InvItemEvent : UnityEvent<InvItem>
{ };

// The swords and armors you can buy from the smithy and give to your guildies, and equip
public class InvItem : EventTrigger
{
    #region Declarations
    public RawImage Icon;
    public InvItemEvent PlayerClicked = new InvItemEvent();

    public enum Type
    {
        ArmorLeather,
        ArmorIron,
        ArmorSteel,
        ArmorMithril,
        SwordWood,
        SwordIron,
        SwordSteel,
        SwordMithril
    }
    public Type ItemType;
    #endregion Declarations

    public override void OnPointerClick(PointerEventData eventData)
    {
        PlayerClicked.Invoke(this);
    }

    public void SetTexture()
    {
        switch (ItemType)
        {
            case Type.ArmorLeather:
                Icon.texture = GameManager.Instance.ArmorLeather;
                break;
            case Type.ArmorIron:
                Icon.texture = GameManager.Instance.ArmorIron;
                break;
            case Type.ArmorSteel:
                Icon.texture = GameManager.Instance.ArmorSteel;
                break;
            case Type.ArmorMithril:
                Icon.texture = GameManager.Instance.ArmorMithril;
                break;
            case Type.SwordWood:
                Icon.texture = GameManager.Instance.SwordWood;
                break;
            case Type.SwordIron:
                Icon.texture = GameManager.Instance.SwordIron;
                break;
            case Type.SwordSteel:
                Icon.texture = GameManager.Instance.SwordSteel;
                break;
            case Type.SwordMithril:
                Icon.texture = GameManager.Instance.SwordMithril;
                break;
        }
    }
}
