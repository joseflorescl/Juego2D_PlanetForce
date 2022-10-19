using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AudioManager Data", menuName = "Planet Force/Audio Manager Data")]
public class AudioManagerData : ScriptableObject
{
    [Header("BGM Sounds")]
    public AudioClip[] inGameMusic;
    public AudioClip[] powerUpMusic;
    public AudioClip[] startGameMusic;
    public AudioClip[] gameOverMusic;
    public AudioClip[] hiScoreMusic;
    public AudioClip[] theEndMusic;

    [Space(10)]
    [Header("SFX Sounds")]
    public AudioClip pressEnter;
    public AudioClip[] extraLife;

}
