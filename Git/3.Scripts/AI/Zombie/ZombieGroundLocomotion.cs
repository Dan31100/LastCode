using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieGroundLocomotion : NetworkBehaviour
{
    private NavMeshAgent navMeshAgent;
    private NetworkPlayer currentTarget;
    [SerializeField]
    private Transform targetTransform;
    private Zombie zombie;
    private ZombieAnimator zombieAnimator;
    private ZombieOffMeshLinkTraverser zombieOffMeshLinkTraverser;

    [SerializeField]
    private float distanceToStopBeforeTarget = 1.5f;
    [SerializeField]
    private float zombieSpeed = 2f;
    [SerializeField]
    private float zombieSpeedAtRotation = 0.4f;
    [SerializeField]
    private float angleToDropSpeed = 50f;
    [SerializeField]
    private float turnRate = 0.1f;

    [Header(" ")]
    [Header("Navigation")]
    [SerializeField]
    private Vector2 priorityMinMax;
    [Header("Target update frequency settings")]
    [SerializeField]
    private float lowFreqUpdateDelay = 10f;
    [SerializeField]
    private float mediumFreqUpdateDistance = 15f;
    [SerializeField]
    private float mediumFreqUpdateDelay = 7f;
    [SerializeField]
    private float highFreqUpdateDistance = 5f;
    [SerializeField]
    private float highFreqUpdateDelay = 1f;

    [SerializeField]
    private bool isDebug;
    private bool isMoving;
    private bool isTraversingAnimationStarted = false;
    private Vector3 spawnPosition;

    private float tempZombieSpeed;


    public bool IsTraversingStarted { get => isTraversingAnimationStarted; }

    private void Start()
    {
        if (isServer || isServerOnly)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updateRotation = false;
            zombieAnimator = GetComponent<ZombieAnimator>();
            zombieOffMeshLinkTraverser = GetComponent<ZombieOffMeshLinkTraverser>();
            StartCoroutine(DestinationUpdate());
            navMeshAgent.stoppingDistance = distanceToStopBeforeTarget;
            navMeshAgent.avoidancePriority = Random.Range(Mathf.RoundToInt(priorityMinMax.x), Mathf.RoundToInt(priorityMinMax.y));
            spawnPosition = transform.position;
        }

        if (isClient)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
        }
    }

    public override void OnStartServer()
    {
        zombie = GetComponent<Zombie>();
        zombie.ZombieDied.AddListener(StopLocomotion);
    }

    private void Update()
    {
        if (isServer || isServerOnly)
        {
            if (!IsAgentTraversing())
            {
                CheckAngleToNextPoint();
                RotateToTarget();
                UpdateAnimatorParams();
            }

            if (isDebug)
            {
                DrawPath();
            }
        }
    }

    private bool IsAgentTraversing()
    {
        if (navMeshAgent.isOnOffMeshLink)
        {
            if (!isTraversingAnimationStarted)
            {
                isTraversingAnimationStarted = true;
                zombieOffMeshLinkTraverser.StartTraverse(navMeshAgent.currentOffMeshLinkData);
            }

            return true;
        }
        else
        {
            if (isTraversingAnimationStarted)
                isTraversingAnimationStarted = false;

            return false;
        }
    }

    public void SetNavMeshTarget(NetworkPlayer _target)
    {
        if (_target)
        {
            currentTarget = _target;
            targetTransform = currentTarget.GetComponent<CharacterHeadTarget>().headTarget.transform;
            SetTargetDestination();
        }
        else if(!isDebug)
        {
            SetTargetDestination(spawnPosition);
        }
    }

    public void SetTargetDestination()
    {
        if (currentTarget)
            navMeshAgent.SetDestination(targetTransform.position);

        else if (isDebug)
            navMeshAgent.SetDestination(targetTransform.position);
    }

    public void SetTargetDestination(Vector3 _position)
    {
        navMeshAgent.SetDestination(_position);
    }

    private void CheckAngleToNextPoint()
    {
        if (Vector3.Angle(transform.forward, (navMeshAgent.steeringTarget - transform.position).normalized) > angleToDropSpeed)
        {
            navMeshAgent.speed = zombieSpeedAtRotation;
        }
        else
        {
            navMeshAgent.speed = zombieSpeed;
        }
    }

    private void RotateToTarget()
    {
        Vector3 direction = (navMeshAgent.steeringTarget - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnRate);
    }

    private IEnumerator DestinationUpdate()
    {
        while (true)
        {
            if (currentTarget && !currentTarget.IsDead)
            {
                SetTargetDestination();
                var distance = Vector3.Distance(transform.position, targetTransform.position);

                if (distance < highFreqUpdateDistance)
                {

                }
                else if (distance < mediumFreqUpdateDistance)
                {
                    yield return new WaitForSeconds(mediumFreqUpdateDelay);
                }

                else
                {
                    yield return new WaitForSeconds(lowFreqUpdateDelay);
                }
            }

            else
            {
                RequestNewTarget();
            }

            yield return null;
        }
    }

    private void UpdateAnimatorParams()
    {
        if (navMeshAgent.velocity.magnitude > 0.2f && !isMoving)
        {
            isMoving = true;
            zombieAnimator.IsMoving = isMoving;
        }

        else if (navMeshAgent.velocity.magnitude <= 0.2f && isMoving)
        {
            isMoving = false;
            zombieAnimator.IsMoving = isMoving;
        }
    }

    private void DrawPath()
    {
        NavMeshPath navMeshPath = navMeshAgent.path;

        for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.red);
        }
    }

    public void RequestNewTarget()
    {
        if (zombie)
        {
            zombie.RequestTargetFromHQ();
        }
    }

    public void ModificateSpeed(float modificator)
    {
        zombieSpeed += zombieSpeed * modificator;
        zombieSpeedAtRotation += zombieSpeedAtRotation * modificator;
    }

    public void PauseLocomotion()
    {
        tempZombieSpeed = zombieSpeed;
        zombieSpeed = 0;
    }

    public void ResumeLocomotion()
    {
        zombieSpeed = tempZombieSpeed;
    }


    private void StopLocomotion()
    {
        zombieSpeed = 0f;
        zombieSpeedAtRotation = 0f;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
