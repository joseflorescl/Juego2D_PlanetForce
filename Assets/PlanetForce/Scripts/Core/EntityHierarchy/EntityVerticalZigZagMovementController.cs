using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityVerticalZigZagMovementController : EntityController
{
    [SerializeField] private float minWaitBeforeChangeDirection = 1.5f;
    [SerializeField] private float maxWaitBeforeChangeDirection = 2f;
    [SerializeField] private float minXMagnitude = 0.6f;
    [SerializeField] private float maxXMagnitude = 1f;

    float RandomXMagnitude => Random.Range(minXMagnitude, maxXMagnitude);
    float DurationBeforeChangeDirection => Random.Range(minWaitBeforeChangeDirection, maxWaitBeforeChangeDirection);
    Vector2 GetDownwardDirection(int xDirection) => new Vector2(xDirection * RandomXMagnitude, -1);
    
    private void OnEnable() // Pool Manager Compatible
    {
        SetKinematicVelocity(transform.up, entityData.speed);
    }

    public override void Init()
    {
        base.Init();
        StartCoroutine(FireRoutineToTarget());
        StartCoroutine(MoveVerticalZigZag());
    }


    IEnumerator MoveVerticalZigZag()
    {
        float speed = rb2D.velocity.magnitude;
        
        int xDirection = GetInitialXDirection();
        Vector2 direction = GetDownwardDirection(xDirection);
        SetKinematicVelocity(direction, speed);
        SetAnimation(xDirection);

        // Ahora espera un ratito antes de cambiarle su dirección al otro lado:        
        yield return new WaitForSeconds(DurationBeforeChangeDirection);
        xDirection *= -1;
        direction = GetDownwardDirection(xDirection);
        SetKinematicVelocity(direction, speed);
        SetAnimation(xDirection);
    }

    void SetAnimation(int xDirection)
    {
        if (xDirection == 1)
            anim.SetTrigger("SpinningToRight");
        else
            anim.SetTrigger("SpinningToLeft");
    }

    int GetInitialXDirection()
    {
        // Se elige un vector de movimiento hacia el otro lado:
        var topLeft = Camera.main.ViewportToWorldPoint(new Vector2(0, 1));
        var topRight = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
        var distanceToLeft = Mathf.Abs(transform.position.x - topLeft.x);
        var distanceToRight = Mathf.Abs(transform.position.x - topRight.x);
        int xDirection = (distanceToLeft < distanceToRight) ? 1 : -1;

        return xDirection;
    }







}
