using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController
{
    // Movimiento del Player:
    // Si se presionan ambas flechas a la vez, el vector resultante NO se creará en la dirección del vector 1,1 
    // sino que se respetará el aspect ratio de la pantalla, es decir apuntará usando el vector de la diagonal de la pantalla    
    [SerializeField] private int firesPerSecond = 20;
    [SerializeField] private float animSpeedX = 1f;
    [SerializeField] private Vector2 respawnPosition;
    [SerializeField] private Collider2D powerUpCollider; // Usado para que el player pueda agarrar los power up cuando está en GodMode

    [Header("Limit Movement in Percentage (0-1)")]
    [SerializeField] private Vector2 minScreen;
    [SerializeField] private Vector2 maxScreen;
        
    private float speedX; // Por defecto, su valor es el seteado en el Scriptable Object, pero se cambia con un powerup
    private float speedY;
    private Vector2 minWorldPosition;
    private Vector2 maxWorldPosition;
    float fireRate;
    float nextFire;
    float blendSpeedX;
    ProjectileFactory originalWeapon;
    bool hasPowerUpWeapon;
    


    protected override void Awake()
    {
        base.Awake();        
        originalWeapon = bulletFactory;
    }

    private void Start()
    {
        transform.position = respawnPosition;
        ResetSpeed();
        CalculateLimitWorldPosition();
        fireRate = 1f / firesPerSecond;
        nextFire = 0f;
        blendSpeedX = 0;
        hasPowerUpWeapon = false;
    }

    void Update()
    {
        if (entityHealth.IsDead) return;

        bool buttonPressedDown = Input.GetKeyDown(KeyCode.LeftControl) 
                              || Input.GetKeyDown(KeyCode.RightControl) 
                              || Input.GetButtonDown("Fire1");

        bool buttonPressed = Input.GetKey(KeyCode.LeftControl) 
                          || Input.GetKey(KeyCode.RightControl) 
                          || Input.GetButton("Fire1");

        if ( (buttonPressedDown || (hasPowerUpWeapon && buttonPressed)) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            FireBullet();
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        UpdateMovement(horizontal, vertical);
        UpdateAnimation(horizontal, vertical);
    }

    

    public void ActivateSpeedAndWeaponPowerUp(float speed, ProjectileFactory weapon)
    {
        hasPowerUpWeapon = true;
        SetNewSpeed(speed);
        bulletFactory = weapon;
        // Notar el uso de false: para que la weapon se reubique en posición CON RESPECTO al player
        weapon.gameObject.transform.SetParent(transform, false);
        anim.SetLayerWeight(anim.GetLayerIndex("PowerUpLayer"), 1);
    }

    public void DeactivateSpeedAndWeaponPowerUp()
    {
        hasPowerUpWeapon = false;
        ResetSpeed();
        ResetWeapon();
        anim.SetLayerWeight(anim.GetLayerIndex("PowerUpLayer"), 0);
    }


    public void ResetSpeed()
    {
        speedX = entityData.speed;
        CalculateSpeedY(speedX);
    }

    public void Respawn()
    {
        entityHealth.RestoreHealth();
        transform.position = respawnPosition;
        anim.SetTrigger("Respawn"); // Al final de la animación se activa el collider de colisión con los enemigos
        powerUpCollider.enabled = true; // pero igual podremos agarrar powerups en GodMode
        // Notar que el layer Player NO colisiona con el layer PowerUp, pero el layer PowerUp sí puede colisionar consigo mismo
        // por eso el objeto Player tiene un collider específico para las colisiones con los powerups,
        // que es un poco más grande que el collider usado para las colisiones con los enemigos
    }

    public void EnablePlayerColliders() // Llamado desde un evento de animacion
    {
        EnableColliders();
    }

    public void SetGodModeDuration(float godModeDuration)
    {
        anim.SetFloat("GodModeDuration", 1f / godModeDuration);
    }

    void SetNewSpeed(float speed)
    {
        speedX = speed;
        CalculateSpeedY(speedX);
    }

    void ResetWeapon()
    {
        bulletFactory = originalWeapon;
    }

    private void UpdateMovement(float horizontal, float vertical)
    {
        if (horizontal == 0 && vertical == 0) return;

        Vector2 movement;
        movement.x = horizontal;
        movement.y = vertical;
        movement.Normalize();
        movement.x *= speedX;
        movement.y *= speedY;
        Move(movement);
        ClampWorldPosition();
    }

    private void UpdateAnimation(float horizontal, float vertical)
    {
        blendSpeedX = Mathf.MoveTowards(blendSpeedX, horizontal, animSpeedX * Time.deltaTime);
        Vector2 movement;
        movement.x = blendSpeedX;
        movement.y = vertical;
        UpdateAnimation(movement);
    }

    private void CalculateSpeedY(float speedX)
    {
        speedY = speedX * Screen.height / Screen.width;
    }

    private void CalculateLimitWorldPosition()
    {
        minWorldPosition = Camera.main.ViewportToWorldPoint(minScreen);
        maxWorldPosition = Camera.main.ViewportToWorldPoint(maxScreen);
        minWorldPosition += (Vector2)entityRenderer.Extents;
        maxWorldPosition -= (Vector2)entityRenderer.Extents;
    }

    private void ClampWorldPosition()
    {
        var pos = transform.position;

        if (pos.x < minWorldPosition.x || pos.x > maxWorldPosition.x || pos.y < minWorldPosition.y || pos.y > maxWorldPosition.y)
        {
            pos.x = Mathf.Clamp(pos.x, minWorldPosition.x, maxWorldPosition.x);
            pos.y = Mathf.Clamp(pos.y, minWorldPosition.y, maxWorldPosition.y);
            transform.position = pos;
        }
    }
    
}
