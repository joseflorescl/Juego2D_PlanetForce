using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerSquare : MonoBehaviour
{
    [SerializeField] private GameObject squarePrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            squarePrefab.SetActive(false);
            var obj = Instantiate(squarePrefab);
        }
    }
}
