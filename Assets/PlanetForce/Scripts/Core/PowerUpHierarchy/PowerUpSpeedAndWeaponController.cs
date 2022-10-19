using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PowerUpSpeedAndWeaponController : PowerUpController
{
    [SerializeField] private float powerUpTime; // el tiempo que puede estar activo el power up
    [SerializeField] private float speed;


    protected override void PowerUpCatched()
    {
        gameManager.SpeedAndWeaponPowerUp(powerUpCatchedClip, powerUpTime, speed);
    }
}
