using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class ZombieAttackManager : NetworkBehaviour
{
    [SerializeField]
    private List<ZombieBaseWeapon> zombieWeapons = new List<ZombieBaseWeapon>();
    /*[SerializeField]
    private ZombieBaseWeapon defaultWeapon;*/
    private ZombieBaseWeapon actualWeapon;
    private ZombieAnimator zombieAnimator;
    private Zombie zombie;
    private NetworkAnimator networkAnimator;
    private Transform target;
    [SerializeField]
    private Transform weaponTransform;
    private string attackTriggerName;
    private bool isAlive = true;
    private ZombieGroundLocomotion zombieGroundLocomotion;

    public Transform Target { get => target; }

    public Transform WeaponTransform { get => weaponTransform; }

    public override void OnStartServer()
    {
        GetAllWeapons();
        StartCoroutine(CheckIfShouldAttack());
        zombieAnimator = GetComponent<ZombieAnimator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        zombieGroundLocomotion = GetComponent<ZombieGroundLocomotion>();
        zombie = GetComponent<Zombie>();
        zombie.ZombieDied.AddListener(SetDead);
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private IEnumerator CheckIfShouldAttack()
    {
        while (true)
        {
            if (target && !target.gameObject.GetComponentInParent<NetworkPlayer>().IsDead && isAlive && !zombieGroundLocomotion.IsTraversingStarted)
            {
                if (Vector3.Distance(weaponTransform.position, target.position) < actualWeapon.AttackDistance 
                    && Vector3.Distance(weaponTransform.position, target.position) > actualWeapon.AttackMinDistance)
                {
                    actualWeapon.Attack();
                    if (networkAnimator)
                    {
                        networkAnimator.SetTrigger(attackTriggerName);
                    }
                    else
                    {
                        zombieAnimator.SetTrigger(attackTriggerName);
                    }
                    
                    yield return new WaitForSeconds(actualWeapon.AttackDelay);
                }

                else if(Vector3.Distance(weaponTransform.position, target.position) < actualWeapon.AttackMinDistance)
                {
                    ChooseActualWeapon();
                }
                
            }
            yield return null;
        }
        yield break;
    }

    private void GetAllWeapons()
    {
        zombieWeapons = GetComponents<ZombieBaseWeapon>().ToList();
        ChooseActualWeapon();
    }

    private void ChooseActualWeapon()
    {
        foreach (var weapon in zombieWeapons)
        {
            if (target && isAlive)
            {
                if(Vector3.Distance(target.position, weaponTransform.position) > weapon.AttackMinDistance)
                {
                    if (!actualWeapon)
                    {
                        actualWeapon = weapon;
                    }

                    else if(weapon.AttackDistance > actualWeapon.AttackDistance)
                    {
                        actualWeapon = weapon;
                    }
                    else if(weapon.AttackMinDistance < actualWeapon.AttackMinDistance)
                    {
                        actualWeapon = weapon;
                    }
                }
            }

            else
            {
                if (!actualWeapon)
                {
                    actualWeapon = weapon;
                }

                else if (weapon.AttackDistance > actualWeapon.AttackDistance)
                {
                    actualWeapon = weapon;
                }
            }
        }
        //Debug.LogError($"Actual Weapon is: {actualWeapon}");
        attackTriggerName = actualWeapon.AttackAnimationTriggerName;
    }

    private void SetDead()
    {
        isAlive = false;
    }

    public void ModificateWeapons(float damageModificator)
    {
        foreach(var weapon in zombieWeapons)
        {
            weapon.ModificateDamage(damageModificator);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
