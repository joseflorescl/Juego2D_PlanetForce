using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class KinematicProjectile : MonoBehaviour
{
    public abstract float Speed { get; }
    public abstract void SetKinematicVelocity(Vector3 direction, float speed);

}
