using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZombieShield : NetworkBehaviour, IDamagebleObject
{
    [SerializeField] 
    private GameObject shieldMesh;
    [SerializeField] 
    private int shieldHP = 500;
    [SerializeField]
    [SyncVar(hook = nameof(SetShieldVisibility))]
    private bool destroyed = false;
    private ZombieAnimator zombieAnimator;

    public bool IsHaveActiveShield => destroyed;

    private void Start()
    {
        if (isServer || isServerOnly)
        {
            zombieAnimator = GetComponent<ZombieAnimator>();
            zombieAnimator.IsHaveShield = true;
        }
    }

    public void TakeDamage(int damage, int damageDealerID)
    {
        if (!destroyed)
        {
            shieldHP -= damage;
            Debug.LogError("ShieldHP: " + shieldHP);
            if (shieldHP <= 0)
            {
                BrokeShield();
                Debug.LogError("Destroyed");
            }
        }
    }

    private void BrokeShield()
    {
        destroyed = true;
        shieldMesh.SetActive(!destroyed);
        zombieAnimator.IsHaveShield = !destroyed;
    }

    private void SetShieldVisibility(bool oldValue, bool newValue)
    {
        Debug.LogError("Destroyed");
        shieldMesh.SetActive(!newValue);
        zombieAnimator.IsHaveShield = !newValue;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
