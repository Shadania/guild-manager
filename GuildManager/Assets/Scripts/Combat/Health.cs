using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthEvent : UnityEvent<GameObject>
{ };

// Pretty basic script for managing a health bar & health points (sends out events)
public class Health : MonoBehaviour
{
    #region Declarations
    public float MaxHealth = 100.0f;
    public float CurrHealth = 0.0f; // gets set to maxhealth in start

    public float IncomingDamageMultiplier = 1.0f;

    public GameObject HealthBarBackground;

    public HealthEvent onTakeDamage = new HealthEvent();
    public HealthEvent onDeath = new HealthEvent();
    #endregion Declarations

    private void Start()
    {
        CurrHealth = MaxHealth;
    }

    // Gets called by the "source" of the damage
    public void DealDamage(float amt, GameObject source)
    {
        CurrHealth -= Mathf.RoundToInt(amt*IncomingDamageMultiplier);
        bool isDead = false;
        if (CurrHealth < 0.0f)
        {
            CurrHealth = 0.0f;
            isDead = true;
        }

        Vector3 scale = HealthBarBackground.transform.GetChild(0).localScale;

        scale.x = CurrHealth / MaxHealth;

        HealthBarBackground.transform.GetChild(0).localScale = scale;

        onTakeDamage.Invoke(source);

        if (isDead)
        {
            onDeath.Invoke(source);
        }

    }
}
