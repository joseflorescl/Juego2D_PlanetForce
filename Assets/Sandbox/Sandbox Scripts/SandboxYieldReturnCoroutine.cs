using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxYieldReturnCoroutine : MonoBehaviour
{

    private IEnumerator Start()
    {
        print(Time.frameCount + " Start A");

        yield return Routine123();
        //yield return StartCoroutine(Routine123());

        print(Time.frameCount + " Start B");
    }

    private IEnumerator Routine123()
    {
        yield return null;
        print(Time.frameCount + " Uno");
        yield return null;

        print(Time.frameCount + " Dos");
        yield return null;

        print(Time.frameCount + " Tres");
        yield return null;
    }
}
