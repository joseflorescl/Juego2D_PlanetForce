using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityHealth))]
public abstract class EntityController : KinematicProjectile
{
    [SerializeField] protected EntityData entityData;
    protected Animator anim;
    protected Collider2D[] colliders2D;
    protected ProjectileFactory bulletFactory;
    protected Rigidbody2D rb2D;
    protected EntityRenderer entityRenderer;
    protected EntityHealth entityHealth;
    protected EntityVFX entityVFX;

    GameManager gameManager; // Como esta componente EntityController se usa en un prefab, NO se puede inic desde el Inspector la var gameManager

    public float DelayToDestroy => entityData.delayToDestroy;
    public int ScorePoints => entityData.scorePoints;
    public AudioClip[] AudioClipsShoot => entityData.audioClipsShoot;
    public AudioClip[] AudioClipsExplosion => entityData.audioClipsExplosion;
    public AudioClip[] AudioClipsDamage => entityData.audioClipsDamage;
    public float MaxHealth => entityData.maxHealth;

    public EntityData.AudioClipsInBattle AudioClipsInBattle => entityData.audioClipsInBattle;

    public override float Speed => entityData.speed;

    protected virtual void Awake()
    {
        gameManager = FindObjectOfType<GameManager>(); // Esto podr�a ser un Singleton o equivalente
        anim = GetComponent<Animator>();
        colliders2D = GetComponentsInChildren<Collider2D>(); // Una entidad puede tener varios colliders
        bulletFactory = GetComponentInChildren<ProjectileFactory>(); // Se setea en objeto hijo Weapon
        rb2D = GetComponent<Rigidbody2D>();
        entityRenderer = GetComponentInChildren<EntityRenderer>(); // Es la abstracci�n del SpriteRenderer que esta en hijo Model
        entityHealth = GetComponent<EntityHealth>();
        entityVFX = GetComponent<EntityVFX>();
    }    

    protected void EntityAppearsInBattle()
    {
        gameManager.EntityAppearsInBattle(this);
    }

    protected void EntityDeadInBattle()
    {
        gameManager.EntityDeadInBattle(this);
    }

    protected virtual Vector3 PlayerPosition()
    {
        if (!gameManager) 
            return Vector3.down * 10;
        return gameManager.PlayerPosition();
    }

    protected virtual Vector3 TargetPosition()
    {
        return PlayerPosition();
    }

    public virtual void Dead()
    {
        StopAllCoroutines();
        anim.SetTrigger("Dead");
        DisableColliders();
        rb2D.velocity = Vector3.zero;
        gameManager?.EntityDead(this);
    }

    protected virtual void DisableColliders()
    {
        for (int i = 0; i < colliders2D.Length; i++)
        {
            colliders2D[i].enabled = false;
        }
    }

    protected virtual void EnableColliders()
    {
        for (int i = 0; i < colliders2D.Length; i++)
        {
            colliders2D[i].enabled = true;
        }
    }

    public virtual void Damage()
    {        
        anim.SetFloat("CurrentHealth", entityHealth. CurrentHealthPercentage);
        gameManager?.EntityDamaged(this);
        entityVFX?.PlayDamage();
    }
    

    public virtual void FireBullet()
    {
        if (bulletFactory.Create())
            gameManager?.EntityFired(this);
    }

    public virtual void FireBulletToTarget(Vector3 targetPosition)
    {
        if (bulletFactory.CreateToTarget(targetPosition))
            gameManager?.EntityFired(this);
    }

    public virtual void Move(Vector3 movement)
    {        
        transform.Translate(Time.deltaTime * movement);
    }

    public virtual void UpdateAnimation(Vector3 movement)
    {
        anim.SetFloat("MovementX", movement.x);
    }
    
    public override void SetKinematicVelocity(Vector3 direction, float speed)
    {
        rb2D.velocity = direction.normalized * speed;
    }

    public virtual bool IsVisible()
    {
        return entityRenderer.IsVisible;
    }

    protected void SendEntityToBackground()
    {
        gameManager.SendToBackground(transform);
    }

    protected IEnumerator FireRoutineToTarget()
    {
        yield return new WaitUntil(() => IsVisible());

        float wait = Random.Range(entityData.minSecondsBetweenFire, entityData.maxSecondsBetweenFire);
        yield return new WaitForSeconds(wait);

        while (!entityHealth.IsDead) 
        {
            FireBulletToTarget(TargetPosition());
            wait = Random.Range(entityData.minSecondsBetweenFire, entityData.maxSecondsBetweenFire);
            yield return new WaitForSeconds(wait);
        }
    }

    protected void SetWeightAnimationLayer(string layer, float weight)
    {
        int idxLayer = anim.GetLayerIndex(layer);
        if (idxLayer >= 0)
        {
            anim.SetLayerWeight(idxLayer, weight);
        }
    }

    
}
