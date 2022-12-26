using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SpawnerManager : MonoBehaviour
{
    // Ahora ya no se usa una lista de los Spawner, sino que se hace uso de Timeline para ir activando a los Spawners
    PlayableDirector playableDirector;

    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
    }


    private void Start()
    {
        SetPositionTopCenterScreen();
        Stop();
        Play();
    }


    public void Play()
    {
        playableDirector.Play();
    }

    public void Stop()
    {
        playableDirector.Stop();
    }

    public void Kill()
    {
        Stop();                
        KillEnemies();
        DestroyWithTag("EnemyBullet");
        DestroyWithTag("PowerUp");
    }

    void SetPositionTopCenterScreen()
    {
        var topCenter = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 1));
        topCenter.z = transform.position.z;
        transform.position = topCenter;
    }

    void KillEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            if (enemy.TryGetComponent(out EntityController entity))
                entity.Dead();
        }
    }

    void DestroyWithTag(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < objects.Length; i++)
        {            
            PoolManager.Instance.Release(objects[i]);
        }
    }

}
