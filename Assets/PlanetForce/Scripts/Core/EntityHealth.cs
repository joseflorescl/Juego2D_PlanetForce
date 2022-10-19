using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityController))]
public class EntityHealth : MonoBehaviour, IDamageable
{
    EntityController entityController;
    float health;

    public float Health => health;
    public bool IsDead => health <= 0f;
    public float CurrentHealthPercentage => IsDead ? 0f : health / entityController.MaxHealth;



    private void Awake()
    {
        entityController = GetComponent<EntityController>();
        RestoreHealth(); // Es fundamental que el seteo de health se haga en el Awake, no en el Start, porque es usada por la entity
    }

    public void RestoreHealth()
    {
        health = entityController.MaxHealth;
    }

    public void Damage(float damage)
    {
        // Condición de borde: un enemigo muerto por 2 balas: genera 2 colisiones, pero la idea es que solo 1 lo mate, la segunda bala no debe marcar doble puntaje
        if (IsDead) return;

        // Se le quita salud al objeto y se valida si se le acaba para destruirlo.
        health -= damage;

        if (health <= 0)
            entityController.Dead();
        else
            entityController.Damage();
    }

}
