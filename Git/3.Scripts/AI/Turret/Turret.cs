using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Turret : NetworkBehaviour, IOwnable
{
    private Transform target;
    [SerializeField]
    private TurretLogic turretTargetDetector;
    [SerializeField]
    private TurretSettings turretSettings;
    [SerializeField]
    private TurretAliveTimer turretAliveTimer;

    private NetworkPlayer ownerPlayer;

    private void Start()
    {
        if (isServer)
        {
        }
    }

    public void SetupTurret(NetworkPlayer _owner)
    {
        ownerPlayer = _owner;
        turretTargetDetector.InitTurret(ownerPlayer);
        turretTargetDetector.StartTargetDetecting();
        turretAliveTimer.StartLifetimeTimer(turretSettings.TurretLifetime);
    }

    public void SetOwner(NetworkPlayer _owner)
    {
        ownerPlayer = _owner;
        SetupTurret(ownerPlayer);
    }

    public void DestroyTurret()
    {
        NetworkServer.Destroy(gameObject);
    }
}
