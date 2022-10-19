using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySimpleMovementController : EntityController
{
    protected override void Awake()
    {
        base.Awake();
        // Por ahora los enemigos se estan moviendo usando el sistema de fisicas, usando su Rb2D kinematico, no los estamos moviendo en el Update con transform.Translate
        // Este tipo de enemigo es el más simple: solo se mueve en su dirección a la que esta mirando:        
        SetKinematicVelocity(transform.up, entityData.speed); // Esto se debe hacer en el Awake y no en el Start, para que se pueda sobreescribir en el spawner despues de hacer el Instantiate
    }

    void Start()
    {
        StartCoroutine(FireRoutineToTarget());
    }
        

}
