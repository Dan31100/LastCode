using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Mirror;

public class ZombieOffMeshLinkTraverser : NetworkBehaviour
{
    private NavMeshAgent navMeshAgent;
    private AnimationCurve defaultTraverseCurve;
    private ZombieAnimator zombieAnimator;
    private NetworkAnimator networkZombieAnimator;
    private ZombieGroundLocomotion zombieLocomotion;
    private Zombie zombie;
    [SerializeField]
    private float traverseSpeed = 0.005f;
    [SerializeField]
    private bool debug;

    public override void OnStartServer()
    {
        zombieAnimator = GetComponent<ZombieAnimator>();
        networkZombieAnimator = GetComponent<NetworkAnimator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieLocomotion = GetComponent<ZombieGroundLocomotion>();
        zombie = GetComponent<Zombie>();
        zombie.ZombieDied.AddListener(StopAllCoroutines);
    }

    public void StartTraverse(OffMeshLinkData _offMeshLinkData)
    {
        var offMeshLink = _offMeshLinkData.offMeshLink;
        if (_offMeshLinkData.offMeshLink)
        {
            StartCoroutine(TraverseCoroutine(offMeshLink.GetComponent<OffMeshLinkExtension>(), _offMeshLinkData));
        }
        else
        {
            //StartCoroutine(TraverseCoroutine(defaultTraverseCurve, _offMeshLinkData));
        }
    }

    private IEnumerator TraverseCoroutine(OffMeshLinkExtension offMeshLinkExtension, OffMeshLinkData offMeshLinkData)
    {
        //OffMeshLinkExtension offMeshLinkExtension = offMeshLinkData.offMeshLink.gameObject.GetComponent<OffMeshLinkExtension>();
        AnimationCurve curveStartToMid = offMeshLinkExtension.AnimationCurveStartToMid;
        AnimationCurve curveMidToEnd = offMeshLinkExtension.AnimationCurveMidToEnd;
        Vector3 startPos = offMeshLinkExtension.AnimationStartPos.position;
        Vector3 midPos = offMeshLinkExtension.AnimationMidPos.position;
        Vector3 endPos = offMeshLinkExtension.AnimationEndPos.position;

        var tempPos = transform.position;
        float t = 0.0f;
        while (t < 0.5f)
        {
            //Debug.LogError("Preparing");
            transform.position = Vector3.Lerp(tempPos, offMeshLinkExtension.AnimationStartPos.position, t);
            t += traverseSpeed;
            yield return null;
        }

        networkZombieAnimator.SetTrigger(offMeshLinkExtension.AreaName);
        //zombieAnimator.SetTrigger(offMeshLinkExtension.AreaName);
        navMeshAgent.updateRotation = false;
        transform.LookAt(new Vector3(offMeshLinkExtension.AnimationEndPos.position.x, transform.position.y, offMeshLinkExtension.AnimationEndPos.position.z));

        float durationStart = curveStartToMid.keys[curveStartToMid.keys.Length - 1].time;
        float durationMid = curveMidToEnd.keys[curveMidToEnd.keys.Length - 1].time;
        float duration = durationMid + durationStart;
        float time = 0.0f;
        t = 0f;
        while (time < durationStart)
        {
            
            LerpTransform(startPos, midPos, t);
            t += curveStartToMid.Evaluate(time);
            time += Time.deltaTime;
            yield return null;
        }

        if (debug)
            Debug.LogError("Start to mid" + " t: " + t);

        t = 0f;
        time = 0f;
        while (time < durationMid)
        {
            
            LerpTransform(midPos, endPos, t);
            t += curveMidToEnd.Evaluate(time);
            //transform.position = Vector3.Lerp(startPos, endPos, t);
            //navMeshAgent.destination = transform.position;
            time += Time.deltaTime;
            yield return null;
        }
        if (debug)
            Debug.LogError("Mid to end" + " t: " + t);

        transform.position = endPos;
        navMeshAgent.destination = transform.position;
        navMeshAgent.updateRotation = true;
        navMeshAgent.CompleteOffMeshLink();
        zombieLocomotion.SetTargetDestination();
        yield break;
    }

    private void LerpTransform(Vector3 startPos, Vector3 endPos, float betweenValue)
    {
        transform.position = Vector3.Lerp(startPos, endPos, betweenValue);
    }
}
