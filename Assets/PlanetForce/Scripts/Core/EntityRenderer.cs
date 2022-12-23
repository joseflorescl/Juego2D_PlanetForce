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
        isVisible = false;
        if (wasVisible && destroyParentOnBecameInvisible)
        {           
            if (!gameObject.activeInHierarchy)
                return; // Se ejecuta OnBecameInvisible en un objeto NO activo: ya se le hizo Release

            StartCoroutine(DestroyWithDelay(delayToDestroy));
        }
    }

    private void OnBecameVisible()
    {
        isVisible = true;
        wasVisible = true;
    }

    IEnumerator DestroyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);        
        PoolManager.Instance.Release(transform.parent.gameObject);
        wasVisible = false;
    }
}
