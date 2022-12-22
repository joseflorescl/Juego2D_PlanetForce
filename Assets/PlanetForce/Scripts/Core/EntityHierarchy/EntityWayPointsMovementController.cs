using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityWayPointsMovementController : EntityController
{
    [SerializeField] private WayPointsData wayPoints;

    private void OnEnable()
    {
        SetKinematicVelocity(transform.up, entityData.speed);
    }

    public override void Init()
    {
        base.Init();
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
        // TODO: entonces como ya tenemos Init() este yield ya no va
        //yield return null; // Se espera 1 frame para que el spawner setee la vel deseada por él, porque no tenemos un método tipo Init().

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
