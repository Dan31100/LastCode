using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class ZombieHQ : NetworkBehaviour
{
    [SerializeField]
    private List<GameObject> zombiesPrefabs = new List<GameObject>();
    [SerializeField]
    private List<ActorRespawn> respawnPoints = new List<ActorRespawn>();
    private List<Zombie> zombiesArray = new List<Zombie>();
    private List<NetworkPlayer> players = new List<NetworkPlayer>();
    [SerializeField]
    private int zombiesCurrentAmount = 3;
    [SerializeField]
    private int zombiesMaxAmount = 40;
    [SerializeField]
    private AnimationCurve zombieAmountPerTime;
    [SerializeField]
    private float zombieSpeedPerPlayer, zombieHealthPerPlayer, ZombieDamagePerPlayer;
    [SerializeField]
    private float spawnLoopDelay = 2f;
    [SerializeField]
    private GameObject respawnVisualPrefab;

    private bool respawnVisibility = false;

    private int zombieTrackNumber = 0;

    private void Start()
    {
        if (isServer || isServerOnly)
        {

        }
    }

    private IEnumerator ActorSpawnLoop()
    {
        while (true)
        {
            if (zombiesArray.Count < zombiesCurrentAmount)
            {
                var spawn = FindSpawn();
                if (spawn)
                    SpawnActor(spawn);
            }

            yield return new WaitForSeconds(spawnLoopDelay);
        } 
    }

    private void SetZombieSound(Zombie _zombieComponent)
    {
        _zombieComponent.SetZombieSound(zombieTrackNumber); 
        zombieTrackNumber = zombieTrackNumber >= _zombieComponent.ZombieAudioManager.zombieSoundsCount - 1 ? 0 : zombieTrackNumber + 1;
    }

    private void SpawnActor(ActorRespawn actorRespawn)
    {
        actorRespawn.DeactivateRespawn();
        var zombie = Instantiate(zombiesPrefabs[Random.Range(0, zombiesPrefabs.Count)], actorRespawn.transform.position, Quaternion.identity);
        var zombieComponent = zombie.GetComponent<Zombie>();
        zombieComponent.Manager = this;
        zombiesArray.Add(zombieComponent);
        zombieComponent.ConfigureZombie(zombieHealthPerPlayer * players.Count, zombieSpeedPerPlayer * players.Count, ZombieDamagePerPlayer * players.Count);
        NetworkServer.Spawn(zombie);
        SetZombieSound(zombieComponent);
    }

    public void DeleteActorFromList(Zombie gameobjectToDelete)
    {
        zombiesArray.Remove(gameobjectToDelete);
    }

    public NetworkPlayer FindTarget(Zombie zombieThatRequesting)
    {
        UpdatePlayersList();
        NetworkPlayer target = null;
        float distance = 100f;
        float tempDistance;

        foreach (var player in players)
        {
            if (!player.IsDead)
            {
                var headTransform = player.GetComponent<CharacterHeadTarget>().headTarget.transform;
                tempDistance = Vector3.Distance(zombieThatRequesting.transform.position, headTransform.position);
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    target = player;
                }
            }
        }

        return target;
    }



    private ActorRespawn FindSpawn()
    {
        System.Random rnd = new System.Random();
        var spawnArray = respawnPoints.OrderBy(x => rnd.Next()).ToArray();

        foreach (var spawn in spawnArray)
        {
            if (spawn.IsRespawnActive)
            {
                return spawn;
            }
        }

        return null;
    }

    private void UpdatePlayersList()
    {
        players = PlayersManager.Instance.GetAllPlayers();
    }

    public void SetSpawnLoopDelay(float _spawnLoopDelay)
    {
        spawnLoopDelay = _spawnLoopDelay;
    }

    public void SetMaxZombieAmount(int _zombiesMaximumAmount)
    {
        zombiesCurrentAmount = _zombiesMaximumAmount;
    }

    public void StartSpawnZombies()
    {
        if (isServer || isServerOnly)
        {
            StartCoroutine(ActorSpawnLoop());
            UpdatePlayersList();
            PlayersManager.Instance.PlayerListsWasChanged.AddListener(UpdatePlayersList);
        }
    }

    public void SetSpawnsDelay(float _spawnDelay)
    {
        foreach(var spawn in respawnPoints)
        {
            spawn.SetRespawnDelay(_spawnDelay);
        }
    }

    public void GetZombieAmountByTime(float time, float maxTime)
    {
        zombiesCurrentAmount = Mathf.Clamp(Mathf.RoundToInt(zombiesMaxAmount * zombieAmountPerTime.Evaluate(time / maxTime)), 1, zombiesMaxAmount);
    }

    public void ChangeRespawnsVisibility()
    {
        if(respawnPoints.Count != 0)
        {
            if (!respawnVisibility)
            {
                respawnVisibility = true;
                foreach(var respawn in respawnPoints)
                {
                    if (respawn.GetComponentInChildren<ZombieRespawnVisual>() == null)
                        Instantiate(respawnVisualPrefab, respawn.transform);
                }
            }
            else
            {
                respawnVisibility = false;
                foreach (var respawn in respawnPoints)
                {
                    if (respawn.GetComponentInChildren<ZombieRespawnVisual>() != null)
                        Destroy(respawn.GetComponentInChildren<ZombieRespawnVisual>());
                }
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = zombiesArray.Count - 1; i >= 0; i--)
        {
            NetworkServer.Destroy(zombiesArray[i].gameObject);
        }

        zombiesArray.Clear();
    }

    private void OnDisable()
    {
        if (!isClient)
        {
            StopAllCoroutines();
            PlayersManager.Instance.PlayerListsWasChanged.RemoveListener(UpdatePlayersList);
        }
    }
}