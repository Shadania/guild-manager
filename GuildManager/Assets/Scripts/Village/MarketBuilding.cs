using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The little sub-buildings of a market. Had to split them up for proper collision
public class MarketBuilding : MonoBehaviour
{
    private void Start()
    {
        Interactable interac = GetComponent<Interactable>();
        if (interac)
        {
            interac.OnPlayerInteract.AddListener(transform.parent.parent.GetComponent<Market>().HandlePlayerInteract);
        }
    }
}
