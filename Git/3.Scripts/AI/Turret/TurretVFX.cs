using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretVFX : MonoBehaviour
{
    [SerializeField]
    private GameObject cardridge;
    [SerializeField]
    private Transform cardridgeSpawnPoint;
    [SerializeField]
    private Transform muzzle;
    [SerializeField]
    private Transform muzzleSecond;
    [SerializeField]
    private GameObject muzzleFlash;
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private AudioClip fireSound;
    [SerializeField]
    private Transform audioSourcePrefab;

    public void Shooting(Vector3 _dir)
    {
        FORGE3D.F3DPoolManager.Pools["GeneratedPool"].Spawn(muzzleFlash.transform, muzzle.transform.position, muzzle.transform.rotation, null);
        FORGE3D.F3DPoolManager.Pools["GeneratedPool"].Spawn(muzzleFlash.transform, muzzleSecond.transform.position, muzzle.transform.rotation, null);
        FORGE3D.F3DPoolManager.Pools["GeneratedPool"].Spawn(projectile.transform, muzzle.transform.position, Quaternion.LookRotation(_dir), null);
        FORGE3D.F3DPoolManager.Pools["GeneratedPool"].SpawnAudio(audioSourcePrefab.transform, fireSound, muzzle.transform.position, null);

        if (cardridge && cardridgeSpawnPoint)
        {
            Instantiate(cardridge, cardridgeSpawnPoint.transform.position, cardridgeSpawnPoint.transform.rotation);
        }
    }
}
