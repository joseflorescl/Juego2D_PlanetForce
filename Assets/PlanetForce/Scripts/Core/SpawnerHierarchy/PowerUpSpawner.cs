using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : Spawner<PowerUpSpeedAndWeaponController>
{
    protected override void SetValuesPostCreation(PowerUpSpeedAndWeaponController powerUp)
    {
        powerUp.PowerUpCreated();
    }

}
