using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Entity Data", menuName = "Planet Force/Entity Data")]
public class EntityData : ScriptableObject
{
    public float speed;
    public float maxHealth;
    public float delayToDestroy = 1f;
    public int scorePoints = 50;    
    public float minSecondsBetweenFire = 1;
    public float maxSecondsBetweenFire = 10;
    public float notFireBelowThisHeight = -8f;
    public float destroyBelowThisHeight = -10f;

    [Header("Audio Clips")]
    [Space(10)]
    public AudioClip[] audioClipsShoot;
    public AudioClip[] audioClipsExplosion;
    public AudioClip[] audioClipsDamage;

    [System.Serializable]
    public struct AudioClipsInBattle
    {
        public AudioClip[] audioClipsEntityAppearance;
        public AudioClip[] audioClipsEntityFight;
        public AudioClip[] audioClipsEntityDead;
    }

    [Header("Optional Audio Clips: when this entity is in the battle field")]
    public AudioClipsInBattle audioClipsInBattle;

}
