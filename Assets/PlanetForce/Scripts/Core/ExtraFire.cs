using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraFire : MonoBehaviour
{
    // Nota: los métodos de disparo (esta clase y el método FireRoutineToTarget podrían ser perfectamente Scriptable Objects)
    //   que implementan diferentes algoritmos de disparo, que se le pueden agregar a la clase EntityController
    [SerializeField] private float initialDelay = 10f;
    [SerializeField] private float delayBetweenFire = 0.2f;
    [SerializeField] private int fireCount = 5;

    EntityController entityController;

    private void Awake()
    {
        entityController = GetComponent<EntityController>();
    }

    private void Start()
    {
        StartCoroutine(ExtraFireRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ExtraFireRoutine()
    {
        while(!entityController.IsDead)
        {
            yield return new WaitForSeconds(initialDelay);

            // Notar lo importante de validar aquí que antes de disparar la entidad NO esté muerta
            //  porque esta corutina NO es controlada por la entityController.
            for (int i = 0; i < fireCount && !entityController.IsDead; i++)
            {
                entityController.FireBulletToTarget();
                yield return new WaitForSeconds(delayBetweenFire);
            }

            // Despues de cada loop de disparos se incrementa en uno para darle más dificultad
            fireCount++;
        }
    }

    
}
