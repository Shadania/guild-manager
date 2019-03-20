using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Simple script to keep track of everything you can press F on to interact with
// Note: The object needs to be in the layer Interactable + have this script to work.
// The object using this script usually adds a listener to the OnPlayerInteract event, as that is what the entire script was built around.
public class Interactable : MonoBehaviour
{
    public UnityEvent OnPlayerInteract = new UnityEvent();

    public void PlayerInteracted()
    {
        // do not do anything if theres any kind of menu up on the player's side
        PlayerController pContr = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>();
        if (pContr.AnyMenuUp)
            return;
        OnPlayerInteract.Invoke();
    }

    protected void Start()
    {
        InteractableManager.Instance.RegisterObject(gameObject);
    }

    private void OnDestroy()
    {
        InteractableManager.Instance.UnRegisterObject(gameObject);
    }
}
