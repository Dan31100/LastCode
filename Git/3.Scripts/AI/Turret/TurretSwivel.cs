using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurretSwivel : NetworkBehaviour
{
    [SerializeField]
    private Vector2 yawMinMax;
    [SerializeField]
    private Transform swivelTransform;
    [SerializeField]
    private Transform rotateTarget;
    [SerializeField]
    private float patrolModeRotationSpeed = 1f;
    [SerializeField]
    private Transform aimTarget;

    private Vector3 targetRotation;

    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(RotationCoroutine());
        }
    }

    public void RotateSwivel(Vector3 _targetRotation)
    {
        targetRotation = _targetRotation;
    }

    private void RotateSwivelByYAxis(float yAxisDegrees)
    {
        swivelTransform.rotation = Quaternion.Euler(new Vector3(swivelTransform.rotation.eulerAngles.x, swivelTransform.rotation.eulerAngles.y + yAxisDegrees, swivelTransform.rotation.eulerAngles.z));
    }

    private void RotateSwiverByXAxis(float xAxisDegrees)
    {
        if (swivelTransform.rotation.eulerAngles.x + xAxisDegrees > yawMinMax.x && swivelTransform.rotation.eulerAngles.x + xAxisDegrees < yawMinMax.y)
            swivelTransform.rotation = Quaternion.Euler(new Vector3(swivelTransform.rotation.eulerAngles.x + xAxisDegrees, swivelTransform.rotation.eulerAngles.y + 1f, swivelTransform.rotation.eulerAngles.z));
    }

    public void SwivelFollowTarget(Transform _target)
    {
        aimTarget = _target;
    }

    public void StopFollow()
    {
        aimTarget = null;
        swivelTransform.rotation = Quaternion.Euler(new Vector3(0, swivelTransform.rotation.eulerAngles.y, swivelTransform.rotation.eulerAngles.z));
    }

    private void TryToFollowRotation()
    {
        RotateSwivelByYAxis(rotateTarget.rotation.eulerAngles.y - swivelTransform.rotation.eulerAngles.y);
        RotateSwiverByXAxis(rotateTarget.rotation.eulerAngles.x - swivelTransform.rotation.eulerAngles.x);
    }

    private IEnumerator RotationCoroutine()
    {
        while (true)
        {
            if (aimTarget)
            {
                rotateTarget.LookAt(aimTarget);
                TryToFollowRotation();
            }

            else
            {
                RotateSwivelByYAxis(patrolModeRotationSpeed);
            }
            yield return null;
        }

        yield break;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
