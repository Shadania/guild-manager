using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Unused
public class VillageNameStandingCard : MonoBehaviour
{
    public Text MyText;

    public void SetText(Village vill, int standing)
    {
        MyText.text = vill.VillageName + ": " + standing.ToString();
    }
}
