using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] protected AudioSource BGMAudioSource;
    [SerializeField] protected AudioSource SFXAudioSourceExplosions;
    [SerializeField] protected AudioSource SFXAudioSourceMisc;
    [SerializeField] protected AudioManagerData audioManagerData;

    protected Coroutine audioRoutine;

    HashSet<AudioClip> clipsPlayedThisFrame; // Se usa para que un mismo clip no se reproduzca 2 veces en un mismo frame, no vale la pena.


    private void Awake()
    {
        clipsPlayedThisFrame = new HashSet<AudioClip>();
    }

    private void LateUpdate()
    {
        clipsPlayedThisFrame.Clear();
    }


    public void PlayRandomDamageSound(AudioClip[] clips)
    {
        PlayRandomSound(clips, SFXAudioSourceMisc);
    }

    
    public void PlayRandomExplosionSound(AudioClip[] clips)
    {
        PlayRandomSound(clips, SFXAudioSourceExplosions);
    }

    public void PlayRandomFireSound(AudioClip[] clips)
    {
        PlayRandomSound(clips, SFXAudioSourceMisc);
    }

    protected void PlayRandomSound(AudioClip[] clips, AudioSource audioSource)
    {
        if (clips == null || clips.Length == 0) return; // Programación defensiva nunca está de más
        var clip = GetRandomClip(clips);
        SFXPlayOneShot(clip, audioSource);
    }

    protected void SFXPlayOneShot(AudioClip clip, AudioSource audioSource)
    {
        if (!clipsPlayedThisFrame.Contains(clip))
        {
            audioSource.PlayOneShot(clip);
            clipsPlayedThisFrame.Add(clip);
        }
    }


    public void PlayExtraLifeSound()
    {
        var clip = GetRandomClip(audioManagerData.extraLife);
        SFXPlayOneShot(clip, SFXAudioSourceMisc);
    }

    public void PlayHiScoreMusic()
    {
        var clip = GetRandomClip(audioManagerData.hiScoreMusic);
        PlayBGMMusic(clip, true);
    }

    public void PlayTheEndMusic()
    {
        StopAudioRoutine();
        var clip = GetRandomClip(audioManagerData.theEndMusic);
        PlayBGMMusic(clip, true);
    }



    public float PlayGameOverMusic()
    {
        StopAudioRoutine();
        var clip = GetRandomClip(audioManagerData.gameOverMusic);
        PlayBGMMusic(clip, false);
        return clip.length;
    }

    

    public void PlayWelcomeMusic()
    {
        StopAudioRoutine();
        PlayInGameMusic();
    }


    public void PlayPressEnter()
    {
        BGMAudioSource.Stop();
        SFXAudioSourceMisc.Stop();
        SFXAudioSourceMisc.clip = audioManagerData.pressEnter;
        SFXAudioSourceMisc.Play();
    }

    public float GetDurationPressEnter()
    {
        return audioManagerData.pressEnter.length;
    }


    public void PlayGameMusic()
    {
        StopAudioRoutine();
        audioRoutine = StartCoroutine(PlayGameMusicRoutine());
    }

    public void StopGameMusic()
    {
        StopAudioRoutine();
        BGMAudioSource.Stop();
    }

    public void PlayPowerUpMusic(AudioClip powerUpCatchedClip, bool playMusic, float powerUpTime)
    {
        if (!playMusic)
        {
            SFXPlayOneShot(powerUpCatchedClip, SFXAudioSourceMisc);
        }
        else
        {
            StopGameMusic();
            SFXPlayOneShot(powerUpCatchedClip, SFXAudioSourceMisc);
            var durationClip = powerUpCatchedClip.length;
            audioRoutine = StartCoroutine(PlayPowerUpMusicRoutine(durationClip, powerUpTime));
        }
        
    }

    IEnumerator PlayPowerUpMusicRoutine(float delay, float powerUpTime)
    {
        // Este delay es por el sonido del clip de atrapar el power up
        yield return new WaitForSeconds(delay); 

        // Ahora se reproduce la música de powerup,         
        var clip = GetRandomClip(audioManagerData.powerUpMusic);
        PlayBGMMusic(clip, true);

        //pero después de un rato se debe volver a la música normal del juego:
        yield return new WaitForSeconds(powerUpTime - delay);
        PlayInGameMusic();
    }


    protected AudioClip GetRandomClip(AudioClip[] audioClips)
    {
        int randomIdx = Random.Range(0, audioClips.Length);
        return audioClips[randomIdx];
    }

    protected void PlayBGMMusic(AudioClip clip, bool loop)
    {
        BGMAudioSource.loop = loop;
        BGMAudioSource.Stop();
        BGMAudioSource.clip = clip;
        BGMAudioSource.Play();
    }

    protected IEnumerator PlayGameMusicRoutine()
    {
        var duration = PlayStartMusic();
        yield return new WaitForSeconds(duration);
        PlayInGameMusic();
    }

    protected float PlayStartMusic()
    {
        var clip = GetRandomClip(audioManagerData.startGameMusic);
        var duration = clip.length;
        PlayBGMMusic(clip, false);
        return duration;
    }

    public void PlayInGameMusic()
    {
        var clip = GetRandomClip(audioManagerData.inGameMusic);
        PlayBGMMusic(clip, true);
    }

    protected void StopAudioRoutine()
    {
        if (audioRoutine != null)
            StopCoroutine(audioRoutine);
    }

}
