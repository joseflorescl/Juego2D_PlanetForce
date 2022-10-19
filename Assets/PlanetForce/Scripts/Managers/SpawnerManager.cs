using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    // Se usara una List de todos los Spawner, no un array
    // y los desactivo a todos esos objects en el Awake.
    // En el Update muevo a todos los spawners de la lista
    // En la corutina que valida la distancia al punto de activación: se recorre toda la lista
    //    si un spawner esta a una distancia OK, entonces SE ACTIVA el objeto y se ELIMINA de la lista para no volverlo a considerar
    //    Con esto ese spawner ya no se moverá más de posición.
    //    Y una vez realizado su cometido, el spawner se desactivará con un SetActive(false)

    // El transform del SpawnerManager se debe colocar justo arriba de la pantalla para que los enemigos
    // se activen antes de aparecer: esto se hace en el Start
    [SerializeField] private float distanceToActivate = 1f; // No puede ser un valor muy chico porque NO se valida en cada frame, sino que de acuerdo al valor de la var waitValidationToActivate
    [SerializeField] private float normalSpeed = 1; // Si se mueve 1m por seg, podremos fácilmente colocar un spawner que se gatillara a los 30seg de iniciar el juego: se coloca a 30m
    [SerializeField] private float waitValidationToActivate = 0.25f; // No es necesario que la validación se ejecute en cada frame


    List<Transform> spawnersToActivate;
    List<Transform> spawnersActivated;
    Vector2 direction = Vector2.down; // OJO que el juego por ahora solo se mueve en esta dirección
    float speed = 0;
    
    private void Awake()
    {        
        spawnersToActivate = new List<Transform>();
        spawnersActivated = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.gameObject.activeInHierarchy)
                spawnersToActivate.Add(child);
        }


        // Cada spawner se activara cuando este a una corta distancia del punto de activación
        DeactivateSpawners();

    }

    void Start()
    {
        SetPositionTopCenterScreen();
        Resume();
        StartCoroutine(ValidateDistanceToActivationPointRoutine());
    }

    void DeactivateSpawners()
    {
        for (int i = 0; i < spawnersToActivate.Count; i++)
        {
            spawnersToActivate[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < spawnersActivated.Count; i++)
        {
            spawnersActivated[i].gameObject.SetActive(false);
        }
    }

    void SetPositionTopCenterScreen()
    {
        var topCenter = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 1));
        topCenter.z = transform.position.z;
        transform.position = topCenter;
    }

    void Update()
    {
        if (speed == 0f || spawnersToActivate.Count == 0) return;
        TranslateEntitySpawners();
    }


    private void TranslateEntitySpawners()
    {
        for (int i = 0; i < spawnersToActivate.Count; i++)
        {
            spawnersToActivate[i].transform.Translate(speed * Time.deltaTime * direction);
        }
    }

    public void Resume()
    {
        speed = normalSpeed;
    }

    public void Stop()
    {
        speed = 0;
    }

    public void Kill()
    {
        Stop();
        StopAllCoroutines();
        DeactivateSpawners();
        KillEnemies();
        DestroyWithTag("EnemyBullet");
        DestroyWithTag("PowerUp");
    }

    void KillEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy.TryGetComponent(out EntityController entity))
                entity.Dead();
        }
    }

    void DestroyWithTag(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < objects.Length; i++)
        {
            Destroy(objects[i]);
        }
    }

    IEnumerator ValidateDistanceToActivationPointRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitValidationToActivate);

            for (int i = spawnersToActivate.Count - 1; i >= 0; i--)
            {
                var spawner = spawnersToActivate[i];
                var distance = Mathf.Abs(spawner.transform.position.y - transform.position.y); // Solo se considera por ahora un mov en eje Y para el Spawner
                if (distance < distanceToActivate)
                {
                    spawner.gameObject.SetActive(true);                    
                    spawnersToActivate.RemoveAt(i);
                    spawnersActivated.Add(spawner);
                }
                
            }
        }
    }

}
