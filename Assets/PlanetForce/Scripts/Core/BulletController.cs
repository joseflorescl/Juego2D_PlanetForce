using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : KinematicProjectile
{
    // La bala se destruye en el OnBecameInvisible del objeto que tiene el renderer: Model    
    [SerializeField] private float speed = 1;

    public override float Speed => speed;
    
    Rigidbody2D rb2D;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    public override void SetKinematicVelocity(Vector3 direction, float speed)
    {
        rb2D.velocity = direction.normalized * speed;
    }

    // Cuando una bala cumple su cometido de chocar con algo: se autodestruye
    // El análisis de cuánto daño hace la bala se hace en la componente DoDamage
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
