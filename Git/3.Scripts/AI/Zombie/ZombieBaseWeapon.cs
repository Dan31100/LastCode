using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ZombieAttackManager))]
public abstract class ZombieBaseWeapon : NetworkBehaviour
{
    public abstract float AttackDelay { get; }
    public abstract float AttackDistance { get; }
    public abstract float AttackMinDistance { get; }
    public abstract string AttackAnimationTriggerName { get; }
    public abstract bool IsWeaponActive { get; }
    

    public abstract void Attack();

    public abstract void ModificateDamage(float modificator);
}
