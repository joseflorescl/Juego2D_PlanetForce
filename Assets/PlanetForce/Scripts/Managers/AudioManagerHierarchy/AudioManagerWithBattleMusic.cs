using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerWithBattleMusic : AudioManager
{

    EntityData.AudioClipsInBattle audioClipsInBattle;
    

    public void SetBattleMusicData(EntityData.AudioClipsInBattle data)
    {
        audioClipsInBattle = data;
    }

    public void PlayBattleMusic()
    {
        StopAudioRoutine();
        audioRoutine = StartCoroutine(PlayBattleMusicRoutine());
    }

    public void EndBattleMusic(bool entityDead, float delay, System.Action callback)
    {
        StopGameMusic();
        audioRoutine = StartCoroutine(EndBattleMusicRoutine(entityDead, delay, callback));
    }

    IEnumerator PlayBattleMusicRoutine()
    {
        AudioClip clip = GetRandomClip(audioClipsInBattle.audioClipsEntityAppearance);
        PlayBGMMusic(clip, false);
        var duration = clip.length;
        yield return new WaitForSeconds(duration);

        clip = GetRandomClip(audioClipsInBattle.audioClipsEntityFight);
        PlayBGMMusic(clip, true);
    }

    

    IEnumerator EndBattleMusicRoutine(bool entityDead, float delay, System.Action callback)
    {
        yield return new WaitForSeconds(delay);

        // Si la entidad ha muerto se debe reproducir el clip asociado al dead de la entity
        if (entityDead)
        {            
            var clip = GetRandomClip(audioClipsInBattle.audioClipsEntityDead);
            PlayBGMMusic(clip, false);
            yield return new WaitForSeconds(clip.length);
        }
        
        callback();
    }

    
}
