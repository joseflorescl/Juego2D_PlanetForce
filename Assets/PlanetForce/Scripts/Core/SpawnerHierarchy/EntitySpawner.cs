using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : Spawner<EntityController>
{
    protected override void SetValuesPostCreation(EntityController entity)
    {    
        // Nota Nivel de Dificultad: aquí se podría setear una speed, maxhealth, etc de la entidad recién creada
        //  dependiendo del nivel de dificultad actual del juego que lo debería manejar el GameManager
        // Lo otro también es que el Spawner incremente sus propios valores de burst de entidades de acuerdo a la dificultad
        entity.SetKinematicVelocity(entity.transform.up, spawnerData.startSpeed);
        entity.Init();
    }
}
