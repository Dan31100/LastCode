using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurretAliveTimer : NetworkBehaviour
{
    private Turret turret;
    private float timer;

    private void Start()
    {
        if (isServer)
        {
            turret = GetComponent<Turret>();
        }
    }

    public void StartLifetimeTimer(float lifetime)
    {
        StartCoroutine(LifettimeTimer(lifetime));
    }

    private IEnumerator LifettimeTimer(float lifetime)
    {
        timer = lifetime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        turret.DestroyTurret();
        yield break;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
