using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;

    [SerializeField] private float delayShowFPS = 0.25f;

    WaitForSeconds waitShowFPS;

    private void Awake()
    {
        waitShowFPS = new WaitForSeconds(delayShowFPS);
    }

    private void Start()
    {
        StartCoroutine(FPSRoutine());
    }

    private IEnumerator FPSRoutine()
    {
        while (true)
        {
            if (Time.smoothDeltaTime != 0)
            {
                float fps = 1f / Time.smoothDeltaTime;
                fpsText.text = Mathf.Round(fps).ToString();
            }
            
            yield return waitShowFPS;
        }
    }
}
