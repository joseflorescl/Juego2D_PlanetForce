using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidateViewport : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        print("1,1 = " + Camera.main.ViewportToWorldPoint(new Vector3(1, 1)));
        print("0,0 = " + Camera.main.ViewportToWorldPoint(new Vector3(0, 0)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
