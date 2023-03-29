using UnityEngine;
using UnityEngine.Events;
using Mirror;
using UnityEngine.AI;

[RequireComponent(typeof(ZombieAnimator))]
[RequireComponent(typeof(ZombieHealth))]
[RequireComponent(typeof(ZombieGroundLocomotion))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ZombiePlayerDetector))]
[RequireComponent(typeof(ZombieOffMeshLinkTraverser))]
[RequireComponent(typeof(ZombieAudio))]

public class Zombie : NetworkBehaviour
{
    private PuppetMasterController puppetMasterC;
    private ZombieHealth zombieHealth;
    private ZombieGroundLocomotion zombieLocomotion;
    private ZombieHQ zombieHQ;
    private ZombieAttackManager zombieAttackManager;
    private ZombieOffMeshLinkTraverser zombieOffMeshLinkTraverser;
    private ZombieAnimator zombieAnimator;
    private ZombieAudio zombieAudio;
    private UnityEvent zombieDiedEvent = new UnityEvent();

    public ZombieHQ Manager
    {
        set { zombieHQ = value; RequestTargetFromHQ(); }
    }

    public UnityEvent ZombieDied
    {
        get => zombieDiedEvent;
    }

    public ZombieAudio ZombieAudioManager { get => zombieAudio; }

    private void Awake()
    {
        zombieAudio = GetComponent<ZombieAudio>();
    }

    private void Start()
    {
        if (isServer || isServerOnly)
        {
            FindDependencies();
        }
        zombieAnimator = GetComponent<ZombieAnimator>();
    }

   // [ClientRpc]   
    public void SetZombieSound(int _soundId)
    {
        if(zombieAudio == null)
            zombieAudio = GetComponent<ZombieAudio>();
        //Debug.LogError($"SetZombieSound  2:  isServer: {isServer}, isClient: {isClient}");
        zombieAudio.ApplyZombieSound(_soundId);
    }

   
    public void ConfigureZombie(float healthModificator, float speedModificator, float damageModificator)
    {
        FindDependencies();
        zombieHealth.ModificateHealth(healthModificator);
        zombieLocomotion.ModificateSpeed(speedModificator);
        zombieAttackManager.ModificateWeapons(damageModificator);
    }

    private void FindDependencies()
    {
        zombieHealth = GetComponent<ZombieHealth>();
        zombieLocomotion = GetComponent<ZombieGroundLocomotion>();
        zombieAttackManager = GetComponent<ZombieAttackManager>();
        zombieOffMeshLinkTraverser = GetComponent<ZombieOffMeshLinkTraverser>();
        puppetMasterC = GetComponent<PuppetMasterController>();
    }

    public void RequestTargetFromHQ()
    {
        if (zombieHQ && zombieLocomotion)
        {
            var _target = zombieHQ.FindTarget(this);
            zombieLocomotion.SetNavMeshTarget(_target);

            if (_target && !_target.IsDead)
            {
                zombieAttackManager.SetTarget(_target.GetComponent<CharacterHeadTarget>().headTarget);
            }
        }
    }

    public void TriggerDieEvent()
    {
        zombieDiedEvent.Invoke();
        if (zombieAudio)
            zombieAudio.OnZombieDie();
    }

    public void ActivateRagdoll()
    {
        puppetMasterC.ActivateRagdoll();
    }

    public void SetAnimatorState(bool _state)
    {
        zombieAnimator.SetAnimatorState(_state);
    }

    private void OnDestroy()
    {
        if(zombieHQ)
            zombieHQ.DeleteActorFromList(this);
    }
}
