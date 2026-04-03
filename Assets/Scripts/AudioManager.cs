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
    public AudioClip bombSound;
    public AudioClip loseSound;
    public AudioClip bgSound;
    public AudioClip buildSound;
    public AudioClip clickSound;

    private void Awake()
    {
        ins = this;
    }

    private void Start()
    {
        PlayMusic();
    }

    public void PlaySound(AudioClip audioClip)
    {
        sound.PlayOneShot(audioClip,1);
    }
    public void PlaySoundClick()
    {
        sound.PlayOneShot(clickSound,1);
    }
    public void PlaySoundMerge()
    {
        sound.PlayOneShot(lstMergeSound[Random.Range(0,lstMergeSound.Count)],1);
    }
    public void PlaySoundReward()
    {
        sound.PlayOneShot(rewardSound,1);
    }
    public void PlaySoundBomb()
    {
        sound.PlayOneShot(bombSound,1);
    }
    public void PlaySoundBuild()
    {
        sound.PlayOneShot(buildSound,1);
    }
    public void PlayMusicLose()
    {
        music.loop = false;
        music.clip = loseSound;
        music.Play();
    }
    public void PlayMusic()
    {
        music.loop = true;
        music.clip = bgSound;
        music.Play();
    }
}
