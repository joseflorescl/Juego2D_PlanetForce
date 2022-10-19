using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    const string WEAPON_NORMAL = "WeaponNormal";
    const string WEAPON_POWERUP = "WeaponPowerUp";

    Dictionary<string, ProjectileFactory> playerWeapons; // La key es el tag de tipo de Weapon
    PlayerController player;

    Coroutine speedAndWeaponRoutine;

    void Awake()
    {
        InitPlayerWeapons();
    }

    public void SetPlayer(PlayerController player)
    {
        this.player = player;
    }

    public void SpeedAndWeaponPowerUp(float powerUpTime, float speed)
    {
        ActivateSpeedAndWeaponPowerUp(speed);

        if (speedAndWeaponRoutine != null)
            StopCoroutine(speedAndWeaponRoutine);
        speedAndWeaponRoutine = StartCoroutine(DeactivateSpeedAndWeaponPowerUpRoutine(powerUpTime));
    }


    void ActivateSpeedAndWeaponPowerUp(float speed)
    {
        var weapon = playerWeapons[WEAPON_POWERUP];
        player.ActivateSpeedAndWeaponPowerUp(speed, weapon);
    }

    IEnumerator DeactivateSpeedAndWeaponPowerUpRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeactivateSpeedAndWeaponPowerUp();
    }

    void DeactivateSpeedAndWeaponPowerUp()
    {
        player.DeactivateSpeedAndWeaponPowerUp();
        var weapon = playerWeapons[WEAPON_POWERUP];
        weapon.gameObject.transform.SetParent(transform, false); // Esto no es estrictamente necesario, pero queda ordenada la jerarquía
    }

    public void PlayerDead()
    {
        if (speedAndWeaponRoutine != null)
        {
            StopCoroutine(speedAndWeaponRoutine);
            DeactivateSpeedAndWeaponPowerUp();
        }
    }

    void InitPlayerWeapons()
    {
        playerWeapons = new Dictionary<string, ProjectileFactory>();
        var arrayWeapons = GetComponentsInChildren<ProjectileFactory>();
        for (int i = 0; i < arrayWeapons.Length; i++)
        {
            var weapon = arrayWeapons[i];
            playerWeapons[weapon.tag] = weapon;
        }
    }


}
