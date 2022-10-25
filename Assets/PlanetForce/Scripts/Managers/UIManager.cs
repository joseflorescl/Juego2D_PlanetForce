using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    // Aqui van las referencias a elementos del Canvas. Ningún otro script tiene referencias directas al canvas.
    [SerializeField] private GameObject pressEnter;
    [SerializeField] private GameObject gameManual;
    [SerializeField] private GameObject respawnMessage;
    [SerializeField] private GameObject gameOverMessage;
    [SerializeField] private GameObject hiScoreMessage;
    [SerializeField] private GameObject theEndMessage;
    [SerializeField] private GameObject holdToFireHint;

    [SerializeField] private GameObject panelTitle;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiScoreText;
    [SerializeField] private TextMeshProUGUI hiScoreFantasticScoreText;
    [SerializeField] private TextMeshProUGUI extraLifePointsText;
    [SerializeField] private TextMeshProUGUI specialBonusText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject livesGroup;
    [SerializeField] private BlinkingGraphic player1Label;
    [SerializeField] private Graphic hiScoreLabel;
    

    Button pressEnterButton;
    TextMeshProUGUI pressEnterText;
    int hiScore;

    GameObject[] messagesPanelCenter;
    Coroutine showAndHideRoutine;
    bool validateFirstUpdateHiScore;
    

    void Awake()
    {
        messagesPanelCenter = new GameObject[] { gameManual, respawnMessage, gameOverMessage, panelTitle, hiScoreMessage, theEndMessage};
        holdToFireHint.SetActive(false);

        // Notar que en Unity NO funciona el operador ?. con las vars serializadas
        if (pressEnter)
        {
            pressEnterButton = pressEnter.GetComponentInChildren<Button>();
            pressEnterText = pressEnter.GetComponentInChildren<TextMeshProUGUI>();
        }

        validateFirstUpdateHiScore = true;


    }

    public void ShowAndHideHoldToFireHint(float secondsActive)
    {
        holdToFireHint.SetActive(true);
        StartCoroutine(SetActiveWithDelay(holdToFireHint, false, secondsActive));
    }

    public void EnterPressed(string text)
    {
        pressEnterButton.interactable = false;
        pressEnterText.text = text;
    }

    public void UpdateScore(int value)
    {
        scoreText.text = value.ToString();

        if (value > hiScore)
        {
            if (validateFirstUpdateHiScore)
            {
                player1Label.SynchronizeBlinkingWith(hiScoreLabel);
                validateFirstUpdateHiScore = false;
            }

            UpdateHiScore(value);
        }
    }

    public void UpdateHiScore(int value)
    {        
        hiScore = value;
        hiScoreText.text = value.ToString();
    }

    
    
    public void UpdateLives(int totalLives)
    {
        if (totalLives <= 0) return;
        
        totalLives--; // siempre se muestra una vida menos
        // Se debe tener cuidado que se puede llegar a tener más de 5 vidas, pero solo se mostraran hasta 5
        int childLifesCount = livesGroup.transform.childCount;
        int min = Math.Min(totalLives, childLifesCount);
        
        for (int i = 0; i < min; i++)
        {
            var childlife = livesGroup.transform.GetChild(i);
            childlife.gameObject.SetActive(true);
        }

        for (int i = totalLives; i < childLifesCount; i++)
        {
            var childlife = livesGroup.transform.GetChild(i);
            childlife.gameObject.SetActive(false);
        }

    }

    public void ShowAndHideGameManual(float secondsActive, int extraLifePoints)
    {
        HideAllMessages();
        extraLifePointsText.text = extraLifePoints.ToString() + " PTS";
        gameManual.SetActive(true);
        StopShowAndHideRoutine();
        showAndHideRoutine = StartCoroutine(SetActiveWithDelay(gameManual, false, secondsActive));
    }

    public void ShowAndHideRespawnMessage(float secondsActive)
    {
        HideAllMessages();
        respawnMessage.SetActive(true);
        StopShowAndHideRoutine();
        showAndHideRoutine = StartCoroutine(SetActiveWithDelay(respawnMessage, false, secondsActive));
    }

    public void ShowGameOverMessage()
    {
        HideAllMessages();
        gameOverMessage.SetActive(true);
        panelTitle.SetActive(true);
        player1Label.enabled = false;
    }

    public void ShowHiScoreMessage()
    {
        HideAllMessages();
        hiScoreFantasticScoreText.text = hiScore.ToString();
        hiScoreMessage.SetActive(true);
        panelTitle.SetActive(true);
    }

    public void ShowTheEndMessage(int specialBonus, int score)
    {
        HideAllMessages();
        specialBonusText.text = specialBonus.ToString() + " PTS";
        finalScoreText.text = score.ToString() ;
        theEndMessage.SetActive(true);
        var button = theEndMessage.GetComponentInChildren<Button>();
        button.Select();
        panelTitle.SetActive(true);
        player1Label.enabled = false;
    }


    IEnumerator SetActiveWithDelay(GameObject gameObj, bool value, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObj.SetActive(value);
    }

    void HideAllMessages()
    {
        for (int i = 0; i < messagesPanelCenter.Length; i++)
        {
            messagesPanelCenter[i].SetActive(false);
        }
    }

    void StopShowAndHideRoutine()
    {
        if (showAndHideRoutine != null)
        {
            StopCoroutine(showAndHideRoutine);
        }
    }
}
