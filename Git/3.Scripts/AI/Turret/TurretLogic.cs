using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurretLogic : NetworkBehaviour
{
    [SerializeField]
    private Transform raycastStartPoint;
    [SerializeField]
    private TurretSwivel turretSwivel;
    [SerializeField]
    private float raycastDistance;
    [SerializeField]
    private LayerMask raycastLayers;
    [SerializeField]
    private float timeToLoseTarget;

    [SerializeField]
    private List<Vector3> raycastDeviation = new List<Vector3>();

    [SerializeField]
    private Transform target;
    private TurretVFX turretVFX;
    private bool isTargetVisible = false;
    private NetworkPlayer ownerPlayer;

    public bool IsTargetVisible => isTargetVisible;

    private int turretDamage;
    private float turretRPM;
    private float spread = 0;
    private float fireDelay;
    private float minute = 60f;
    [SerializeField]
    private bool debugMode;

    private void Start()
    {
        turretVFX = GetComponent<TurretVFX>();
    }

    public void InitTurret(NetworkPlayer owner)
    {
        ownerPlayer = owner;
        var turretSettings = GetComponent<TurretSettings>();
        turretDamage = turretSettings.Damage;
        turretRPM = turretSettings.RPM;
        spread = turretSettings.Spread;
        fireDelay = minute / turretRPM;
    }

    public void StartTargetDetecting()
    {
        StartCoroutine(TargetDetecting());
    }

    private void Update()
    {
        if (debugMode)
        {
            foreach (var deviation in raycastDeviation)
            {
                Vector3 fireVector = raycastStartPoint.transform.TransformDirection(deviation);
                Debug.DrawRay(raycastStartPoint.position, (raycastStartPoint.transform.forward + fireVector).normalized * raycastDistance, Color.red, 0.02f);
            }
        }
    }

    private IEnumerator TargetDetecting()
    {
        Debug.LogError("Start");
        target = null;
        RaycastHit hit;
        while (target == null)
        {
            foreach (var deviation in raycastDeviation)
            {
                Vector3 fireVector = raycastStartPoint.transform.TransformDirection(deviation);
                Physics.Raycast(raycastStartPoint.position, (raycastStartPoint.transform.forward + fireVector).normalized * raycastDistance, out hit, raycastDistance, raycastLayers);
                //Debug.DrawRay(raycastStartPoint.position, (raycastStartPoint.transform.forward + fireVector).normalized * raycastDistance, Color.red, 0.02f);
                if (hit.collider && hit.collider.GetComponentInParent<Zombie>() != null)
                {
                    try
                    {
                        target = hit.collider.GetComponentInParent<TurretPriorityTargetLink>().Target;
                    }
                    catch { }

                    if(target != null)
                    {
                        turretSwivel.SwivelFollowTarget(target);
                        target.GetComponent<Zombie>().ZombieDied.AddListener(ForgetTarget);
                        StartCoroutine(CheckIsTargetVisible());
                        yield break;
                    }

                    else
                    {
                        target = hit.collider.transform;
                        target.GetComponent<Zombie>().ZombieDied.AddListener(ForgetTarget);
                        turretSwivel.SwivelFollowTarget(target);
                        StartCoroutine(CheckIsTargetVisible());
                        yield break;
                    }
                }
            }

            yield return null;
        }

        yield break;
    }

    private IEnumerator CheckIsTargetVisible()
    {
        RaycastHit hit;
        var loseTargetTimer = 0f;
        var shootTimer = 0f;
        var targetZombie = target.GetComponentInParent<Zombie>();
        Zombie hitZombie = null;
        while (target != null)
        {
            Physics.Raycast(raycastStartPoint.position, raycastStartPoint.transform.forward.normalized * raycastDistance, out hit, raycastDistance, raycastLayers);
            Debug.DrawRay(raycastStartPoint.position, raycastStartPoint.transform.forward.normalized * raycastDistance, Color.green, 0.02f);
            try
            {
                hitZombie = hit.collider.GetComponentInParent<Zombie>();
            }
            catch
            {
                
            }

            Debug.LogError(hitZombie + "  ==?  " + targetZombie);

            if (targetZombie && hitZombie != null && targetZombie == hitZombie)
            {
                isTargetVisible = true;
                loseTargetTimer = 0f;
                if (isTargetVisible && shootTimer >= fireDelay)
                {
                    shootTimer = 0f;
                    Shoot();
                }
            }

            else
            {
                isTargetVisible = false;
                loseTargetTimer += Time.deltaTime;
                if (loseTargetTimer >= timeToLoseTarget)
                {
                    ForgetTarget();
                    yield break;
                }
            }

            Debug.LogError("Target visible :[" + isTargetVisible + "]");
            shootTimer += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    private void ForgetTarget()
    {
        target.GetComponent<Zombie>().ZombieDied.RemoveAllListeners();
        target = null;
        turretSwivel.StopFollow();
        StartCoroutine(TargetDetecting());
    }

    private void Shoot()
    {
        Debug.LogError("Pew");
        Vector3 fireVector = Quaternion.Euler(
                    0 + Random.Range(-spread, spread),
                    0 + Random.Range(-spread, spread),
                    0 + Random.Range(-spread, spread)) * raycastStartPoint.forward;

        Shoot(fireVector);
        

        RaycastHit hit;
        if (Physics.Raycast(raycastStartPoint.position, fireVector, out hit, raycastDistance, raycastLayers))
        {
            Debug.LogError($"Turret hit: {hit.transform.gameObject}");
            var hitbox = hit.transform.gameObject.GetComponent<Hitbox>();
            var damagebleObject = hit.transform.GetComponentInParent<IDamagebleObject>();
            if (hitbox)
            {
                var zombie = hitbox.GetComponentInParent<Zombie>();
                if (zombie)
                {
                    ownerPlayer.DamageZombie(zombie.GetComponent<NetworkIdentity>(), turretDamage, ownerPlayer, hitbox.isCritical);
                }
                Debug.LogError($"Turret hit zombie: {zombie}");
            }

            else if (damagebleObject != null)
            {
                Debug.LogError($"Hit Object");
                ownerPlayer.DamageObject(damagebleObject.GetGameObject().GetComponent<NetworkIdentity>(), turretDamage);
            }
        }
    }

    private void Shoot(Vector3 _fireVector)
    {
        turretVFX.Shooting(_fireVector);
        RpcShootVFX(_fireVector);
    }

    [ClientRpc]
    public void RpcShootVFX(Vector3 _fireVector)
    {
        turretVFX.Shooting(_fireVector);
    }
}
