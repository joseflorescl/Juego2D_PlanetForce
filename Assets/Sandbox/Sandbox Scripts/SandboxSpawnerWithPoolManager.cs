using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxSpawnerWithPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject squarePrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var obj = PoolManager.Instance.Get(squarePrefab, transform.position, transform.rotation);
        }
    }
}
