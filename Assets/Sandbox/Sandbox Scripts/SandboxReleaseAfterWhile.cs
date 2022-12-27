using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxReleaseAfterWhile : MonoBehaviour
{
    [SerializeField] private float delayRelease = 3f;

    private void OnEnable()
    {
        Invoke(nameof(Release), delayRelease);
    }

    void Release()
    {
        PoolManager.Instance.Release(gameObject);
    }

}
