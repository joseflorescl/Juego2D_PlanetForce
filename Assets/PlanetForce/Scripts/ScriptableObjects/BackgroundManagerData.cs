using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BackgroundManager Data", menuName = "Planet Force/Background Manager Data")]
public class BackgroundManagerData : ScriptableObject
{
    public SpriteRenderer[] backgroundPrefabs;
    public float normalSpeed = 2f;
    public float highSpeed = 4f; // Se usa en la escena de High Score para darle un toque más de velocidad
    public int size = 2; // cantidad de backgrounds que van a estar moviéndose a la vez. Depende del tamaño de los sprites usados

    int currentIdx;
    int prevIdx; // se usa cuando se genere un nuevo background, no sea igual al último ya creado. Random pero no tanto


    private void Awake()
    {
        prevIdx = 0;
        currentIdx = 0;
    }

    public SpriteRenderer InstantiateBackground(Transform parent)
    {
        prevIdx = currentIdx;
        currentIdx = Random.Range(0, backgroundPrefabs.Length);
        if (currentIdx == prevIdx) // para que no tengamos 2 sprites seguidos
            currentIdx = (currentIdx + 1) % backgroundPrefabs.Length;
        var spriteRenderer = Instantiate(backgroundPrefabs[currentIdx], parent);
        return spriteRenderer;
    }
}
