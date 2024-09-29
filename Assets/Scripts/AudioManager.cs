using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Variables
    private static AudioManager instance;
    private AudioSource _audioSource;
    public AudioClip TitleTheme;
    public AudioClip BattleTheme;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("AudioManager").GetComponent<AudioManager>();
            return instance;
        }
    }
    
    #endregion
    
    #region Initialization
    void Awake()
    {
        _audioSource = GameObject.Find("CurrentTheme").GetComponent<AudioSource>();

        PlayTitleTheme();
    }
    
    #endregion
    
    #region Logic

    public void PlayTitleTheme()
    {
        if (_audioSource.clip != TitleTheme)
        {
            StartCoroutine(CrossfadeAudio(TitleTheme, 1f));
        }
    }

    public void PlayBattleTheme()
    {
        if (_audioSource.clip != BattleTheme)
        {
            StartCoroutine(CrossfadeAudio(BattleTheme, 1f));
        }
    }
    
    
    public IEnumerator CrossfadeAudio(AudioClip newClip, float fadeDuration)
    {
        float currentTime = 0;
        float startVolume = _audioSource.volume;
        
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
            yield return null;
        }
        
        _audioSource.Stop();
        _audioSource.clip = newClip;
        _audioSource.Play();
        
        currentTime = 0;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0, startVolume, currentTime / fadeDuration);
            yield return null;
        }
    }

    public void VolumeChange(float value)
    {
        _audioSource.volume = value;
    }

    #endregion

}
