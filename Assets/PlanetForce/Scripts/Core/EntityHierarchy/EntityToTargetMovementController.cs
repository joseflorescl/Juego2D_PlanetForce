using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityToTargetMovementController : EntityController
{
    [SerializeField] private float minWaitInMovement = 0.5f;
    [SerializeField] private float maxWaitInMovement  = 1f;
    [SerializeField] private float minWaitNoMovement = 0.2f;
    [SerializeField] private float maxWaitNoMovement = 0.5f;

    float DurationInMovement => Random.Range(minWaitInMovement, maxWaitInMovement);
    float DurationNoMovement => Random.Range(minWaitNoMovement, maxWaitNoMovement);

    protected override void Awake()
    {
        base.Awake();
        SetKinematicVelocity(transform.up, entityData.speed);
    }

    //TODO: Pendiente: probar bien por qué a veces se instancian enemigos Yashichi pero son eliminados casi inmeditamente
    //  yo creo que la culpa es estar probando en el editor (no en el build) y el incorrecto funcionamiento
    //  del OnBecomeInvisible que es el que seguramente me lo está destruyendo.
    //TODO: tengo desactivado en el prefab, en comp Entity Renderer, por si acaso...
    //private void OnDisable()
    //{
    //    print("   OnDisable de Yashichi con velocidad = " + rb2D.velocity + " Posición: " + transform.position);
    //}

    void Start()
    {
        StartCoroutine(FireRoutineToTarget());
        StartCoroutine(MoveToTarget());
    }

    public override void Damage()
    {
        base.Damage();
        anim.SetTrigger("Damage");
    }

    public override void Dead()
    {
        anim.ResetTrigger("Damage");
        base.Dead();        
        SetWeightAnimationLayer("Damage Layer", 0);
    }

    IEnumerator MoveToTarget()
    {
        float speed = rb2D.velocity.magnitude;

        // Al ppio lo moveremos a una speed == a la mitad seteada por el creador de este enemigo (spawner o manual)
        SetKinematicVelocity(rb2D.velocity, speed / 2f);

        while (!entityHealth.IsDead)
        {
            // Nota corutinas: para que esta corutina funcione bien es necesario que en la clase padre
            //  se llame a StopAllCoroutines() en el método Dead().
            //  Si no, tendríamos que agregar un "if (isDead)" después de cada yield.

            // Fase 1: ya estamos en movimiento, se espera un rato antes de detenerse
            yield return new WaitForSeconds(DurationInMovement);
            SetKinematicVelocity(transform.up, 0);

            // Fase 2: estamos recién detenidos: se quiere volver a girar despacio
            anim.SetFloat("AngularSpeed", 1);
            //  se espera un rato antes de volver a moverse: la primera mitad del tiempo se seguirá girando despacio
            yield return new WaitForSeconds(DurationNoMovement / 2f);            
            //  y la segunda mitad del tiempo seguimos detenidos pero girando más rápido
            anim.SetFloat("AngularSpeed", 5);
            yield return new WaitForSeconds(DurationNoMovement / 2f);            

            // Fase 3: ahora nos vamos a mover hacia el target: notar que ya estamos girando rápido
            var direction = (Vector2)TargetPosition() - (Vector2)transform.position;
            SetKinematicVelocity(direction, speed);
        }
        
    }    

}
