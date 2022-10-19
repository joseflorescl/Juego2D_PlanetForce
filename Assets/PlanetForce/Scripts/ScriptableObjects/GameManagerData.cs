using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GameManager Data", menuName = "Planet Force/Game Manager Data")]
public class GameManagerData : ScriptableObject
{    
    public int extraLifeEveryPoints = 20000;
    public int specialBonusEndGame = 10000000;
    public float secondsShowGameManual = 4f;
    public float secondsShowRespawnMessage = 2f;    
    public float godModeDuration = 1;
    [Range(1, 10)]
    public int initialLives = 3; // se pueden tener más vidas de las que caben en la UI

    public float extraDurationEnterPressed = 0.2f;
    public float waitForPlayerExplosion = 1f;
    public float waitForEnemyExplosion = 2f;    
}
