using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxAwakesOnEnables : MonoBehaviour
{

    private void Awake()
    {
        print("Awake " + name);
    }

    private void OnEnable()
    {
        print("OnEnable " + name);
    }

    private void Start()
    {
        print("Start " + name);
    }
}
