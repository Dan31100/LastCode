using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSettings : MonoBehaviour
{
    [SerializeField]
    private float turretLifetime;
    [SerializeField]
    private int turretDamage;
    [SerializeField]
    private float turretRPM;
    [SerializeField]
    private float spread = 0;

    public float TurretLifetime{ get { return turretLifetime;} }
    public int Damage => turretDamage;
    public float RPM => turretRPM;
    public float Spread => spread;
}
