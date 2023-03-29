using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZombieShieldWeapon : ZombieBaseWeapon
{
    [SerializeField]
    private int attackDamage;
    [SerializeField]
    private float attackDistance;
    [SerializeField]
    private float attackMinDistance;
    [SerializeField]
    private float attackDelay;
    [SerializeField]
    private string attackTriggerName;
    [SerializeField]
    private float dealDamageDelay;
    [SerializeField]
    private GameObject Shield;

    [SerializeField]
    private LayerMask playerLayerMask;
    [SerializeField]
    private LayerMask attackCheckLayerMask;
    private Transform weaponTransform;

    private float readyToAttackTime;

    public override float AttackDistance { get => attackDistance; }
    public override float AttackMinDistance { get => attackMinDistance; }
    public override float AttackDelay { get => attackDelay; }
    public override string AttackAnimationTriggerName { get => attackTriggerName; }
    public override bool IsWeaponActive { get => Shield.gameObject == null ? false:true; }

    private void Start()
    {
        weaponTransform = GetComponent<ZombieAttackManager>().WeaponTransform;
    }

    public override void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(dealDamageDelay);
        if (Time.time > readyToAttackTime)
        {
            readyToAttackTime = Time.deltaTime + attackDelay;
            Collider[] hitColliders = Physics.OverlapSphere(weaponTransform.position, attackDistance, playerLayerMask);
            foreach (var collider in hitColliders)
            {
                var player = collider.GetComponentInParent<NetworkPlayer>();
                //Debug.LogError(player); 
                if (player && !player.IsDead)
                {
                    Debug.LogError("Attack 6");
                    player.DamagePlayer(player.GetComponent<NetworkIdentity>(), attackDamage, (int)DamageSources.Zombie);
                    yield break;
                }
            }
        }
    }

    public override void ModificateDamage(float modificator)
    {
        attackDamage = Mathf.RoundToInt((float)attackDamage + (float)attackDamage * modificator);
    }
}
