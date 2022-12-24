using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAndorGenesis : EntityWayPointsMovementController
{
    [SerializeField] private GameObject[] explosions;

    EntitySpawner[] entitySpawners; // Un enemigo grande puede no solo disparar, sino que también generar otros enemigos durante su vida

    protected override void Awake()
    {
        base.Awake();
        entitySpawners = GetComponentsInChildren<EntitySpawner>(); 
    }

    public override void Init()
    {
        base.Init();
        EntityAppearsInBattle();
    }

    public override void Damage()
    {
        base.Damage();
        ActivateExplosions(entityHealth.CurrentHealthPercentage);        
        anim.SetTrigger("Damage");
    }

    public override void Dead()
    {
        DeactivateSpawners();
        anim.ResetTrigger("Damage"); // Para asegurarnos de que se mande correctamente el trigger de Dead se debe apagar el trigger de Damage
        base.Dead();
        ActivateExplosions(0);
        EntityDeadInBattle();
    }

    public void EndDeadAnimation() // Llamada desde el evento de animación en Dead, para que al final de la animación la nave se fije al fondo.
    {
        SendEntityToBackground();
        entityVFX.PlayDead();
    }

    void ActivateExplosions(float currentHealthPercentage)
    {
        float damagePercentage = 1 - currentHealthPercentage;
        int damageCount = (int)Mathf.Lerp(0, explosions.Length, damagePercentage);

        for (int i = 0; i < damageCount; i++)
        {
            explosions[i].SetActive(true);
            bulletFactory.AnalyzeExplosion(explosions[i].transform.position);
        }

    }

    void DeactivateSpawners()
    {
        for (int i = 0; i < entitySpawners.Length; i++)
        {
            entitySpawners[i].Deactivate();
        }
    }

}
