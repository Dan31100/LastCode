using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieShiledTransform : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private Vector3 offsetPos;
    [SerializeField] private Vector3 offsetRot;
    [SerializeField] bool useFixedUpdate = false;


    void Update()
    {
        if (!useFixedUpdate)
        {
            gameObject.transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles + offsetRot);
            gameObject.transform.position = target.transform.position + (transform.rotation * offsetPos);
        }
    }
    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            gameObject.transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles + offsetRot);
            gameObject.transform.position = target.transform.position + (transform.rotation * offsetPos);
        }
    }
}
