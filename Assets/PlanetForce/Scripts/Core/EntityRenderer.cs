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
            Destroy(transform.parent.gameObject, delayToDestroy);
        }
        
    }

    private void OnBecameVisible()
    {
        isVisible = true;
        wasVisible = true;
    }
}
