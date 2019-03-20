using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A type of Interactable that can give you resources and destroy itself when said resources are depleted.
// Types in use: Trees, Berry Bushes, Rocks
public class Harvestable : Interactable
{
    public int AmtResources = 10;
    public Inventory.ResourceType HarvestableType = Inventory.ResourceType.Wood;
    public int AmtHarvestAtOnce = 1;

    // private bool _isPlayerInTrigger;
    public float NPCRange = 5.0f;
    public float PlayerRange = 5.0f;
    private AudioSource _soundEffect = null;



    new private void Start()
    {
        OnPlayerInteract.AddListener(HandlePlayerInteract);
        InteractableManager.Instance.RegisterHarvestable(gameObject);
        _soundEffect = GetComponent<AudioSource>();
        
        // This gives a warning that it's deprecated but I honestly can not find another way to make this work
        transform.RotateAround(new Vector3(0, 1, 0), Random.Range(0.0f, 360));
    }

    void HandlePlayerInteract()
    {
        if ((GameManager.Instance.PlayerAvatar.transform.position - transform.position).sqrMagnitude <= PlayerRange)
        {
            

            Inventory inv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();

            bool isEmpty;
            int amtCanGive = CheckCanGive(AmtHarvestAtOnce, out isEmpty);
            inv.AddResource(HarvestableType, amtCanGive);
            AmtResources -= amtCanGive;

            if (_soundEffect)
                _soundEffect.Play();

            if (isEmpty)
                Destroy(gameObject);
        }
    }
    public void HandleNPCInteract(GuildMemberController NPC)
    {
        Transform npcTransform = NPC.transform;

        if ((npcTransform.position - transform.position).sqrMagnitude <= NPCRange)
        {
            InventoryNPC npcInv = NPC.GetComponent<InventoryNPC>();

            bool isEmpty;
            int amtCanGive = CheckCanGive(AmtHarvestAtOnce, out isEmpty);
            npcInv.AddResource(HarvestableType, amtCanGive);
            AmtResources -= amtCanGive;

            if (isEmpty)
                Destroy(gameObject);
            else if (_soundEffect)
            {
                _soundEffect.Stop();
                _soundEffect.Play();
            }
        }
    }
    public bool IsNPCInRange(Transform target)
    {
        return (transform.position - target.position).sqrMagnitude < NPCRange;
    }

    private int CheckCanGive(int amt, out bool isDepleted)
    {
        if (amt >= AmtResources)
        {
            isDepleted = true;
            return AmtResources;
        }

        isDepleted = false;
        return amt;
    }
}
