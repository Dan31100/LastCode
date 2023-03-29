using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZombieHealth : NetworkBehaviour
{
    [SerializeField]
    private int health = 100;
    [SerializeField]
    private float deathDelay = 5f;
    private Zombie zombie;
    private PuppetMasterController puppetMasterController;
    private bool isAlive = true;

    private void Start()
    {
        zombie = GetComponent<Zombie>();
        puppetMasterController = GetComponent<PuppetMasterController>();
    }
     
    public void TakeDamage(int damage, NetworkPlayer _attacker)
    {
        if (isAlive)
        {
            health -= damage;
            Debug.LogError($"Zombie Take Damage. HP: {health}");
            if (_attacker)
                PlayersManager.Instance.AddToAssistList((int)netId, (int)_attacker.netId);
            if (health <= 0)
            {
                Die();
                if (_attacker)
                {
                    //_attacker.PlayerInfo.Kills++;
                    //_attacker.PlayerInfo.Points++;
                    //_attacker.PlayerInfo.Money += 50;

                    PlayersManager.Instance.InvokeKilledEvent((int)_attacker.netId, (int)DamageSources.Zombie);
                }
            }
        }
    }

    public void ModificateHealth(float modificator)
    {
        health = Mathf.RoundToInt((float)health + health * modificator);
    }

    public void Die()
    {
        isAlive = false;

        zombie.TriggerDieEvent();
        if (puppetMasterController)
        {
            //zombie.ActivateRagdoll();
            zombie.SetAnimatorState(false);
        }
        else
        {
            RpcEnableRagdoll();
            EnableRagdoll();
            StartCoroutine(DeathDelay());
        }
    }

    [ClientRpc]
    void RpcEnableRagdoll()
    {
        EnableRagdoll();
    }

    void EnableRagdoll()
    {
        zombie.SetAnimatorState(false);
        Rigidbody[] rigidbories = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidbody in rigidbories)
        {
            rigidbody.isKinematic = false;
        }
    }

    public void Spawn()
    {
        NetworkServer.Spawn(this.gameObject);
    }

    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        NetworkServer.Destroy(this.gameObject);
        yield break;
    }
}
