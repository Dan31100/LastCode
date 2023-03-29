using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPriorityTargetLink : MonoBehaviour
{
    [SerializeField]
    private GameObject target;

    public Transform Target => target.transform;
}
