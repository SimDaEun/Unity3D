using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;  //�̱���

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    Dictionary<string,AudioClip> bgmClips = new Dictionary<string, AudioClip> ();
    Dictionary<string,AudioClip> sfxClips = new Dictionary<string, AudioClip> ();

    [System.Serializable]
    public struct NamedAudioClip    //��ųʸ��� �ν����� â���� ǥ�Ⱑ �ȵǱ� ������, ����ü�� ���� ǥ�� 
    {
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;



    private void Awake()
    {
        if (instance == null)  //�ߺ��� �����ϱ� ���� ����ó�� 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioClips();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
    }

    void Update()
    {
    }

    void InitializeAudioClips()
    {
        foreach (var bgm in bgmClipList)
        {
            if (!bgmClips.ContainsKey(bgm.name))
            {
                bgmClips.Add(bgm.name, bgm.clip);
            }
        }
        foreach (var sfx in sfxClipList)
        {
            if (!sfxClips.ContainsKey(sfx.name))
            {
                sfxClips.Add(sfx.name, sfx.clip);
            }
        }
    }

    public void PlayBGM(string name)
    {
        if (bgmClips.ContainsKey(name))
        {
            bgmSource.clip = bgmClips[name];
            bgmSource.Play();
        }
    }
    public void PlaySFX(string name, Vector3 position)
    {
        if (sfxClips.ContainsKey(name))
        {
            AudioSource.PlayClipAtPoint(sfxClips[name], position);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }

    public void StopBGM()
    {
        bgmSource.Stop();    
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void PlayFootSound(string hitTag, Vector3 position)
    {
        PlaySFX(hitTag, position);
    }

}
