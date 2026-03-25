using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum SoundKey
{
    NormalBGM,
    TitleBGM,
    CombatBGM,
    EliteBGM,
    BossBGM,
    EndingBGM,
    TouchSFX,
}
[Serializable]
public struct Sound
{
    public Sound(SoundKey key, AudioClip audioClip)
    {
        this.key = key; 
        this.audioClip = audioClip;
    }
    public SoundKey key;
    public AudioClip audioClip;
}

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private Sound[] sounds;
    [SerializeField] private AudioSource sourceBGM;
    [SerializeField] private Transform SFXTransform;
    private List<AudioSource> SFXSources = new List<AudioSource>();

    public bool isLoadComplete { get; private set; }

    public Dictionary<SoundKey, Sound> soundDictionary = new Dictionary<SoundKey, Sound>();
    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        SFXSources.Clear();
        foreach (AudioSource source in SFXTransform.GetComponentsInChildren<AudioSource>())
        {
            SFXSources.Add(source);
        }
    }
    private void OnApplicationQuit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
    public void Load(Action callback = null)
    {
        StartCoroutine(Cor());

        IEnumerator Cor()
        {
            foreach (Sound sound in sounds)
            {
                soundDictionary.Add(sound.key, sound);
            }

            isLoadComplete = true;

            yield return null;

            callback?.Invoke();
        }
    }
    public void PlayBGM(SoundKey key)
    {
        if (soundDictionary.TryGetValue(key, out Sound sound) &&
            sourceBGM.clip != sound.audioClip)
        {
            sourceBGM.volume = 0f;
            sourceBGM.clip = sound.audioClip;

            sourceBGM.Play();
            sourceBGM.DOFade(1f, 0.25f);
        }
    }
    public void StopBGM()
    {
        sourceBGM.Stop();
    }
    public void PlaySFX(SoundKey key)
    {
        if (soundDictionary.TryGetValue(key, out Sound sound))
        {
            AudioSource src = null;

            for (int i = 0; i < SFXSources.Count; i++)
            {
                if (!SFXSources[i].isPlaying)
                {
                    src = SFXSources[i];
                    break;
                }
            }

            if (src == null)
            {
                return;
            }

            src.clip = sound.audioClip;
            src.Play();
        }
    }
    public void Init(float volumeBGM, float volumeSFX)
    {
        SetVolumeBGM(volumeBGM);
        SetVolumeSFX(volumeSFX);
    }
    public void SetVolumeBGM(float volume)
    {
        sourceBGM.volume = volume;
    }
    public void SetVolumeSFX(float volume)
    {
        for (int i = 0; i < SFXSources.Count; i++)
        {
            SFXSources[i].volume = volume;
        }
    }
}