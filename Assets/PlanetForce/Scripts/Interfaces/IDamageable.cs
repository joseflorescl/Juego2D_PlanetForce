using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{

    float Health { get; }
    float CurrentHealthPercentage { get; }
    bool IsDead { get; }
    public void Damage(float damage);
}
