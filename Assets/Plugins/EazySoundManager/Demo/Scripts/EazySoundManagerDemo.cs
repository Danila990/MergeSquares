using System;
using Hellmade;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EazySoundManagerDemo : MonoBehaviour
{
    public EazySoundDemoAudioControls[] AudioControls;
    public Slider globalVolSlider;
    public Slider globalMusicVolSlider;
    public Slider globalSoundVolSlider;

    public GameObject test;

    private EazySoundManager _soundManager;

    [Inject]
    public void Construct(EazySoundManager soundManager)
    {
        _soundManager = soundManager;
    }

    private void Update()
    {
        // Update UI
        for (var i = 0; i < AudioControls.Length; i++)
        {
            EazySoundDemoAudioControls audioControl = AudioControls[i];
            if (audioControl.audio != null && audioControl.audio.IsPlaying)
            {
                if (audioControl.pauseButton != null)
                {
                    audioControl.playButton.interactable = false;
                    audioControl.pauseButton.interactable = true;
                    audioControl.stopButton.interactable = true;
                    audioControl.pausedStatusTxt.enabled = false;
                }
            }
            else if (audioControl.audio != null && audioControl.audio.Paused)
            {
                if (audioControl.pauseButton != null)
                {
                    audioControl.playButton.interactable = true;
                    audioControl.pauseButton.interactable = false;
                    audioControl.stopButton.interactable = false;
                    audioControl.pausedStatusTxt.enabled = true;
                }
            }
            else
            {
                if (audioControl.pauseButton != null)
                {
                    audioControl.playButton.interactable = true;
                    audioControl.pauseButton.interactable = false;
                    audioControl.stopButton.interactable = false;
                    audioControl.pausedStatusTxt.enabled = false;
                }
            }
        }
    }

    public void PlayMusic1()
    {
        var audioControl = AudioControls[0];

        if (audioControl.audio == null)
        {
            var audioID =
                _soundManager.PlayMusic(audioControl.audioclip, audioControl.volumeSlider.value, true, false);
            AudioControls[0].audio = _soundManager.GetAudio(audioID);
        }
        else if (audioControl.audio != null && audioControl.audio.Paused)
        {
            audioControl.audio.Resume();
        }
        else
        {
            audioControl.audio.Play();
        }
    }

    public void PlayMusic2()
    {
        var audioControl = AudioControls[1];

        if (audioControl.audio == null)
        {
            var audioID =
                _soundManager.PlayMusic(audioControl.audioclip, audioControl.volumeSlider.value, true, false);
            AudioControls[1].audio = _soundManager.GetAudio(audioID);
        }
        else if (audioControl.audio != null && audioControl.audio.Paused)
        {
            audioControl.audio.Resume();
        }
        else
        {
            audioControl.audio.Play();
        }
    }

    public void PlaySound1()
    {
        var audioControl = AudioControls[2];
        var audioID = _soundManager.PlaySound(audioControl.audioclip, audioControl.volumeSlider.value);

        AudioControls[2].audio = _soundManager.GetAudio(audioID);
    }

    public void PlaySound2()
    {
        var audioControl = AudioControls[3];
        var audioID = _soundManager.PlaySound(audioControl.audioclip, audioControl.volumeSlider.value);

        AudioControls[3].audio = _soundManager.GetAudio(audioID);
    }

    public void Pause(string audioControlIDStr)
    {
        var audioControlID = int.Parse(audioControlIDStr);
        var audioControl = AudioControls[audioControlID];

        audioControl.audio.Pause();
    }

    public void Stop(string audioControlIDStr)
    {
        var audioControlID = int.Parse(audioControlIDStr);
        var audioControl = AudioControls[audioControlID];

        audioControl.audio.Stop();
    }

    public void AudioVolumeChanged(string audioControlIDStr)
    {
        var audioControlID = int.Parse(audioControlIDStr);
        var audioControl = AudioControls[audioControlID];

        audioControl.audio?.SetVolume(audioControl.volumeSlider.value, 0);
    }

    public void GlobalVolumeChanged()
    {
        _soundManager.GlobalVolume = globalVolSlider.value;
    }

    public void GlobalMusicVolumeChanged()
    {
        _soundManager.GlobalMusicVolume = globalMusicVolSlider.value;
    }

    public void GlobalSoundVolumeChanged()
    {
        _soundManager.GlobalSoundsVolume = globalSoundVolSlider.value;
    }
}

[Serializable]
public struct EazySoundDemoAudioControls
{
    public AudioClip audioclip;
    public Audio audio;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Slider volumeSlider;
    public Text pausedStatusTxt;
}