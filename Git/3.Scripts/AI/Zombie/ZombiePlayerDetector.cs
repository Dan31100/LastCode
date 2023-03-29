using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZombiePlayerDetector : NetworkBehaviour
{
    [SerializeField]
    private float checkDelay = 1f;
    [SerializeField]
    private float checkRadius = 5f;
    [SerializeField]
    private float checkStartDelay = 40f;
    [SerializeField]
    private float delayAfterFoundNewTarget = 10f;
    [SerializeField]
    private LayerMask playersLayerMask;
    private Zombie zombie;
    private float delayedTime;

    public override void OnStartServer()
    {
        zombie = GetComponent<Zombie>();
        StartCoroutine(CheckPlayersLoop());
    }

    private IEnumerator CheckPlayersLoop()
    {
        yield return new WaitForSeconds(checkStartDelay);
        while (true)
        {
            yield return new WaitForSeconds(checkDelay);
            CheckForPlayers();
            yield return null;
        }
    }

    private void CheckForPlayers()
    {
        if (Time.time > delayedTime) 
        {
            var colliders = Physics.OverlapSphere(transform.position, checkRadius, playersLayerMask);
            if (colliders.Length > 0)
            {
                delayedTime = Time.time + delayAfterFoundNewTarget;
                Debug.LogError("Targer Request");
                zombie.RequestTargetFromHQ();
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
