using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : Spawner<EntityController>
{
    protected override void SetValuesPostCreation(EntityController entity)
    {    
        // Nota Nivel de Dificultad: aqu� se podr�a setear una speed, maxhealth, etc de la entidad reci�n creada
        //  dependiendo del nivel de dificultad actual del juego que lo deber�a manejar el GameManager
        // Lo otro tambi�n es que el Spawner incremente sus propios valores de burst de entidades de acuerdo a la dificultad
        entity.SetKinematicVelocity(entity.transform.up, spawnerData.startSpeed);
        entity.Init();
    }
}
