using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private BackgroundManagerData backgroundManagerData;

    /*[SerializeField]*/
    private Vector2 direction = Vector2.down; // Solo está implementada la validación de cambio de sprite con movimiento hacia abajo.
    private SpriteRenderer[] backgrounds; // Aqui están las instancias propiamente tal de los prefabs usados, de tamaño size
    private float yCameraBase;
    private float speed;
    private float targetSpeed;
    private float pausedSpeed;

    Coroutine changeSpeedRoutine;

    public bool IsStopped => targetSpeed == 0f;
    


    private void Awake()
    {
        speed = pausedSpeed = targetSpeed = 0;        
        backgrounds = new SpriteRenderer[backgroundManagerData.size];
    }

    void Start()
    {
        if (IsStopped)
            NormalSpeed();

        yCameraBase = Camera.main.ViewportToWorldPoint(Vector3.zero).y;
        InstantiateBackgrounds();
        StartBackgroundPositions();
        ActivateBackgrounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (speed == 0f) return;

        TranslateBackgrounds();

        // Validación de posición de backgrounds, considerando una direction == Vector.down. Para up NO se está considerando aún.
        var firstBackground = backgrounds[0];
        float yTop = firstBackground.bounds.max.y;
        
        if (yTop < yCameraBase)
        {
            SwapNewBackgrounds();
        }

    }


    public void SendToBackground(Transform transformToBackground)
    {
        transformToBackground.parent = backgrounds[backgrounds.Length - 1].gameObject.transform;
    }

    public void Stop()
    {
        targetSpeed = 0;
        StopChangeSpeedRoutine();
        changeSpeedRoutine = StartCoroutine(ChangeSpeed(targetSpeed, 0.5f));
    }

    public void NormalSpeed()
    {
        targetSpeed = backgroundManagerData.normalSpeed;
        StopChangeSpeedRoutine();
        changeSpeedRoutine = StartCoroutine(ChangeSpeed(targetSpeed, 0.5f));
    }

    public void HighSpeed()
    {
        targetSpeed = backgroundManagerData.highSpeed;
        StopChangeSpeedRoutine();
        changeSpeedRoutine = StartCoroutine(ChangeSpeed(targetSpeed, 1));
    }

    public void Pause()
    {
        if (speed == 0) return;

        targetSpeed = 0;
        pausedSpeed = speed; // se guarda la speed actual para que al hacer el Resume() se retome la speed en la que estábamos
        StopChangeSpeedRoutine();
        changeSpeedRoutine = StartCoroutine(ChangeSpeed(targetSpeed, 1));
    }

    public void Resume()
    {
        // Se retoma la speed antes de pausar, pero se obliga a que sea mayor a 0
        if (pausedSpeed == 0) return;

        targetSpeed = pausedSpeed;
        StopChangeSpeedRoutine();
        changeSpeedRoutine = StartCoroutine(ChangeSpeed(targetSpeed, 1));
    }


    private IEnumerator ChangeSpeed(float newSpeed, float duration)
    {
        float timeElapsed = 0;        
        var startValue = speed;
        var targetValue = newSpeed;
        while (timeElapsed < duration)
        {
            speed = Mathf.Lerp(startValue, targetValue, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        speed = newSpeed;
    }


    private void TranslateBackgrounds()
    {
        // Notar que NO se hace un transform.Translate del objeto padre, porque o si no la posición de ese objeto tendra un valor MUY grande despues de un rato
        //transform.Translate(speed * Time.deltaTime * direction);

        // Cuando cada background llegue por debajo de la camara se auto destruye, por lo que nunca tendremos valores grandes en la position de los backgrounds
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].transform.Translate(speed * Time.deltaTime * direction);
        }
    }

    private void InstantiateBackgrounds()
    {        
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i] = backgroundManagerData.InstantiateBackground(transform);
            backgrounds[i].gameObject.SetActive(false);
        }
     
    }

    private void StartBackgroundPositions()
    {
        float yBase = yCameraBase;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            yBase = RepositionBackgroundAtYPos(backgrounds[i], yBase);
        }
    }    

    private float RepositionBackgroundAtYPos(SpriteRenderer rendSprite, float yPos)
    {
        // La funcion retorna el valor de yTop del sprite reposicionado
        // Solo se reposiciona en el eje Y, no se toca el eje X: con esto es el diseñador el que elige cómo 
        // ubicar el sprite c/r al eje X.        
        float exentsY = rendSprite.bounds.extents.y;
        var newPos = rendSprite.transform.position;
        newPos.y = yPos + exentsY;
        rendSprite.transform.position = newPos;

        return yPos + rendSprite.bounds.size.y;
    }

    private void ActivateBackgrounds()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].gameObject.SetActive(true);
        }
    }

    private void SwapNewBackgrounds()
    {
        // El primer background se elimina
        //Destroy(backgrounds[0].gameObject);
        PoolManager.Instance.Release(backgrounds[0].gameObject);

        // Se reacomoda el array de backgrounds (es como un stack)
        int size = backgrounds.Length;
        for (int i = 1; i < size; i++)
        {
            backgrounds[i - 1] = backgrounds[i];
        }

        backgrounds[size - 1] = backgroundManagerData.InstantiateBackground(transform);
        
        var lastBackground = backgrounds[size - 1];
        lastBackground.gameObject.SetActive(false);

        float yBase = backgrounds[size - 2].bounds.max.y;
        RepositionBackgroundAtYPos(lastBackground, yBase);
        lastBackground.gameObject.SetActive(true);
    }

    void StopChangeSpeedRoutine()
    {
        if (changeSpeedRoutine != null)
            StopCoroutine(changeSpeedRoutine);
    }


}
