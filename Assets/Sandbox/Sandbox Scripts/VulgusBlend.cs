using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VulgusBlend : MonoBehaviour
{
    public Animator anim;
    public float animFreq;
    public float recoveryRate = 1f;

    float blend = 0;

    // Update is called once per frame
    void Update()
    {
        //float t = Mathf.PingPong(Time.time * animFreq, 1);
        //float blend = Mathf.Lerp(-1, 1, t);
        float horizontal = Input.GetAxisRaw("Horizontal");
        blend = Mathf.MoveTowards(blend, horizontal, recoveryRate * Time.deltaTime);

        //anim.SetFloat("MovementX", blend);
        anim.SetFloat("MovementX", -1f);
    }
}
