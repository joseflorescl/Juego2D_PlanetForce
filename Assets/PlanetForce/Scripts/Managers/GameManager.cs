using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{    
    const string READY_TEXT = "READY!";
    const string GAMEDATA_KEY = "PlanetForceData";

    public enum GameState {Welcome, Game, TheEnd}
    public enum Ship { Xevious = 0, Vulgus }
    public enum MusicState { Normal = 0, Battle }

    [SerializeField] private GameState gameState;
    [SerializeField] private Ship playerShip;
    [SerializeField] private GameManagerData gameManagerData;

    AudioManagerWithBattleMusic  audioManager;
    SceneController sceneController;
    UIManager uiManager;
    BackgroundManager backgroundManager;
    SpawnerManager spawnerManager;
    PowerUpManager powerUpManager;
    PlayerController player;    
    GameplayData gameplayData;
    int initialHiScore;
    MusicState musicState;
    
    void Awake()
    {
        audioManager = (AudioManagerWithBattleMusic)FindObjectOfType<AudioManager>();
        sceneController = FindObjectOfType<SceneController>();
        uiManager = FindObjectOfType<UIManager>();
        backgroundManager = FindObjectOfType<BackgroundManager>();
        spawnerManager = FindObjectOfType<SpawnerManager>();        
        player = FindObjectOfType<PlayerController>();
        powerUpManager = FindObjectOfType<PowerUpManager>();
        powerUpManager?.SetPlayer(player);
        musicState = MusicState.Normal;

#if UNITY_EDITOR
        Application.targetFrameRate = 30;
#endif

    }


    void Start()
    {
        gameplayData = GameDataRepository.GetById(GAMEDATA_KEY);
        initialHiScore = gameplayData.HiScore;

        ResetGameplayData();

        uiManager.UpdateHiScore(gameplayData.HiScore);

        switch (gameState)
        {
            case GameState.Welcome:
                if (musicState == MusicState.Normal)
                    audioManager.PlayWelcomeMusic();
                break;
            case GameState.Game:
                if (musicState == MusicState.Normal)
                    audioManager.PlayGameMusic();
                uiManager.ShowAndHideGameManual(gameManagerData.secondsShowGameManual, gameManagerData.extraLifeEveryPoints);
                uiManager.UpdateLives(gameplayData.Lives);
                break;            
        }

    }

    void OnDisable()
    {        
        if (gameState != GameState.Welcome)
            SaveDataGame();
    }

    public void SpeedAndWeaponPowerUp(AudioClip powerUpCatchedClip, float powerUpTime, float speed)
    {
        bool playMusic = musicState != MusicState.Battle;
        // Notar que la musica de Batalla tiene preferencia sobre la musica de power up        
        audioManager.PlayPowerUpMusic(powerUpCatchedClip, playMusic, powerUpTime); // true: si debe reproducir música después del sfx
        powerUpManager.SpeedAndWeaponPowerUp(powerUpTime, speed);
        uiManager.ShowAndHideHoldToFireHint(gameManagerData.secondsShowGameManual);
    }

    public void EntityAppearsInBattle(EntityController entity)
    {
        // Por ahora solo se maneja la entrada de los enemigos jefes
        if (entity.gameObject.CompareTag("Enemy") || entity.gameObject.CompareTag("FinalBoss"))
        {
            musicState = MusicState.Battle;
            audioManager.StopGameMusic();
            audioManager.SetBattleMusicData(entity.AudioClipsInBattle);
            audioManager.PlayBattleMusic();

            backgroundManager.HighSpeed();
        }
    }

    public void EntityDeadInBattle(EntityController entity)
    {
        // Por diseño del juego solo 1 entidad hace aparición/muerte a la vez
        //musicState = MusicState.Normal;
        audioManager.EndBattleMusic(true, gameManagerData.waitForEnemyExplosion, SetMusicAfterBattle); // Por ahora solo se llama cuando la entidad muere, es decir, el enemigo es muerto por el player
        backgroundManager.NormalSpeed();
    }

    public void SetMusicAfterBattle()
    {
        musicState = MusicState.Normal;
        audioManager.PlayInGameMusic();
    }

    public void EnterPressed() // Esta función es llamada desde un Botón de la UI, por eso tiene 0 referencias
    {
        StartCoroutine(EnterPressedRoutine(READY_TEXT));
    }

    public void EntityFired(EntityController entity)
    {
        audioManager.PlayRandomFireSound(entity.AudioClipsShoot);
    }

    public void EntityDead(EntityController entity)
    {
        audioManager.PlayRandomExplosionSound(entity.AudioClipsExplosion);

        if (gameState == GameState.TheEnd) return;

        if (entity.CompareTag("Enemy"))
            EnemyDead(entity);
        else if (entity.CompareTag("FinalBoss"))
        {
            EnemyDead(entity);
            StopAllCoroutines();
            StartCoroutine(TheEnd());
        }
        else if (entity.CompareTag("Player"))
            StartCoroutine(PlayerDead());
    }


    IEnumerator TheEnd()
    {
        gameState = GameState.TheEnd;
        spawnerManager.Kill();

        //esperar a que la musica vuelva a estado Normal, en caso de haber estado en modo Battle
        yield return new WaitUntil(() => musicState == MusicState.Normal);

        gameplayData.Score += gameManagerData.specialBonusEndGame;
        uiManager.UpdateScore(gameplayData.Score);
        uiManager.ShowTheEndMessage(gameManagerData.specialBonusEndGame, gameplayData.Score);
        backgroundManager.HighSpeed();
        audioManager.PlayTheEndMusic();
        
    }

    public void EntityDamaged(EntityController entity)
    {        
        audioManager.PlayRandomDamageSound(entity.AudioClipsDamage);
    }

    public Vector3 PlayerPosition()
    {
        if (player)
            return player.transform.position;
        else
            return 10 * Vector3.down;
    }

    public void SendToBackground(Transform transform)
    {
        backgroundManager.SendToBackground(transform);
    }

    void ExtraLifeValidation()
    {
        while (gameplayData.Score > gameplayData.NextScoreToExtendLife)
        {
            gameplayData.NextScoreToExtendLife += gameManagerData.extraLifeEveryPoints;
            gameplayData.Lives++;
            uiManager.UpdateLives(gameplayData.Lives);
            audioManager.PlayExtraLifeSound();
        }
    }

    void EnemyDead(EntityController enemy)
    {                
        StartCoroutine(DestroyWithDelay(enemy.gameObject, enemy.DelayToDestroy)); // con tiempo para que se alcance a reproducir la animación de muerte
        gameplayData.Score += enemy.ScorePoints;
        uiManager.UpdateScore(gameplayData.Score);
        ExtraLifeValidation();
    }

    IEnumerator DestroyWithDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolManager.Instance.Release(obj);
    }



    IEnumerator PlayerDead()
    {
        gameplayData.Lives--;
        if (musicState != MusicState.Battle)
            audioManager.StopGameMusic();

        powerUpManager.PlayerDead();

        // Se prefiere NO detener el spawner, porque si el player muere varias veces y si detenemos el spawner
        // entonces pueden pasar varios segundos hasta que un spawner se active.
        //spawnerManager.Stop();

        // Se espera un poco para que alcance a mostrarse la explosion del player
        yield return new WaitForSeconds(gameManagerData.waitForPlayerExplosion);
        backgroundManager.Pause(); // No se usa Stop, porque luego se quiere hacer un Resume

        if (gameplayData.Lives == 0)
            StartCoroutine(GameOver());
        else
            StartCoroutine(RespawnPlayerRoutine());
    }

    IEnumerator GameOver()
    {
        spawnerManager.Stop();
        uiManager.ShowGameOverMessage();
        float waitForRestartGame = audioManager.PlayGameOverMusic();
        yield return new WaitForSeconds(waitForRestartGame);
        HighScoreValidation();
    }

    void HighScoreValidation()
    {
        if (gameplayData.Score > initialHiScore)
        {            
            uiManager.ShowHiScoreMessage();
            backgroundManager.HighSpeed();
            audioManager.PlayHiScoreMusic();
        }
        else
            sceneController.LoadWelcome();
    }

    void SaveDataGame()
    {
        GameDataRepository.Update(gameplayData);
        GameDataRepository.Save();
    }

    IEnumerator RespawnPlayerRoutine()
    {
        if (musicState != MusicState.Battle)
            audioManager.PlayGameMusic();
        uiManager.ShowAndHideRespawnMessage(gameManagerData.secondsShowRespawnMessage);
        yield return new WaitForSeconds(gameManagerData.secondsShowRespawnMessage);
        uiManager.UpdateLives(gameplayData.Lives);
        player.SetGodModeDuration(gameManagerData.godModeDuration);
        player.Respawn();
        backgroundManager.Resume();
        //spawnerManager.Resume(); // Por ahora no se está haciendo un Stop cuando muere el player
    }

    IEnumerator EnterPressedRoutine(string text)
    {
        backgroundManager.Stop();
        audioManager.PlayPressEnter();
        uiManager.EnterPressed(text);
        float clipDuration = audioManager.GetDurationPressEnter();
        yield return new WaitForSeconds(clipDuration + gameManagerData.extraDurationEnterPressed);
        sceneController.LoadGame();
    }    

    void ResetGameplayData()
    {
        gameplayData.NextScoreToExtendLife = gameManagerData.extraLifeEveryPoints;
        gameplayData.Score = 0;
        gameplayData.Lives = gameManagerData.initialLives;
    }

}
