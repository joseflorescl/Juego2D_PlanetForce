using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class BlinkingGraphic : MonoBehaviour
{
    [SerializeField] private float visibleTime = 1f;
    [SerializeField] private float invisibleTime = 0.5f;

    List<Graphic> synchronizeBlinking;
    Graphic graphic;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();

        synchronizeBlinking = new List<Graphic>();
        synchronizeBlinking.Add(graphic);
    }

    // Se usa OnEnable() y NO Start(): si un gameobject es desactivado, al activarlo nuevamente, NO se ejecuta su Start, pero sí el OnEnable
    private void OnEnable()
    {
        StartCoroutine(BlinkingObject());
    }

    private IEnumerator BlinkingObject()
    {
        while (synchronizeBlinking.Count > 0)
        {
            SetEnabled(true);
            yield return new WaitForSeconds(visibleTime);
            SetEnabled(false);
            yield return new WaitForSeconds(invisibleTime);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        SetEnabled(true);
    }

    public void SynchronizeBlinkingWith(Graphic other)
    {
        synchronizeBlinking.Add(other);
    }


    void SetEnabled(bool value)
    {
        for (int i = 0; i < synchronizeBlinking.Count; i++)
        {
            synchronizeBlinking[i].enabled = value;
        }
    }
}
