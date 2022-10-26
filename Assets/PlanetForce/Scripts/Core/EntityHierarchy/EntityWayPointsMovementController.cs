using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityWayPointsMovementController : EntityController
{
    [SerializeField] private WayPointsData wayPoints;

    protected override void Awake()
    {
        base.Awake();        
        SetKinematicVelocity(transform.up, entityData.speed);
    }


    protected override void Start()
    {        
        base.Start();
        StartCoroutine(FireRoutineToTarget());
        StartCoroutine(MoveBetweenWayPointsRoutine());
    }

    public override void Dead()
    {
        base.Dead();
        // Aquí se nota la necesidad de usar herencia: se quiere ejecutar código genérico de una EntityController, pero además para un tipo
        // en particular de entity se quiere ejecutar algo más, como el seteo del Damage Layer!
        SetWeightAnimationLayer("Damage Layer", 0);
    }


    

    IEnumerator MoveBetweenWayPointsRoutine()
    {
        float speed = rb2D.velocity.magnitude;
        while (!entityHealth.IsDead)
        {

            var targetPosition = wayPoints.GetRandomPosition();
            var direction = targetPosition - (Vector2)transform.position;
            SetKinematicVelocity(direction, speed);

            yield return new WaitUntil(() => (Vector2.Distance(targetPosition, (Vector2)transform.position) < wayPoints.distanceToTarget));

            SetKinematicVelocity(direction, 0);
            float wait = Random.Range(wayPoints.minWaitInTarget, wayPoints.maxWaitInTarget);
            yield return new WaitForSeconds(wait);
        }


    }

}
