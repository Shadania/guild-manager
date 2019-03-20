using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// In the Guild Desk Reputation&Fame menu
public class InfluenceInfoCard : MonoBehaviour
{
    public Text VillageNameText;
    public Text InfluenceText;

    public void SetVillageName(string name)
    {
        VillageNameText.text = "Village name: " + name;
    }
    public void SetInfluence(int influence)
    {
        InfluenceText.text = "Influence: " + influence.ToString();
    }
}
