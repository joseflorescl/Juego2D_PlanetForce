using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnerShape { Point, LineRandom, LineEquidistant }

public abstract class Spawner<T> : MonoBehaviour where T : MonoBehaviour // Sin este where NO se podría hacer el Instantiate
{
    // Se quiere que el tipo de dato del prefab NO sea GameObject para así al hacer un Instantiate se retorne
    //   altiro la componente respectiva, para ahorrarnos un GetComponent
    //  Por eso esta clase se hace genérica
    [SerializeField] protected T prefab;
    [SerializeField] protected SpawnerData spawnerData;

    float nextInstanceTime;
    int countInstances;

    // Esta data se usa solo en el caso de elegir la forma SpawnerShape.LineEquidistant
    (float xIniEquidistant,
     float xEquidistant,
     float deltaXEquidistant) lineEquidistantData;

    protected virtual void OnEnable() 
    {
        // No se usa Start porque el spawner se podría volver a activar segun las necesidades del SpawnerManager
        //  Aquí se podrían modificar los valores que vienen en spawnerData de acuerdo al nivel de dificultad del juego

        nextInstanceTime = 0;
        countInstances = 0;
        if (spawnerData.shape == SpawnerShape.LineEquidistant)
        {
            CalculateEquidistantValues();
        }

    }

    protected virtual void Update()
    {
        if (Time.time > nextInstanceTime)
        {
            nextInstanceTime = Time.time + spawnerData.SpawnerRate;

            if (spawnerData.shape == SpawnerShape.LineEquidistant)
                lineEquidistantData.xEquidistant = lineEquidistantData.xIniEquidistant;

            for (int i = 0; i < spawnerData.burstCount && countInstances < spawnerData.maxInstances; i++)
            {
                Vector3 pos = transform.position;

                switch (spawnerData.shape)
                {
                    case SpawnerShape.Point:
                        break;
                    case SpawnerShape.LineRandom:
                        // Nota: se usa int en xpos para que NO se sobrepongan los enemigos: el peor escenario es que 2 enemigos salgan exactamente
                        // en la misma posición, pero eso en la práctica no se notará, se comportará como 1 solo enemigo.
                        // Se suma 1 al final porque con ints el Range es excluyerte
                        int xRandomPos = Random.Range((int)(pos.x - spawnerData.horizontalRange), (int)(pos.x + spawnerData.horizontalRange) + 1);
                        pos.x = xRandomPos;
                        break;
                    case SpawnerShape.LineEquidistant:
                        pos.x = lineEquidistantData.xEquidistant;
                        lineEquidistantData.xEquidistant += lineEquidistantData.deltaXEquidistant;
                        break;
                }

                T instanceController = Create(pos);
                SetValuesPostCreation(instanceController);
                countInstances++;
            }

        }

    }

    protected abstract void SetValuesPostCreation(T instance);
    

    private void CalculateEquidistantValues()
    {
        if (spawnerData.burstCount == 1)
        {
            lineEquidistantData.xIniEquidistant = transform.position.x;
            lineEquidistantData.deltaXEquidistant = 0;
        }
        else
        {
            lineEquidistantData.xIniEquidistant = transform.position.x - spawnerData.horizontalRange;
            lineEquidistantData.deltaXEquidistant =
                (spawnerData.horizontalRange * 2f) / ((float)spawnerData.burstCount - 1f);
        }

    }


    // Este método de desactivación se llamar en caso de crear un spawner a mano, por ej, dentro de un enemigo
    //  y así poder detener el spawner una vez muerto ese enemigo.
    //  Recordar que los spawners del juego ppamente tal están siendo controlados por el Timeline
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }    

    T Create(Vector3 position)
    {        
        return Instantiate(prefab, position + spawnerData.offsetPosition, spawnerData.SpawnRotation);
    }


}
