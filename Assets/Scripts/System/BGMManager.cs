using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    public AudioSource bgmSource; // 인스펙터에서 메인 BGM AudioSource 할당
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }

    public void MuteBGM(bool isMuted)
    {
        bgmSource.volume = isMuted ? 0f : 1f;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }



}