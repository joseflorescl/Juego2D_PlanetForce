using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeOnEnableStart : MonoBehaviour
{
    private void Awake()
    {
        print(Time.frameCount + " Awake");
    }

    void Start()
    {
        print(Time.frameCount + " Start");
    }

    private void OnEnable()
    {
        print(Time.frameCount + " OnEnable");
    }

}
