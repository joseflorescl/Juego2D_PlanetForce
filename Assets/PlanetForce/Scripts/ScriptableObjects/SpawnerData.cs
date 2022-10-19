using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spawner Data", menuName = "Planet Force/Spawner Data")]
public class SpawnerData : ScriptableObject
{
    public float duration = 5f;
    public float rateOverTime = 5;
    public float startSpeed = 1f;
    public float startRotation; 
    public int maxInstances = 100;
    public int burstCount = 5; // cantidad de elementos que se instancian a la vez en un mismo frame

    public SpawnerShape shape;

    public float horizontalRange = 8f;
    public Vector3 offsetPosition; // al instanciar, la posición se mueve de acuerdo a este offset

    public float SpawnerRate => 1f / rateOverTime;
    public Quaternion SpawnRotation => Quaternion.AngleAxis(startRotation, Vector3.forward);

}
