using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityRenderer : MonoBehaviour
{
    [SerializeField] private bool destroyParentOnBecameInvisible = true;
    [SerializeField] private float delayToDestroy;

    private SpriteRenderer spriteRenderer;
    private bool isVisible;

    public bool IsVisible => isVisible;

    public bool DestroyParentOnBecameInvisible => destroyParentOnBecameInvisible;

    // Esta var la uso para que este script me funcione usando el editor: el evento OnBecameInvisible se gatilla en el frame 2, aunque el objeto parta siendo visible
    bool wasVisible = false;


    public Vector3 Extents => spriteRenderer.bounds.extents;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void OnBecameInvisible()
    {
        // Nota: a veces en el editor de Unity puede salir este error:
        // Coroutine couldn't be started because the the game object 'Model' is inactive!
        // Pero ese es un error que solo pasa en el editor de vez en cuando: es como que un objeto se instancia 
        // pero sin estar visible en pantalla ni menos hacerse invisible, se llama al evento OnBecameInvisible()
        // pero el objeto no está activo. Por eso se agrega esta condición de borde, para que no aparezca ese error
        if (!gameObject.activeInHierarchy)
        {
            print("Se ejecuta OnBecameInvisible en un objeto NO activo...es un error que solo pasa en el editor");
            return;
        }

        isVisible = false;
        if (wasVisible && destroyParentOnBecameInvisible)
            StartCoroutine(DestroyWithDelay(delayToDestroy));
    }

    private void OnBecameVisible()
    {
        isVisible = true;
        wasVisible = true;
    }

    IEnumerator DestroyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Destroy(transform.parent.gameObject);
        PoolManager.Instance.Release(transform.parent.gameObject);
    }
}
