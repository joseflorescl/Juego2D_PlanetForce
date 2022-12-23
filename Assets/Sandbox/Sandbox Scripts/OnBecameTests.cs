using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnBecameTests : MonoBehaviour
{

    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //print(rend.isVisible);
    }

    private void OnBecameVisible()
    {
        print("OnBecameVisible " + name);
    }

    private void OnBecameInvisible()
    {
        print("OnBecameInvisible " + name);
    }
}
