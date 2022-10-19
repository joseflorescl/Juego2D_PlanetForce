using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerUpController : MonoBehaviour
{
    [SerializeField] protected AudioClip powerUpCatchedClip;

    protected GameManager gameManager;

    protected abstract void PowerUpCatched();

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }


    public void PowerUpCreated() // Es llamado desde el spawner
    {
        gameManager.SendToBackground(transform);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // El layer PowerUp solo puede colisionar consigo mismo
        // Por eso el objeto del player tiene un collider especializado para esto.
        // Nada más puede colisionar con un powerup, por eso no se valida con un if el tipo de objeto que viene en collision
        PowerUpCatched();// Esto perfectamente podría ser un evento...
        Destroy(gameObject);
    }

}
