using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager ins;
    public AudioSource sound;
    public AudioSource music;
    public List<AudioClip> lstMergeSound;
    public AudioClip rewardSound;
    public AudioClip loseSound;
    public AudioClip bgSound;
    public AudioClip winSound;
    public AudioClip clickSound;
    public AudioClip swipeLeftSound;
    public AudioClip swipeRightSound;

    public List<AudioClip> lstMoveSound;

    private void Awake()
    {
        ins = this;
    }

    private void Start()
    {
        bgSound=LunaManager.ins.bgMusic;
        PlayMusic();
    }

    public void PlaySound(AudioClip audioClip)
    {
        sound.PlayOneShot(audioClip,1);
    }

    public void PlaySoundMerge()
    {
        sound.PlayOneShot(lstMergeSound[Random.Range(0,lstMergeSound.Count)],1);
    }
    public void PlaySoundClick()
    {
        sound.PlayOneShot(clickSound,1);
    }
    public void PlayswipeLeftSound()
    {
        sound.PlayOneShot(swipeLeftSound,1);
    }
    public void PlayswipeRightSound()
        {
            sound.PlayOneShot(swipeRightSound,1);
        }
    public void PlaySoundMove()
    {
        sound.PlayOneShot(lstMoveSound[Random.Range(0,lstMoveSound.Count)],1);
    }
    public void PlaySoundReward()
    {
        sound.PlayOneShot(rewardSound,1);
    }
    public void PlayMusicLose()
    {
        music.Stop();
        music.loop = false;
        music.clip = loseSound;
        music.Play();
    }
    public void PlayMusicWin()
    {
        music.Stop();
        music.loop = false;
        music.clip = winSound;
        music.Play();
    }
    public void StopMusic()
    {
        music.Stop();
    }
    public void PlayMusic()
    {
        music.Stop();
        music.loop = true;
        music.clip = bgSound;
        music.Play();
    }
}
